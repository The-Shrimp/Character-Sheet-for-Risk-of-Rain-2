using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    // Original descriptions of named public buff facts; no external mod implementation is included.
    public sealed class BuffStackComponent
    {
        public readonly string Label;
        public readonly double PerStack;
        public BuffStackComponent(string label,double perStack){Label=label;PerStack=perStack;}
    }
    public sealed class OriginalBuffDefinition
    {
        public readonly string Id,PublicId,Name,Effect,StackMeaning,Conditions,Source,Revision,Confidence;
        public readonly IReadOnlyList<string> Aliases;
        readonly BuffStackComponent[] components;
        internal OriginalBuffDefinition(string id,string publicId,string name,string effect,string stackMeaning,
            string conditions,int sourceLine,params BuffStackComponent[] components)
        {
            Id=id;PublicId=publicId;Name=name;Effect=effect;StackMeaning=stackMeaning;Conditions=conditions;
            Source=OriginalBuffDefinitions.Source+"#L"+sourceLine;Revision=OriginalBuffDefinitions.Revision;
            Confidence="Named public source snapshot; installed behavior unverified. No remaining-duration prediction.";
            Aliases=Array.AsReadOnly(new[]{id,publicId,publicId.Substring(publicId.LastIndexOf('.')+1)}.Distinct().ToArray());
            this.components=components;
        }
        public string Kind
        {
            get
            {
                switch(Id)
                {
                    case "ImmuneToDebuff":return "Ready charges";
                    case "ImmuneToDebuffConsumed":return "Recharge state";
                    case "BearVoidCooldown":case "KnockBackUnavailable":return "Cooldown state";
                    case "BearVoidReady":case "KnockBackAvailable":case "OutOfCombatArmorBuff":return "Ready state";
                    case "DLC2Content.Buffs.BoostAllStatsBuff":return "Type counter";
                    case "DLC2Content.Buffs.ExtraStatsOnLevelUpBuff":return "Stored level counter";
                    case "JumpDamageStrikeCharge":return "Charge counter";
                    case "MedkitHeal":return "Pending heal";
                    case "ElusiveAntlersSpeedBuff":case "SpeedOnPickup":case "AttackSpeedOnCrit":return "Active stat stacks";
                    default:return "Active state";
                }
            }
        }
        public string Describe(long stacks)
        {
            if(stacks<0)return "Observed count invalid; active effect unavailable.\nReference effect: "+Effect;
            if(stacks==0)return "State inactive (0 observed stacks); no active effect predicted.\nReference effect: "+Effect;
            string text=Effect;
            foreach(var c in components)
            {
                double value=c.PerStack*stacks;
                text+="\nObserved active-stack "+c.Label+" component: "+NumberFormatting.Display(value)+"%; not complete final stat attribution.";
            }
            return text+"\nStack meaning: "+StackMeaning+"\nConditions: "+Conditions;
        }
    }
    public static class OriginalBuffDefinitions
    {
        public const string Revision=OriginalItemDefinitions.Revision;
        public const string Source="https://github.com/Wet-Boys/LookingGlass/blob/"+Revision+"/LookingGlass/BuffDescriptions/BuffDefinitions.cs";
        static OriginalBuffDefinition D(string id,string publicId,string name,string effect,string meaning,string conditions,
            int line,params BuffStackComponent[] components)=>new OriginalBuffDefinition(id,publicId,name,effect,meaning,conditions,line,components);
        static readonly OriginalBuffDefinition[] definitions={
            D("RoR2Content.Buffs.FullCrit","RoR2Content.Buffs.FullCrit","Ocular HUD critical buff",
                "Reference +100 raw crit points; later conversions and final damage remain separate.",
                "Active crit state; observed count is not item copies.",
                "Active buff required. Raw points are distinct from capped/luck-adjusted rolls.",96),
            D("DLC2Content.Buffs.BoostAllStatsBuff","DLC2Content.Buffs.BoostAllStatsBuff","Growth Nectar counter",
                "Qualifying active buff-type counter; public bonus reference is 4% per counted type.",
                "Counter of unique qualifying TYPES, not the sum of their stacks.",
                "Own live current/max counters remain authoritative. Eligibility uses observed metadata; this counter excludes itself.",210),
            D("DLC2Content.Buffs.ExtraStatsOnLevelUpBuff","DLC2Content.Buffs.ExtraStatsOnLevelUpBuff","Prayer Beads stored level gains",
                "Stored level-gain counter, not a live percent-stat buff. Current-copy storage-rate reference does not certify cached removal payout.",
                "Stored level-gain units; not current health, damage or percent-current-stats.",
                "Payout/removal and cached copies are separate; no retroactive payout is predicted.",207),
            D("ElusiveAntlersSpeedBuff","DLC2Content.Buffs.ElusiveAntlersBuff","Elusive Antlers speed",
                "Movement component from collected Antlers orbs.",
                "Active movement stacks, not held item copies or buff-type count.",
                "Only observed active stacks are modeled. Nominal lifetime is separate; remaining duration unavailable. Nectar follows live flags.",203,
                new BuffStackComponent("movement bonus",12)),
            D("SpeedOnPickup","DLC3Content.Buffs.SpeedOnPickup","Collector's Compulsion",
                "Pickup-triggered stat-bonus component; individual final stat order remains incomplete.",
                "Active pickup-effect stacks, not maximum item capacity.",
                "Current buff count is separate from held item count/capacity; remaining duration unavailable.",233,
                new BuffStackComponent("stat-bonus",3)),
            D("ImmuneToDebuff","DLC1Content.Buffs.ImmuneToDebuffReady","Ben's Raincoat ready charges",
                "Ready state for preventing an eligible temporary debuff and granting its documented barrier component.",
                "Ready prevention charges; not armor, probability, or general invulnerability.",
                "No blanket protection claim for freeze/permanent/cooldown/Tonic Affliction. Nectar uses this state's live metadata.",159),
            D("ImmuneToDebuffConsumed","DLC1Content.Buffs.ImmuneToDebuffCooldown","Ben's Raincoat recharge state",
                "Consumed prevention/recharge state; it does not represent an always-active defense multiplier; remaining duration unavailable.",
                "Consumed/recharge state count; not a certified remaining timer.",
                "Recharging does not imply a ready prevention charge; remaining duration unavailable. Nectar metadata is assessed separately.",160),
            D("Energized","RoR2Content.Buffs.Energized","War Horn energized",
                "Fixed public attack-speed bonus reference: 70% while active.",
                "Active energized state; observed count does not multiply the fixed reference.",
                "Equipment-triggered effect; item duration scaling is separate from this instance's remaining timer.",71),
            D("BearVoidReady","DLC1Content.Buffs.BearVoidReady","Safer Spaces ready",
                "Ready damage-negation state for a next eligible damage event.",
                "Ready state; not a random Tougher Times chance or one charge per item copy.",
                "Damage-event eligibility still matters; activation transitions to cooldown. Nectar uses live metadata.",163),
            D("BearVoidCooldown","DLC1Content.Buffs.BearVoidCooldown","Safer Spaces cooldown",
                "Damage negation is recharging; no ready block is inferred from this state.",
                "Cooldown/state count; not a certified remaining-time readout.",
                "Nominal item cooldown scaling does not establish this instance's timer. Nectar uses live metadata.",164),
            D("OutOfCombatArmorBuff","DLC1Content.Buffs.OutOfCombatArmorBuff","Oddly-shaped Opal ready armor",
                "Active conditional armor protection from Opal.",
                "Ready armor state; buff count is not the number of effective Opal copies.",
                "Public armor component is copy-dependent; activity does not certify incoming-damage reduction or recovery time.",155),
            D("Warbanner","RoR2Content.Buffs.Warbanner","Warbanner",
                "Fixed public bonus reference: 30% attack speed and 30% movement speed while active.",
                "Active banner effect; observed count does not multiply its fixed reference.",
                "Current presence establishes only this buff state; final speed and remaining duration are separate.",56),
            D("JumpDamageStrikeCharge","DLC3Content.Buffs.JumpDamageStrikeCharge","Faraday charge",
                "Stored Faraday charge components for movement and jump height.",
                "Charge counter; not item copies, seconds, or predicted explosion damage.",
                "Only observed charge components are shown; future charge generation/consumption and final movement are not predicted.",232,
                new BuffStackComponent("movement bonus",1.6),new BuffStackComponent("jump-height bonus",2)),
            D("KnockBackAvailable","DLC2Content.Buffs.KnockBackAvailable","Breaching Fin ready",
                "Fin's launch effect is ready subject to its eligible-hit conditions.",
                "Ready state; not a guaranteed launch count or damage bonus.",
                "Target/hit conditions are not inspected; no automatic attack or guaranteed proc is inferred.",205),
            D("KnockBackUnavailable","DLC2Content.Buffs.KnockBackUnavailable","Breaching Fin cooldown",
                "Fin's launch effect is recharging rather than ready.",
                "Cooldown/state count; not elapsed or remaining seconds.",
                "Public nominal cooldown reference is 15 seconds; remaining duration unavailable.",206),
            D("TeslaField","RoR2Content.Buffs.TeslaField","Unstable Tesla Coil active",
                "Active periodic shock effect; hit/proc context and target counts remain separate.",
                "Active field state; not shocks dealt, target count or a DPS multiplier.",
                "No enemy/target data is accessed; remaining active/cooldown interval unavailable.",80),
            D("MedkitHeal","RoR2Content.Buffs.MedkitHeal","Medkit pending heal",
                "Pending delayed heal event; no immediate healing or raw regen bonus is inferred.",
                "Pending-heal state; not restored HP, owned copies or remaining seconds.",
                "Public nominal delay is two seconds; actual heal modifiers and remaining duration unavailable.",54),
            D("WhipBoost","RoR2Content.Buffs.WhipBoost","Red Whip active",
                "Conditional movement boost associated with Red Whip.",
                "Active movement state; buff count is not effective item count.",
                "Public bonus depends on held effective copies; no magnitude is inferred from this status count.",63),
            D("AttackSpeedOnCrit","RoR2Content.Buffs.AttackSpeedOnCrit","Predatory Instincts attack speed",
                "Attack-speed component built by eligible critical-hit triggers.",
                "Active attack-speed stacks, not owned items or additional crit chance.",
                "Observed stacks do not guarantee future critical hits, cap capacity or remaining lifetime.",65,
                new BuffStackComponent("attack-speed bonus",12))
        };
        static readonly Dictionary<string,OriginalBuffDefinition> aliases=BuildAliases();
        static Dictionary<string,OriginalBuffDefinition> BuildAliases()
        {
            var result=new Dictionary<string,OriginalBuffDefinition>(StringComparer.Ordinal);
            foreach(var definition in definitions)
                foreach(string alias in definition.Aliases)result.Add(alias,definition);
            return result;
        }
        public static IReadOnlyList<OriginalBuffDefinition> SupportedDefinitions=>Array.AsReadOnly(definitions.OrderBy(d=>d.PublicId,StringComparer.Ordinal).ToArray());
        public static OriginalBuffDefinition Find(string publicId)
        {
            OriginalBuffDefinition result;if(publicId==null)return null;
            if(publicId.StartsWith("bd",StringComparison.Ordinal)&&publicId.IndexOf('.')<0)publicId=publicId.Substring(2);
            return aliases.TryGetValue(publicId,out result)?result:null;
        }
        public static double? BeadStoredLevelUnits(long? counter,long? effectiveCopies)
        {
            if(!counter.HasValue||!effectiveCopies.HasValue||counter.Value<0||effectiveCopies.Value<=0)return null;
            double value=counter.Value*(.20+.05*(effectiveCopies.Value-1));
            return NumberFormatting.IsFinite(value)?(double?)value:null;
        }
    }
}
