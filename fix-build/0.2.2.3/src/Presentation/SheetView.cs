using System;
using System.Collections.Generic;
using System.Linq;
using Alexander.CharacterSheet.Core;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Alexander.CharacterSheet
{
    // Native strips, icons, transforms and ordering remain owned by the game.
    internal sealed class SheetView:IDisposable
    {
        readonly HUD hud;
        readonly float scale;
        readonly Dictionary<ItemIcon,NativeItemHover> bindings=new Dictionary<ItemIcon,NativeItemHover>();
        readonly Dictionary<string,SheetRow> items=new Dictionary<string,SheetRow>();
        readonly List<AddedRowHover> statRows=new List<AddedRowHover>();
        readonly List<GameObject> effectRows=new List<GameObject>();
        CharacterBody body;
        ScoreboardStrip ownStrip;
        Canvas canvas;
        NativeUiStyle style;
        RectTransform root,content;
        NativeSheetLayer layer;
        NativeHoverPopup popup;
        object hovered;
        SheetRow hoveredRow;
        float leaveAt;
        bool effectsExpanded;
        TextMeshProUGUI effectsHeading;
        internal string Status{get;private set;}="Waiting for the native own-player strip.";
        string hoverCoverage="Own item hover coverage pending.";
        internal SheetView(HUD source,float presentationScale){hud=source;scale=presentationScale;}
        internal bool IsOwnIcon(ItemIcon icon)
        {
            if(!body||!body.master||!body.inventory||!icon)return false;
            var strip=icon.GetComponentInParent<ScoreboardStrip>();if(!strip)return false;
            var identity=strip.GetComponent<NativeStripIdentity>();
            if(!identity||identity.OwnMaster!=body.master)return false;
            var def=ItemCatalog.GetItemDef(icon.itemIndex);
            return def&&NativeTabPolicy.OwnVisibleItem(body.master,identity.OwnMaster,def.hidden,
                body.inventory.GetItemCountPermanent(def),body.inventory.GetItemCountEffective(def));
        }
        internal bool CanHandleIcon(ItemIcon icon)=>IsOwnIcon(icon)&&items.ContainsKey("item:"+(int)icon.itemIndex);
        internal void Refresh(CharacterBody source)
        {
            body=source;if(!body||!body.master||!hud||!hud.scoreboardPanel)return;
            var strips=hud.scoreboardPanel.GetComponentsInChildren<ScoreboardStrip>(false);
            var own=strips.FirstOrDefault(s=>{var identity=s.GetComponent<NativeStripIdentity>();return identity&&identity.OwnMaster==body.master;});
            if(!own){DetachBindings();if(root)root.gameObject.SetActive(false);popup?.Hide();Status="Native own-player strip is not ready.";return;}
            if(ownStrip!=own){DetachBindings();ownStrip=own;}
            if(style==null)
            {
                style=NativeUiStyle.Capture(ownStrip.transform);
                if(style==null||!NativeSheetLayer.TryCreate(hud,out layer)){style=null;Status="A safe native HUD branch or TMP font is unavailable; additions declined.";return;}
                canvas=layer.Canvas;CreatePanel();popup=new NativeHoverPopup(layer.Root,canvas,style,scale);
            }
            items.Clear();var inventory=SheetData.Inventory(body,false);foreach(var row in inventory)items[row.Id]=row;
            RefreshBindings();var character=SheetData.Character(body);
            if(statRows.Count!=character.Count)
            {
                foreach(var row in statRows)if(row){row.gameObject.SetActive(false);UnityEngine.Object.Destroy(row.gameObject);}
                statRows.Clear();for(int i=0;i<character.Count;i++)statRows.Add(CreateRow(character[i],(float)Metrics.RowHeight));
                for(int i=0;i<statRows.Count;i++)statRows[i].transform.SetSiblingIndex(1+i);
            }
            for(int i=0;i<character.Count;i++)SetRow(statRows[i],character[i]);
            var buffs=SheetData.Buffs(body);
            effectsHeading.text="Effects & buffs  ("+(inventory.Count+buffs.Count)+")  "+(effectsExpanded?"v":">");
            if(effectsExpanded)RefreshEffects(inventory,buffs);else ClearEffects();
            Dock(strips);
            if(hovered is NativeItemHover item&&item&&IsOwnIcon(item.Icon))items.TryGetValue("item:"+(int)item.Icon.itemIndex,out hoveredRow);
            else if(hovered is AddedRowHover added&&added)hoveredRow=added.Row;
            else if(hovered!=null){hovered=null;hoveredRow=null;popup.Hide();}
        }
        CompactSheetMetrics Metrics=>new CompactSheetMetrics(style.BodyFontSize,scale);
        void CreatePanel()
        {
            var metrics=Metrics;
            root=NativeUiStyle.Rect(layer.Root,"AlexanderTabAdditions",Vector2.zero,Vector2.zero);root.pivot=Vector2.zero;
            root.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;style.Background(root,true);style.Border(root,scale);
            var viewport=NativeUiStyle.Rect(root,"Viewport",Vector2.zero,Vector2.one);
            viewport.offsetMin=Vector2.one*(float)metrics.Padding;viewport.offsetMax=-viewport.offsetMin;
            viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            content=NativeUiStyle.Rect(viewport,"Content",new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);
            var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=(float)metrics.Spacing;
            layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;
            scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=24;
            var heading=NativeUiStyle.Rect(content,"CharacterHeader",Vector2.zero,Vector2.one);
            heading.gameObject.AddComponent<LayoutElement>().preferredHeight=(float)metrics.HeaderHeight;
            var title=style.Text(heading,"Character stats",(style.BodyFontSize+2)*scale);title.fontStyle=FontStyles.Bold;
            title.rectTransform.anchorMax=new Vector2(.75f,1);title.enableWordWrapping=false;
            var live=style.Text(heading,"Live",style.BodyFontSize*scale);live.alignment=TextAlignmentOptions.MidlineRight;
            live.rectTransform.anchorMin=new Vector2(.75f,0);live.alpha=.7f;live.enableWordWrapping=false;
            var buttonRoot=NativeUiStyle.Rect(content,"EffectsDisclosure",Vector2.zero,Vector2.one);
            buttonRoot.gameObject.AddComponent<LayoutElement>().preferredHeight=(float)metrics.ControlHeight;
            var buttonImage=buttonRoot.gameObject.AddComponent<Image>();buttonImage.color=new Color(1,1,1,.04f);
            effectsHeading=style.Text(buttonRoot,"Effects  >",style.BodyFontSize*scale);
            effectsHeading.rectTransform.offsetMin=new Vector2(6*scale,0);
            buttonRoot.gameObject.AddComponent<Button>().onClick.AddListener(()=>{effectsExpanded=!effectsExpanded;Refresh(body);scroll.verticalNormalizedPosition=1;});
        }
        AddedRowHover CreateRow(SheetRow row,float height)
        {
            var rect=NativeUiStyle.Rect(content,row.Id,Vector2.zero,Vector2.one);
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight=height;
            rect.gameObject.AddComponent<Image>().color=new Color(1,1,1,.025f);
            var label=style.Text(rect,"",style.BodyFontSize*scale);
            label.rectTransform.anchorMax=new Vector2(.65f,1);label.rectTransform.offsetMin=new Vector2(6*scale,0);
            label.rectTransform.offsetMax=new Vector2(-6*scale,0);label.enableWordWrapping=false;label.overflowMode=TextOverflowModes.Ellipsis;
            var value=style.Text(rect,"",style.BodyFontSize*scale);value.alignment=TextAlignmentOptions.MidlineRight;
            value.rectTransform.anchorMin=new Vector2(.65f,0);value.rectTransform.offsetMax=new Vector2(-6*scale,0);
            value.enableWordWrapping=false;value.overflowMode=TextOverflowModes.Ellipsis;
            var hover=rect.gameObject.AddComponent<AddedRowHover>();hover.Owner=this;hover.Label=label;hover.Value=value;SetRow(hover,row);return hover;
        }
        void SetRow(AddedRowHover target,SheetRow row)
        {
            target.Row=row;target.Label.text=TooltipText.SingleLine(row.Title);target.Value.text=TooltipText.SingleLine(row.Detail??"");
            target.Label.rectTransform.anchorMax=new Vector2(string.IsNullOrEmpty(row.Detail)?1:.65f,1);
        }
        void RefreshEffects(List<SheetRow> inventory,List<SheetRow> buffs)
        {
            var data=new List<SheetRow>();
            foreach(var group in inventory.GroupBy(r=>r.Group).OrderBy(g=>g.Key,StringComparer.Ordinal))
            {
                data.Add(new SheetRow{Id="group:"+group.Key,Title=group.Key,Detail="",ShortTooltip="Presentation group; trigger coverage can be incomplete.",
                    FullTooltip="Classification uses own visible item tags and explicitly known terms. A group does not certify a formula or luck applicability."});
                data.AddRange(group);
            }
            if(buffs.Count>0)
            {
                data.Add(new SheetRow{Id="group:buffs",Title="Active buff glossary",ShortTooltip="Own active visible buffs only.",
                    FullTooltip="Stacks, effect coverage, counter distinction, Nectar eligibility and timer availability are shown per entry."});
                data.AddRange(buffs);
            }
            bool rebuild=effectRows.Count!=data.Count;
            if(!rebuild)for(int i=0;i<data.Count;i++)if(effectRows[i].GetComponent<AddedRowHover>().Row.Id!=data[i].Id){rebuild=true;break;}
            if(rebuild){ClearEffects();foreach(var row in data)effectRows.Add(CreateRow(row,(float)Metrics.RowHeight).gameObject);}
            else for(int i=0;i<data.Count;i++)SetRow(effectRows[i].GetComponent<AddedRowHover>(),data[i]);
        }
        void ClearEffects(){foreach(var row in effectRows)if(row){row.SetActive(false);UnityEngine.Object.Destroy(row);}effectRows.Clear();}
        void RefreshBindings()
        {
            var native=ownStrip.itemInventoryDisplay;var visible=new HashSet<ItemIcon>();
            if(native)foreach(var icon in native.GetComponentsInChildren<ItemIcon>(false))if(IsOwnIcon(icon))
            {
                visible.Add(icon);
                NativeItemHover existing;
                if(bindings.TryGetValue(icon,out existing)&&!existing)bindings.Remove(icon);
                if(!bindings.ContainsKey(icon))
                {
                    var hover=icon.GetComponents<NativeItemHover>().FirstOrDefault(h=>h.isActiveAndEnabled&&h.Icon==icon);
                    if(hover)continue;
                    hover=icon.gameObject.AddComponent<NativeItemHover>();
                    if(hover.Bind(this,icon))bindings.Add(icon,hover);else{hover.enabled=false;UnityEngine.Object.Destroy(hover);}
                }
                else bindings[icon].RefreshProvider();
            }
            foreach(var pair in bindings.ToArray())if(!pair.Key||!visible.Contains(pair.Key))
            {if(pair.Value){pair.Value.Detach();UnityEngine.Object.Destroy(pair.Value);}bindings.Remove(pair.Key);}
            int routed=bindings.Values.Count(h=>h&&h.CanShow);
            hoverCoverage="Own item hover targets: "+routed+"/"+visible.Count+" ready; "+
                items.Values.Count(r=>r.FormulaRegistered)+"/"+items.Count+" registered formula definitions. "+
                "Native buff/equipment/skill targets are outside this candidate.";
        }
        void DetachBindings()
        {
            foreach(var pair in bindings)if(pair.Value){pair.Value.Detach();UnityEngine.Object.Destroy(pair.Value);}
            bindings.Clear();hovered=null;hoveredRow=null;popup?.Hide();
        }
        bool BoundsOf(Transform source,out UiRect area)
        {
            area=default(UiRect);if(!source||!source.gameObject.activeInHierarchy)return false;
            var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(layer.Root,source);
            area=new UiRect(bounds.min.x-layer.Root.rect.xMin,bounds.min.y-layer.Root.rect.yMin,bounds.size.x,bounds.size.y);
            return area.Valid;
        }
        void Dock(ScoreboardStrip[] strips)
        {
            var canvasRect=layer.Root;var occupied=new List<UiRect>();UiRect own=default(UiRect),area;
            foreach(var strip in strips)if(BoundsOf(strip.transform,out area)){occupied.Add(area);if(strip==ownStrip)own=area;}
            foreach(var icon in hud.GetComponentsInChildren<SkillIcon>(false))if(BoundsOf(icon.transform,out area))occupied.Add(area);
            foreach(var icon in hud.GetComponentsInChildren<EquipmentIcon>(false))if(BoundsOf(icon.transform,out area))occupied.Add(area);
            foreach(var bar in hud.GetComponentsInChildren<HealthBar>(false))if(BoundsOf(bar.transform,out area))occupied.Add(area);
            foreach(var bar in hud.GetComponentsInChildren<ExpBar>(false))if(BoundsOf(bar.transform,out area))occupied.Add(area);
            foreach(var buffs in hud.GetComponentsInChildren<BuffDisplay>(false))if(BoundsOf(buffs.transform,out area))occupied.Add(area);
            var metrics=Metrics;UiRect dock;
            double preferredHeight=metrics.PanelHeight(canvasRect.rect.height,statRows.Count,effectRows.Count,effectsExpanded);
            bool fits=CompactSheetPlacement.TryBelowOwn(canvasRect.rect.width,canvasRect.rect.height,own,occupied,
                metrics.Width,preferredHeight,metrics.MinWidth,metrics.MinHeight,12*scale,out dock);
            root.gameObject.SetActive(fits);
            if(fits)
            {
                root.anchoredPosition=new Vector2((float)dock.X,(float)dock.Y);root.sizeDelta=new Vector2((float)dock.Width,(float)dock.Height);
                Status="Compact native Tab stats attached below own strip; runtime layout verification pending. "+hoverCoverage;
            }
            else Status="No safe region below own strip; native panels retained and item hovers remain attached. "+hoverCoverage;
        }
        internal bool EnterItem(NativeItemHover source)
        {
            if(!source.CanShow||!IsOwnIcon(source.Icon))return false;SheetRow row;
            if(!items.TryGetValue("item:"+(int)source.Icon.itemIndex,out row))return false;
            hovered=source;hoveredRow=row;leaveAt=float.PositiveInfinity;return true;
        }
        internal void EnterRow(AddedRowHover source){hovered=source;hoveredRow=source.Row;leaveAt=float.PositiveInfinity;}
        internal void Leave(object source){if(ReferenceEquals(hovered,source))leaveAt=Time.unscaledTime+.16f;}
        internal void SetNativeVisibility(bool visible){layer?.SetVisible(visible);}
        internal void UpdateTooltip()
        {
            if(popup==null)return;
            if(hovered is NativeItemHover item&&(!item||!item.CanShow||!IsOwnIcon(item.Icon)) ||
               hovered is AddedRowHover added&&(!added||!added.gameObject.activeInHierarchy))
            {hovered=null;hoveredRow=null;popup.Hide();return;}
            if(hovered is NativeItemHover current)
                items.TryGetValue("item:"+(int)current.Icon.itemIndex,out hoveredRow);
            if(hoveredRow==null||Time.unscaledTime>leaveAt&&!popup.PointerInside){hovered=null;hoveredRow=null;popup.Hide();return;}
            popup.Set(hoveredRow,Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift),float.IsPositiveInfinity(leaveAt));
        }
        public void Dispose()
        {
            try{DetachBindings();}
            finally
            {
                try{ClearEffects();}
                finally
                {
                    try{popup?.Dispose();}
                    finally{popup=null;layer?.Dispose();layer=null;}
                }
            }
        }
    }
}
