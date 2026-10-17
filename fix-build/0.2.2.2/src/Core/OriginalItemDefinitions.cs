using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    // Independent definitions of public mathematical facts. No source mod implementation is included.
    public sealed class ItemComponentDefinition
    {
        readonly double first,additional;
        readonly ValueUnit unit;
        readonly string verb,label,formula,conditions;
        readonly Func<long,double> evaluate;
        internal ItemComponentDefinition(double first,double additional,ValueUnit unit,string verb,string label,
            string conditions="",Func<long,double> evaluate=null,string formula=null)
        {this.first=first;this.additional=additional;this.unit=unit;this.verb=verb;this.label=label;
         this.conditions=conditions;this.evaluate=evaluate;this.formula=formula;}
        internal ItemTooltipView Create(OriginalItemDefinition owner,string name,long? copies)=>ItemTooltipPresentation.Create(new ItemScalingInput {
            Name=name,Verb=verb,BaseValue=first,PerAdditionalCopy=additional,OwnedCopies=copies,Unit=unit,ValueLabel=label,
            ComponentAtCopies=evaluate,ComponentFormula=formula,Evidence=owner.Evidence,
            CopySingular=owner.Id=="ExtraStatsOnLevelUp"?"bead":"item",CopyPlural=owner.Id=="ExtraStatsOnLevelUp"?"beads":"items",
            Conditions=owner.Notes+" "+conditions,Luck=owner.Luck,LuckDetail=owner.LuckScope
        });
    }
    public sealed class OriginalItemDefinition
    {
        public readonly string Id,Group,Source,Revision,Notes,TriggerContext,LuckScope;
        public readonly PredictionEvidence Evidence;
        public readonly LuckApplicability Luck;
        readonly ItemComponentDefinition[] components;
        internal OriginalItemDefinition(string id,string group,LuckApplicability luck,string luckScope,string notes,
            string trigger,params ItemComponentDefinition[] components)
        {
            Id=id;Group=group;Luck=luck;LuckScope=luckScope;Notes=notes;TriggerContext=trigger;
            Source=OriginalItemDefinitions.Source;Revision=OriginalItemDefinitions.Revision;
            Evidence=PredictionEvidence.ConditionalEstimate;this.components=components;
        }
        public ItemTooltipView Create(string localizedName,long? effectiveCopies)=>components[0].Create(this,localizedName,effectiveCopies);
        public IReadOnlyList<ItemTooltipView> CreateTerms(string localizedName,long? effectiveCopies,long? eligibleBuffTypes=null)
        {
            var terms=components.Select(c=>c.Create(this,localizedName,effectiveCopies)).ToArray();
            if(Id=="BoostAllStats"&&eligibleBuffTypes.HasValue&&eligibleBuffTypes.Value>=0)
            {
                long types=eligibleBuffTypes.Value;
                terms[1]=ItemTooltipPresentation.Create(new ItemScalingInput {
                    Name=localizedName,Verb="Modeled active stat-bonus component:",OwnedCopies=effectiveCopies,Unit=ValueUnit.Percent,
                    ComponentAtCopies=n=>4*Math.Min((double)types,4.0*n),
                    ComponentFormula="4% x min("+NumberFormatting.Count(types)+" observed eligible types, 4 x copies)",
                    ValueLabel="at the same active buff-type set",Evidence=Evidence,Luck=Luck,LuckDetail=LuckScope,
                    Conditions="Public snapshot model; fixed "+NumberFormatting.Count(types)+" observed qualifying types. No activation prediction. Game counter remains authoritative; stat order/conversions remain separate."
                });
            }
            return Array.AsReadOnly(terms);
        }
        public string SupplementarySource=>OriginalItemDefinitions.SupplementarySource(Id);
        public int ComponentCount=>components.Length;
    }
    public static class OriginalItemDefinitions
    {
        public const string Revision="4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf";
        public const string Source="https://github.com/Wet-Boys/LookingGlass/blob/"+Revision+"/LookingGlass/ItemStats/ItemDefinitions.cs";
        const string Snapshot="Public definition snapshot; installed build/mod balance not verified. One extra effective copy with the stated conditions held fixed. Component only, not full attribution or DPS.";
        const string StatLuck="Only the displayed stat/capacity/time terms are classified: they are not luck rolls. Other effects require their own scope.";
        const string MixedProc="Luck affects the eligible trigger roll, not displayed damage/count/range terms. Attack proc coefficient, proc-chain exclusions and other modifiers require context.";
        const string CritLuck="The additive crit-stat term is unaffected by luck; standard critical rolls are affected. A raw term is not final crit chance; caps/conversions can make next-copy benefit differ.";
        static ItemComponentDefinition Term(double first,double extra,ValueUnit unit,string verb,string label,string conditions="")=>
            new ItemComponentDefinition(first,extra,unit,verb,label,conditions);
        static ItemComponentDefinition Curve(ValueUnit unit,string verb,string label,Func<long,double> evaluator,string formula,string conditions="")=>
            new ItemComponentDefinition(0,0,unit,verb,label,conditions,evaluator,formula);
        static OriginalItemDefinition Define(string id,string group,LuckApplicability luck,string luckScope,string notes,string trigger,
            params ItemComponentDefinition[] terms)=>new OriginalItemDefinition(id,group,luck,luckScope,Snapshot+" "+notes,trigger,terms);
        static readonly Dictionary<string,OriginalItemDefinition> definitions=Build();
        static Dictionary<string,OriginalItemDefinition> Build()
        {
            var d=new Dictionary<string,OriginalItemDefinition>(StringComparer.Ordinal);
            d["Syringe"]=Define("Syringe","Stat bonuses",LuckApplicability.Unaffected,StatLuck,
                "Attack-speed bonus pool, not a promise of the same percent change in final attack speed.","",
                Term(15,15,ValueUnit.Percent,"Attack-speed bonus:","before other attack-speed terms"));
            d["Hoof"]=Define("Hoof","Stat bonuses",LuckApplicability.Unaffected,StatLuck,
                "Movement-speed bonus pool; survivor/sprint/buffs and conversions are separate.","",
                Term(14,14,ValueUnit.Percent,"Movement-speed bonus:","before other movement terms"));
            d["AttackSpeedAndMoveSpeed"]=Define("AttackSpeedAndMoveSpeed","Stat bonuses",LuckApplicability.Unaffected,StatLuck,
                "Two separate additive bonus pools. Final movement/attack speed is not inferred.","",
                Term(7.5,7.5,ValueUnit.Percent,"Attack-speed bonus:",""),
                Term(7,7,ValueUnit.Percent,"Movement-speed bonus:",""));
            d["CritGlasses"]=Define("CritGlasses","On hit",LuckApplicability.Mixed,CritLuck,
                "Raw crit component before conversions. Standard roll caps at 100%; other sources and conversion items are separate.","",
                Term(10,10,ValueUnit.PercentagePoints,"Adds","to the raw crit component"));
            d["FlatHealth"]=Define("FlatHealth","Defense / sustain",LuckApplicability.Unaffected,StatLuck,
                "Flat term before shared scaling and conversions, not the final max-health difference.","",
                Term(25,25,ValueUnit.Health,"Adds","before shared scaling and conversions"));
            d["PersonalShield"]=Define("PersonalShield","Defense / sustain",LuckApplicability.Unaffected,StatLuck,
                "Shield-capacity fraction of the applicable maximum-health basis. Health basis/conversions/recharge behavior are not modeled.","",
                Term(8,8,ValueUnit.Percent,"Shield-capacity term:","of the applicable max-health basis"));
            d["ArmorPlate"]=Define("ArmorPlate","Defense / sustain",LuckApplicability.Unaffected,StatLuck,
                "Flat incoming-damage reduction term, not armor points. The game's minimum damage floor and damage ordering can reduce realized benefit.","",
                Term(5,5,ValueUnit.None,"Flat damage reduction:","per eligible incoming damage event"));
            d["FragileDamageBonus"]=Define("FragileDamageBonus","Conditional damage",LuckApplicability.Unaffected,StatLuck,
                "Only the unbroken effective item contributes. Broken watch state, low-health break rules and other damage multipliers are separate.","",
                Term(20,20,ValueUnit.Percent,"Unbroken damage bonus:","in the relevant bonus pool"));
            d["Bear"]=Define("Bear","Defense / sustain",LuckApplicability.Unaffected,
                "The public block definition uses an independent block-chance curve rather than luck adjustment; this exception is scoped to Tougher Times.",
                "Diminishing returns. This chance does not certify blockability of a particular damage event; not Tougher Times Void/Safer Spaces.","",
                Curve(ValueUnit.Percent,"Block-chance component:","before event eligibility",n=>100*(.15*n)/(1+.15*n),
                    "100 x (0.15 x copies) / (1 + 0.15 x copies)"));
            d["Missile"]=Define("Missile","On hit",LuckApplicability.Mixed,MixedProc,
                "Same eligible triggering hit; not a total damage or DPS prediction.",
                "Reference trigger chance: 10% at proc coefficient 1, before luck. Actual attack proc coefficient/chance: unavailable. Additional copies scale missile damage, not base trigger chance.",
                Term(300,300,ValueUnit.Percent,"Missile damage:","of the triggering hit's damage component"));
            d["MissileVoid"]=Define("MissileVoid","On hit",LuckApplicability.Unaffected,
                "The shield-gated trigger is deterministic for an eligible hit; the displayed missile coefficient is not a luck roll. Secondary procs have separate scope.",
                "Requires active shield and an eligible nonzero-proc hit. Shield loss, proc-chain restrictions and other missile modifiers are separate.",
                "No random base trigger roll is modeled. Secondary missile proc coefficient in the public snapshot: 0.2; it is not the attack's proc coefficient.",
                Term(40,40,ValueUnit.Percent,"Missile damage:","of the triggering hit's damage component"));
            d["ChainLightning"]=Define("ChainLightning","On hit",LuckApplicability.Mixed,MixedProc,
                "A triggered chain's limits, not guaranteed target hits or damage. Suitable targets and proc-chain eligibility required.",
                "Reference trigger: 25% at attack proc coefficient 1 before luck; unchanged by copies. Actual attack chance unavailable. Secondary proc coefficient reference: 0.2.",
                Term(3,2,ValueUnit.None,"Maximum chain targets:","per triggered chain"),
                Term(20,2,ValueUnit.Meters,"Chain reach:","per hop in the public definition"));
            d["ChainLightningVoid"]=Define("ChainLightningVoid","On hit",LuckApplicability.Mixed,MixedProc,
                "Maximum hits in a triggered same-target strike sequence; target survival/eligibility can reduce realized hits.",
                "Reference trigger: 25% at attack proc coefficient 1 before luck; unchanged by copies. Actual attack chance unavailable. Secondary proc coefficient reference: 0.2.",
                Term(3,3,ValueUnit.None,"Maximum strikes:","per triggered sequence"));
            d["BleedOnHit"]=Define("BleedOnHit","On hit",LuckApplicability.Affected,
                "Luck affects the eligible bleed trigger roll after attack proc scaling. The displayed proc-1 reference has zero luck; actual hit context is unavailable.",
                "Capped reference is for attack proc coefficient 1, luck 0. Lower proc coefficients can need more copies; proc-chain exclusions still apply. Bleed damage/duration is not modeled.",
                "Reference only: actual trigger chance requires attack proc coefficient and luck. Do not interpret a 100% proc-1 reference as guaranteed bleed on every attack.",
                Curve(ValueUnit.Percent,"Bleed reference chance:","at attack proc coefficient 1, luck 0",
                    n=>Math.Min(100,10.0*n),"min(100%, 10% x copies)"));
            d["BleedOnHitVoid"]=Define("BleedOnHitVoid","On hit",LuckApplicability.Affected,
                "Luck affects the eligible collapse trigger roll after attack proc scaling. The displayed proc-1 reference has zero luck; actual hit context is unavailable.",
                "Capped reference is for attack proc coefficient 1, luck 0. Collapse damage/delay, corruption and proc-chain exclusions are separate.",
                "Reference only: actual trigger chance requires attack proc coefficient and luck. Additional copies increase the base reference chance, not the collapse damage term.",
                Curve(ValueUnit.Percent,"Collapse reference chance:","at attack proc coefficient 1, luck 0",
                    n=>Math.Min(100,10.0*n),"min(100%, 10% x copies)"));
            d["Behemoth"]=Define("Behemoth","On hit",LuckApplicability.Unaffected,StatLuck,
                "Radius only. Extra copies do not imply extra per-target damage; explosion eligibility and falloff are separate.","",
                Term(4,2.5,ValueUnit.Meters,"Explosion radius:","radius component only"));
            d["ImmuneToDebuff"]=Define("ImmuneToDebuff","Defense / sustain",LuckApplicability.Unaffected,StatLuck,
                "Maximum debuff-prevention charges; current ready/consumed state is separate. Barrier/recharge are fixed per event, not increased by copies. Does not promise protection from freeze, permanent debuffs, cooldown statuses or Tonic Affliction.","",
                Term(1,1,ValueUnit.None,"Maximum prevention charges:","not current ready charges"),
                Term(10,0,ValueUnit.Percent,"Barrier per blocked debuff:","of maximum health","Fixed documented component; not actual current barrier."),
                Term(5,0,ValueUnit.Seconds,"Exhausted-charge recharge:","after blocks are exhausted","Documented nominal timer, not remaining cooldown."));
            d["SpeedBoostPickup"]=Define("SpeedBoostPickup","Conditional movement",LuckApplicability.Unaffected,StatLuck,
                "Elusive Antlers snapshot, not the old Antler Shield retaliation effect. Full-buff maximum needs collected orbs; current active buff count and timers are separate. Documented nominal buff duration: 12 seconds.","",
                Term(3,3,ValueUnit.None,"Maximum Antlers buff stacks:",""),
                Term(36,36,ValueUnit.Percent,"Maximum movement bonus:","when all allowed stacks are active","Public per-buff movement term: 12%."),
                Curve(ValueUnit.Seconds,"Orb interval:","nominal generation interval",n=>2+24.0/(n+2),
                    "2 s + 24 s / (copies + 2)","Nominal interval only; actual scheduling/availability not inspected."));
            d["EnergizedOnEquipmentUse"]=Define("EnergizedOnEquipmentUse","On equipment use",LuckApplicability.Unaffected,StatLuck,
                "War Horn effect duration after equipment activation, not remaining duration or sustained uptime. Buff magnitude remains a separate term.","",
                Term(8,4,ValueUnit.Seconds,"Triggered buff duration:","per activation"));
            d["SpeedOnPickup"]=Define("SpeedOnPickup","On pickup",LuckApplicability.Unaffected,StatLuck,
                "Collector's Compulsion capacity/full-stack component, not a passive bonus. Documented active bonus is 3% per buff stack for 20 seconds after eligible pickups (including Antlers orbs). Individual stat order remains incompletely attributed.","",
                Term(3,2,ValueUnit.None,"Maximum Compulsion buff stacks:","capacity, not current active stacks"),
                Term(9,6,ValueUnit.Percent,"Maximum Compulsion bonus component:","when all allowed stacks are active"));
            d["ExtraStatsOnLevelUp"]=Define("ExtraStatsOnLevelUp","Stat growth",LuckApplicability.Unaffected,StatLuck,
                "Storage rate per stored level-gain unit. Stored counter times this rate measures level-gain units, NOT a percentage of current character stats. Cached payout/removal state is separate.","",
                Term(20,5,ValueUnit.Percent,"Stores","of a level-up stat gain per stored unit"));
            d["BoostAllStats"]=Define("BoostAllStats","Conditional stat bonus",LuckApplicability.Unaffected,StatLuck,
                "Capacity only: qualifying active types can be fewer than capacity. Maximum bonus assumes full capacity; next actual benefit needs uncapped qualifying-type context. Applies to the documented health/regen/damage/attack speed/movement/crit/armor terms, not acceleration. Stat order/conversions remain separate.","",
                Term(4,4,ValueUnit.None,"Qualifying buff-type capacity:","types, not individual buff stacks"),
                Term(16,16,ValueUnit.Percent,"Maximum stat-bonus component:","if all capacity is filled","Public per-qualifying-type term: 4%."));
            d["ShockDamageAura"]=Define("ShockDamageAura","Periodic buff",LuckApplicability.Unaffected,
                "The periodic pulse, displayed stat buff terms and guaranteed electric crit condition are not random luck rolls. Other effects retain separate scope.",
                "Documented pulse buff lasts 7 seconds: +30% attack/movement speed and electric attacks crit; documented ally range 45 m. Ideal continuous coverage assumes each pulse reaches the subject and refreshes the buff. Buff ID/remaining timer not registered.",
                "Source-qualified exponential interval, not the wiki's inconsistent hyperbolic label. Five copies first cover the 7-second window under ideal assumptions; four leave gaps.",
                Curve(ValueUnit.Seconds,"Pulse interval:","nominal repeat interval",ConductorInterval,"15 s x 0.8^(copies - 1)"),
                Curve(ValueUnit.Percent,"Ideal buff uptime:","assuming every pulse refreshes the 7-second buff",ConductorUptime,
                    "100% x min(1, 7 s / pulse interval)"));
            return d;
        }
        static double ConductorInterval(long copies)
        {
            double interval=15*Math.Pow(.8,copies-1);
            return NumberFormatting.IsFinite(interval)&&interval>0?interval:double.NaN;
        }
        static double ConductorUptime(long copies)
        {
            double interval=ConductorInterval(copies);
            return NumberFormatting.IsFinite(interval)?100*Math.Min(1,7/interval):double.NaN;
        }
        public static string SupplementarySource(string id)
        {
            switch(id)
            {
                case "ExtraStatsOnLevelUp":return "https://riskofrain2.wiki.gg/wiki/Prayer_Beads";
                case "BoostAllStats":return "https://riskofrain2.wiki.gg/wiki/Growth_Nectar";
                case "SpeedBoostPickup":return "https://riskofrain2.wiki.gg/wiki/Elusive_Antlers";
                case "SpeedOnPickup":return "https://riskofrain2.wiki.gg/wiki/Collector%27s_Compulsion";
                case "ImmuneToDebuff":return "https://riskofrain2.wiki.gg/wiki/Ben%27s_Raincoat";
                case "ShockDamageAura":return "https://riskofrain2.wiki.gg/wiki/Faulty_Conductor";
                default:return "";
            }
        }
        public static IReadOnlyList<string> SupportedIds=>Array.AsReadOnly(definitions.Keys.OrderBy(x=>x,StringComparer.Ordinal).ToArray());
        public static OriginalItemDefinition Find(string id){OriginalItemDefinition result;return id!=null&&definitions.TryGetValue(id,out result)?result:null;}
        // Pure conditional reference, never called with invented attack context by the game adapter.
        public static double? EstimateAttackRoll(string id,long? copies,double? procCoefficient,double? luck)
        {
            if(!copies.HasValue||copies.Value<=0||!procCoefficient.HasValue||!luck.HasValue||
                !NumberFormatting.IsFinite(procCoefficient.Value)||procCoefficient.Value<0||!NumberFormatting.IsFinite(luck.Value))return null;
            double basis;
            switch(id)
            {
                case "Missile": basis=.1;break;
                case "ChainLightning":case "ChainLightningVoid":basis=.25;break;
                case "BleedOnHit":case "BleedOnHitVoid":basis=.1*copies.Value;break;
                default:return null;
            }
            double raw=basis*procCoefficient.Value;
            return NumberFormatting.IsFinite(raw)?(double?)LuckMath.EffectiveChance(raw,luck.Value):null;
        }
    }
}
