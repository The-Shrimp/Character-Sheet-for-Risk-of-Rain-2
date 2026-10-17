using System;
using System.Linq;
using Alexander.CharacterSheet.Core;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Alexander.CharacterSheet
{
    internal sealed class NativeHoverPopup:IDisposable
    {
        readonly Canvas canvas;
        readonly RectTransform canvasRect,root,content,luckDetail,luckBadge,heading,viewport,footer;
        readonly TextMeshProUGUI title,body,luck,luckText,detailsLabel;
        readonly ScrollRect scroll;
        readonly Image panelImage,headerImage;
        readonly NativeTooltipStyle nativeStyle=new NativeTooltipStyle();
        readonly DetailsInteraction interaction=new DetailsInteraction();
        readonly OwnedDetailsButton detailsButton;
        readonly float scale;
        NativeTooltipAnchor itemAnchor;
        MPEventSystem focusContext;
        SheetRow row;
        bool lastFull;
        internal bool PointerInside{get=>interaction.PointerInside;set{if(value)interaction.PopupEnter();else interaction.PopupExit(Time.unscaledTime);}}
        internal bool Active=>interaction.Active;
        internal NativeHoverPopup(RectTransform ownedHost,Canvas eventCanvas,NativeUiStyle style,float presentationScale)
        {
            canvas=eventCanvas;canvasRect=ownedHost;scale=presentationScale;
            root=NativeUiStyle.Rect(canvasRect,"AlexanderItemDetails",Vector2.zero,Vector2.zero);root.pivot=new Vector2(0,1);
            root.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;panelImage=style.Background(root,true);
            root.gameObject.AddComponent<PopupPresence>().Popup=this;
            heading=NativeUiStyle.Rect(root,"Header",new Vector2(0,1),Vector2.one);heading.pivot=new Vector2(0,1);
            headerImage=heading.gameObject.AddComponent<Image>();headerImage.color=Color.clear;headerImage.raycastTarget=false;
            title=style.Text(heading,"",(style.BodyFontSize+2)*scale);
            title.rectTransform.offsetMin=new Vector2(12*scale,4*scale);title.rectTransform.offsetMax=new Vector2(-12*scale,-4*scale);
            title.enableWordWrapping=false;title.overflowMode=TextOverflowModes.Ellipsis;
            viewport=NativeUiStyle.Rect(root,"BodyViewport",Vector2.zero,Vector2.one);
            viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            content=NativeUiStyle.Rect(viewport,"Body",new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);
            body=style.Text(content,"",style.BodyFontSize*scale);body.alignment=TextAlignmentOptions.TopLeft;
            scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;
            scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=24;
            footer=NativeUiStyle.Rect(root,"Footer",Vector2.zero,new Vector2(1,0));footer.pivot=Vector2.zero;
            var control=NativeUiStyle.Rect(footer,"DetailsToggle",new Vector2(.03f,.06f),new Vector2(.6f,.94f));
            var buttonImage=control.gameObject.AddComponent<Image>();buttonImage.color=Color.clear;
            detailsButton=control.gameObject.AddComponent<OwnedDetailsButton>();detailsButton.targetGraphic=buttonImage;
            foreach(var automaticTooltip in control.GetComponents<TooltipProvider>())automaticTooltip.enabled=false;
            var navigation=detailsButton.navigation;navigation.mode=Navigation.Mode.None;detailsButton.navigation=navigation;
            detailsLabel=style.Text(control,"Details  >",Mathf.Max(14,style.BodyFontSize-1)*scale);
            detailsButton.onClick.AddListener(()=>interaction.Toggle(CursorAvailable(),true));
            luckBadge=NativeUiStyle.Rect(footer,"Luck",new Vector2(.66f,.08f),new Vector2(.97f,.92f));var badge=luckBadge;
            badge.gameObject.AddComponent<Image>().color=Color.clear;badge.gameObject.AddComponent<LuckPresence>().Popup=this;
            var index=ItemCatalog.FindItemIndex("Clover");var def=index==ItemIndex.None?null:ItemCatalog.GetItemDef(index);
            var iconRect=NativeUiStyle.Rect(badge,"Clover",new Vector2(0,.08f),new Vector2(.24f,.92f));
            if(def){var image=iconRect.gameObject.AddComponent<Image>();image.sprite=def.pickupIconSprite;image.preserveAspect=true;image.raycastTarget=false;}
            luck=style.Text(badge,"?",Mathf.Max(12,style.BodyFontSize-3)*scale);luck.rectTransform.anchorMin=new Vector2(.28f,0);
            luckDetail=NativeUiStyle.Rect(root,"LuckDetail",new Vector2(1,0),new Vector2(1,0));luckDetail.pivot=Vector2.one;
            style.Background(luckDetail,false);luckDetail.gameObject.AddComponent<RectMask2D>();
            luckText=style.Text(luckDetail,"",style.BodyFontSize*scale);luckText.overflowMode=TextOverflowModes.Ellipsis;luckText.alignment=TextAlignmentOptions.TopLeft;
            luckText.rectTransform.offsetMin=new Vector2(12*scale,12*scale);luckText.rectTransform.offsetMax=new Vector2(-12*scale,-12*scale);
            luckDetail.gameObject.SetActive(false);root.gameObject.SetActive(false);
        }
        bool CursorAvailable()
        {
            if(row!=null&&row.IsItem&&(itemAnchor==null||!itemAnchor.Current))return false;
            var locator=root.GetComponentInParent<MPEventSystemLocator>();
            var owner=itemAnchor!=null?itemAnchor.EventSystem:focusContext?focusContext:locator?locator.eventSystem:null;
            return owner&&owner.isCursorVisible&&owner.cursorOpenerCount>0;
        }
        internal void Enter(object source,string key)
        {if(interaction.Enter(source,key)){focusContext=null;itemAnchor=null;row=null;lastFull=false;scroll.verticalNormalizedPosition=1;}}
        internal void Leave(object source)=>interaction.Leave(source,Time.unscaledTime);
        internal bool Expired()=>interaction.Expire(Time.unscaledTime);
        internal bool CanDefer(TooltipProvider provider,out MPEventSystem context)
        {
            context=null;
            if(!root||!root.gameObject.activeInHierarchy||itemAnchor==null||!itemAnchor.Current||!ReferenceEquals(provider,itemAnchor.Provider))return false;
            context=itemAnchor.EventSystem;return true;
        }
        internal bool Set(SheetRow source,TooltipProvider provider,bool followPointer)
        {
            if(source==null||!interaction.Active||interaction.Key!=source.Id){Hide();return false;}
            if(source.IsItem)
            {
                if(!source.HasItemAddition){Hide();return false;}
                if(!provider){Hide();return false;}
                if(itemAnchor==null||!ReferenceEquals(itemAnchor.Provider,provider)||!itemAnchor.Current)
                    itemAnchor=NativeTooltipAnchor.Find(provider,canvasRect,canvas);
                if(itemAnchor==null||!itemAnchor.Current){root.gameObject.SetActive(false);return false;}
            }
            bool hasDetails=!source.IsItem||!string.IsNullOrWhiteSpace(source.ItemDetailsRichTooltip)||!string.IsNullOrWhiteSpace(source.ExpandedItemNote);
            bool full=interaction.Expanded&&hasDetails,changed=row==null||row.Id!=source.Id||lastFull!=full;
            row=source;lastFull=full;nativeStyle.Refresh(title,body,panelImage,headerImage,scale,source.IsItem?itemAnchor.Controller:null);
            heading.gameObject.SetActive(!source.IsItem);
            if(!source.IsItem){title.text=TooltipText.SingleLine(source.Title);if(source.Icon)title.color=source.Color;}
            string raw=source.IsItem?full?string.Join("\n",new[]{source.CombinedItemNote,source.ExpandedItemNote,source.ItemDetailsRichTooltip}.Where(s=>!string.IsNullOrWhiteSpace(s))):source.CombinedItemNote:full?source.FullRichTooltip:source.ShortRichTooltip;
            string text=source.IsItem?raw:raw??TooltipMarkup.Literal(full?source.FullTooltip??source.ShortTooltip:source.ShortTooltip);
            body.text=TooltipMarkup.Render(text,nativeStyle.CanStyle);
            detailsButton.gameObject.SetActive(hasDetails);
            luckBadge.gameObject.SetActive(!source.IsItem||NativeItemSurfacePolicy.KnownLuck(source.LuckMark));
            detailsLabel.text=full?"Details  v":"Details  >";detailsButton.interactable=CursorAvailable();
            detailsLabel.font=body.font;detailsLabel.fontSharedMaterial=body.fontSharedMaterial;detailsLabel.color=body.color;
            luck.text=source.LuckMark=="mix"?"Mixed":source.LuckMark=="x"?"No":source.LuckMark=="\u2713"?"Yes":source.LuckMark;
            luckText.text=TooltipText.Clip(source.LuckTooltip,700);
            float width=source.IsItem?(float)itemAnchor.PinnedBounds.Width*1f:nativeStyle.PreferredWidth*scale;
            width=Mathf.Min(width,canvasRect.rect.width-24*scale);
            bool hasFooter=hasDetails||luckBadge.gameObject.activeSelf;footer.gameObject.SetActive(hasFooter);
            float headerHeight=source.IsItem?0:title.fontSize*1.4f+8*scale,footerHeight=hasFooter?Mathf.Max(30*scale,detailsLabel.fontSize+10*scale):0;
            bool showBody=!source.IsItem||full||!string.IsNullOrWhiteSpace(source.CombinedItemNote);
            viewport.gameObject.SetActive(showBody);
            float preferred=showBody?body.GetPreferredValues(body.text,Mathf.Max(20,width-24*scale),0).y:0;
            CompactTooltipSize sizing;
            if(!CompactTooltipSizing.TryCreate(canvasRect.rect.width,canvasRect.rect.height,width,preferred,headerHeight,footerHeight,scale,full,out sizing))
            {root.gameObject.SetActive(false);return false;}
            float height=(float)sizing.Height;
            UiRect dock=default(UiRect);
            if(source.IsItem)
            {
                if(!showBody)height=footerHeight+12*scale;
                if(!NativeTooltipAddonPlacement.TryBelow(canvasRect.rect.width,canvasRect.rect.height,itemAnchor.PinnedBounds,
                    height,footerHeight+(showBody?40:12)*scale,scale,out dock)){root.gameObject.SetActive(false);return false;}
                width=(float)dock.Width;height=(float)dock.Height;
            }
            root.sizeDelta=new Vector2(width,height);
            heading.sizeDelta=new Vector2(0,headerHeight);heading.anchoredPosition=Vector2.zero;
            footer.sizeDelta=new Vector2(0,footerHeight);
            viewport.offsetMin=new Vector2(12*scale,footerHeight+8*scale);viewport.offsetMax=new Vector2(-12*scale,-headerHeight-8*scale);
            content.sizeDelta=new Vector2(0,Mathf.Max(24*scale,height-headerHeight-footerHeight-16*scale,preferred));
            float luckWidth=Mathf.Min(320*scale,width-24*scale);
            float luckHeight=Mathf.Min(320*scale,luckText.GetPreferredValues(luckText.text,Mathf.Max(20,luckWidth-24*scale),0).y+24*scale);
            luckHeight=Mathf.Min(Mathf.Max(48*scale,luckHeight),canvasRect.rect.height-24*scale);
            luckDetail.sizeDelta=new Vector2(luckWidth,luckHeight);
            if(changed)scroll.verticalNormalizedPosition=1;
            root.gameObject.SetActive(true);root.SetAsLastSibling();
            if(source.IsItem)root.anchoredPosition=new Vector2((float)dock.X,(float)(dock.Y+dock.Height));
            else Position(followPointer&&!PointerInside);
            float panelBottom=root.anchoredPosition.y-height;
            float luckBottom=Mathf.Clamp(panelBottom+footerHeight+8*scale,12*scale,canvasRect.rect.height-12*scale-luckHeight);
            luckDetail.anchoredPosition=new Vector2(-12*scale,luckBottom-panelBottom);
            return true;
        }
        void Position(bool followPointer)
        {
            Vector2 point=root.anchoredPosition+canvasRect.rect.min;
            if(followPointer)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,Input.mousePosition,
                    canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,out point);
                point+=new Vector2(18,-18);
            }
            point.x=Mathf.Clamp(point.x,canvasRect.rect.xMin+12,canvasRect.rect.xMax-root.rect.width-12);
            point.y=Mathf.Clamp(point.y,canvasRect.rect.yMin+root.rect.height+12,canvasRect.rect.yMax-12);
            root.anchoredPosition=point-canvasRect.rect.min;
        }
        internal void Focus(bool focused,MPEventSystem context=null){focusContext=focused?context:null;interaction.Focus(focused,Time.unscaledTime);}
        internal bool FocusToggle()=>interaction.FocusToggle(CursorAvailable());
        internal bool FocusExpand(bool expanded)=>interaction.FocusExpand(CursorAvailable(),expanded);
        internal void ShowLuck(bool show){if(luckDetail)luckDetail.gameObject.SetActive(show);}
        internal void Hide(){focusContext=null;interaction.Reset();itemAnchor=null;row=null;if(root)root.gameObject.SetActive(false);ShowLuck(false);}
        public void Dispose(){Hide();if(root)UnityEngine.Object.Destroy(root.gameObject);}
    }
}
