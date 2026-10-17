using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Alexander.CharacterSheet
{
    // Runtime references only; no extracted fonts, sprites or prefabs.
    internal sealed class NativeUiStyle
    {
        readonly TMP_FontAsset font;
        readonly Material material;
        readonly Color textColor,backgroundColor;
        readonly Sprite backgroundSprite;
        readonly Image.Type backgroundType;
        internal readonly float BodyFontSize;
        internal NativeUiStyle(TMP_Text text,Image image,float bodySize)
        {
            font=text.font;material=text.fontSharedMaterial;textColor=text.color;
            backgroundSprite=image?image.sprite:null;backgroundType=image?image.type:Image.Type.Simple;
            backgroundColor=image?image.color:new Color(.08f,.09f,.11f,.97f);
            BodyFontSize=Mathf.Clamp(bodySize,18,24);
        }
        internal static NativeUiStyle Capture(Transform nativeStrip)
        {
            var texts=nativeStrip.GetComponentsInChildren<TMP_Text>(false).Where(t=>t.font).ToArray();
            if(texts.Length==0)return null;
            var sizes=texts.Where(t=>t.fontSize>=14&&t.fontSize<=40).Select(t=>t.fontSize).OrderBy(s=>s).ToArray();
            float size=sizes.Length>0?sizes[sizes.Length/2]:18;
            var rect=nativeStrip as RectTransform;
            var image=nativeStrip.GetComponent<Image>();
            if(image&&(!image.sprite||image.color.a<=.05f))image=null;
            if(!image&&rect)image=nativeStrip.GetComponentsInChildren<Image>(false)
                .Where(i=>i.type==Image.Type.Sliced&&i.sprite&&i.rectTransform.rect.width>=rect.rect.width*.75f&&
                    i.rectTransform.rect.height>=rect.rect.height*.6f)
                .OrderByDescending(i=>i.rectTransform.rect.width*i.rectTransform.rect.height).FirstOrDefault();
            return new NativeUiStyle(texts[0],image,size);
        }
        internal TextMeshProUGUI Text(Transform parent,string value,float size)
        {
            var root=Rect(parent,"Text",Vector2.zero,Vector2.one);
            var text=root.gameObject.AddComponent<TextMeshProUGUI>();
            text.font=font;text.fontSharedMaterial=material;
            text.fontSize=size;text.color=textColor;text.richText=false;text.raycastTarget=false;
            text.enableWordWrapping=true;text.alignment=TextAlignmentOptions.MidlineLeft;text.text=value;return text;
        }
        internal Image Background(RectTransform root,bool raycast)
        {
            var image=root.gameObject.AddComponent<Image>();
            image.sprite=backgroundSprite;image.type=backgroundType;image.color=backgroundColor;
            image.raycastTarget=raycast;return image;
        }
        internal void Border(RectTransform parent,float scale)
        {
            Color color=new Color(textColor.r,textColor.g,textColor.b,.35f);
            foreach(var side in new[]{"Top","Bottom","Left","Right"})
            {
                bool horizontal=side=="Top"||side=="Bottom";
                Vector2 min=side=="Top"?new Vector2(0,1):side=="Right"?new Vector2(1,0):Vector2.zero;
                Vector2 max=side=="Bottom"?new Vector2(1,0):side=="Left"?new Vector2(0,1):Vector2.one;
                var edge=Rect(parent,side+"Border",min,max);
                edge.pivot=side=="Top"?new Vector2(.5f,1):side=="Right"?new Vector2(1,.5f):Vector2.zero;
                edge.sizeDelta=horizontal?new Vector2(0,scale):new Vector2(scale,0);
                var line=edge.gameObject.AddComponent<Image>();line.color=color;line.raycastTarget=false;
            }
        }
        internal static RectTransform Rect(Transform parent,string name,Vector2 min,Vector2 max)
        {
            var result=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            result.SetParent(parent,false);result.anchorMin=min;result.anchorMax=max;
            result.offsetMin=Vector2.zero;result.offsetMax=Vector2.zero;return result;
        }
    }
}
