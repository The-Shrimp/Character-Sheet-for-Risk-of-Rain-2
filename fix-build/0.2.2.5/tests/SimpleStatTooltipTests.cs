using System;
using System.Linq;
using Alexander.CharacterSheet.Core;
static class SimpleStatTooltipTests
{
    const string Growth="Level growth (level - 1)";
    static string Plain(double live,double growth,Calculation calculation,string suffix="")=>
        TooltipMarkup.Render(StatTooltipPresentation.Create(live,growth,calculation,"cIsUtility",suffix),s=>false);
    static Calculation Basis(double baseline,double growth,int level)
    {
        var calculation=new Calculation("Survivor baseline",baseline);
        calculation.Apply(Growth,CalculationStage.Flat,growth*(level-1));return calculation;
    }
    public static void Run(Action<bool,string> assert)
    {
        var level1=Basis(110,33,1);
        assert(Plain(110,33,level1)=="Total: 110\nBase: 110 + 33 per level\nBonuses: None","Level1 shows the actual growth coefficient even though no level growth has been applied");
        var level4=Basis(110,33,4);
        string plain=Plain(209,33,level4);
        assert(plain=="Total: 209\nBase: 110 + 33 per level\nBonuses: None","Level4 has total209 and the same base110 +33 per level");
        assert(!plain.Contains("99")&&!plain.Contains("Base: 209"),"Applied growth is not repeated as a bonus or mislabeled as the base");
        assert(level4.Terms[1].Operand==99&&level4.Terms[1].After==209&&level4.Predicted==209,"Level growth is applied exactly once");
        string trace=level4.Describe(209,false,true);
        assert(trace.Contains("+99")&&trace.Contains("110 -> 209")&&trace.Contains("Flat"),"Optional trace keeps the real growth contribution and ordered transition");
        var damage=Basis(12,2.4,1);
        assert(Plain(12,2.4,damage)=="Total: 12\nBase: 12 + 2.4 per level\nBonuses: None","Screenshot level1 damage no longer displays growth12 or a technical explanation");
        var regen=Basis(1,.2,4);
        assert(Plain(regen.Predicted,.2,regen," /s")=="Total: 1.6 /s\nBase: 1 /s + 0.2 /s per level\nBonuses: None","Regen retains units without extra commentary");
        var armor=Basis(0,0,4);
        assert(Plain(0,0,armor)=="Total: 0\nBase: 0 + 0 per level\nBonuses: None","A zero armor base and growth are actual zero values");
        var negativeGrowth=Basis(10,-2,4);
        assert(Plain(4,-2,negativeGrowth).Contains("Base: 10 - 2 per level"),"A negative coefficient uses subtraction");

        var bonuses=Basis(100,10,4);
        bonuses.Apply("Flat item",CalculationStage.Flat,25);
        bonuses.Apply("Penalty",CalculationStage.AdditiveMultiplier,-.2);
        bonuses.Apply("Late flat",CalculationStage.LateFlat,30);
        bonuses.Apply("Final modifier",CalculationStage.LateMultiplier,1.1);
        string stages=string.Join(",",bonuses.Terms.Select(t=>t.Stage));
        double prediction=bonuses.Predicted;
        string grouped=Plain(prediction,10,bonuses);
        assert(grouped.StartsWith("Total: "+NumberFormatting.Display(prediction)+"\nBase: 100 + 10 per level\nBonuses:\n"),"Bonus view has exactly the requested top-level labels");
        assert(grouped.IndexOf("Flat item: +25")<grouped.IndexOf("Late flat: +30")&&grouped.IndexOf("Late flat: +30")<grouped.IndexOf("Penalty: -20%"),"Known flat sources display before percentage modifiers");
        assert(grouped.Contains("Final modifier: +10%")&&!grouped.Contains("x1.1"),"Multiplicative factors display as readable signed percentages");
        assert(string.Join(",",bonuses.Terms.Select(t=>t.Stage))==stages&&bonuses.Predicted==prediction,"Display grouping does not reorder the underlying operations");
        trace=bonuses.Describe(prediction,false,true);
        assert(trace.IndexOf("Penalty:")<trace.IndexOf("Late flat:")&&trace.IndexOf("Late flat:")<trace.IndexOf("Final modifier:"),"Optional ordered math preserves early modifier then late flat then late modifier");
        assert(Plain(prediction+5,10,bonuses).EndsWith("Other/unknown: +5"),"A positive unaccounted difference is a short honest bonus label");
        assert(Plain(prediction-8,10,bonuses).EndsWith("Other/unknown: -8"),"A negative unaccounted difference is not disguised as an item penalty");
        assert(!Plain(prediction,10,bonuses).Contains("Other/unknown"),"Zero difference does not add a coverage warning");
        var unknownOnly=Basis(110,33,4);
        assert(Plain(229,33,unknownOnly).EndsWith("Bonuses:\nOther/unknown: +20"),"Unattributed sources are labeled even when no known bonuses are present");
        assert(Plain(double.NaN,33,unknownOnly).Contains("Other/unknown: Unavailable"),"Missing live data does not imply zero bonuses");
        var neutral=Basis(10,0,1);neutral.Apply("Zero flat",CalculationStage.Flat,0);neutral.Apply("Zero percent",CalculationStage.AdditiveMultiplier,0);neutral.Apply("Identity factor",CalculationStage.Multiplier,1);
        assert(Plain(10,0,neutral).EndsWith("Bonuses: None"),"Neutral operations do not clutter the ordinary view");
        var cursed=Basis(100,0,1);cursed.Apply("Late factor",CalculationStage.LateMultiplier,1.16);cursed.Apply("Curse",CalculationStage.FinalConversion,.5);
        assert(Plain(cursed.Predicted,0,cursed).Contains("Curse: -50%"),"A known final conversion keeps a readable percentage");
        trace=cursed.Describe(cursed.Predicted,false,true);
        assert(trace.IndexOf("Late factor:")<trace.IndexOf("Curse:")&&Math.Abs(cursed.Predicted-58)<1e-9,"Final conversion is still calculated after the late multiplier");
        var levelNamedItem=Basis(10,0,1);levelNamedItem.Apply("Level reward item",CalculationStage.Flat,5);
        assert(Plain(15,0,levelNamedItem).Contains("Level reward item: +5"),"Only the exact growth source is excluded, not every item starting with Level");
        var unsafeName=Basis(10,0,1);unsafeName.Apply("<link=target>Flat",CalculationStage.Flat,1);
        assert(!Plain(11,0,unsafeName).Contains("<link="),"Bonus source names cannot inject tooltip markup");

        int examples=0;
        foreach(int level in new[]{1,4})
        foreach(var values in new[]{new[]{110d,33d},new[]{12d,2.4d},new[]{1d,.2d},new[]{0d,0d}})
        foreach(double difference in new[]{0d,7d,-3d})
        {
            var calculation=Basis(values[0],values[1],level);
            string text=Plain(calculation.Predicted+difference,values[1],calculation);
            assert(text.StartsWith("Total: ")&&text.Contains("\nBase: ")&&text.Contains("\nBonuses:"),"Every requested stat follows Total / Base / Bonuses");
            assert(text.Contains(NumberFormatting.Display(values[0])+" + "+NumberFormatting.Display(values[1])+" per level"),"Base always uses the coefficient, independently of level and bonus difference");
            assert(!new[]{"authoritative","modeled","hooks","evidence","partial","coefficient","unverified","Shift","Level growth:"}.Any(s=>text.IndexOf(s,StringComparison.OrdinalIgnoreCase)>=0),"Ordinary stat body contains no diagnostics or expansion shortcut");
            assert((difference==0)==text.EndsWith("Bonuses: None"),"Only actual zero differences use the empty bonus label");
            examples++;
        }
        object own=new object(),other=new object();
        assert(NativeItemSurfacePolicy.OwnSurface(own,own,other,true,false),"Own scoreboard identity takes precedence over the HUD target");
        assert(NativeItemSurfacePolicy.OwnSurface(own,null,own,false,true),"Exact own top HUD display is an eligible native item surface");
        assert(!NativeItemSurfacePolicy.OwnSurface(own,other,own,true,true),"Another player's scoreboard cannot be admitted through the top HUD branch");
        assert(!NativeItemSurfacePolicy.OwnSurface(own,null,other,false,true),"A spectated foreign HUD target is excluded");
        assert(!NativeItemSurfacePolicy.OwnSurface(null,null,null,true,true),"Missing local ownership is excluded");
        assert(!NativeItemSurfacePolicy.OwnSurface(own,own,own,false,false),"Unrelated item surfaces are excluded");
        assert(ItemDetailsPresentation.Create(null,1,1)=="","Unsupported items produce no unavailable-formula body");
        var missing=ItemTooltipPresentation.Create(new ItemScalingInput{Name="Missing",OwnedCopies=1});
        assert(ItemDetailsPresentation.Create(new[]{missing},1,1)=="","An unsupported supplied component is also omitted");
        assert(!NativeItemSurfacePolicy.HasAddition("","?"),"Unknown formula and luck retain the native tooltip alone");
        assert(NativeItemSurfacePolicy.HasAddition("","!")&&!NativeItemSurfacePolicy.KnownLuck("?"),"Explicit scoped luck information may add the small clover without a formula button");
        var dagger=OriginalItemDefinitions.Find("BleedOnHit").Create("Native name",1);
        string addition=ItemDetailsPresentation.Create(new[]{missing,dagger},1,1);
        assert(addition.Contains("10%")&&addition.Contains("20%")&&!addition.Contains("unavailable")&&!addition.Contains("Native name"),"Available calculations extend the native tooltip without duplicating its title or missing-formula filler");
        Console.WriteLine("Simple stat display: "+examples+" level/stat/bonus examples, coefficients, negative modifiers, ordered Details, both item ownership paths and unknown-formula omission.");
    }
}
