using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    public sealed class NectarBuffObservation
    {
        public string Id;
        public long? Stacks;
        public bool? Hidden,DefinitionIgnored,CatalogIgnored,SelfCounter;
    }
    public sealed class NectarCounterView
    {
        public readonly long? EligibleTypes,ReferenceCap,Used,ObservedCap;
        public readonly double? BonusPercent;
        public readonly bool Discrepancy;
        public readonly string Compact,Details;
        internal NectarCounterView(long? eligible,long? referenceCap,long? used,long? observedCap,double? bonus,
            bool discrepancy,string compact,string details)
        {EligibleTypes=eligible;ReferenceCap=referenceCap;Used=used;ObservedCap=observedCap;BonusPercent=bonus;
         Discrepancy=discrepancy;Compact=compact;Details=details;}
    }
    public static class NectarCounterPresentation
    {
        public const double PercentPerType=4;
        public static long? EligibleTypes(IEnumerable<NectarBuffObservation> observations)
        {
            if(observations==null)return null;
            var unique=new Dictionary<string,NectarBuffObservation>(StringComparer.Ordinal);
            foreach(var entry in observations)
            {
                if(entry==null||string.IsNullOrEmpty(entry.Id))return null;
                NectarBuffObservation prior;
                if(unique.TryGetValue(entry.Id,out prior))
                {
                    if(prior.Stacks!=entry.Stacks||prior.Hidden!=entry.Hidden||prior.DefinitionIgnored!=entry.DefinitionIgnored||
                        prior.CatalogIgnored!=entry.CatalogIgnored||prior.SelfCounter!=entry.SelfCounter)return null;
                }
                else unique.Add(entry.Id,entry);
            }
            long count=0;
            foreach(var entry in unique.Values)
            {
                if(entry.Stacks==0||entry.Hidden==true||entry.DefinitionIgnored==true||entry.CatalogIgnored==true||entry.SelfCounter==true)continue;
                if(!entry.Stacks.HasValue||entry.Stacks<0||!entry.Hidden.HasValue||!entry.DefinitionIgnored.HasValue||
                    !entry.CatalogIgnored.HasValue||!entry.SelfCounter.HasValue)return null;
                count++;
            }
            return count;
        }
        static long? Valid(long? value)=>value.HasValue&&value>=0?value:null;
        static string Count(long? value)=>value.HasValue?NumberFormatting.Count(value):"Unknown";
        public static NectarCounterView Create(long? copies,IEnumerable<NectarBuffObservation> observations,
            long? observedCount,long? observedCap,bool? counterActive,bool formulaVerified)
        {
            long? eligible=EligibleTypes(observations),cap=Valid(observedCap),raw=Valid(observedCount);
            long? referenceCap=formulaVerified&&copies.HasValue&&copies>=0&&copies<=long.MaxValue/4?copies*4:null;
            long? used=raw.HasValue&&cap.HasValue?Math.Min(raw.Value,cap.Value):(long?)null;
            bool discrepancy=(referenceCap.HasValue&&cap.HasValue&&referenceCap!=cap)||
                (eligible.HasValue&&cap.HasValue&&used.HasValue&&Math.Min(eligible.Value,cap.Value)!=used.Value);
            double? bonus=null;
            if(formulaVerified&&referenceCap.HasValue&&cap==referenceCap&&used.HasValue&&counterActive.HasValue)
                bonus=counterActive.Value?PercentPerType*used.Value:0;
            string compact="Effective buffs: "+Count(used)+" / "+Count(cap);
            if(!eligible.HasValue)compact+="\nEligible types: Unknown";
            else if(cap.HasValue&&eligible>cap)compact+="\nEligible types: "+Count(eligible)+" (over cap)";
            compact+="\nNectar bonus: "+(bonus.HasValue?NumberFormatting.Signed(bonus)+"%"+(counterActive==false?" (inactive)":""):"Unknown");
            if(discrepancy)compact+="\nCounter/model differs; game counter shown.";
            string details="Distinct active eligible buff types: "+Count(eligible)+
                "\nObserved game counter: "+Count(raw)+" / "+Count(cap)+
                "\nReference capacity (4 x effective copies): "+Count(referenceCap)+
                "\nNectar counter buff active: "+(counterActive.HasValue?(counterActive.Value?"yes":"no"):"Unknown")+
                "\nEligibility excludes hidden definitions, definition/catalog ignoreGrowthNectar flags and Nectar's own counter. A type counts once, independently of stacks."+
                "\nBonus is the public 4%-per-used-type Nectar component when its counter buff is active; it is not full final stat attribution."+
                "\nGame observations remain authoritative. Unavailable metadata, formula or activation is labeled Unknown.";
            return new NectarCounterView(eligible,referenceCap,used,cap,bonus,discrepancy,compact,details);
        }
    }
}
