using System;
namespace Alexander.CharacterSheet.Core
{
    public static class NativeHudPlacementPolicy
    {
        // An owned child must be below an existing HUD branch, never in HUD's selected sibling list.
        public static bool CanAttach(int nativeParentDepth,bool parentVisible,bool containsScoreboard)=>
            nativeParentDepth>=1&&parentVisible&&containsScoreboard;
    }
    public sealed class NativeInfoActivationGate
    {
        object local,hud;
        bool blocked,warned;
        public bool CaptureOpen(object nextLocal,object nextHud,bool infoHeld,bool freshPress)
        {
            if(nextLocal==null||nextHud==null)return false;
            if(!ReferenceEquals(local,nextLocal)||!ReferenceEquals(hud,nextHud))
            {Reset();local=nextLocal;hud=nextHud;}
            if(freshPress||!infoHeld){blocked=false;warned=false;}
            return !blocked;
        }
        public bool Allows(object nextLocal,object nextHud)=>nextLocal!=null&&nextHud!=null&&
            (local==null||!ReferenceEquals(local,nextLocal)||!ReferenceEquals(hud,nextHud)||!blocked);
        public void NativeClosed(object currentLocal,object currentHud)
        {
            if(currentLocal==null||currentHud==null)return;
            if(local==null){local=currentLocal;hud=currentHud;}
            if(ReferenceEquals(local,currentLocal)&&ReferenceEquals(hud,currentHud))blocked=true;
        }
        public bool TakeWarning(){if(!blocked||warned)return false;warned=true;return true;}
        public void Reset(){local=hud=null;blocked=warned=false;}
    }
}
