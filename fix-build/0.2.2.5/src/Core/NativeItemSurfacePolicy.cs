using System;
namespace Alexander.CharacterSheet.Core
{
    public static class NativeItemSurfacePolicy
    {
        public static bool OwnSurface(object localMaster,object scoreboardMaster,object hudMaster,
            bool inScoreboard,bool inTopHud)=>
            localMaster!=null&&(inScoreboard?ReferenceEquals(localMaster,scoreboardMaster):
                inTopHud&&ReferenceEquals(localMaster,hudMaster));
        public static bool KnownLuck(string mark)=>mark=="x"||mark=="mix"||mark=="\u2713"||mark=="!";
        public static bool HasAddition(string details,string luckMark)=>
            !string.IsNullOrWhiteSpace(details)||KnownLuck(luckMark);
    }
}
