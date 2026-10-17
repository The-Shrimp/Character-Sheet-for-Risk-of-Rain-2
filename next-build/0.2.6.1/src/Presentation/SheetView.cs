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
    // Local HUD buff hovers and native item hovers; native data remains owned by the game.
    internal sealed class SheetView:IDisposable
    {
        readonly HUD hud;
        readonly float scale;
        readonly Dictionary<ItemIcon,NativeItemHover> bindings=new Dictionary<ItemIcon,NativeItemHover>();
        readonly Dictionary<BuffIcon,NativeBuffHover> buffBindings=new Dictionary<BuffIcon,NativeBuffHover>();
        readonly Dictionary<string,SheetRow> buffRows=new Dictionary<string,SheetRow>();
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
        NativeInventoryViewport inventoryViewport;
        ItemInventoryDisplay declinedDisplay;
        object hovered;
        SheetRow hoveredRow;
        float leaveAt;
        bool effectsExpanded;
        TextMeshProUGUI effectsHeading;
        internal string Status{get;private set;}="Waiting for the native own-player strip.";
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
            RefreshInventoryViewport();RefreshBindings();var character=SheetData.Character(body);
            if(statRows.Count!=character.Count)
            {
                foreach(var row in statRows)if(row){row.gameObject.SetActive(false);UnityEngine.Object.Destroy(row.gameObject);}
                statRows.Clear();for(int i=0;i<character.Count;i++)statRows.Add(CreateRow(character[i],31));
                for(int i=0;i<statRows.Count;i++)statRows[i].transform.SetSiblingIndex(1+i);
            }
            for(int i=0;i<character.Count;i++)SetRow(statRows[i],character[i]);
            var buffs=SheetData.Buffs(body);buffRows.Clear();foreach(var row in buffs)buffRows[row.Id]=row;
            RefreshBuffBindings();
            effectsHeading.text=(effectsExpanded?"v ":" > ")+"Effects & buff glossary ("+(inventory.Count+buffs.Count)+")";
            if(effectsExpanded)RefreshEffects(inventory,buffs);else ClearEffects();
            Dock(strips);
            if(hovered is NativeItemHover item&&item&&IsOwnIcon(item.Icon))items.TryGetValue("item:"+(int)item.Icon.itemIndex,out hoveredRow);
            else if(hovered is NativeBuffHover buff&&buff&&buff.CanShow)buffRows.TryGetValue("buff:"+(int)buff.Icon.buffDef.buffIndex,out hoveredRow);
            else if(hovered is AddedRowHover added&&added)hoveredRow=added.Row;
            else if(hovered!=null){hovered=null;hoveredRow=null;popup.Hide();}
        }
        void CreatePanel()
        {
            root=NativeUiStyle.Rect(layer.Root,"AlexanderTabAdditions",Vector2.zero,Vector2.zero);root.pivot=Vector2.zero;
            root.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;style.Background(root,true);
            var viewport=NativeUiStyle.Rect(root,"Viewport",Vector2.zero,Vector2.one);
            viewport.offsetMin=new Vector2(10,10);viewport.offsetMax=new Vector2(-10,-10);
            viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            content=NativeUiStyle.Rect(viewport,"Content",new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);
            var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=4;
            layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;
            scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=24;
            Label("Your character - live totals",28);
            var buttonRoot=NativeUiStyle.Rect(content,"EffectsDisclosure",Vector2.zero,Vector2.one);
            buttonRoot.gameObject.AddComponent<LayoutElement>().preferredHeight=30;
            style.Background(buttonRoot,true);effectsHeading=style.Text(buttonRoot," > Effects & buff glossary",14*scale);
            buttonRoot.gameObject.AddComponent<Button>().onClick.AddListener(()=>{effectsExpanded=!effectsExpanded;Refresh(body);});
            Label("Hover for sources; Shift for full details.",28);
        }
        void Label(string text,float height)
        {
            var rect=NativeUiStyle.Rect(content,"Label",Vector2.zero,Vector2.one);
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight=height;style.Text(rect,text,13*scale);
        }
        AddedRowHover CreateRow(SheetRow row,float height)
        {
            var rect=NativeUiStyle.Rect(content,row.Id,Vector2.zero,Vector2.one);
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight=height*scale;
            rect.gameObject.AddComponent<Image>().color=new Color(1,1,1,.025f);
            var text=style.Text(rect,"",14*scale);text.rectTransform.offsetMin=new Vector2(4,0);text.rectTransform.offsetMax=new Vector2(-4,0);
            var hover=rect.gameObject.AddComponent<AddedRowHover>();hover.Owner=this;SetRow(hover,row);return hover;
        }
        void SetRow(AddedRowHover target,SheetRow row)
        {target.Row=row;target.GetComponentInChildren<TextMeshProUGUI>().text=row.Title+"  "+row.Detail;}
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
            if(rebuild){ClearEffects();foreach(var row in data)effectRows.Add(CreateRow(row,42).gameObject);}
            else for(int i=0;i<data.Count;i++)SetRow(effectRows[i].GetComponent<AddedRowHover>(),data[i]);
        }
        void ClearEffects(){foreach(var row in effectRows)if(row){row.SetActive(false);UnityEngine.Object.Destroy(row);}effectRows.Clear();}
        void RefreshInventoryViewport()
        {
            var display=ownStrip?ownStrip.itemInventoryDisplay:null;
            if(inventoryViewport!=null&&inventoryViewport.Display!=display){inventoryViewport.Dispose();inventoryViewport=null;declinedDisplay=null;}
            if(inventoryViewport!=null&&!inventoryViewport.Refresh(scale)){inventoryViewport.Dispose();inventoryViewport=null;declinedDisplay=display;}
            if(display&&inventoryViewport==null&&display!=declinedDisplay)
            {
                try{inventoryViewport=new NativeInventoryViewport(display,scale,style);}
                catch(InvalidOperationException){declinedDisplay=display;}
            }
        }
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
        }
        bool CursorContextOpen()
        {
            var user=LocalUserManager.GetFirstLocalUser();
            if(user==null||!hud||hud.localUserViewer!=user||!hud.scoreboardPanel||!hud.scoreboardPanel.activeInHierarchy)return false;
            var input=MPEventSystem.readOnlyInstancesList.FirstOrDefault(e=>e&&e.localUser==user);
            return input&&input.isCursorVisible&&input.cursorOpenerCount>0;
        }
        internal bool CanHandleBuff(BuffIcon icon)
        {
            if(!hud||!body||!icon||!icon.buffDef||icon.buffCount<=0||!icon.gameObject.activeInHierarchy)return false;
            var display=icon.GetComponentInParent<BuffDisplay>();
            if(!display||display!=hud.buffDisplay||hud.targetBodyObject!=body.gameObject)return false;
            return NativeBuffHoverPolicy.CanHover(hud,icon.GetComponentInParent<HUD>(),body.gameObject,hud.targetBodyObject,
                hud.scoreboardPanel&&hud.scoreboardPanel.activeInHierarchy,CursorContextOpen(),false,
                icon.buffDef.isHidden,body.GetBuffCount(icon.buffDef.buffIndex))&&buffRows.ContainsKey("buff:"+(int)icon.buffDef.buffIndex);
        }
        void RefreshBuffBindings()
        {
            var visible=new HashSet<BuffIcon>();
            foreach(var display in new[]{hud.buffDisplay})
            {
                if(!display||hud.targetBodyObject!=body.gameObject)continue;
                foreach(var icon in display.GetComponentsInChildren<BuffIcon>(false))
                {
                    if(!icon.buffDef||icon.buffCount<=0||icon.buffDef.isHidden||body.GetBuffCount(icon.buffDef.buffIndex)<=0)continue;
                    visible.Add(icon);NativeBuffHover binding;
                    if(buffBindings.TryGetValue(icon,out binding)&&!binding)buffBindings.Remove(icon);
                    if(!buffBindings.ContainsKey(icon))
                    {
                        binding=icon.gameObject.AddComponent<NativeBuffHover>();
                        if(binding.Bind(this,icon))buffBindings.Add(icon,binding);
                        else{binding.enabled=false;UnityEngine.Object.Destroy(binding);}
                    }
                    else buffBindings[icon].RefreshContext();
                }
            }
            foreach(var pair in buffBindings.ToArray())if(!pair.Key||!visible.Contains(pair.Key))
            {if(pair.Value){pair.Value.Detach();UnityEngine.Object.Destroy(pair.Value);}buffBindings.Remove(pair.Key);}
        }
        internal bool EnterBuff(NativeBuffHover source)
        {
            if(!source.CanShow)return false;SheetRow row;
            if(!buffRows.TryGetValue("buff:"+(int)source.Icon.buffDef.buffIndex,out row))return false;
            hovered=source;hoveredRow=row;leaveAt=float.PositiveInfinity;return true;
        }
        void DetachBindings()
        {
            foreach(var pair in bindings)if(pair.Value){pair.Value.Detach();UnityEngine.Object.Destroy(pair.Value);}
            bindings.Clear();
            foreach(var pair in buffBindings)if(pair.Value){pair.Value.Detach();UnityEngine.Object.Destroy(pair.Value);}
            buffBindings.Clear();buffRows.Clear();
            inventoryViewport?.Dispose();inventoryViewport=null;declinedDisplay=null;hovered=null;hoveredRow=null;popup?.Hide();
        }
        void Dock(ScoreboardStrip[] strips)
        {
            var canvasRect=layer.Root;var occupied=new List<UiRect>();
            foreach(var strip in strips)
            {
                var rect=strip.transform as RectTransform;if(!rect)continue;
                var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect,strip.transform);
                occupied.Add(new UiRect(bounds.min.x-canvasRect.rect.xMin,bounds.min.y-canvasRect.rect.yMin,bounds.size.x,bounds.size.y));
            }
            UiRect dock;
            bool fits=NativeTabPolicy.TryDock(canvasRect.rect.width,canvasRect.rect.height,occupied,310*scale,510*scale,out dock,240*scale,180*scale);
            root.gameObject.SetActive(fits);
            if(fits)
            {
                root.anchoredPosition=new Vector2((float)dock.X,(float)dock.Y);root.sizeDelta=new Vector2((float)dock.Width,(float)dock.Height);
                Status="Native Tab additions attached; HUD buff bindings: "+buffBindings.Count+". "+(inventoryViewport?.Status??"Native inventory viewport unavailable; original layout retained.");
            }
            else Status="No free canvas region for stats. HUD buff/item hovers attached; "+(inventoryViewport?.Status??"native inventory viewport unavailable.");
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
            inventoryViewport?.SetCursorContext(CursorContextOpen());
            foreach(var pair in buffBindings)if(pair.Value)pair.Value.RefreshContext();
            if(hovered is NativeItemHover item&&(!item||!item.CanShow||!IsOwnIcon(item.Icon)) ||
               hovered is NativeBuffHover buff&&(!buff||!buff.CanShow) ||
               hovered is AddedRowHover added&&(!added||!added.gameObject.activeInHierarchy))
            {hovered=null;hoveredRow=null;popup.Hide();return;}
            if(hovered is NativeItemHover current)
                items.TryGetValue("item:"+(int)current.Icon.itemIndex,out hoveredRow);
            if(hovered is NativeBuffHover currentBuff)
                buffRows.TryGetValue("buff:"+(int)currentBuff.Icon.buffDef.buffIndex,out hoveredRow);
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
