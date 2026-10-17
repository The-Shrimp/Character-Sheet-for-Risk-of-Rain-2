using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
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
        static readonly HashSet<NativeItemHover> Sources=new HashSet<NativeItemHover>();
        readonly NativePointerExitLease exitLease=new NativePointerExitLease();
        SheetView owner;
        ItemIcon icon;
        AddedRowHover added;
        TooltipProvider provider;
        object registryKey;
        MPEventSystem deferredContext;
        bool replaying;
        internal ItemIcon Icon=>icon;
        internal TooltipProvider Provider=>provider;
        internal string RowId=>icon?"item:"+(int)icon.itemIndex:added?.Row?.Id;
        internal bool CanShow=>owner!=null&&isActiveAndEnabled&&provider&&provider.enabled&&Registry.Owns(registryKey,this)&&
            (provider.transform==(icon?icon.transform:added?added.transform:null)||provider.transform.IsChildOf(icon?icon.transform:added?added.transform:null))&&
            (icon?owner.CanHandleIcon(icon):added&&owner.CanHandleAddedItem(added));
        internal bool Bind(SheetView view,ItemIcon source)
        {
            if(!source||!source.tooltipProvider||!Registry.TryClaim(source,this))return false;
            owner=view;icon=source;provider=source.tooltipProvider;registryKey=source;Sources.Add(this);return true;
        }
        internal bool BindAdded(SheetView view,AddedRowHover source,TooltipProvider nativeProvider)
        {
            if(!source||!nativeProvider||!Registry.TryClaim(nativeProvider,this))return false;
            owner=view;added=source;provider=nativeProvider;registryKey=nativeProvider;Sources.Add(this);return true;
        }
        internal void RefreshProvider()
        {
            var next=icon?icon.tooltipProvider:provider;
            if(next!=provider){ReleaseDeferred();provider=next;}
        }
        internal void Detach()
        {
            ReleaseDeferred();var previous=owner;owner=null;
            if(registryKey!=null)Registry.Release(registryKey,this);
            Sources.Remove(this);previous?.Leave(this);enabled=false;
        }
        internal static void BeforeNativeEnter(TooltipProvider next,PointerEventData data)
        {
            if(!next)return;
            var context=data!=null&&data.currentInputModule?data.currentInputModule.GetComponent<MPEventSystem>():null;
            if(!context){var locator=next.GetComponentInParent<MPEventSystemLocator>();context=locator?locator.eventSystem:null;}
            foreach(var source in Sources.ToArray())
            {
                if(!source||!source.exitLease.Pending)continue;
                if(context&&ReferenceEquals(context,source.deferredContext))
                {
                    if(ReferenceEquals(next,source.provider))source.CancelDeferred();
                    else source.ReleaseDeferred();
                }
            }
        }
        internal static void AfterNativeEnter(TooltipProvider source)
        {
            if(!source)return;
            foreach(var probe in source.GetComponentsInParent<NativeItemHover>())
                if(probe.CanShow&&ReferenceEquals(probe.provider,source)){probe.owner.EnterItem(probe);return;}
        }
        internal static bool TryDeferNativeExit(TooltipProvider source,PointerEventData data)
        {
            if(!source||data==null)return false;
            foreach(var probe in source.GetComponentsInParent<NativeItemHover>())
            {
                if(probe.replaying||!probe.CanShow||!ReferenceEquals(probe.provider,source))continue;
                MPEventSystem context;
                if(!probe.owner.CanDeferExit(probe,out context))return false;
                // Keep this actual UI exit event; no gameplay input is fabricated.
                if(!probe.exitLease.Defer(source,context,()=>probe.ReplayExit(source,data)))return false;
                probe.deferredContext=context;probe.owner.Leave(probe);return true;
            }
            return false;
        }
        void ReplayExit(TooltipProvider source,PointerEventData data)
        {
            if(!source)return;replaying=true;
            try{((IPointerExitHandler)source).OnPointerExit(data);}
            finally{replaying=false;}
        }
        void CancelDeferred(){exitLease.Cancel();deferredContext=null;}
        internal void ReleaseDeferred()
        {
            bool owns=provider&&deferredContext&&ReferenceEquals(deferredContext.currentTooltipProvider,provider);
            try{exitLease.Release(owns);}catch(Exception){/* Owned lease is cleared before calling the native interface. */}
            finally{deferredContext=null;}
        }
        internal static void ReleaseForView(SheetView view)
        {foreach(var source in Sources.ToArray())if(source&&ReferenceEquals(source.owner,view))source.ReleaseDeferred();}
        public void OnPointerEnter(PointerEventData data){if(CanShow)owner.EnterItem(this);}
        public void OnPointerExit(PointerEventData data){owner?.Leave(this);}
        void OnDisable(){ReleaseDeferred();owner?.Leave(this);}
        void OnDestroy(){Detach();}
    }
    internal static class NativePointerContract
    {
        internal static MethodBase Resolve(Type contract,string method)
        {
            var map=typeof(TooltipProvider).GetInterfaceMap(contract);
            for(int i=0;i<map.InterfaceMethods.Length;i++)
                if(map.InterfaceMethods[i].Name==method)return map.TargetMethods[i];
            throw new MissingMethodException("TooltipProvider's public Unity pointer contract is unavailable.");
        }
    }
    [HarmonyPatch]
    internal static class NativePointerEnterPatch
    {
        [HarmonyTargetMethod]static MethodBase TargetMethod()=>NativePointerContract.Resolve(typeof(IPointerEnterHandler),nameof(IPointerEnterHandler.OnPointerEnter));
        // Native enter always runs: title, description, localization and other mods stay native.
        [HarmonyPrefix]static void Prefix(TooltipProvider __instance,PointerEventData __0)
        {
            if(Plugin.Instance==null||!Plugin.Instance.CanRun)return;
            try{NativeItemHover.BeforeNativeEnter(__instance,__0);}catch(Exception){}
        }
        [HarmonyPostfix]static void Postfix(TooltipProvider __instance)
        {
            if(Plugin.Instance==null||!Plugin.Instance.CanRun)return;
            try{NativeItemHover.AfterNativeEnter(__instance);}catch(Exception){}
        }
    }
    [HarmonyPatch]
    internal static class NativePointerExitPatch
    {
        [HarmonyTargetMethod]static MethodBase TargetMethod()=>NativePointerContract.Resolve(typeof(IPointerExitHandler),nameof(IPointerExitHandler.OnPointerExit));
        [HarmonyPrefix]static bool Prefix(TooltipProvider __instance,PointerEventData __0)
        {
            if(Plugin.Instance==null||!Plugin.Instance.CanRun)return true;
            try{return !NativeItemHover.TryDeferNativeExit(__instance,__0);}catch(Exception){return true;}
        }
    }
    internal sealed class AddedRowHover:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        internal SheetView Owner;
        internal SheetRow Row;
        internal TextMeshProUGUI Label,Value;
        internal TooltipProvider NativeProvider;
        internal NativeItemHover ItemHover;
        public void OnPointerEnter(PointerEventData data){if(Row!=null&&!Row.IsItem)Owner?.EnterRow(this);}
        public void OnPointerExit(PointerEventData data){if(Row!=null&&!Row.IsItem)Owner?.Leave(this);}
        void OnDisable(){Owner?.Leave(this);}
    }
    internal sealed class PopupPresence:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,IPointerClickHandler
    {
        internal NativeHoverPopup Popup;
        public void OnPointerEnter(PointerEventData data){if(Popup!=null)Popup.PointerInside=true;}
        public void OnPointerExit(PointerEventData data){if(Popup!=null)Popup.PointerInside=false;}
        public void OnPointerDown(PointerEventData data){data.Use();}
        public void OnPointerUp(PointerEventData data){data.Use();}
        public void OnPointerClick(PointerEventData data){data.Use();}
        void OnDisable(){if(Popup!=null)Popup.PointerInside=false;}
    }
    internal sealed class OwnedDetailsButton:HGButton
    {
        public override void OnPointerDown(PointerEventData data){base.OnPointerDown(data);data.Use();}
        public override void OnPointerUp(PointerEventData data){base.OnPointerUp(data);data.Use();}
        public override void OnPointerClick(PointerEventData data){base.OnPointerClick(data);data.Use();}
        public override void OnSubmit(BaseEventData data){base.OnSubmit(data);data.Use();}
    }
    internal sealed class LuckPresence:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        internal NativeHoverPopup Popup;
        public void OnPointerEnter(PointerEventData data){Popup?.ShowLuck(true);}
        public void OnPointerExit(PointerEventData data){Popup?.ShowLuck(false);}
        void OnDisable(){Popup?.ShowLuck(false);}
    }
}
