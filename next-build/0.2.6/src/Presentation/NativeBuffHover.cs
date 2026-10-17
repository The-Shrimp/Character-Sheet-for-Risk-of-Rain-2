using System;
using Alexander.CharacterSheet.Core;
using RoR2.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Alexander.CharacterSheet
{
    // An original, per-instance hit target. No shared prefab, native image or provider is changed.
    internal sealed class NativeBuffHover:MonoBehaviour
    {
        static readonly NativeBindingRegistry Registry=new NativeBindingRegistry();
        SheetView owner;
        BuffIcon icon;
        RectTransform hit;
        internal BuffIcon Icon=>icon;
        internal bool CanShow=>owner!=null&&isActiveAndEnabled&&icon&&Registry.Owns(icon,this)&&owner.CanHandleBuff(icon);
        internal bool Bind(SheetView view,BuffIcon source)
        {
            if(!Registry.TryClaim(source,this))return false;
            owner=view;icon=source;
            try
            {
                hit=NativeUiStyle.Rect(icon.transform,"AlexanderBuffHoverTarget",Vector2.zero,Vector2.one);
                hit.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;
                // Own nested raycaster: do not add components to a shared HUD canvas.
                hit.gameObject.AddComponent<Canvas>().overrideSorting=false;
                hit.gameObject.AddComponent<GraphicRaycaster>();
                var graphic=hit.gameObject.AddComponent<Image>();graphic.color=Color.clear;graphic.raycastTarget=true;
                hit.gameObject.AddComponent<NativeBuffHit>().Binding=this;
                RefreshContext();return true;
            }
            catch{Detach();UnityEngine.Object.Destroy(this);throw;}
        }
        internal void RefreshContext(){if(hit)hit.gameObject.SetActive(CanShow);}
        internal void Enter(){if(CanShow)owner.EnterBuff(this);}
        internal void Leave(){owner?.Leave(this);}
        internal static bool TryRoute(TooltipProvider source)
        {
            if(!source)return false;
            foreach(var binding in source.GetComponentsInParent<NativeBuffHover>())
                if(binding.CanShow&&binding.owner.EnterBuff(binding))return true;
            return false;
        }
        internal void Detach()
        {
            var previous=owner;owner=null;
            if(!ReferenceEquals(icon,null))Registry.Release(icon,this);
            previous?.Leave(this);
            if(hit){hit.gameObject.SetActive(false);UnityEngine.Object.Destroy(hit.gameObject);hit=null;}
            enabled=false;
        }
        void OnDisable(){Leave();if(hit)hit.gameObject.SetActive(false);}
        void OnDestroy(){Detach();}
    }
    internal sealed class NativeBuffHit:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        internal NativeBuffHover Binding;
        public void OnPointerEnter(PointerEventData data){Binding?.Enter();}
        public void OnPointerExit(PointerEventData data){Binding?.Leave();}
        void OnDisable(){Binding?.Leave();}
    }
}
