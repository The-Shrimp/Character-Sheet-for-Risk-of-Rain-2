using System;
using System.Linq;
using Alexander.CharacterSheet.Core;
using RoR2.UI;
using UnityEngine;
namespace Alexander.CharacterSheet
{
    internal sealed class NativeTooltipAnchor
    {
        internal readonly TooltipProvider Provider;
        internal readonly TooltipController Controller;
        internal readonly MPEventSystem EventSystem;
        internal readonly UiRect PinnedBounds;
        internal bool Current=>Provider&&Provider.enabled&&Controller&&Controller.gameObject.activeInHierarchy&&
            EventSystem&&ReferenceEquals(Controller.owner,EventSystem)&&ReferenceEquals(EventSystem.currentTooltipProvider,Provider);
        NativeTooltipAnchor(TooltipProvider provider,TooltipController controller,UiRect bounds)
        {Provider=provider;Controller=controller;EventSystem=controller.owner;PinnedBounds=bounds;}
        internal static NativeTooltipAnchor Find(TooltipProvider provider,RectTransform ownedHost,Canvas eventCanvas)
        {
            if(!provider||!provider.enabled||!ownedHost||!eventCanvas)return null;
            var controllers=Resources.FindObjectsOfTypeAll<TooltipController>()
                .Where(c=>c&&c.gameObject.scene.IsValid()&&c.gameObject.activeInHierarchy&&c.owner&&
                    ReferenceEquals(c.owner.currentTooltipProvider,provider)).ToArray();
            if(controllers.Length!=1)return null;
            var controller=controllers[0];var rect=controller.transform as RectTransform;
            var nativeCanvas=controller.GetComponentInParent<Canvas>();if(!rect||!nativeCanvas)return null;
            var corners=new Vector3[4];rect.GetWorldCorners(corners);
            Vector2 min=new Vector2(float.PositiveInfinity,float.PositiveInfinity),max=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
            foreach(var world in corners)
            {
                Vector2 point;
                var screen=RectTransformUtility.WorldToScreenPoint(nativeCanvas.renderMode==RenderMode.ScreenSpaceOverlay?null:nativeCanvas.worldCamera,world);
                if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(ownedHost,screen,
                    eventCanvas.renderMode==RenderMode.ScreenSpaceOverlay?null:eventCanvas.worldCamera,out point))return null;
                point-=ownedHost.rect.min;min=Vector2.Min(min,point);max=Vector2.Max(max,point);
            }
            var bounds=new UiRect(min.x,min.y,max.x-min.x,max.y-min.y);
            return bounds.Valid?new NativeTooltipAnchor(provider,controller,bounds):null;
        }
    }
}
