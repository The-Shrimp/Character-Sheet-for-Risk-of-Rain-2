using RoR2.UI;
using UnityEngine.EventSystems;
class PublicNativeTooltipApiProbe {
 void Read(TooltipController controller,TooltipProvider provider) {
  IPointerEnterHandler enter=provider;
  IPointerExitHandler exit=provider;
  MPEventSystem eventOwner=controller.owner;
  TooltipProvider current=controller.owner.currentTooltipProvider;
  string body=provider.bodyText;
  string title=provider.titleText;
  var eventSystem=provider.GetComponentInParent<MPEventSystemLocator>().eventSystem;
  var cursor=eventOwner.isCursorVisible;
  var count=eventOwner.cursorOpenerCount;
  var data=new PointerEventData(eventOwner);
  var nativeButton=typeof(HGButton);
 }
}
