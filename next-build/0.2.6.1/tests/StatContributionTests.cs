using System;
using System.Linq;
using Alexander.CharacterSheet.Core;
static class StatContributionTests
{
    static bool Near(double? value,double expected)=>value.HasValue&&Math.Abs(value.Value-expected)<1e-8*Math.Max(1,Math.Abs(expected));
    static CoreStatSnapshot Sample()
    {
        var s=new CoreStatSnapshot {Level=1,InfusionBonus=0,HealthCurseDivisor=1,IsSprinting=false,OpalActive=false,FullCritActive=false,
            NectarActive=false,NectarCounter=0,OutOfDanger=false};
        s.Stats[CoreStatKind.Health]=new StatObservation(100,30,100);
        s.Stats[CoreStatKind.Damage]=new StatObservation(12,2.4,12);
        s.Stats[CoreStatKind.Regen]=new StatObservation(1,.2,1);
        s.Stats[CoreStatKind.Armor]=new StatObservation(20,0,20);
        s.Stats[CoreStatKind.Crit]=new StatObservation(1,0,1);
        foreach(string id in CoreStatContributions.RelevantItemIds)s.Items[id]=new ObservedItemCount(0,0);
        return s;
    }
    static void Count(CoreStatSnapshot s,string id,long permanent,long effective)=>s.Items[id]=new ObservedItemCount(permanent,effective);
    public static void Run(Action<bool,string> assert)
    {
        var s=Sample();var all=CoreStatContributions.Build(s);
        assert(all.Count==5&&all.Values.All(r=>r.MatchesWithinTolerance&&r.Status.Contains("incomplete")),"Five baseline stats reconcile but matching alone never declares complete attribution");
        assert(all.Values.All(r=>r.Residual==0),"Baseline-only reference has zero signed residual");
        s.Level=3;Count(s,"FlatHealth",3,3);Count(s,"Knurl",1,1);Count(s,"Infusion",2,2);s.InfusionBonus=20;
        Count(s,"Pearl",2,2);Count(s,"ShinyPearl",1,1);Count(s,"BoostAllStats",1,1);s.NectarActive=true;s.NectarCounter=4;s.HealthCurseDivisor=2;
        // (100 + 60 level + 75 steak + 40 knurl + 20 cached) x (1 + .2 + .1) x 1.16 / 2 = 222.43
        s.Stats[CoreStatKind.Health].Live=227.43;
        var health=CoreStatContributions.Build(s)[CoreStatKind.Health];
        assert(Near(health.ModeledFinal,222.43)&&Near(health.Residual,5),"Health orders all flat terms before shared additive pool, Nectar and one combined curse divisor");
        assert(health.OrderedTerms.Select(t=>t.Stage).SequenceEqual(health.OrderedTerms.Select(t=>t.Stage).OrderBy(x=>x)),"Health trace preserves declared order rather than regrouping arithmetic by display source");
        assert(health.OrderedTerms.Count(t=>t.Stage==CalculationStage.AdditiveMultiplier)==2&&health.OrderedTerms.First(t=>t.Stage==CalculationStage.AdditiveMultiplier).Before==295,"Pearl pool factors add on one shared base instead of compounding");
        assert(health.Describe(true).Contains("Live - modeled: +5")&&health.Describe(true).Contains("Incomplete attribution"),"Positive unexplained residual remains explicit");
        s.Stats[CoreStatKind.Health].Live=217.43;
        assert(Near(CoreStatContributions.Build(s)[CoreStatKind.Health].Residual,-5),"Negative unexplained residual is preserved");
        Count(s,"LunarDagger",2,2);
        health=CoreStatContributions.Build(s)[CoreStatKind.Health];
        assert(Near(health.ModeledFinal,222.43)&&health.OrderedTerms.Count(t=>t.Stage==CalculationStage.FinalConversion)==1,"Observed combined curse is not double-applied as separate Glass health factors");
        s=Sample();s.Level=3;Count(s,"ShinyPearl",2,2);Count(s,"LunarDagger",2,2);Count(s,"BoostAllStats",1,1);s.NectarActive=true;s.NectarCounter=4;
        foreach(string id in new[]{"FragileDamageBonus","Crowbar","BossDamageBonus","NearbyDamageBonus","Missile","MissileVoid","Behemoth"})Count(s,id,5,5);
        s.Stats[CoreStatKind.Damage].Live=93.5424;var damage=CoreStatContributions.Build(s)[CoreStatKind.Damage];
        assert(Near(damage.ModeledFinal,93.5424)&&damage.MatchesWithinTolerance,"Damage reference has level growth, additive Pearl, independent Glass and late Nectar");
        assert(damage.OrderedTerms.All(t=>!t.Source.Contains("Crowbar")&&!t.Source.Contains("Missile")&&!t.Source.Contains("Watch"))&&damage.Conditions.Count(c=>c.Contains("excluded from raw"))==7,"Per-hit conditions and proc missiles never enter CharacterBody.damage subtotal");
        s=Sample();Count(s,"ShinyPearl",1,1);Count(s,"SprintArmor",2,2);Count(s,"OutOfCombatArmor",1,1);Count(s,"BoostAllStats",1,1);
        s.IsSprinting=true;s.OpalActive=true;s.NectarActive=true;s.NectarCounter=4;s.Stats[CoreStatKind.Armor].Live=211.12;
        var armor=CoreStatContributions.Build(s)[CoreStatKind.Armor];
        assert(Near(armor.ModeledFinal,211.12)&&armor.OrderedTerms.First(t=>t.Stage==CalculationStage.LateFlat).Before==22,"Armor applies early Pearl to baseline before conditional late armor flats");
        s.IsSprinting=false;s.OpalActive=false;s.NectarActive=false;
        assert(Near(CoreStatContributions.Build(s)[CoreStatKind.Armor].ModeledFinal,22),"Inactive sprint/Opal conditions contribute no flat armor");
        s.IsSprinting=null;s.OpalActive=null;
        assert(CoreStatContributions.Build(s)[CoreStatKind.Armor].Unknowns.Count(c=>c.Contains("active condition unavailable"))==2,"Unknown conditional armor states are not guessed active");
        s=Sample();Count(s,"CritGlasses",11,11);Count(s,"ShinyPearl",2,2);Count(s,"AttackSpeedOnCrit",4,4);Count(s,"HealOnCrit",10,10);Count(s,"BleedOnHitAndExplode",4,4);
        s.Stats[CoreStatKind.Crit].Live=146;var crit=CoreStatContributions.Build(s)[CoreStatKind.Crit];
        assert(Near(crit.ModeledFinal,146)&&crit.MatchesWithinTolerance,"Raw crit retains values over 100 and three fixed first-copy five-point terms");
        Count(s,"CritGlassesVoid",1,1);Count(s,"BoostAllStats",1,1);s.FullCritActive=true;s.NectarActive=true;s.NectarCounter=4;s.Stats[CoreStatKind.Crit].Live=0;
        crit=CoreStatContributions.Build(s)[CoreStatKind.Crit];
        assert(crit.ModeledFinal==0&&crit.OrderedTerms.First(t=>t.Stage==CalculationStage.Conversion).Before==246,"Crit conversion removes prior points before Nectar and cannot be restored by later multipliers");
        s=Sample();s.Level=4;Count(s,"Knurl",2,2);Count(s,"ShinyPearl",1,1);Count(s,"BoostAllStats",1,1);s.NectarActive=true;s.NectarCounter=4;s.RawRegenItemScale=1.6;
        s.Stats[CoreStatKind.Regen].Live=8.57472;var regen=CoreStatContributions.Build(s)[CoreStatKind.Regen];
        assert(Near(regen.ModeledFinal,8.57472),"Explicitly supplied raw regen scale applies once before additive/late multipliers");
        s.RawRegenItemScale=null;regen=CoreStatContributions.Build(s)[CoreStatKind.Regen];
        assert(Near(regen.ModeledFinal,2.0416)&&regen.Unknowns.Any(u=>u.Contains("raw level/difficulty placement unregistered"))&&regen.Describe(false).Contains("Titanic Knurl base rate omitted"),"Unknown raw Knurl scaling stays in residual and short hover instead of substituting healing efficiency");
        Count(s,"HealOnCrit",10,10);
        assert(Near(CoreStatContributions.Build(s)[CoreStatKind.Regen].ModeledFinal,2.0416)&&regen.Semantics.Contains("Healing-event"),"Heal-on-crit does not become a raw regen term");
        Count(s,"HealWhileSafe",2,2);s.OutOfDanger=false;s.RawRegenItemScale=1;
        assert(CoreStatContributions.Build(s)[CoreStatKind.Regen].Conditions.Any(c=>c.Contains("inactive while in danger")),"Slug base term requires observed danger state");
        var slugOnly=Sample();Count(slugOnly,"HealWhileSafe",3,3);slugOnly.OutOfDanger=true;slugOnly.Stats[CoreStatKind.Regen].Live=10;
        var slug=CoreStatContributions.Build(slugOnly)[CoreStatKind.Regen];
        assert(slug.ModeledFinal==1&&slug.Residual==9&&slug.Describe(false).Contains("Cautious Slug base rate omitted"),"Active Slug with unknown raw scaling stays visibly omitted with signed residual");
        slugOnly.OutOfDanger=false;slug=CoreStatContributions.Build(slugOnly)[CoreStatKind.Regen];
        assert(slug.ModeledFinal==1&&slug.CoverageNotes.Count==0&&slug.Conditions.Any(c=>c.Contains("inactive while in danger")),"Inactive Slug has no falsely active omitted-rate notice");
        slugOnly.OutOfDanger=null;slug=CoreStatContributions.Build(slugOnly)[CoreStatKind.Regen];
        assert(slug.ModeledFinal==1&&slug.Describe(false).Contains("active danger condition unavailable"),"Unknown Slug activation is visibly omitted rather than guessed active");
        s=Sample();Count(s,"FlatHealth",0,3);s.Stats[CoreStatKind.Health].Live=175;
        assert(CoreStatContributions.Build(s)[CoreStatKind.Health].ModeledFinal==175,"Temporary/effective positive count contributes even with zero permanent copies");
        Count(s,"FlatHealth",5,0);
        var suppressed=CoreStatContributions.Build(s)[CoreStatKind.Health];
        assert(suppressed.ModeledFinal==100&&suppressed.CountNotes.Any(c=>c.Contains("0 effective; item term inactive")),"Disabled/suppressed zero-effective inventory never falls back to permanent count");
        s.Items["FlatHealth"]=new ObservedItemCount(5,null);
        assert(CoreStatContributions.Build(s)[CoreStatKind.Health].ModeledFinal==100&&CoreStatContributions.Build(s)[CoreStatKind.Health].Unknowns.Any(u=>u.Contains("FlatHealth effective count unavailable")),"Unknown effective count creates uncertainty rather than an invented five-copy contribution");
        s.Items["FlatHealth"]=new ObservedItemCount(5,-1);
        assert(CoreStatContributions.Build(s)[CoreStatKind.Health].ModeledFinal==100,"Negative count is invalid and contributes no fabricated negative item bonus");
        s=Sample();Count(s,"BoostAllStats",2,2);s.NectarActive=true;s.NectarCounter=3;s.Stats[CoreStatKind.Health].Live=112;
        assert(Near(CoreStatContributions.Build(s)[CoreStatKind.Health].ModeledFinal,112),"Observed Nectar counter determines current bonus rather than assuming all owned capacity is filled");
        s.NectarActive=false;s.NectarCounter=8;
        assert(CoreStatContributions.Build(s)[CoreStatKind.Health].ModeledFinal==100,"Inactive Nectar does not use stale cached type count");
        s.NectarActive=true;Count(s,"BoostAllStats",2,0);
        assert(CoreStatContributions.Build(s)[CoreStatKind.Health].ModeledFinal==100,"Zero effective Nectar count does not activate item-derived multiplication");
        s=Sample();Count(s,"Infusion",0,0);s.InfusionBonus=200;
        assert(CoreStatContributions.Build(s)[CoreStatKind.Health].ModeledFinal==100&&CoreStatContributions.Build(s)[CoreStatKind.Health].Unknowns.Any(u=>u.Contains("Stored Infusion counter")),"Inactive cached Infusion applicability is explicit rather than guessed from capacity");
        s=Sample();Count(s,"ShieldOnly",1,1);s.Stats[CoreStatKind.Health].Live=1;s.Pools=new ProtectionPools{CurrentHealth=1,MaxHealth=1,CurrentShield=150,MaxShield=150,Barrier=30};
        health=CoreStatContributions.Build(s)[CoreStatKind.Health];
        assert(health.ModeledFinal==null&&health.Residual==null&&health.ModeledSubtotal==100,"Unregistered health-to-shield conversion does not compare a preconversion subtotal as final HP");
        assert(s.Pools.Describe().Contains("Health: 1 / 1")&&s.Pools.Describe().Contains("Shield: 150 / 150")&&s.Pools.Describe().Contains("Barrier: 30")&&!s.Pools.Describe().Contains("181"),"Health, shield and barrier remain separate instead of being summed into max health");
        var unavailablePools=new ProtectionPools{CurrentHealth=1,MaxHealth=1,CurrentShield=null,MaxShield=null,Barrier=0};
        assert(unavailablePools.Describe().Contains("Shield: Unavailable / Unavailable")&&unavailablePools.Describe().Contains("no converted-health source total is inferred"),"Missing shield observations cannot be inferred from health or collapse to zero");
        s=Sample();s.Stats[CoreStatKind.Regen]=new StatObservation(-6,0,-6);s.Stats[CoreStatKind.Armor]=new StatObservation(-20,0,-20);
        all=CoreStatContributions.Build(s);
        assert(all[CoreStatKind.Regen].ModeledFinal==-6&&all[CoreStatKind.Armor].ModeledFinal==-20,"Signed raw regen and negative armor are not silently clamped away");
        s.Stats[CoreStatKind.Damage].Baseline=null;
        assert(CoreStatContributions.Build(s)[CoreStatKind.Damage].ModeledFinal==null&&CoreStatContributions.Build(s)[CoreStatKind.Damage].Live==12,"Missing baseline preserves observed live total with no guessed prediction");
        s.Stats.Remove(CoreStatKind.Armor);
        var absent=CoreStatContributions.Build(s)[CoreStatKind.Armor];
        assert(absent.Live==null&&absent.ModeledFinal==null&&absent.FinalUnavailableReason.Contains("Stat observation unavailable")&&absent.Status.Contains("Live total unavailable"),"Missing whole stat observation preserves explicit unavailable state without a guessed live value");
        s=Sample();s.Stats[CoreStatKind.Health].Live=double.NaN;
        assert(CoreStatContributions.Build(s)[CoreStatKind.Health].Live==null&&CoreStatContributions.Build(s)[CoreStatKind.Health].Residual==null&&CoreStatContributions.Build(s)[CoreStatKind.Health].Status.Contains("Live total unavailable"),"Nonfinite observed total cannot yield a valid reconciliation or an authoritative-live claim");
        s=Sample();Count(s,"LunarDagger",2000,2000);
        assert(CoreStatContributions.Build(s)[CoreStatKind.Damage].ModeledFinal==null,"Overflowing independent multiplier suppresses a final numeric prediction");
        s=Sample();s.Stats[CoreStatKind.Health]=new StatObservation(1000000000,0,1000000000.005);
        health=CoreStatContributions.Build(s)[CoreStatKind.Health];
        assert(health.Residual>0&&health.MatchesWithinTolerance&&health.Describe(true).Contains("Live - modeled: +0.00"),"Tiny large-value residual is retained even inside comparison tolerance");
        assert(health.Describe(true).Contains("\n")&&!health.Describe(true).Contains("\\n"),"Detailed stat trace uses readable multiline text");
    }
}
