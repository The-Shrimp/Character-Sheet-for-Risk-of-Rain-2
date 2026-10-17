using System;
namespace Alexander.CharacterSheet.Core
{
    public static class NativeBuffHoverPolicy
    {
        public static bool CanHover(object localHud,object iconHud,object ownBody,object displaySource,
            bool tabOpen,bool cursorVisible,bool cursorLocked,bool hidden,long stacks)=>
            localHud!=null&&ownBody!=null&&ReferenceEquals(localHud,iconHud)&&ReferenceEquals(ownBody,displaySource)&&
            tabOpen&&cursorVisible&&!cursorLocked&&!hidden&&stacks>0;
    }
}
