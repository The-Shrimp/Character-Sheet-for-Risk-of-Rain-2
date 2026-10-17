using System;
using System.Collections.Generic;
using System.Linq;
using Alexander.CharacterSheet.Core;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Alexander.CharacterSheet
{
    internal sealed class SheetView:IDisposable
    {
        sealed class Control
        {
            internal SheetNode Node;
            internal SheetFocusButton Button;
            internal AddedRowHover Hover;
            internal TextMeshProUGUI Label,Value;
            internal Image Icon,FocusLine,Rule;
            internal SheetGlyphGraphic Glyph;
            internal RectTransform Rect;
        }
        readonly HUD hud;
        readonly float scale;
        readonly SheetExpansionState expansion;
        readonly Dictionary<ItemIcon,NativeItemHover> bindings=new Dictionary<ItemIcon,NativeItemHover>();
        readonly Dictionary<string,SheetRow> items=new Dictionary<string,SheetRow>();
        readonly Dictionary<string,Control> controls=new Dictionary<string,Control>(StringComparer.Ordinal);
        readonly Dictionary<string,SheetStructureEntry> entries=new Dictionary<string,SheetStructureEntry>(StringComparer.Ordinal);
        List<SheetNode> allNodes=new List<SheetNode>(),visibleNodes=new List<SheetNode>();
        CharacterBody body;
        ScoreboardStrip ownStrip;
        Canvas canvas;
        NativeUiStyle style;
        RectTransform root,content,viewport,luckFooter;
        NativeSheetLayer layer;
        NativeHoverPopup popup;
        ScrollRect scroll;
        TextMeshProUGUI scrollHint;
        SheetFocusButton focused;
        MPEventSystem focusedContext;
        TooltipProvider focusedProvider;
        object hovered;
        SheetRow hoveredRow;
        internal string Status{get;private set;}="Waiting for the native own-player strip.";
        string hoverCoverage="Own item hover coverage pending.";
        internal SheetView(HUD source,float presentationScale,SheetExpansionState state)
        {hud=source;scale=presentationScale;expansion=state??new SheetExpansionState();}
        CompactSheetMetrics Metrics=>new CompactSheetMetrics(style.BodyFontSize,scale);
        internal bool IsOwnIcon(ItemIcon icon)
        {
            if(!body||!body.master||!body.inventory||!hud||!icon)return false;
            var strip=icon.GetComponentInParent<ScoreboardStrip>();
            var identity=strip?strip.GetComponent<NativeStripIdentity>():null;
            var top=hud.itemInventoryDisplay;
            bool ownScoreboard=strip&&strip==ownStrip&&strip.itemInventoryDisplay&&icon.transform.IsChildOf(strip.itemInventoryDisplay.transform);
            bool topHud=!strip&&top&&icon.transform.IsChildOf(top.transform)&&icon.GetComponentInParent<HUD>()==hud;
            if(strip&&!ownScoreboard)return false;
            if(!NativeItemSurfacePolicy.OwnSurface(body.master,identity?identity.OwnMaster:null,hud.targetMaster,ownScoreboard,topHud))return false;
            var def=ItemCatalog.GetItemDef(icon.itemIndex);
            return def&&NativeTabPolicy.OwnVisibleItem(body.master,body.master,def.hidden,
                body.inventory.GetItemCountPermanent(def),body.inventory.GetItemCountEffective(def));
        }
        internal bool CanHandleIcon(ItemIcon icon)=>IsOwnIcon(icon)&&items.ContainsKey("item:"+(int)icon.itemIndex);
        internal bool CanHandleAddedItem(AddedRowHover added)=>added&&ReferenceEquals(added.Owner,this)&&added.Row!=null&&
            added.Row.IsItem&&root&&added.transform.IsChildOf(root)&&items.ContainsKey(added.Row.Id);
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
            RefreshBindings();
            var previousNodes=allNodes;var previousFocused=focused;
            var previousContext=focusedContext;bool ownedSelection=previousFocused&&previousContext&&ReferenceEquals(previousContext.currentSelectedGameObject,previousFocused.gameObject);
            var structure=SheetStructureData.VisualBuild(body,inventory);
            entries.Clear();foreach(var entry in structure)entries.Add(entry.Node.Id,entry);
            allNodes=structure.Select(e=>e.Node).ToList();visibleNodes=SheetHierarchy.Visible(allNodes,expansion).ToList();
            var visibleIds=new HashSet<string>(visibleNodes.Select(n=>n.Id));
            foreach(var pair in controls)if(!visibleIds.Contains(pair.Key))pair.Value.Rect.gameObject.SetActive(false);
            foreach(var node in visibleNodes)
            {
                Control control;if(!controls.TryGetValue(node.Id,out control)){control=CreateControl(node);controls.Add(node.Id,control);}
                UpdateControl(control,entries[node.Id]);control.Rect.gameObject.SetActive(true);control.Rect.SetAsLastSibling();
            }
            ConnectNavigation();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            Dock(strips);
            if(root.gameObject.activeInHierarchy)LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            content.anchoredPosition=new Vector2(content.anchoredPosition.x,Mathf.Clamp(content.anchoredPosition.y,0,Mathf.Max(0,content.rect.height-viewport.rect.height)));
            scrollHint.text=content.rect.height>viewport.rect.height+1?"Scroll":"";
            if(previousFocused&&(!visibleIds.Contains(previousFocused.NodeId)||!previousFocused.gameObject.activeInHierarchy))
            {
                string next=SheetHierarchy.RestoreFocus(previousFocused.NodeId,previousNodes.Concat(allNodes.Where(n=>!previousNodes.Any(p=>p.Id==n.Id))),visibleNodes);
                if(ownedSelection&&previousContext&&(previousContext.currentSelectedGameObject==null||ReferenceEquals(previousContext.currentSelectedGameObject,previousFocused.gameObject))&&next!=null&&controls.ContainsKey(next))
                    previousContext.SetSelectedGameObject(controls[next].Button.gameObject);
                else if(ReferenceEquals(focused,previousFocused))Unfocus(previousFocused);
            }
            if(hovered is NativeItemHover item&&item&&item.CanShow)items.TryGetValue(item.RowId,out hoveredRow);
            else if(hovered is AddedRowHover added&&added&&added.gameObject.activeInHierarchy)hoveredRow=added.Row;
            else if(hovered!=null)EndHover();
        }
        void CreatePanel()
        {
            var metrics=Metrics;
            root=NativeUiStyle.Rect(layer.Root,"AlexanderCharacterSheet",Vector2.zero,Vector2.zero);root.pivot=Vector2.zero;root.gameObject.SetActive(false);
            root.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;
            float footerHeight=(float)metrics.RowHeight+10*scale;
            var bodyBackground=NativeUiStyle.Rect(root,"SheetBodyBackground",Vector2.zero,Vector2.one);
            bodyBackground.offsetMin=new Vector2(0,footerHeight);
            var background=style.Background(bodyBackground,true);var color=background.color;
            background.color=new Color(color.r,color.g,color.b,(float)VisualSheetSizing.BackgroundOpacity);
            style.Border(root,scale);
            var header=NativeUiStyle.Rect(root,"StatsHeader",new Vector2(0,1),Vector2.one);header.pivot=new Vector2(.5f,1);header.sizeDelta=new Vector2(0,(float)metrics.HeaderHeight);
            var title=style.Text(header,"Stats",(style.BodyFontSize+4)*scale);title.fontStyle=FontStyles.Bold|FontStyles.Underline;
            title.alignment=TextAlignmentOptions.Center;title.rectTransform.offsetMin=new Vector2((float)metrics.Padding,0);
            title.rectTransform.offsetMax=new Vector2(-(float)metrics.Padding,0);title.enableWordWrapping=false;Opaque(title);
            scrollHint=style.Text(header,"",Mathf.Max(10,style.BodyFontSize-7)*scale);scrollHint.rectTransform.anchorMin=new Vector2(.82f,0);
            scrollHint.rectTransform.offsetMax=new Vector2(-(float)metrics.Padding,0);scrollHint.alignment=TextAlignmentOptions.BottomRight;Opaque(scrollHint);
            viewport=NativeUiStyle.Rect(root,"SheetViewport",Vector2.zero,Vector2.one);
            viewport.offsetMin=new Vector2(0,footerHeight+(float)metrics.Padding);
            viewport.offsetMax=new Vector2(0,-(float)(metrics.HeaderHeight+metrics.Padding));
            var surface=viewport.gameObject.AddComponent<Image>();surface.color=Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            content=NativeUiStyle.Rect(viewport,"SheetContent",new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);
            var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=0;
            layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;
            scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=24;
            luckFooter=NativeUiStyle.Rect(root,"FixedLuckFooter",Vector2.zero,new Vector2(1,0));luckFooter.pivot=Vector2.zero;
            luckFooter.sizeDelta=new Vector2(0,footerHeight);
            var green=luckFooter.gameObject.AddComponent<Image>();green.color=new Color(.16f,.72f,.08f,(float)VisualSheetSizing.BackgroundOpacity);
            green.raycastTarget=true;
            var rule=NativeUiStyle.Rect(luckFooter,"LuckTopRule",new Vector2(0,1),Vector2.one);rule.pivot=new Vector2(.5f,1);rule.sizeDelta=new Vector2(0,scale);
            var line=rule.gameObject.AddComponent<Image>();line.color=Color.white;line.raycastTarget=false;
        }
        static void Opaque(TMP_Text text){var c=text.color;text.color=new Color(c.r,c.g,c.b,1);}
        Control CreateControl(SheetNode node)
        {
            var c=new Control();bool fixedFooter=VisualSheetStructure.IsFooter(node);
            c.Rect=NativeUiStyle.Rect(fixedFooter?luckFooter:content,node.Id,Vector2.zero,Vector2.one);c.Rect.gameObject.SetActive(false);
            var element=c.Rect.gameObject.AddComponent<LayoutElement>();element.ignoreLayout=fixedFooter;
            element.preferredHeight=node.Header?(float)Metrics.RowHeight+12*scale:(float)Metrics.RowHeight;
            var image=c.Rect.gameObject.AddComponent<Image>();image.color=Color.clear;
            c.Button=c.Rect.gameObject.AddComponent<SheetFocusButton>();c.Button.Owner=this;c.Button.NodeId=node.Id;c.Button.targetGraphic=image;
            foreach(var auto in c.Rect.GetComponents<TooltipProvider>())auto.enabled=false;
            c.Button.onClick.AddListener(()=>{if(c.Node.Header)Toggle(c.Node.Id);});
            c.Label=style.Text(c.Rect,"",style.BodyFontSize*scale);c.Label.color=Color.white;
            c.Value=style.Text(c.Rect,"",style.BodyFontSize*scale);c.Value.color=Color.white;c.Value.alignment=TextAlignmentOptions.MidlineRight;
            c.Label.enableWordWrapping=c.Value.enableWordWrapping=false;c.Label.overflowMode=c.Value.overflowMode=TextOverflowModes.Ellipsis;
            var iconRect=NativeUiStyle.Rect(c.Rect,"MarginIcon",new Vector2(0,.5f),new Vector2(0,.5f));iconRect.pivot=new Vector2(0,.5f);
            c.Glyph=iconRect.gameObject.AddComponent<SheetGlyphGraphic>();c.Glyph.raycastTarget=false;c.Glyph.color=Color.white;
            var itemRect=NativeUiStyle.Rect(c.Rect,"ItemIcon",new Vector2(0,.5f),new Vector2(0,.5f));itemRect.pivot=new Vector2(0,.5f);itemRect.sizeDelta=new Vector2(20*scale,20*scale);
            c.Icon=itemRect.gameObject.AddComponent<Image>();c.Icon.raycastTarget=false;c.Icon.preserveAspect=true;c.Icon.color=Color.white;
            var focusRect=NativeUiStyle.Rect(c.Rect,"Focus",Vector2.zero,new Vector2(0,1));focusRect.pivot=new Vector2(0,.5f);focusRect.sizeDelta=new Vector2(2*scale,0);
            c.FocusLine=focusRect.gameObject.AddComponent<Image>();c.FocusLine.raycastTarget=false;c.FocusLine.color=Color.white;c.FocusLine.gameObject.SetActive(false);
            var rule=NativeUiStyle.Rect(c.Rect,"CategoryBottomRule",Vector2.zero,new Vector2(1,0));rule.pivot=Vector2.zero;rule.sizeDelta=new Vector2(0,scale);
            c.Rule=rule.gameObject.AddComponent<Image>();c.Rule.color=Color.white;c.Rule.raycastTarget=false;c.Rule.gameObject.SetActive(node.Header);
            if(!node.Header)
            {
                c.Hover=c.Rect.gameObject.AddComponent<AddedRowHover>();c.Hover.Owner=this;c.Hover.Label=c.Label;c.Hover.Value=c.Value;c.Button.Hover=c.Hover;
            }
            return c;
        }
        void UpdateControl(Control c,SheetStructureEntry entry)
        {
            var node=entry.Node;c.Node=node;bool fixedFooter=VisualSheetStructure.IsFooter(node);
            float indent=(node.Header||fixedFooter?10:36)*scale;
            float iconWidth=node.Header&&node.Glyph!=SheetGlyph.None||fixedFooter?40*scale:entry.Row!=null&&entry.Row.Icon?24*scale:0;
            c.Label.fontStyle=node.Header||fixedFooter?FontStyles.Bold:FontStyles.Normal;
            c.Label.fontSize=(node.Header||fixedFooter?style.BodyFontSize+3:Mathf.Max(18,style.BodyFontSize-2))*scale;
            c.Value.fontSize=(node.Header?Mathf.Max(13,style.BodyFontSize-5):style.BodyFontSize)*scale;
            c.Label.text=node.Header?node.Title:TooltipText.SingleLine(entry.Row.Title);
            c.Value.text=node.Header?VisualSheetStructure.ToggleLabel(expansion.IsOpen(node.Id)):TooltipText.SingleLine(entry.Row.Detail??"");
            c.Value.gameObject.SetActive(true);
            c.Label.rectTransform.anchorMin=Vector2.zero;c.Label.rectTransform.anchorMax=new Vector2(node.Header?1:.66f,1);
            c.Label.rectTransform.offsetMin=new Vector2(indent+iconWidth,0);
            c.Label.rectTransform.offsetMax=new Vector2(node.Header?-98*scale:-2*scale,0);
            c.Value.rectTransform.anchorMin=new Vector2(node.Header?1:.66f,0);c.Value.rectTransform.anchorMax=Vector2.one;
            c.Value.rectTransform.offsetMin=new Vector2(node.Header?-92*scale:0,0);c.Value.rectTransform.offsetMax=new Vector2(-12*scale,0);
            c.Glyph.Kind=node.Glyph;c.Glyph.gameObject.SetActive(node.Header&&node.Glyph!=SheetGlyph.None||fixedFooter);
            c.Glyph.rectTransform.sizeDelta=new Vector2(32*scale,32*scale);c.Glyph.rectTransform.anchoredPosition=new Vector2(indent,0);c.Glyph.SetVerticesDirty();
            c.Icon.sprite=entry.Row?.Icon;c.Icon.gameObject.SetActive(!node.Header&&!fixedFooter&&c.Icon.sprite);c.Icon.rectTransform.anchoredPosition=new Vector2(indent,0);
            c.Rule.gameObject.SetActive(node.Header);
            if(c.Hover){c.Hover.Row=entry.Row;if(entry.Row.IsItem)RefreshAddedItemProvider(c.Hover);}
        }
        void ConnectNavigation()
        {
            for(int i=0;i<visibleNodes.Count;i++)
            {
                var navigation=new Navigation{mode=Navigation.Mode.Explicit};
                navigation.selectOnUp=i>0?controls[visibleNodes[i-1].Id].Button:null;
                navigation.selectOnDown=i+1<visibleNodes.Count?controls[visibleNodes[i+1].Id].Button:null;
                controls[visibleNodes[i].Id].Button.navigation=navigation;
            }
        }
        internal void Submit(SheetFocusButton source)
        {
            SheetStructureEntry entry;if(!source||!entries.TryGetValue(source.NodeId,out entry))return;
            if(entry.Node.Header)Toggle(entry.Node.Id);else if(ReferenceEquals(focused,source))popup?.FocusToggle();
        }
        internal bool Move(SheetFocusButton source,MoveDirection direction)
        {
            if(direction!=MoveDirection.Left&&direction!=MoveDirection.Right)return false;
            SheetStructureEntry entry;if(!source||!entries.TryGetValue(source.NodeId,out entry))return false;
            if(entry.Node.Header)
            {bool open=direction==MoveDirection.Right;if(expansion.IsOpen(entry.Node.Id)!=open){expansion.Set(entry.Node.Id,open);Refresh(body);}return true;}
            if(ReferenceEquals(focused,source))
            {
                if(direction==MoveDirection.Right){popup?.FocusExpand(true);return true;}
                if(direction==MoveDirection.Left&&focusedContext&&controls.ContainsKey(entry.Node.Parent))
                {focusedContext.SetSelectedGameObject(controls[entry.Node.Parent].Button.gameObject);return true;}
            }
            return false;
        }
        void Toggle(string id){if(expansion.Toggle(id))Refresh(body);}
        MPEventSystem ResolveContext(SheetFocusButton source,BaseEventData data)
        {
            var context=data!=null&&data.currentInputModule?data.currentInputModule.GetComponent<MPEventSystem>():null;
            if(!context&&source){var locator=source.GetComponentInParent<MPEventSystemLocator>();context=locator?locator.eventSystem:null;}
            return context;
        }
        internal void Focus(SheetFocusButton source,BaseEventData data)
        {
            if(!source||!source.gameObject.activeInHierarchy)return;
            focused=source;focusedContext=ResolveContext(source,data);controls[source.NodeId].FocusLine.gameObject.SetActive(true);EnsureVisible(source);
            if(!source.Hover){EndHover();return;}
            if(source.Hover.Row.IsItem)
            {
                var provider=source.Hover.NativeProvider;
                if(provider&&provider.enabled&&focusedContext&&focusedContext.isCursorVisible)
                {
                    focusedProvider=provider;
                    ((IPointerEnterHandler)provider).OnPointerEnter(new PointerEventData(focusedContext));
                    popup?.Focus(true,focusedContext);
                }
            }
            else {EnterRow(source.Hover);popup?.Focus(true,focusedContext);}
        }
        internal void Unfocus(SheetFocusButton source)
        {
            if(source&&controls.ContainsKey(source.NodeId))controls[source.NodeId].FocusLine.gameObject.SetActive(false);
            if(!ReferenceEquals(focused,source))return;
            popup?.Focus(false);
            if(focusedProvider&&focusedContext&&ReferenceEquals(focusedContext.currentTooltipProvider,focusedProvider)&&source&&source.Hover&&!source.Hover.PointerInside)
                ((IPointerExitHandler)focusedProvider).OnPointerExit(new PointerEventData(focusedContext));
            if(source&&source.Hover&&!source.Hover.PointerInside)Leave(source.Hover.Row.IsItem?(object)source.Hover.ItemHover:source.Hover);
            focused=null;focusedProvider=null;focusedContext=null;
        }
        void EnsureVisible(SheetFocusButton source)
        {
            if(!content||!viewport||!source||source.NodeId==VisualSheetStructure.FooterId)return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(viewport,source.transform);
            double top=bounds.max.y-viewport.rect.yMin,bottom=bounds.min.y-viewport.rect.yMin;
            double offset=HierarchySheetSizing.EnsureVisible(top,bottom,viewport.rect.height,content.anchoredPosition.y,content.rect.height);
            content.anchoredPosition=new Vector2(content.anchoredPosition.x,(float)offset);
        }
        void RefreshAddedItemProvider(AddedRowHover target)
        {
            if(!target.NativeProvider)
            {
                target.NativeProvider=target.gameObject.AddComponent<TooltipProvider>();
                target.ItemHover=target.gameObject.AddComponent<NativeItemHover>();
                if(!target.ItemHover.BindAdded(this,target,target.NativeProvider)){target.NativeProvider.enabled=false;return;}
            }
            var reference=bindings.Values.FirstOrDefault(h=>h&&h.CanShow&&h.RowId==target.Row.Id)?.Provider;
            target.NativeProvider.enabled=reference;
            if(!reference)return;
            // This provider belongs to our added row. The actual native icon/provider is read only.
            target.NativeProvider.overrideTitleText=reference.titleText;
            target.NativeProvider.overrideBodyText=reference.bodyText;
            target.NativeProvider.titleColor=reference.titleColor;
            target.NativeProvider.bodyColor=reference.bodyColor;
        }
        void RefreshBindings()
        {
            var visible=new HashSet<ItemIcon>();
            var surfaces=new[]{ownStrip.itemInventoryDisplay,hud.targetMaster==body.master?hud.itemInventoryDisplay:null};
            foreach(var native in surfaces.Where(s=>s).Distinct())
            foreach(var icon in native.GetComponentsInChildren<ItemIcon>(false))if(IsOwnIcon(icon))
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
            int scoreboard=bindings.Keys.Count(i=>i&&i.GetComponentInParent<ScoreboardStrip>());
            int topHud=bindings.Keys.Count(i=>i&&!i.GetComponentInParent<ScoreboardStrip>());
            hoverCoverage="Own scoreboard/top-HUD item targets: "+scoreboard+"/"+topHud+"; "+routed+"/"+visible.Count+" ready; "+
                items.Values.Count(r=>r.FormulaRegistered)+"/"+items.Count+" registered formula definitions. "+
                "Native buff/equipment/skill targets are outside this candidate.";
        }
        void DetachBindings()
        {
            NativeItemHover.ReleaseForView(this);
            foreach(var pair in bindings)if(pair.Value){pair.Value.Detach();UnityEngine.Object.Destroy(pair.Value);}
            bindings.Clear();EndHover();
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
            int headerCount=visibleNodes.Count(n=>n.Header),rowCount=visibleNodes.Count(n=>!n.Header&&!VisualSheetStructure.IsFooter(n));
            double footerHeight=metrics.RowHeight+10*scale;
            double minimumHeight=Math.Max(metrics.MinHeight,metrics.HeaderHeight+footerHeight+2*metrics.Padding+metrics.RowHeight);
            double preferredHeight=VisualSheetSizing.PanelHeight(canvasRect.rect.height,headerCount,rowCount,metrics.RowHeight,metrics.RowHeight+12*scale,metrics.HeaderHeight,footerHeight,metrics.Padding,minimumHeight);
            double width=425*scale,minimum=330*scale;
            bool fits=CompactSheetPlacement.TryBelowOwn(canvasRect.rect.width,canvasRect.rect.height,own,occupied,width,preferredHeight,minimum,minimumHeight,12*scale,out dock);
            root.gameObject.SetActive(fits);
            if(fits){root.anchoredPosition=new Vector2((float)dock.X,(float)dock.Y);root.sizeDelta=new Vector2((float)dock.Width,(float)dock.Height);
                var regions=new VisualSheetRegions(dock.Height,metrics.HeaderHeight,footerHeight,metrics.Padding);
                viewport.offsetMin=new Vector2(0,(float)regions.ViewportBottom);
                viewport.offsetMax=new Vector2(0,(float)(regions.ViewportTop-dock.Height));
                luckFooter.sizeDelta=new Vector2(0,(float)regions.FooterHeight);Status="Figma-structure Stats sheet attached with fixed Luck footer; live visual verification pending. "+hoverCoverage;}
            else Status="No safe region below own strip; native item hovers retained. "+hoverCoverage;
        }
        void EndHover()
        {
            if(hovered is NativeItemHover native&&native)native.ReleaseDeferred();
            hovered=null;hoveredRow=null;popup?.Hide();
        }
        internal bool EnterItem(NativeItemHover source)
        {
            if(popup==null||!source||!source.CanShow)return false;SheetRow row;
            if(!items.TryGetValue(source.RowId,out row))return false;
            if(!row.HasItemAddition){EndHover();return false;}
            if(!ReferenceEquals(hovered,source))EndHover();
            hovered=source;hoveredRow=row;popup.Enter(source,row.Id);return true;
        }
        internal void EnterRow(AddedRowHover source)
        {
            if(popup==null||!source||source.Row==null)return;
            if(!ReferenceEquals(hovered,source))EndHover();
            hovered=source;hoveredRow=source.Row;popup.Enter(source,source.Row.Id);
        }
        internal void Leave(object source){if(ReferenceEquals(hovered,source))popup?.Leave(source);}
        internal bool CanDeferExit(NativeItemHover source,out MPEventSystem context)
        {
            context=null;return ReferenceEquals(hovered,source)&&popup!=null&&popup.CanDefer(source.Provider,out context);
        }
        void CloseOwnedFocusTooltip()
        {
            var provider=focusedProvider;var context=focusedContext;focusedProvider=null;
            try
            {
                if(provider&&context&&ReferenceEquals(context.currentTooltipProvider,provider))
                    ((IPointerExitHandler)provider).OnPointerExit(new PointerEventData(context));
            }
            finally{if(focused&&focused.Hover&&focused.Hover.ItemHover)focused.Hover.ItemHover.ReleaseDeferred();}
        }
        internal void SetNativeVisibility(bool visible)
        {
            if(!visible){CloseOwnedFocusTooltip();EndHover();}
            layer?.SetVisible(visible);
        }
        internal void UpdateTooltip()
        {
            if(popup==null)return;
            if(hovered is NativeItemHover item&&(!item||!item.CanShow) ||
               hovered is AddedRowHover added&&(!added||!added.gameObject.activeInHierarchy))
            {EndHover();return;}
            if(hovered is NativeItemHover current)items.TryGetValue(current.RowId,out hoveredRow);
            if(hoveredRow==null||popup.Expired()){EndHover();return;}
            var provider=hovered is NativeItemHover native?native.Provider:null;
            if(!popup.Set(hoveredRow,provider,true)&&hovered is NativeItemHover failed&&failed)failed.ReleaseDeferred();
        }
        public void Dispose()
        {
            try{CloseOwnedFocusTooltip();}
            finally
            {
                try{DetachBindings();}
                finally
                {
                    try{foreach(var c in controls.Values)if(c.Rect)c.Rect.gameObject.SetActive(false);controls.Clear();entries.Clear();focused=null;focusedProvider=null;focusedContext=null;}
                    finally{try{popup?.Dispose();}finally{popup=null;layer?.Dispose();layer=null;}}
                }
            }
        }
    }
}
