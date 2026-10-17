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
        internal NativeUiStyle(TMP_Text text,Image image)
        {
            font=text.font;material=text.fontSharedMaterial;textColor=text.color;
            backgroundSprite=image?image.sprite:null;backgroundType=image?image.type:Image.Type.Simple;
            backgroundColor=image?image.color:new Color(.08f,.09f,.11f,.95f);
        }
        internal static NativeUiStyle Capture(Transform nativeStrip)
        {
            var text=nativeStrip.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t=>t.font);
            if(!text)return null;
            var image=nativeStrip.GetComponentsInChildren<Image>(true).FirstOrDefault(i=>i.type==Image.Type.Sliced&&i.sprite);
            return new NativeUiStyle(text,image);
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
        internal static RectTransform Rect(Transform parent,string name,Vector2 min,Vector2 max)
        {
            var result=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            result.SetParent(parent,false);result.anchorMin=min;result.anchorMax=max;
            result.offsetMin=Vector2.zero;result.offsetMax=Vector2.zero;return result;
        }
    }
}
