using UnityEngine.EventSystems;
using RoR2.UI;
namespace Alexander.CharacterSheet
{
    internal sealed class SheetFocusButton:HGButton
    {
        internal SheetView Owner;
        internal string NodeId;
        internal AddedRowHover Hover;
        public override void OnSelect(BaseEventData data){base.OnSelect(data);Owner?.Focus(this,data);}
        public override void OnDeselect(BaseEventData data){base.OnDeselect(data);Owner?.Unfocus(this);}
        public override void OnSubmit(BaseEventData data){Owner?.Submit(this);data.Use();}
        public override void OnMove(AxisEventData data)
        {if(Owner!=null&&Owner.Move(this,data.moveDir)){data.Use();return;}base.OnMove(data);}
        public override void OnPointerDown(PointerEventData data){base.OnPointerDown(data);data.Use();}
        public override void OnPointerUp(PointerEventData data){base.OnPointerUp(data);data.Use();}
        public override void OnPointerClick(PointerEventData data){base.OnPointerClick(data);data.Use();}
    }
}
