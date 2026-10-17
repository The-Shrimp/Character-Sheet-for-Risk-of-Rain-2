using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    public enum CoreStatKind { Health, Damage, Regen, Armor, Crit }
    public sealed class StatObservation
    {
        public double? Baseline,PerLevel,Live;
        public StatObservation(double? baseline,double? perLevel,double? live){Baseline=baseline;PerLevel=perLevel;Live=live;}
    }
    public sealed class ObservedItemCount
    {
        public readonly long? Permanent,Effective;
        public ObservedItemCount(long? permanent,long? effective){Permanent=permanent;Effective=effective;}
    }
    public sealed class ProtectionPools
    {
        public double? CurrentHealth,MaxHealth,CurrentShield,MaxShield,Barrier;
        public string Describe()=>"Health: "+NumberFormatting.Display(CurrentHealth)+" / "+NumberFormatting.Display(MaxHealth)+
            "\nShield: "+NumberFormatting.Display(CurrentShield)+" / "+NumberFormatting.Display(MaxShield)+
            "\nBarrier: "+NumberFormatting.Display(Barrier)+" (temporary protection, not maximum health)";
    }
    public sealed class CoreStatSnapshot
    {
        public double? Level,InfusionBonus,HealthCurseDivisor,RawRegenItemScale;
        public bool? IsSprinting,OpalActive,FullCritActive,NectarActive,OutOfDanger;
        public long? NectarCounter;
        public readonly Dictionary<CoreStatKind,StatObservation> Stats=new Dictionary<CoreStatKind,StatObservation>();
        public readonly Dictionary<string,ObservedItemCount> Items=new Dictionary<string,ObservedItemCount>(StringComparer.Ordinal);
        public ProtectionPools Pools=new ProtectionPools();
    }
    public sealed class ContributionResult
    {
        public readonly CoreStatKind Kind;
        public readonly double? Live,ModeledSubtotal,ModeledFinal,Residual;
        public readonly bool MatchesWithinTolerance;
        public readonly IReadOnlyList<CalculationTerm> OrderedTerms;
        public readonly IReadOnlyList<string> Unknowns,Conditions,CountNotes;
        public readonly string Semantics,FinalUnavailableReason;
        internal ContributionResult(CoreStatKind kind,double? live,double? subtotal,double? final,Calculation calculation,
            IEnumerable<string> unknowns,IEnumerable<string> conditions,IEnumerable<string> counts,string finalReason)
        {
            Kind=kind;Live=Finite(live)?live:null;ModeledSubtotal=Finite(subtotal)?subtotal:null;ModeledFinal=Finite(final)?final:null;
            double difference=Live.HasValue&&ModeledFinal.HasValue?Live.Value-ModeledFinal.Value:double.NaN;
            Residual=NumberFormatting.IsFinite(difference)?(double?)difference:null;
            MatchesWithinTolerance=Residual.HasValue&&Math.Abs(Residual.Value)<=.00001+1e-6*Math.Max(Math.Abs(Live.Value),Math.Abs(ModeledFinal.Value));
            OrderedTerms=Array.AsReadOnly(calculation==null?new CalculationTerm[0]:calculation.Terms.ToArray());
            Unknowns=Array.AsReadOnly(unknowns.Distinct().ToArray());Conditions=Array.AsReadOnly(conditions.Distinct().ToArray());
            CountNotes=Array.AsReadOnly(counts.Distinct().ToArray());FinalUnavailableReason=finalReason;
            Semantics=kind==CoreStatKind.Health?"Maximum health only. Shield/barrier are separate pools.":
                kind==CoreStatKind.Damage?"Raw damage-stat coefficient basis. Skill coefficients, critical damage, proc missiles and conditional hit multipliers are separate.":
                kind==CoreStatKind.Regen?"Raw regeneration stat. Healing-event efficiency, heal-on-hit, percent-health healing and heal suppression are separate.":
                kind==CoreStatKind.Armor?"Raw armor points. Flat damage reduction and the armor damage-reduction curve are separate.":
                "Raw crit percentage points. Capped/luck-adjusted rolls and crit damage are separate.";
        }
        static bool Finite(double? value)=>value.HasValue&&NumberFormatting.IsFinite(value.Value);
        public string Status=>!Live.HasValue?"Live total unavailable; attribution remains incomplete":ModeledFinal.HasValue?
            (MatchesWithinTolerance?"Reviewed subtotal matches live within tolerance; attribution remains incomplete":"Incomplete attribution; live total remains authoritative"):
            "Final prediction unavailable; live total remains authoritative";
        public string Describe(bool full)
        {
            Func<double?,string> f=n=>full&&Finite(n)?NumberFormatting.Exact(n.Value):NumberFormatting.Display(n);
            string unit=Kind==CoreStatKind.Crit?" percentage points":Kind==CoreStatKind.Regen?" /s":Kind==CoreStatKind.Health?" health":"";
            string text=Semantics+"\n"+string.Join("\n",OrderedTerms.Select(t=>t.Stage+" / "+t.Source+": "+f(t.After)+unit+
                (full&&t.Stage!=CalculationStage.Baseline?" ("+f(t.Before)+" -> "+f(t.After)+"; modeled subtotal change "+NumberFormatting.Signed(t.After-t.Before)+")":"")));
            text+="\nReviewed subtotal: "+f(ModeledSubtotal)+unit;
            if(!ModeledFinal.HasValue)text+="\n"+FinalUnavailableReason;
            text+="\nAuthoritative live total: "+f(Live)+unit+"\nLive - modeled: "+(Residual.HasValue?(Residual.Value>0?"+":"")+f(Residual):"Unavailable")+unit+"\n"+Status;
            if(full)
            {
                if(CountNotes.Count>0)text+="\nCount basis: "+string.Join("; ",CountNotes);
                if(Conditions.Count>0)text+="\nConditions: "+string.Join("; ",Conditions);
                text+="\nUnregistered/uncertain: "+string.Join("; ",Unknowns);
                text+="\nOrdering is an explicit reviewed-source reference, not a complete mirror of game/mod hooks. A zero residual does not prove complete attribution.";
            }
            return text;
        }
    }
    public static class CoreStatContributions
    {
        public const string StatApiDocumentation="https://github.com/risk-of-thunder/R2API/blob/master/R2API.RecalculateStats/RecalculateStatsAPI.cs";
        static readonly string[] relevant={
            "FlatHealth","Knurl","Infusion","Pearl","ShinyPearl","LunarDagger","ShieldOnly","HealWhileSafe","SprintArmor","OutOfCombatArmor",
            "CritGlasses","AttackSpeedOnCrit","HealOnCrit","BleedOnHitAndExplode","CritGlassesVoid","BoostAllStats",
            "FragileDamageBonus","Crowbar","BossDamageBonus","NearbyDamageBonus","Missile","MissileVoid","Behemoth"
        };
        public static IReadOnlyList<string> RelevantItemIds=>Array.AsReadOnly(relevant);
        static bool Finite(double? n)=>n.HasValue&&NumberFormatting.IsFinite(n.Value);
        sealed class Builder
        {
            public readonly CoreStatKind Kind;
            public readonly CoreStatSnapshot Input;
            public readonly List<string> Unknowns=new List<string>(),Conditions=new List<string>(),Counts=new List<string>();
            public Calculation Calculation;
            public string FinalReason="";
            public Builder(CoreStatSnapshot input,CoreStatKind kind){Input=input;Kind=kind;}
            public long? Count(string id)
            {
                ObservedItemCount count;
                if(!Input.Items.TryGetValue(id,out count)||count==null||!count.Effective.HasValue||count.Effective.Value<0)
                {Unknowns.Add(id+" effective count unavailable; no permanent-count fallback");return null;}
                if(count.Permanent.HasValue&&count.Permanent.Value>0&&count.Effective.Value==0)
                    Counts.Add(id+": "+count.Permanent.Value+" permanent / 0 effective; item term inactive");
                else if(count.Effective.Value>0)
                    Counts.Add(id+": "+NumberFormatting.Count(count.Permanent)+" permanent / "+count.Effective.Value+" effective");
                return count.Effective.Value;
            }
            public void Flat(string id,string name,double perCopy,CalculationStage stage=CalculationStage.Flat)
            {
                long? n=Count(id);if(n>0)Apply(name+" ("+n+" effective x "+NumberFormatting.Exact(perCopy)+")",stage,perCopy*n.Value);
            }
            public void Pool(string id,string name,double perCopy)
            {
                long? n=Count(id);if(n>0)Apply(name+" additive bonus pool (+"+NumberFormatting.Exact(100*perCopy*n.Value)+"%)",CalculationStage.AdditiveMultiplier,perCopy*n.Value);
            }
            public void FixedFirst(string id,string name,double points)
            {
                long? n=Count(id);if(n>0){Apply(name+" first-copy component (additional copies add no crit points)",CalculationStage.Flat,points);}
            }
            public void Apply(string source,CalculationStage stage,double operand)
            {
                if(Calculation==null)return;
                if(!NumberFormatting.IsFinite(operand)){Unknowns.Add(source+": numeric range exceeded");FinalReason="Numeric range exceeded; no final comparison";return;}
                Calculation.Apply(source,stage,operand);
                if(!NumberFormatting.IsFinite(Calculation.Predicted))FinalReason="Numeric range exceeded; no final comparison";
            }
            public void ConditionalArmor(string id,string name,bool? active,double perCopy)
            {
                long? n=Count(id);if(!(n>0))return;
                if(!active.HasValue){Unknowns.Add(name+" active condition unavailable");return;}
                Conditions.Add(name+(active.Value?" active":" inactive"));
                if(active.Value)Apply(name+" ("+n+" effective)",CalculationStage.LateFlat,perCopy*n.Value);
            }
            public void Nectar()
            {
                long? n=Count("BoostAllStats");if(!(n>0))return;
                if(Input.NectarActive==false){Conditions.Add("Nectar buff inactive; cached count not used");return;}
                if(Input.NectarActive!=true||!Input.NectarCounter.HasValue||Input.NectarCounter.Value<0)
                {Unknowns.Add("Nectar active counter unavailable");return;}
                long counter=Input.NectarCounter.Value;
                Apply("Growth Nectar observed active counter ("+counter+" types, x"+NumberFormatting.Exact(1+.04*counter)+")",
                    CalculationStage.LateMultiplier,1+.04*counter);
                Conditions.Add("Nectar uses its observed active counter, not owned copies x an assumed filled capacity");
            }
            public ContributionResult Result(double? live)
            {
                double? subtotal=Calculation!=null&&NumberFormatting.IsFinite(Calculation.Predicted)?(double?)Calculation.Predicted:null;
                return new ContributionResult(Kind,live,subtotal,FinalReason.Length==0?subtotal:null,Calculation,Unknowns,Conditions,Counts,FinalReason);
            }
        }
        public static IReadOnlyDictionary<CoreStatKind,ContributionResult> Build(CoreStatSnapshot snapshot)
        {
            if(snapshot==null)throw new ArgumentNullException(nameof(snapshot));
            var results=new Dictionary<CoreStatKind,ContributionResult>();
            foreach(CoreStatKind kind in Enum.GetValues(typeof(CoreStatKind)))results[kind]=BuildOne(snapshot,kind);
            return results;
        }
        static ContributionResult BuildOne(CoreStatSnapshot input,CoreStatKind kind)
        {
            var b=new Builder(input,kind);StatObservation observed;
            b.Unknowns.Add("Other items/buffs, stat hooks, cached payouts and unregistered conversions");
            if(!input.Stats.TryGetValue(kind,out observed)||observed==null)return b.Result(null);
            if(!Finite(observed.Baseline)||!Finite(observed.PerLevel)||!Finite(input.Level))
            {b.FinalReason="Baseline/level inputs unavailable; no invented subtotal";return b.Result(observed.Live);}
            double growth=observed.PerLevel.Value*Math.Max(0,input.Level.Value-1);
            if(!NumberFormatting.IsFinite(growth)){b.FinalReason="Level growth numeric range exceeded";return b.Result(observed.Live);}
            b.Calculation=new Calculation("Observed survivor baseline",observed.Baseline.Value);
            b.Apply("Level growth ("+NumberFormatting.Exact(observed.PerLevel.Value)+" x max(0, level - 1))",CalculationStage.Flat,growth);
            if(kind==CoreStatKind.Health)
            {
                b.Flat("FlatHealth","Bison Steak flat health",25);
                b.Flat("Knurl","Titanic Knurl flat health",40);
                long? infusion=b.Count("Infusion");
                if(infusion>0)
                {
                    if(Finite(input.InfusionBonus)&&input.InfusionBonus>=0)b.Apply("Infusion observed stored health points (not copy-count capacity)",CalculationStage.Flat,input.InfusionBonus.Value);
                    else b.Unknowns.Add("Infusion cached points unavailable");
                }
                else if(input.InfusionBonus>0)b.Unknowns.Add("Stored Infusion counter with no effective item: applicability unregistered");
                b.Pool("Pearl","Pearl",.1);b.Pool("ShinyPearl","Irradiant Pearl",.1);
            }
            else if(kind==CoreStatKind.Damage)
            {
                b.Pool("ShinyPearl","Irradiant Pearl",.1);
                long? glass=b.Count("LunarDagger");
                if(glass>0)b.Apply("Shaped Glass independent damage multiplier (2^"+glass+")",CalculationStage.Multiplier,Math.Pow(2,glass.Value));
                foreach(string id in new[]{"FragileDamageBonus","Crowbar","BossDamageBonus","NearbyDamageBonus","Missile","MissileVoid","Behemoth"})
                    if(b.Count(id)>0)b.Conditions.Add(id+" effect belongs to hit/proc context; excluded from raw damage-stat subtotal");
            }
            else if(kind==CoreStatKind.Regen)
            {
                foreach(string id in new[]{"Knurl","HealWhileSafe"})
                {
                    long? n=b.Count(id);if(!(n>0))continue;
                    bool active=id=="Knurl"||input.OutOfDanger==true;
                    if(id=="HealWhileSafe"&&!input.OutOfDanger.HasValue){b.Unknowns.Add("Cautious Slug danger state unavailable");continue;}
                    if(!active){b.Conditions.Add("Cautious Slug inactive while in danger");continue;}
                    double rate=id=="Knurl"?1.6:3;
                    if(Finite(input.RawRegenItemScale)&&input.RawRegenItemScale>=0)
                        b.Apply((id=="Knurl"?"Titanic Knurl":"Cautious Slug")+" base-rate term with explicitly supplied raw-stat scaling",CalculationStage.Flat,rate*n.Value*input.RawRegenItemScale.Value);
                    else b.Unknowns.Add((id=="Knurl"?"Titanic Knurl":"Cautious Slug")+" base-rate reference "+NumberFormatting.Exact(rate*n.Value)+" /s: raw level/difficulty placement unregistered; omitted from subtotal");
                }
                b.Pool("ShinyPearl","Irradiant Pearl",.1);
                b.Conditions.Add("No healing-event efficiency factor is inferred from raw regen or difficulty");
            }
            else if(kind==CoreStatKind.Armor)
            {
                long? pearl=b.Count("ShinyPearl");
                if(pearl>0)b.Apply("Irradiant Pearl early armor factor",CalculationStage.Multiplier,1+.1*pearl.Value);
                b.ConditionalArmor("SprintArmor","Rose Buckler sprint armor",input.IsSprinting,30);
                b.ConditionalArmor("OutOfCombatArmor","Oddly-shaped Opal ready armor",input.OpalActive,100);
                b.Conditions.Add("Armor Plate is flat incoming damage reduction, not armor; no Plate term added");
            }
            else if(kind==CoreStatKind.Crit)
            {
                b.Flat("CritGlasses","Lens-Maker's Glasses raw crit points",10);
                b.Flat("ShinyPearl","Irradiant Pearl raw crit points",10);
                b.FixedFirst("AttackSpeedOnCrit","Predatory Instincts",5);
                b.FixedFirst("HealOnCrit","Harvester's Scythe",5);
                b.FixedFirst("BleedOnHitAndExplode","Shatterspleen",5);
                if(input.FullCritActive==true)b.Apply("Ocular HUD active FullCrit component",CalculationStage.Flat,100);
                else if(!input.FullCritActive.HasValue)b.Unknowns.Add("FullCrit buff state unavailable");
                if(b.Count("CritGlassesVoid")>0)b.Apply("Lost Seer's Lenses remove raw crit before later multipliers",CalculationStage.Conversion,0);
                b.Conditions.Add("No 100% cap applied to raw stat points; probability/luck remains a separate scoped calculation");
            }
            b.Nectar();
            if(kind==CoreStatKind.Health)
            {
                if(Finite(input.HealthCurseDivisor)&&input.HealthCurseDivisor>0)
                {
                    if(input.HealthCurseDivisor!=1)b.Apply("Observed combined health curse divisor (single application)",CalculationStage.FinalConversion,1/input.HealthCurseDivisor.Value);
                    b.Conditions.Add("Combined observed curse is applied once; Glass health reduction is not duplicated as a second multiplier");
                }
                else b.Unknowns.Add("Health curse divisor unavailable; final conversion remains in residual");
                if(b.Count("ShieldOnly")>0)
                {
                    b.FinalReason="Transcendence final health/shield conversion unregistered; shown subtotal is pre-conversion, not predicted final HP";
                    b.Unknowns.Add("Transcendence capacity/conversion order, health floor and shield partition");
                }
                b.Unknowns.Add("Health floor, other health-to-shield conversions and cached growth terms");
            }
            return b.Result(observed.Live);
        }
    }
}
