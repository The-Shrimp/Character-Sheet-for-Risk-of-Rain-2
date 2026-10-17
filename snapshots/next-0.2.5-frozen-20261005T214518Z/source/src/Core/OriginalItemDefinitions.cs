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

            d["Knurl"]=Define("Knurl","Defense / sustain",LuckApplicability.Unaffected,StatLuck,
                "Flat health before shared scaling. Regen is a base-rate reference; raw stat level/difficulty placement and healing-event efficiency are separate.","",
                Term(40,40,ValueUnit.Health,"Flat health:","before shared scaling and conversions"),
                Term(1.6,1.6,ValueUnit.None,"Base regen rate:","HP/s before unregistered raw-stat scaling"));
            d["Pearl"]=Define("Pearl","Defense / sustain",LuckApplicability.Unaffected,StatLuck,
                "Health additive bonus pool. Not a guarantee of this percent change in final health after other pools/conversions.","",
                Term(10,10,ValueUnit.Percent,"Health bonus pool:","before independent multipliers/conversions"));
            d["ShinyPearl"]=Define("ShinyPearl","Stat bonuses",LuckApplicability.Mixed,CritLuck,
                "Separate health/damage/regen bonus pools, raw crit points and an early armor factor. Each component has its own order; not a blanket multiplier of every final stat. Attack/movement components are not registered here.","",
                Term(10,10,ValueUnit.Percent,"Health bonus pool:",""),
                Term(10,10,ValueUnit.Percent,"Damage bonus pool:","raw damage stat, not hit damage"),
                Term(10,10,ValueUnit.Percent,"Regen bonus pool:","raw regen, not healing efficiency"),
                Term(10,10,ValueUnit.PercentagePoints,"Raw crit contribution:","before conversions"),
                Term(10,10,ValueUnit.Percent,"Early armor bonus factor:","before later conditional flat armor"));
            d["SprintArmor"]=Define("SprintArmor","Defense / sustain",LuckApplicability.Unaffected,StatLuck,
                "Rose Buckler armor applies only while sprinting. Not a passive armor bonus or guaranteed incoming-damage reduction.","",
                Term(30,30,ValueUnit.None,"Conditional armor:","armor points while sprinting"));
            d["OutOfCombatArmor"]=Define("OutOfCombatArmor","Defense / sustain",LuckApplicability.Unaffected,StatLuck,
                "Oddly-shaped Opal armor applies only with its active ready buff. Owned capacity does not imply that the buff is ready; remaining cooldown unavailable.","",
                Term(100,100,ValueUnit.None,"Conditional armor:","armor points while the Opal ready buff is active"));
            d["AttackSpeedOnCrit"]=Define("AttackSpeedOnCrit","On hit",LuckApplicability.Mixed,CritLuck,
                "Predatory Instincts adds crit points only on the first copy. Maximum attack-speed component requires all allowed triggered buff stacks; current active buff stacks/duration are separate.","Eligible critical hits are a trigger condition, not a guaranteed attack-speed bonus.",
                Term(5,0,ValueUnit.PercentagePoints,"First-copy raw crit:","additional copies add no crit points"),
                Term(36,24,ValueUnit.Percent,"Maximum triggered attack-speed bonus:","if all allowed buff stacks are active"));
            d["HealOnCrit"]=Define("HealOnCrit","Defense / sustain",LuckApplicability.Mixed,CritLuck,
                "Harvester's Scythe adds crit points only on the first copy. Healing is an eligible critical-hit event at reference proc coefficient 1; not regeneration. Actual proc/healing efficiency and suppression are separate.","Requires an eligible critical hit; actual attack proc coefficient unavailable.",
                Term(5,0,ValueUnit.PercentagePoints,"First-copy raw crit:","additional copies add no crit points"),
                Term(8,4,ValueUnit.Health,"Reference heal per critical hit:","at proc coefficient 1, before healing-event modifiers"));
            d["BleedOnHitAndExplode"]=Define("BleedOnHitAndExplode","On kill / on hit",LuckApplicability.Mixed,
                "The raw crit term is not luck-adjusted; standard critical rolls are. Conditional bleed and kill explosion have separate trigger scope; no explosion roll or final damage is predicted.",
                "Shatterspleen adds crit points only on the first copy. A qualifying kill explosion has two separate damage coefficients. Hidden target health is never read; percentages are reference components only.","Requires the documented eligible bleed/kill context; proc-chain, falloff and other damage terms are separate.",
                Term(5,0,ValueUnit.PercentagePoints,"First-copy raw crit:","additional copies add no crit points"),
                Term(400,400,ValueUnit.Percent,"Kill explosion coefficient:","of the character base-damage basis"),
                Term(15,15,ValueUnit.Percent,"Additional explosion coefficient:","of target maximum health; no target value is accessed"));
            d["Infusion"]=Define("Infusion","On kill",LuckApplicability.Unaffected,StatLuck,
                "Capacity is not current health gained. The stat sheet uses the observed stored Infusion bonus; it never grants maximum capacity from copy count. Remaining capacity can limit eligible kill gains.","Eligible kill and remaining capacity required; no future kill count is assumed.",
                Term(100,100,ValueUnit.Health,"Stored-health capacity:","not the current stored bonus"),
                Term(1,1,ValueUnit.Health,"Reference stored gain per kill:","subject to remaining capacity"));
            d["LunarDagger"]=Define("LunarDagger","Stat conversion",LuckApplicability.Unaffected,StatLuck,
                "Shaped Glass has independent exponential damage and health-reference factors. Health reference is not final HP; floors, shield conversions and combined curse placement are separate. The stat model applies observed combined curse once rather than duplicating this health factor.","",
                Curve(ValueUnit.None,"Independent damage factor:","multiplicative raw damage component",n=>Math.Pow(2,n),"2^copies"),
                Curve(ValueUnit.Percent,"Remaining health-basis reference:","not predicted final HP",n=>100*Math.Pow(.5,n),"100% x 0.5^copies"));
            d["HealWhileSafe"]=Define("HealWhileSafe","Defense / sustain",LuckApplicability.Unaffected,StatLuck,
                "Cautious Slug base-rate reference applies only while out of danger. Raw regen level/difficulty placement is unregistered and is omitted from the live subtotal; heal efficiency is separate.","",
                Term(3,3,ValueUnit.None,"Conditional base regen rate:","HP/s while out of danger, before unregistered raw-stat scaling"));
            d["Crowbar"]=Define("Crowbar","Conditional damage",LuckApplicability.Unaffected,StatLuck,
                "Bonus applies to eligible hits on sufficiently healthy targets. Hit-context term, not CharacterBody raw damage. Target state is not inspected.","Eligible high-health target condition held fixed; no target health is accessed.",
                Term(75,75,ValueUnit.Percent,"Eligible hit bonus:","within the Crowbar hit-damage factor"));
            d["BossDamageBonus"]=Define("BossDamageBonus","Conditional damage",LuckApplicability.Unaffected,StatLuck,
                "Armor-Piercing Rounds affects eligible boss hits. Hit-context term, not raw damage or a guaranteed total DPS change.","Eligible boss-hit condition held fixed; no enemy classification is accessed.",
                Term(20,20,ValueUnit.Percent,"Eligible boss-hit bonus:","within the boss-hit damage factor"));
            d["NearbyDamageBonus"]=Define("NearbyDamageBonus","Conditional damage",LuckApplicability.Unaffected,StatLuck,
                "Focus Crystal affects eligible nearby-target hits. Hit-context term, not raw damage. Distance, proc-chain and other multipliers are separate.","Eligible nearby-hit condition held fixed; no target position/distance is accessed.",
                Term(20,20,ValueUnit.Percent,"Eligible nearby-hit bonus:","within the nearby-hit damage factor"));
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
