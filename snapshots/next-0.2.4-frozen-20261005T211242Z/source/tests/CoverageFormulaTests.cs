using System;
using Alexander.CharacterSheet.Core;
static class CoverageFormulaTests
{
    static bool Near(double? actual,double expected)=>actual.HasValue&&Math.Abs(actual.Value-expected)<1e-7*Math.Max(1,Math.Abs(expected));
    // Fixed expected values are independent of implementation evaluation, including nonlinear terms.
    static void Samples(Action<bool,string> assert,string id,double[][] current,double[][] next)
    {
        long[] counts={1,4,10};var definition=OriginalItemDefinitions.Find(id);
        for(int i=0;i<counts.Length;i++)
        {
            var terms=definition.CreateTerms(id,counts[i]);bool valid=terms.Count==current[i].Length;
            for(int j=0;valid&&j<terms.Count;j++)
                valid=Near(terms[j].CurrentValue,current[i][j])&&Near(terms[j].NextValue,next[i][j])&&
                    terms[j].Comparison.Confidence.Contains("Conditional")&&terms[j].NextLabel.Contains("Next copy:")&&
                    !terms[j].FullMath.Contains("\\n")&&terms[j].FullMath.Contains("\n");
            assert(valid,id+" current/next components and readable math at "+counts[i]+" effective copies");
        }
    }
    public static void Run(Action<bool,string> assert)
    {
        Samples(assert,"Syringe",new[]{new[]{15.0},new[]{60.0},new[]{150.0}},new[]{new[]{30.0},new[]{75.0},new[]{165.0}});
        Samples(assert,"Hoof",new[]{new[]{14.0},new[]{56.0},new[]{140.0}},new[]{new[]{28.0},new[]{70.0},new[]{154.0}});
        Samples(assert,"AttackSpeedAndMoveSpeed",new[]{new[]{7.5,7.0},new[]{30.0,28.0},new[]{75.0,70.0}},new[]{new[]{15.0,14.0},new[]{37.5,35.0},new[]{82.5,77.0}});
        Samples(assert,"CritGlasses",new[]{new[]{10.0},new[]{40.0},new[]{100.0}},new[]{new[]{20.0},new[]{50.0},new[]{110.0}});
        Samples(assert,"FlatHealth",new[]{new[]{25.0},new[]{100.0},new[]{250.0}},new[]{new[]{50.0},new[]{125.0},new[]{275.0}});
        Samples(assert,"PersonalShield",new[]{new[]{8.0},new[]{32.0},new[]{80.0}},new[]{new[]{16.0},new[]{40.0},new[]{88.0}});
        Samples(assert,"ArmorPlate",new[]{new[]{5.0},new[]{20.0},new[]{50.0}},new[]{new[]{10.0},new[]{25.0},new[]{55.0}});
        Samples(assert,"FragileDamageBonus",new[]{new[]{20.0},new[]{80.0},new[]{200.0}},new[]{new[]{40.0},new[]{100.0},new[]{220.0}});
        Samples(assert,"Bear",new[]{new[]{13.043478260869565},new[]{37.5},new[]{60.0}},new[]{new[]{23.076923076923077},new[]{42.85714285714286},new[]{62.26415094339623}});
        Samples(assert,"Missile",new[]{new[]{300.0},new[]{1200.0},new[]{3000.0}},new[]{new[]{600.0},new[]{1500.0},new[]{3300.0}});
        Samples(assert,"MissileVoid",new[]{new[]{40.0},new[]{160.0},new[]{400.0}},new[]{new[]{80.0},new[]{200.0},new[]{440.0}});
        Samples(assert,"ChainLightning",new[]{new[]{3.0,20.0},new[]{9.0,26.0},new[]{21.0,38.0}},new[]{new[]{5.0,22.0},new[]{11.0,28.0},new[]{23.0,40.0}});
        Samples(assert,"ChainLightningVoid",new[]{new[]{3.0},new[]{12.0},new[]{30.0}},new[]{new[]{6.0},new[]{15.0},new[]{33.0}});
        Samples(assert,"BleedOnHit",new[]{new[]{10.0},new[]{40.0},new[]{100.0}},new[]{new[]{20.0},new[]{50.0},new[]{100.0}});
        Samples(assert,"BleedOnHitVoid",new[]{new[]{10.0},new[]{40.0},new[]{100.0}},new[]{new[]{20.0},new[]{50.0},new[]{100.0}});
        Samples(assert,"Behemoth",new[]{new[]{4.0},new[]{11.5},new[]{26.5}},new[]{new[]{6.5},new[]{14.0},new[]{29.0}});
        Samples(assert,"ImmuneToDebuff",new[]{new[]{1.0,10.0,5.0},new[]{4.0,10.0,5.0},new[]{10.0,10.0,5.0}},new[]{new[]{2.0,10.0,5.0},new[]{5.0,10.0,5.0},new[]{11.0,10.0,5.0}});
        Samples(assert,"SpeedBoostPickup",new[]{new[]{3.0,36.0,10.0},new[]{12.0,144.0,6.0},new[]{30.0,360.0,4.0}},new[]{new[]{6.0,72.0,8.0},new[]{15.0,180.0,5.428571428571429},new[]{33.0,396.0,3.8461538461538463}});
        Samples(assert,"EnergizedOnEquipmentUse",new[]{new[]{8.0},new[]{20.0},new[]{44.0}},new[]{new[]{12.0},new[]{24.0},new[]{48.0}});
        Samples(assert,"SpeedOnPickup",new[]{new[]{3.0,9.0},new[]{9.0,27.0},new[]{21.0,63.0}},new[]{new[]{5.0,15.0},new[]{11.0,33.0},new[]{23.0,69.0}});
        Samples(assert,"ExtraStatsOnLevelUp",new[]{new[]{20.0},new[]{35.0},new[]{65.0}},new[]{new[]{25.0},new[]{40.0},new[]{70.0}});
        Samples(assert,"BoostAllStats",new[]{new[]{4.0,16.0},new[]{16.0,64.0},new[]{40.0,160.0}},new[]{new[]{8.0,32.0},new[]{20.0,80.0},new[]{44.0,176.0}});
        Samples(assert,"ShockDamageAura",new[]{new[]{15.0,46.666666666666664},new[]{7.68,91.14583333333333},new[]{2.01326592,100.0}},new[]{new[]{12.0,58.33333333333333},new[]{6.144,100.0},new[]{1.610612736,100.0}});


        Samples(assert,"Knurl",new[]{new[]{40.0,1.6},new[]{160.0,6.4},new[]{400.0,16.0}},new[]{new[]{80.0,3.2},new[]{200.0,8.0},new[]{440.0,17.6}});
        Samples(assert,"Pearl",new[]{new[]{10.0},new[]{40.0},new[]{100.0}},new[]{new[]{20.0},new[]{50.0},new[]{110.0}});
        Samples(assert,"ShinyPearl",new[]{new[]{10.0,10.0,10.0,10.0,10.0},new[]{40.0,40.0,40.0,40.0,40.0},new[]{100.0,100.0,100.0,100.0,100.0}},new[]{new[]{20.0,20.0,20.0,20.0,20.0},new[]{50.0,50.0,50.0,50.0,50.0},new[]{110.0,110.0,110.0,110.0,110.0}});
        Samples(assert,"SprintArmor",new[]{new[]{30.0},new[]{120.0},new[]{300.0}},new[]{new[]{60.0},new[]{150.0},new[]{330.0}});
        Samples(assert,"OutOfCombatArmor",new[]{new[]{100.0},new[]{400.0},new[]{1000.0}},new[]{new[]{200.0},new[]{500.0},new[]{1100.0}});
        Samples(assert,"AttackSpeedOnCrit",new[]{new[]{5.0,36.0},new[]{5.0,108.0},new[]{5.0,252.0}},new[]{new[]{5.0,60.0},new[]{5.0,132.0},new[]{5.0,276.0}});
        Samples(assert,"HealOnCrit",new[]{new[]{5.0,8.0},new[]{5.0,20.0},new[]{5.0,44.0}},new[]{new[]{5.0,12.0},new[]{5.0,24.0},new[]{5.0,48.0}});
        Samples(assert,"BleedOnHitAndExplode",new[]{new[]{5.0,400.0,15.0},new[]{5.0,1600.0,60.0},new[]{5.0,4000.0,150.0}},new[]{new[]{5.0,800.0,30.0},new[]{5.0,2000.0,75.0},new[]{5.0,4400.0,165.0}});
        Samples(assert,"Infusion",new[]{new[]{100.0,1.0},new[]{400.0,4.0},new[]{1000.0,10.0}},new[]{new[]{200.0,2.0},new[]{500.0,5.0},new[]{1100.0,11.0}});
        Samples(assert,"LunarDagger",new[]{new[]{2.0,50.0},new[]{16.0,6.25},new[]{1024.0,.09765625}},new[]{new[]{4.0,25.0},new[]{32.0,3.125},new[]{2048.0,.048828125}});
        Samples(assert,"HealWhileSafe",new[]{new[]{3.0},new[]{12.0},new[]{30.0}},new[]{new[]{6.0},new[]{15.0},new[]{33.0}});
        Samples(assert,"Crowbar",new[]{new[]{75.0},new[]{300.0},new[]{750.0}},new[]{new[]{150.0},new[]{375.0},new[]{825.0}});
        Samples(assert,"BossDamageBonus",new[]{new[]{20.0},new[]{80.0},new[]{200.0}},new[]{new[]{40.0},new[]{100.0},new[]{220.0}});
        Samples(assert,"NearbyDamageBonus",new[]{new[]{20.0},new[]{80.0},new[]{200.0}},new[]{new[]{40.0},new[]{100.0},new[]{220.0}});
        foreach(var id in new[]{"AttackSpeedOnCrit","HealOnCrit","BleedOnHitAndExplode"})
        {
            var terms=OriginalItemDefinitions.Find(id).CreateTerms(id,4);
            assert(terms[0].Comparison.Direction==ValueDirection.NoImmediateChange&&terms[1].NextValue>terms[1].CurrentValue,
                id+" next copy changes its other component while first-copy-only raw crit remains fixed");
        }
        var glass=OriginalItemDefinitions.Find("LunarDagger").CreateTerms("Glass",1);
        assert(glass[0].NextValue==4&&glass[1].NextValue==25&&glass[1].Comparison.Direction==ValueDirection.Decrease,
            "Glass shows distinct higher damage and lower health-basis reference, without predicting final HP");
        assert(OriginalItemDefinitions.Find("LunarDagger").CreateTerms("Glass",2000)[0].CurrentValue==null,
            "Exponential damage overflow is unavailable, not a finite next-copy promise");
        assert(OriginalItemDefinitions.Find("Infusion").Notes.Contains("not current health gained")&&
            OriginalItemDefinitions.Find("Infusion").CreateTerms("Infusion",4)[0].Content.Sections.Count>0,
            "Infusion capacity is explicitly separated from observed stored health");
        assert(OriginalItemDefinitions.Find("Knurl").Notes.Contains("raw stat level/difficulty")&&
            OriginalItemDefinitions.Find("HealWhileSafe").Notes.Contains("omitted from the live subtotal"),
            "Known Knurl/Slug base-rate components do not claim an uncertain raw regeneration total");
        assert(OriginalItemDefinitions.Find("HealOnCrit").Notes.Contains("not regeneration")&&
            OriginalItemDefinitions.Find("HealOnCrit").TriggerContext.Contains("proc coefficient unavailable"),
            "Scythe heal-event reference does not masquerade as raw regen or known attack context");
        assert(OriginalItemDefinitions.Find("ShinyPearl").ComponentCount==5&&
            OriginalItemDefinitions.Find("ShinyPearl").Notes.Contains("not a blanket multiplier"),
            "Irradiant Pearl pools, raw crit and early armor retain separate stat scopes");
        assert(OriginalItemDefinitions.Find("SprintArmor").Notes.Contains("only while sprinting")&&
            OriginalItemDefinitions.Find("OutOfCombatArmor").Notes.Contains("active ready buff"),
            "Buckler and Opal quantities are conditional, not permanent passive armor");
        foreach(var id in new[]{"Crowbar","BossDamageBonus","NearbyDamageBonus"})
            assert(OriginalItemDefinitions.Find(id).Notes.Contains("not raw damage")||
                OriginalItemDefinitions.Find(id).Notes.Contains("not CharacterBody raw damage"),
                id+" hit-context coefficient cannot imply a raw damage-stat increase");
        assert(OriginalItemDefinitions.Find("BleedOnHitAndExplode").Notes.Contains("Hidden target health is never read")&&
            OriginalItemDefinitions.EstimateAttackRoll("BleedOnHitAndExplode",4,1,1)==null,
            "Shatterspleen target-health coefficient has no hidden-data read or invented generic luck roll");

        assert(OriginalItemDefinitions.Find("ShockDamageAura").Create("Conductor",100000).CurrentValue==null,"Floating-point interval underflow cannot promise a zero-second pulse");
        var bleed=OriginalItemDefinitions.Find("BleedOnHit");
        assert(bleed.Create("Dagger",10).Comparison.Direction==ValueDirection.NoImmediateChange&&bleed.Notes.Contains("proc coefficient 1"),"Proc-1 chance cap has zero next delta without promising every-attack certainty");
        assert(Near(OriginalItemDefinitions.EstimateAttackRoll("BleedOnHit",10,.5,0),.5)&&Near(OriginalItemDefinitions.EstimateAttackRoll("BleedOnHit",11,.5,0),.55),"Low-proc attack still benefits beyond ten Daggers in a conditional reference");
        assert(Near(OriginalItemDefinitions.EstimateAttackRoll("BleedOnHit",4,1,1),.64)&&Near(OriginalItemDefinitions.EstimateAttackRoll("BleedOnHit",5,1,1),.75),"Luck applies to scaled eligible bleed roll, not a blanket item effect");
        assert(Near(OriginalItemDefinitions.EstimateAttackRoll("BleedOnHitVoid",10,1,-2),1),"Capped collapse reference remains certain under negative luck");
        assert(Near(OriginalItemDefinitions.EstimateAttackRoll("Missile",1,1,0),.1)&&Near(OriginalItemDefinitions.EstimateAttackRoll("Missile",10,1,0),.1),"AtG extra copies change missile damage, not base roll chance");
        assert(Near(OriginalItemDefinitions.EstimateAttackRoll("ChainLightning",4,.5,1),.234375),"Ukulele proc context scales base roll before luck");
        assert(OriginalItemDefinitions.EstimateAttackRoll("Missile",4,null,1)==null&&OriginalItemDefinitions.EstimateAttackRoll("Missile",4,1,null)==null,"Unknown attack context suppresses adjusted chance");
        assert(OriginalItemDefinitions.EstimateAttackRoll("MissileVoid",4,1,1)==null&&OriginalItemDefinitions.EstimateAttackRoll("Bear",4,1,1)==null,"Deterministic Shrimp and independent block scopes do not use generic attack-roll math");
        assert(OriginalItemDefinitions.EstimateAttackRoll("LowerPricedChests",4,1,1)==null&&LuckEvidence.AdjustedProbability(.1,1,LuckApplicability.Affected,LuckEvidenceState.ReportedBug)==null,"Sale Star uncertainty remains separate from supported on-hit calculations");
        assert(OriginalItemDefinitions.EstimateAttackRoll("BleedOnHit",4,double.NaN,0)==null&&OriginalItemDefinitions.EstimateAttackRoll("BleedOnHit",4,-.1,0)==null,"Invalid proc context cannot generate numeric chance");
        var nectar=OriginalItemDefinitions.Find("BoostAllStats");
        var low=nectar.CreateTerms("Nectar",1,3);var high=nectar.CreateTerms("Nectar",1,9);var enough=nectar.CreateTerms("Nectar",4,9);
        assert(Near(low[1].CurrentValue,12)&&Near(low[1].NextValue,12)&&low[1].Comparison.Direction==ValueDirection.NoImmediateChange,"Nectar next capacity copy gives no immediate bonus at three fixed types");
        assert(Near(high[1].CurrentValue,16)&&Near(high[1].NextValue,32)&&Near(enough[1].CurrentValue,36)&&Near(enough[1].NextValue,36),"Nectar fixed-type cap determines immediate delta rather than an unconditional sixteen points");
        assert(nectar.CreateTerms("Nectar",1,null)[1].Headline.Contains("Maximum")&&nectar.Notes.Contains("Capacity only"),"Unknown eligible-type count only shows conditional maximum, not fabricated current benefit");
        assert(OriginalItemDefinitions.Find("CritGlasses").Notes.Contains("caps at 100")&&OriginalItemDefinitions.Find("CritGlasses").Create("Glasses",10).NextValue==110,"Raw crit term stays distinct from capped actual chance");
        assert(OriginalItemDefinitions.Find("ArmorPlate").Notes.Contains("minimum damage floor")&&OriginalItemDefinitions.Find("FragileDamageBonus").Notes.Contains("unbroken"),"Plate damage floor and Watch activation remain explicit conditions");
        var rain=OriginalItemDefinitions.Find("ImmuneToDebuff").CreateTerms("Raincoat",4);
        assert(rain[1].Comparison.Direction==ValueDirection.NoImmediateChange&&rain[2].Comparison.Direction==ValueDirection.NoImmediateChange,"More Raincoats do not multiply barrier percent or shorten documented recharge");
        assert(OriginalItemDefinitions.Find("Behemoth").Notes.Contains("Radius only")&&OriginalItemDefinitions.Find("MissileVoid").Notes.Contains("active shield"),"Radius-only and shield-gated effect scopes remain bounded");
        assert(OriginalItemDefinitions.Find("SpeedBoostPickup").Notes.Contains("not the old Antler Shield")&&OriginalItemDefinitions.Find("ShockDamageAura").TriggerContext.Contains("Five copies"),"Version-sensitive Antlers and ideal Conductor threshold retain context");
        assert(Near(OriginalBuffDefinitions.BeadStoredLevelUnits(6,4),2.1)&&Near(OriginalBuffDefinitions.BeadStoredLevelUnits(6,5),2.4),"Bead buff counter times rate gives stored level-gain equivalents with 0.05b next delta");
        assert(OriginalBuffDefinitions.BeadStoredLevelUnits(null,4)==null&&OriginalBuffDefinitions.BeadStoredLevelUnits(6,null)==null&&OriginalBuffDefinitions.BeadStoredLevelUnits(6,0)==null,"Unknown/absent Bead payout state does not produce invented units");
        assert(OriginalBuffDefinitions.Find("DLC2Content.Buffs.ExtraStatsOnLevelUpBuff").Effect.Contains("not a live percent-stat buff"),"Bead counter is not labeled as percent-current-stats");
        assert(OriginalBuffDefinitions.Find("bdElusiveAntlersSpeedBuff").Describe(4).Contains("48%")&&OriginalBuffDefinitions.Find("SpeedOnPickup").Describe(4).Contains("12%"),"Known buff aliases show per-active-stack components independently of owned item caps");
        assert(OriginalBuffDefinitions.Find("unreviewedBuff")==null&&OriginalBuffDefinitions.Find(null)==null,"Unknown buff names retain the effect-unverified fallback");
        assert(OriginalBuffDefinitions.Find("ImmuneToDebuff").Name!=OriginalBuffDefinitions.Find("ImmuneToDebuffConsumed").Name&&OriginalBuffDefinitions.Find("ImmuneToDebuffConsumed").Effect.Contains("remaining duration unavailable"),"Raincoat ready and recharge states stay distinct with no invented timer");
        assert(!BuffGlossaryRules.NectarEligible(true,false,false,false)&&!BuffGlossaryRules.NectarEligible(false,true,false,false)&&!BuffGlossaryRules.NectarEligible(false,false,true,false)&&!BuffGlossaryRules.NectarEligible(false,false,false,true),"Hidden, definition/catalog exclusion and Nectar self-counter independently exclude a type");
        assert(BuffGlossaryRules.NectarEligible(false,false,false,false),"A visible unignored active type is eligible regardless of stack count or guessed debuff semantics");
        foreach(var id in OriginalItemDefinitions.SupportedIds)
        {
            var definition=OriginalItemDefinitions.Find(id);var zero=definition.CreateTerms(id,0);var unknown=definition.CreateTerms(id,null);
            bool safe=true;for(int i=0;i<zero.Count;i++)safe&=zero[i].CurrentValue==0&&unknown[i].CurrentValue==null&&unknown[i].NextValue==null;
            assert(safe,id+" absence and unknown effective count cannot imply an active component");
        }
    }
}
