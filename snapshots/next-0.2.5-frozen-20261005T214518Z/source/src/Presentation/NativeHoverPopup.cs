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
        readonly RectTransform canvasRect,root,content,luckDetail;
        readonly TextMeshProUGUI title,body,luck,luckText;
        readonly ScrollRect scroll;
        SheetRow row;
        bool lastFull;
        internal bool PointerInside;
        internal NativeHoverPopup(Canvas parent,NativeUiStyle style,float scale)
        {
            canvas=parent;canvasRect=(RectTransform)canvas.transform;
            root=NativeUiStyle.Rect(canvasRect,"AlexanderItemHover",Vector2.zero,Vector2.zero);root.pivot=new Vector2(0,1);
            root.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;style.Background(root,true);
            root.gameObject.AddComponent<PopupPresence>().Popup=this;
            var heading=NativeUiStyle.Rect(root,"Header",new Vector2(0,1),Vector2.one);
            heading.pivot=new Vector2(0,1);heading.sizeDelta=new Vector2(0,42);heading.anchoredPosition=Vector2.zero;
            title=style.Text(heading,"",20*scale);title.rectTransform.offsetMin=new Vector2(14,4);title.rectTransform.offsetMax=new Vector2(-14,-4);
            var viewport=NativeUiStyle.Rect(root,"BodyViewport",Vector2.zero,Vector2.one);
            viewport.offsetMin=new Vector2(14,62);viewport.offsetMax=new Vector2(-14,-48);
            viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            content=NativeUiStyle.Rect(viewport,"Body",new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);
            body=style.Text(content,"",15*scale);body.alignment=TextAlignmentOptions.TopLeft;
            scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;
            scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=24;
            var footer=NativeUiStyle.Rect(root,"Footer",Vector2.zero,new Vector2(1,0));
            footer.pivot=Vector2.zero;footer.sizeDelta=new Vector2(0,50);
            var note=style.Text(footer,"Shift: full math\nScroll for longer details",11*scale);
            note.rectTransform.anchorMax=new Vector2(.52f,1);note.rectTransform.offsetMin=new Vector2(14,7);
            var badge=NativeUiStyle.Rect(footer,"Luck",new Vector2(.53f,.12f),new Vector2(.97f,.92f));
            style.Background(badge,true);badge.gameObject.AddComponent<LuckPresence>().Popup=this;
            var index=ItemCatalog.FindItemIndex("Clover");var def=index==ItemIndex.None?null:ItemCatalog.GetItemDef(index);
            var iconRect=NativeUiStyle.Rect(badge,"Clover",new Vector2(.02f,.1f),new Vector2(.21f,.9f));
            if(def){var image=iconRect.gameObject.AddComponent<Image>();image.sprite=def.pickupIconSprite;image.preserveAspect=true;image.raycastTarget=false;}
            luck=style.Text(badge,"? Unverified",11*scale);luck.rectTransform.anchorMin=new Vector2(.23f,0);
            luckDetail=NativeUiStyle.Rect(root,"LuckDetail",new Vector2(1,0),new Vector2(1,0));
            luckDetail.pivot=Vector2.one;luckDetail.sizeDelta=new Vector2(290,220);luckDetail.anchoredPosition=new Vector2(-12,60);
            style.Background(luckDetail,false);luckDetail.gameObject.AddComponent<RectMask2D>();luckText=style.Text(luckDetail,"",12*scale);luckText.overflowMode=TextOverflowModes.Ellipsis;luckText.alignment=TextAlignmentOptions.TopLeft;
            luckText.rectTransform.offsetMin=new Vector2(12,12);luckText.rectTransform.offsetMax=new Vector2(-12,-12);
            luckDetail.gameObject.SetActive(false);root.gameObject.SetActive(false);
        }
        internal void Set(SheetRow source,bool full,bool followPointer)
        {
            if(source==null){Hide();return;}
            bool changed=row==null||row.Id!=source.Id||lastFull!=full;
            row=source;lastFull=full;title.text=TooltipText.SingleLine(source.Title);
            string text=TooltipText.Clean(full?source.FullTooltip??source.ShortTooltip:source.ShortTooltip);
            bool textChanged=changed||body.text!=text;body.text=text;
            luck.text=source.LuckMark+" "+source.LuckLabel;luckText.text=TooltipText.Clip(source.LuckTooltip,700);
            float width=Mathf.Min(470,canvasRect.rect.width-24),height=Mathf.Min(full?580:320,canvasRect.rect.height-24);
            root.sizeDelta=new Vector2(width,height);
            if(textChanged)
            {
                Canvas.ForceUpdateCanvases();
                float textHeight=Mathf.Max(height-110,body.GetPreferredValues(body.text,Mathf.Max(20,width-28),0).y+8);
                content.sizeDelta=new Vector2(0,textHeight);
            }
            if(changed)scroll.verticalNormalizedPosition=1;
            root.gameObject.SetActive(true);root.SetAsLastSibling();if(followPointer&&!PointerInside)Position();
        }
        void Position()
        {
            Vector2 point;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,Input.mousePosition,
                canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,out point);
            point+=new Vector2(18,-18);
            point.x=Mathf.Clamp(point.x,canvasRect.rect.xMin+12,canvasRect.rect.xMax-root.rect.width-12);
            point.y=Mathf.Clamp(point.y,canvasRect.rect.yMin+root.rect.height+12,canvasRect.rect.yMax-12);
            root.anchoredPosition=new Vector2(point.x-canvasRect.rect.xMin,point.y-canvasRect.rect.yMin);
        }
        internal void ShowLuck(bool show){if(luckDetail)luckDetail.gameObject.SetActive(show);}
        internal void Hide(){row=null;PointerInside=false;if(root)root.gameObject.SetActive(false);ShowLuck(false);}
        public void Dispose(){Hide();if(root)UnityEngine.Object.Destroy(root.gameObject);}
    }
}
