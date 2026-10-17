using System;
using Alexander.CharacterSheet.Core;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Alexander.CharacterSheet
{
    internal sealed class NativeHoverPopup:IDisposable
    {
        readonly Canvas canvas;
        readonly RectTransform canvasRect,root,content,luckDetail,heading,viewport,footer;
        readonly TextMeshProUGUI title,body,luck,luckText,note;
        readonly ScrollRect scroll;
        readonly Image panelImage,headerImage;
        readonly NativeTooltipStyle nativeStyle=new NativeTooltipStyle();
        readonly float scale;
        SheetRow row;
        bool lastFull;
        internal bool PointerInside;
        internal NativeHoverPopup(RectTransform ownedHost,Canvas eventCanvas,NativeUiStyle style,float presentationScale)
        {
            canvas=eventCanvas;canvasRect=ownedHost;scale=presentationScale;
            root=NativeUiStyle.Rect(canvasRect,"AlexanderItemHover",Vector2.zero,Vector2.zero);root.pivot=new Vector2(0,1);
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
            note=style.Text(footer,"Shift: details",Mathf.Max(12,style.BodyFontSize-3)*scale);
            note.rectTransform.anchorMax=new Vector2(.6f,1);note.rectTransform.offsetMin=new Vector2(12*scale,0);note.alpha=.7f;
            var badge=NativeUiStyle.Rect(footer,"Luck",new Vector2(.64f,.08f),new Vector2(.97f,.92f));
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
        internal void Set(SheetRow source,bool full,bool followPointer)
        {
            if(source==null){Hide();return;}
            bool changed=row==null||row.Id!=source.Id||lastFull!=full;
            row=source;lastFull=full;nativeStyle.Refresh(title,body,panelImage,headerImage,scale);
            title.text=TooltipText.SingleLine(source.Title);if(source.Icon)title.color=source.Color;
            string raw=full?source.FullRichTooltip:source.ShortRichTooltip;
            string text=TooltipMarkup.Render(raw??TooltipMarkup.Literal(full?source.FullTooltip??source.ShortTooltip:source.ShortTooltip),nativeStyle.CanStyle);
            body.text=text;note.text=full?"Release Shift: summary":"Shift: details";
            luck.text=source.LuckMark=="mix"?"Mixed":source.LuckMark=="x"?"No":source.LuckMark=="\u2713"?"Yes":source.LuckMark;
            luckText.text=TooltipText.Clip(source.LuckTooltip,700);
            float width=Mathf.Min(nativeStyle.PreferredWidth*scale,canvasRect.rect.width-24*scale);
            float headerHeight=title.fontSize*1.4f+8*scale,footerHeight=Mathf.Max(24*scale,note.fontSize+8*scale);
            double preferred=body.GetPreferredValues(body.text,Mathf.Max(20,width-24*scale),0).y;
            CompactTooltipSize sizing;
            if(!CompactTooltipSizing.TryCreate(canvasRect.rect.width,canvasRect.rect.height,width,preferred,headerHeight,footerHeight,scale,full,out sizing)){Hide();return;}
            root.sizeDelta=new Vector2((float)sizing.Width,(float)sizing.Height);
            heading.sizeDelta=new Vector2(0,headerHeight);heading.anchoredPosition=Vector2.zero;
            footer.sizeDelta=new Vector2(0,footerHeight);
            viewport.offsetMin=new Vector2(12*scale,footerHeight+8*scale);viewport.offsetMax=new Vector2(-12*scale,-headerHeight-8*scale);
            content.sizeDelta=new Vector2(0,Mathf.Max((float)sizing.ViewportHeight,(float)preferred));
            float luckWidth=Mathf.Min(320*scale,(float)sizing.Width-24*scale);
            float luckHeight=Mathf.Min(Mathf.Min(320*scale,(float)sizing.Height-footerHeight-20*scale),luckText.GetPreferredValues(luckText.text,Mathf.Max(20,luckWidth-24*scale),0).y+24*scale);
            luckDetail.sizeDelta=new Vector2(luckWidth,Mathf.Max(48*scale,luckHeight));luckDetail.anchoredPosition=new Vector2(-12*scale,footerHeight+8*scale);
            if(changed)scroll.verticalNormalizedPosition=1;
            root.gameObject.SetActive(true);root.SetAsLastSibling();Position(followPointer&&!PointerInside);
        }
        void Position(bool followPointer)
        {
            Vector2 point=root.anchoredPosition+new Vector2(canvasRect.rect.xMin,canvasRect.rect.yMin);
            if(followPointer)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,Input.mousePosition,
                    canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,out point);
                point+=new Vector2(18,-18);
            }
            point.x=Mathf.Clamp(point.x,canvasRect.rect.xMin+12,canvasRect.rect.xMax-root.rect.width-12);
            point.y=Mathf.Clamp(point.y,canvasRect.rect.yMin+root.rect.height+12,canvasRect.rect.yMax-12);
            root.anchoredPosition=new Vector2(point.x-canvasRect.rect.xMin,point.y-canvasRect.rect.yMin);
        }
        internal void ShowLuck(bool show){if(luckDetail)luckDetail.gameObject.SetActive(show);}
        internal void Hide(){row=null;PointerInside=false;if(root)root.gameObject.SetActive(false);ShowLuck(false);}
        public void Dispose(){Hide();if(root)UnityEngine.Object.Destroy(root.gameObject);}
    }
}
