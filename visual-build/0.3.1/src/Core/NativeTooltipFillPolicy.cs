using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    public sealed class NativeTooltipFillCandidate
    {
        public readonly bool CoversBody,BodyAncestor,NamedFill;
        public readonly double Alpha,Area;
        public NativeTooltipFillCandidate(bool coversBody,bool bodyAncestor,bool namedFill,double alpha,double area)
        {CoversBody=coversBody;BodyAncestor=bodyAncestor;NamedFill=namedFill;Alpha=alpha;Area=area;}
    }
    public static class NativeTooltipFillPolicy
    {
        // Select among observed native references. Never synthesize color or opacity.
        public static int Select(IReadOnlyList<NativeTooltipFillCandidate> candidates)
        {
            if(candidates==null)return -1;
            return Enumerable.Range(0,candidates.Count)
                .Where(i=>candidates[i]!=null&&candidates[i].CoversBody&&NumberFormatting.IsFinite(candidates[i].Alpha)&&
                    candidates[i].Alpha>.05&&candidates[i].Alpha<=1&&NumberFormatting.IsFinite(candidates[i].Area)&&candidates[i].Area>0)
                .OrderByDescending(i=>candidates[i].NamedFill).ThenByDescending(i=>candidates[i].BodyAncestor)
                .ThenByDescending(i=>candidates[i].Alpha).ThenByDescending(i=>candidates[i].Area)
                .Select(i=>(int?)i).FirstOrDefault()??-1;
        }
    }
}
