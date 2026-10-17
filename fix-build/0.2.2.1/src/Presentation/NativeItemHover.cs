using System;
using System.Reflection;
using Alexander.CharacterSheet.Core;
using HarmonyLib;
using RoR2.UI;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Alexander.CharacterSheet
{
    internal sealed class NativeItemHover:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        static readonly NativeBindingRegistry Registry=new NativeBindingRegistry();
        SheetView owner;
        ItemIcon icon;
        TooltipProvider provider;
        internal ItemIcon Icon=>icon;
        internal bool CanShow=>owner!=null&&isActiveAndEnabled&&icon&&Registry.Owns(icon,this)&&
            (!provider||provider.enabled&&(provider.transform==icon.transform||provider.transform.IsChildOf(icon.transform)))&&owner.CanHandleIcon(icon);
        internal bool Bind(SheetView view,ItemIcon source)
        {
            if(!Registry.TryClaim(source,this))return false;
            owner=view;icon=source;provider=source.tooltipProvider;return true;
        }
        internal void RefreshProvider(){provider=icon?icon.tooltipProvider:null;}
        internal void Detach()
        {
            var oldOwner=owner;owner=null;
            if(icon)Registry.Release(icon,this);
            else if(!ReferenceEquals(icon,null))Registry.Release(icon,this);
            if(oldOwner!=null)oldOwner.Leave(this);enabled=false;
        }
        // Route only this local, active, exclusively claimed icon. No provider field is written.
        internal static bool TryRoute(TooltipProvider source)
        {
            if(!source)return false;
            foreach(var probe in source.GetComponentsInParent<NativeItemHover>())
                if(probe.CanShow&&ReferenceEquals(probe.provider,source)&&probe.owner.EnterItem(probe))return true;
            return false;
        }
        public void OnPointerEnter(PointerEventData data){if(CanShow)owner.EnterItem(this);}
        public void OnPointerExit(PointerEventData data){owner?.Leave(this);}
        void OnDisable(){owner?.Leave(this);}
        void OnDestroy(){Detach();}
    }
    // Resolve the publicly implemented Unity interface; never inspect its implementation or private fields.
    [HarmonyPatch]
    internal static class NativePointerRoutePatch
    {
        [HarmonyTargetMethod]static MethodBase TargetMethod()
        {
            var map=typeof(TooltipProvider).GetInterfaceMap(typeof(IPointerEnterHandler));
            for(int i=0;i<map.InterfaceMethods.Length;i++)
                if(map.InterfaceMethods[i].Name==nameof(IPointerEnterHandler.OnPointerEnter))return map.TargetMethods[i];
            throw new MissingMethodException("TooltipProvider's public IPointerEnterHandler contract is unavailable.");
        }
        [HarmonyPrefix]static bool Prefix(TooltipProvider __instance)
        {
            if(Plugin.Instance==null||!Plugin.Instance.CanRun)return true;
            try{return !NativeItemHover.TryRoute(__instance);}
            catch(Exception){return true;} // Preserve native hover if enhancement cannot handle this event.
        }
    }
    internal sealed class AddedRowHover:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        internal SheetView Owner;
        internal SheetRow Row;
        public void OnPointerEnter(PointerEventData data){Owner?.EnterRow(this);}
        public void OnPointerExit(PointerEventData data){Owner?.Leave(this);}
        void OnDisable(){Owner?.Leave(this);}
    }
    internal sealed class PopupPresence:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        internal NativeHoverPopup Popup;
        public void OnPointerEnter(PointerEventData data){if(Popup!=null)Popup.PointerInside=true;}
        public void OnPointerExit(PointerEventData data){if(Popup!=null)Popup.PointerInside=false;}
        void OnDisable(){if(Popup!=null)Popup.PointerInside=false;}
    }
    internal sealed class LuckPresence:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        internal NativeHoverPopup Popup;
        public void OnPointerEnter(PointerEventData data){Popup?.ShowLuck(true);}
        public void OnPointerExit(PointerEventData data){Popup?.ShowLuck(false);}
        void OnDisable(){Popup?.ShowLuck(false);}
    }
}
