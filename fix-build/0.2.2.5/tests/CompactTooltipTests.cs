using System;
using System.Linq;
using Alexander.CharacterSheet.Core;
static class CompactTooltipTests
{
    public static void Run(Action<bool,string> assert)
    {
        Func<string,bool> known=s=>s=="cIsDamage"||s=="cIsHealing"||s=="cIsUtility"||s=="cStack";
        string native="Deal <style=cIsDamage>300% damage</style> and <style=cIsHealing>heal 10 health</style>.";
        string rendered=TooltipMarkup.Render(native,known);
        assert(rendered==native,"Native semantic styles and surrounding spaces survive unchanged");
        assert(TooltipMarkup.Render(native,s=>false)=="Deal 300% damage and heal 10 health.","Unavailable native styles fall back to normal text without an invented color");
        assert(TooltipMarkup.Render("<color=#FF7700>Native fire</color>",known)=="<color=#FF7700>Native fire</color>","A color supplied in the native description remains exact");
        assert(TooltipMarkup.Render("<color=orange>Fire</color>",known)=="Fire","Unsupported named colors do not create a replacement palette");
        assert(TooltipMarkup.Render("<link=x><size=999><sprite=1>text</size></link>",known)=="text","Interactive, size and sprite tags cannot enter the custom body");
        assert(!TooltipMarkup.Render(TooltipMarkup.Literal("<style=cIsDamage>user text</style>"),known).Contains("<style="),"Own plain strings cannot inject native-style tags");
        assert(TooltipMarkup.Render("<style=cIsDamage>damage",known).EndsWith("</style>"),"Unclosed permitted markup is balanced");
        assert(TooltipMarkup.Render("<style=cIsDamage>A<style=Unknown>B</style>C</style>",known)=="<style=cIsDamage>ABC</style>","Unknown nested styles do not consume a known parent's close");
        assert(TooltipMarkup.Render("A<br>B\r\nC",known)=="A\nB\nC","Native line breaks and source newlines remain readable");
        var term=ItemTooltipPresentation.Create(new ItemScalingInput {Name="Supplied beads",Verb="Stores",ValueLabel="of level-up gains",OwnedCopies=4,
            BaseValue=20,PerAdditionalCopy=5,Unit=ValueUnit.Percent,Evidence=PredictionEvidence.ConditionalEstimate,CopySingular="bead",CopyPlural="beads"});
        string compact=TooltipMarkup.Render(CompactTooltipPresentation.Item(new[]{term},"Stores level-up gains.",4,4),known);
        assert(compact.Contains("35%")&&compact.Contains("(20% + 5%/extra bead)"),"Current stack result and additional-copy wording use n-1");
        assert(!compact.Contains("Next copy")&&!compact.Contains("Definition source")&&!compact.Contains("4 total beads"),"Default body omits next-copy math, source notes and repeated count");
        assert(compact.Contains("Reference components"),"Conditional terms remain visibly identified in the short body");
        assert(term.FullMath.Contains("4 - 1 = 3")&&term.NextLabel.Contains("40%"),"optional Details data keeps exact copy arithmetic and next-copy result");
        var unsupported=ItemTooltipPresentation.Create(new ItemScalingInput {Name="Missing",OwnedCopies=2});
        compact=CompactTooltipPresentation.Item(new[]{unsupported},"Native pickup.",2,2);
        assert(compact.Contains("Current component unavailable")&&!compact.Contains("Provides 0"),"Absent formula stays unavailable instead of becoming zero");
        compact=CompactTooltipPresentation.Item(null,"Native pickup.",0,9);
        assert(compact.Contains("Scaling unverified")&&compact.Contains("0 effective / 9 owned"),"Inactive owned inventory and missing registration remain distinct");
        var bleed=OriginalItemDefinitions.Find("BleedOnHit").Create("Bleed",12);
        compact=CompactTooltipPresentation.Item(new[]{bleed},"Bleed.",12,12);
        assert(compact.Contains("100%")&&compact.Contains("attack proc coefficient 1, luck 0"),"A capped reference does not lose its required attack/luck assumptions");
        assert(!compact.Contains("min(100%")&&bleed.FullMath.Contains("min(100%"),"Nonlinear equations remain in optional Details details");
        assert(OriginalItemDefinitions.EstimateAttackRoll("BleedOnHit",12,null,0)==null,"Compact presentation does not invent missing attack context");
        var multi=OriginalItemDefinitions.Find("ChainLightning").CreateTerms("Chain",3);
        compact=CompactTooltipPresentation.Item(multi,"Chain lightning.",3,3);
        assert(compact.Contains("7")&&compact.Contains("24 m")&&multi.Count==2,"Multiple component terms remain visible in the concise body");
        foreach(var id in OriginalItemDefinitions.SupportedIds)
        foreach(long copies in new long[]{0,1,4,10})
        {
            var terms=OriginalItemDefinitions.Find(id).CreateTerms(id,copies);
            string shortBody=CompactTooltipPresentation.Item(terms,"Native summary.",copies,copies);
            assert(shortBody.Contains("Reference components")&&!shortBody.Contains("Next-copy equation"),"Every registered component keeps concise evidence and defers equations: "+id);
            foreach(var item in terms)
                assert(!item.CompactValue.Contains("NaN")&&!item.CompactValue.Contains("Infinity"),"Invalid numbers cannot reach compact component text: "+id);
        }
        var calculation=new Calculation("Baseline",100);calculation.Apply("Level",CalculationStage.Flat,20);
        string stat=TooltipMarkup.Render(StatTooltipPresentation.Create(150,20,calculation,"cIsHealing",growthSource:"Level"),s=>false);
        assert(stat=="Total: 150\nBase: 100 + 20 per level\nBonuses:\nOther/unknown: +30","Simple stat distinguishes the live total, base coefficient and unknown bonus");
        stat=StatTooltipPresentation.Create(120,20,calculation,"cIsHealing",growthSource:"Level");
        assert(stat.Contains("Bonuses: None")&&!stat.Contains("Other/unknown"),"Matching values add no zero-difference warning");
        assert(calculation.Describe(120,false,true).Contains("Partial attribution"),"Coverage limits remain in optional Details even when values match");
        var screenshotHealth=new Calculation("Survivor baseline",110);
        screenshotHealth.Apply("Level growth (level - 1)",CalculationStage.Flat,33*(4-1));
        string health=TooltipMarkup.Render(StatTooltipPresentation.Create(209,33,screenshotHealth,"cIsHealing"),s=>false);
        assert(health.Split((char)10).SequenceEqual(new[]{"Total: 209","Base: 110 + 33 per level","Bonuses: None"}),"Screenshot health uses the exact simple display contract");
        string expandedHealth=screenshotHealth.Describe(209,false,true);
        assert(expandedHealth.Contains("Level growth (level - 1): +99")&&!expandedHealth.Contains("Level growth (level - 1): 209"),"Details growth remains the contribution rather than cumulative209");
        assert(screenshotHealth.Predicted==209&&screenshotHealth.Terms[1].After==209&&screenshotHealth.Terms[1].Operand==99,"Simple presentation preserves calculation semantics");
        var dagger=OriginalItemDefinitions.Find("BleedOnHit").Create("Tri-Tip Dagger",1);
        string daggerShort=TooltipMarkup.Render(CompactTooltipPresentation.Item(new[]{dagger},"Chance to bleed enemies.",1,1),s=>false);
        assert(daggerShort.Contains("10%")&&!daggerShort.Contains("20%")&&!daggerShort.Contains("additional")&&!daggerShort.Contains("Public snapshot"),"Screenshot dagger default shows current10% without next20% or repeated snapshot/count boilerplate");
        var fills=new[]{
            new NativeTooltipFillCandidate(true,true,false,.15,200000),
            new NativeTooltipFillCandidate(true,true,true,.9,60000),
            new NativeTooltipFillCandidate(false,false,true,1,3000)};
        assert(NativeTooltipFillPolicy.Select(fills)==1,"Body-covering native fill is preferred to large translucent decoration and unrelated header");
        assert(NativeTooltipFillPolicy.Select(new[]{new NativeTooltipFillCandidate(false,false,true,1,500)})==-1,"Unrelated image is not borrowed as a body fill");
        assert(NativeTooltipFillPolicy.Select(new[]{new NativeTooltipFillCandidate(true,true,true,double.NaN,500)})==-1,"Unknown native opacity is rejected");
        assert(NativeTooltipFillPolicy.Select(null)==-1,"Missing native fill remains unavailable rather than inventing a color");
        assert(calculation.Predicted==120&&calculation.Terms.Count==2,"Presentation does not mutate the calculation");
        var ordered=new Calculation("Base",100);ordered.Apply("Level growth",CalculationStage.Flat,0);
        ordered.Apply("Item A",CalculationStage.Flat,25);ordered.Apply("Item B",CalculationStage.AdditiveMultiplier,.2);
        ordered.Apply("Later buff",CalculationStage.LateFlat,30);ordered.Apply("Nectar",CalculationStage.LateMultiplier,1.16);
        string originalStages=string.Join(",",ordered.Terms.Select(t=>t.Stage));
        string grouped=StatTooltipPresentation.Create(208.8,0,ordered,"cIsHealing",growthSource:"Level growth");
        assert(grouped.Contains("Item A:")&&grouped.Contains("Item B:")&&grouped.Contains("+20%"),"Only present sources appear in applicable flat/modifier categories");
        assert(!grouped.Contains("Level growth:"),"Zero growth does not add an inapplicable default row");
        assert(string.Join(",",ordered.Terms.Select(t=>t.Stage))==originalStages&&Math.Abs(ordered.Predicted-208.8)<1e-9,"Category presentation preserves late-flat and late-multiplier calculation order");

        int matrices=0;
        foreach(var resolution in new[]{new[]{1280d,720d},new[]{1920d,1080d},new[]{3440d,1440d}})
        foreach(double scale in new[]{.7,.85,1,1.2})
        foreach(double bodyHeight in new[]{18d,80d,240d,2000d})
        foreach(bool full in new[]{false,true})
        {
            CompactTooltipSize size;
            bool fits=CompactTooltipSizing.TryCreate(resolution[0],resolution[1],420*scale,bodyHeight*scale,40*scale,26*scale,scale,full,out size);
            assert(fits&&size.Width<=resolution[0]-24*scale&&size.Height<=resolution[1]-24*scale&&size.ViewportHeight>0,"Popup fits owned canvas with bounded overflow viewport");
            if(bodyHeight==18)assert(size.Height<200*scale,"Short text does not retain an empty fixed-height card");
            if(bodyHeight==2000)assert(size.ViewportHeight<bodyHeight*scale,"Long optional Details body is bounded for scrolling");
            matrices++;
        }
        CompactTooltipSize invalid;
        assert(!CompactTooltipSizing.TryCreate(50,50,420,80,40,26,1,false,out invalid),"Too-small host fails closed");
        assert(!CompactTooltipSizing.TryCreate(double.NaN,720,420,80,40,26,1,false,out invalid),"Unknown geometry fails closed");
        Console.WriteLine("Compact tooltip coverage: "+matrices+" sizing cases; "+OriginalItemDefinitions.SupportedIds.Count+" item definitions; native-style safety and conditional context.");
    }
}
