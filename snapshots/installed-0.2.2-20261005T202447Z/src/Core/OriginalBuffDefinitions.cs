using System;
using System.Collections.Generic;
namespace Alexander.CharacterSheet.Core
{
    public sealed class OriginalBuffDefinition
    {
        public readonly string Id,Name,Effect,Source,Confidence;
        readonly double? perStackBonus;
        internal OriginalBuffDefinition(string id,string name,string effect,string source,string confidence,double? perStackBonus=null)
        {Id=id;Name=name;Effect=effect;Source=source;Confidence=confidence;this.perStackBonus=perStackBonus;}
        public string Describe(long stacks)=>Effect+(perStackBonus.HasValue&&stacks>=0?"\nObserved active-stack bonus component: "+NumberFormatting.Display(perStackBonus.Value*stacks)+"%; not complete final stat attribution.":"");
    }
    public static class OriginalBuffDefinitions
    {
        const string PublicConfidence="Public definition snapshot; installed behavior unverified. No remaining-duration prediction.";
        static readonly Dictionary<string,OriginalBuffDefinition> definitions=new Dictionary<string,OriginalBuffDefinition>(StringComparer.Ordinal)
        {
            ["RoR2Content.Buffs.FullCrit"]=new OriginalBuffDefinition("RoR2Content.Buffs.FullCrit","Guaranteed critical strikes",
                "Adds 100 crit percentage points before later conversions. This does not certify final damage or conversion behavior.",
                "Earlier reviewed component recorded in docs/OwnSourceAudit.md; no new game implementation inspection.",PublicConfidence),
            ["DLC2Content.Buffs.BoostAllStatsBuff"]=new OriginalBuffDefinition("DLC2Content.Buffs.BoostAllStatsBuff","Growth Nectar counter",
                "Tracks qualifying active buff TYPES, not individual stacks. Public per-type bonus component is 4%; capacity grows by four types per effective Nectar copy. The counter excludes itself.",
                OriginalItemDefinitions.Source+"#L1513",PublicConfidence),
            ["DLC2Content.Buffs.ExtraStatsOnLevelUpBuff"]=new OriginalBuffDefinition("DLC2Content.Buffs.ExtraStatsOnLevelUpBuff","Prayer Beads stored level gains",
                "A stored level-gain counter, not a live percent-stat buff. Reference stored units = counter x [0.20 + 0.05 x (effective copies - 1)]. Payout requires removal and cached state; current copies do not certify a retroactive payout.",
                OriginalItemDefinitions.Source+"#L909",PublicConfidence),
            ["ElusiveAntlersSpeedBuff"]=new OriginalBuffDefinition("ElusiveAntlersSpeedBuff","Elusive Antlers speed",
                "Collected-orb movement buff: 12% movement bonus per active stack, at most three stacks per effective Antlers copy. Documented nominal lifetime: 12 seconds; remaining duration unavailable. One active type for Nectar, independently of stack count.",
                OriginalItemDefinitions.SupplementarySource("SpeedBoostPickup"),PublicConfidence,12),
            ["SpeedOnPickup"]=new OriginalBuffDefinition("SpeedOnPickup","Collector's Compulsion",
                "Pickup-triggered bonus component: 3% per active buff stack, capped at two per effective copy plus one. Documented nominal lifetime: 20 seconds; remaining duration unavailable. Individual final stat application is not fully attributed.",
                OriginalItemDefinitions.SupplementarySource("SpeedOnPickup"),PublicConfidence,3),
            ["ImmuneToDebuff"]=new OriginalBuffDefinition("ImmuneToDebuff","Ben's Raincoat ready charges",
                "Ready charges prevent eligible temporary debuffs; maximum one charge per effective Raincoat copy. Each blocked debuff grants a documented 10% max-health barrier component. Does not claim freeze protection or blanket permanent/cooldown/Tonic Affliction immunity. Nectar eligibility uses this state's live metadata.",
                OriginalItemDefinitions.SupplementarySource("ImmuneToDebuff"),PublicConfidence),
            ["ImmuneToDebuffConsumed"]=new OriginalBuffDefinition("ImmuneToDebuffConsumed","Ben's Raincoat recharge state",
                "Consumed-charge/recharge state, not an always-active defense multiplier. Documented recharge is five seconds after charges are exhausted; remaining duration unavailable. Nectar eligibility is read for this state separately.",
                OriginalItemDefinitions.SupplementarySource("ImmuneToDebuff"),PublicConfidence),
            ["Energized"]=new OriginalBuffDefinition("Energized","War Horn energized",
                "Equipment-triggered attack-speed buff. Public War Horn duration is 8 seconds plus 4 per additional effective copy; buff magnitude is not quantitatively registered. A nominal item duration does not reveal this instance's remaining timer.",
                OriginalItemDefinitions.Source+"#L1205",PublicConfidence)
        };
        public static OriginalBuffDefinition Find(string publicId)
        {OriginalBuffDefinition result;if(publicId==null)return null;
            if(publicId.StartsWith("bd",StringComparison.Ordinal))publicId=publicId.Substring(2);
            return definitions.TryGetValue(publicId,out result)?result:null;}
        public static double? BeadStoredLevelUnits(long? counter,long? effectiveCopies)
        {
            if(!counter.HasValue||!effectiveCopies.HasValue||counter.Value<0||effectiveCopies.Value<=0)return null;
            double value=counter.Value*(.20+.05*(effectiveCopies.Value-1));
            return NumberFormatting.IsFinite(value)?(double?)value:null;
        }
    }
}
