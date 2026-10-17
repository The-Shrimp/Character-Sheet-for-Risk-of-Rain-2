using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Alexander.CharacterSheet.Core;
static class PresentationTests
{
    public static void Run(Action<bool,string> assert)
    {
        Action<string,string,string> equal=(actual,expected,message)=>assert(actual==expected,message+" (got "+actual+")");
        equal(NumberFormatting.Display(12500000.125),"12.5M","Millions use a readable compact row value");
        equal(NumberFormatting.Exact(12500000.125),"12,500,000.125","Details retain round-trip precision");
        equal(NumberFormatting.Display(-1250000),"-1.25M","Compact negative values retain sign");
        equal(NumberFormatting.Display(999999999),"1B","Rounded compact boundaries carry to next suffix");
        equal(NumberFormatting.Display(.00005),"5e-5","Tiny nonzero values must not look like zero");
        equal(NumberFormatting.Display(-.0001),"-1e-4","Tiny negative values retain sign");
        equal(NumberFormatting.Display(0),"0","Zero remains distinguishable from unavailable");
        equal(NumberFormatting.Signed(0),"0","Zero changes are not positive gains");
        equal(NumberFormatting.Signed(2.5),"+2.5","Positive changes carry an explicit sign");
        equal(NumberFormatting.Exact(.1),"0.1","Details do not invent trailing noise for decimal tenths");
        equal(NumberFormatting.Display(null),"Unavailable","Missing values are not zero");
        equal(NumberFormatting.Display(double.NaN),"Unavailable","NaN is not a numeric UI total");
        equal(NumberFormatting.Display(double.PositiveInfinity),"Unavailable","Infinity is not a numeric UI total");
        equal(NumberFormatting.Signed(double.NegativeInfinity),"Unavailable","Unknown range is not a signed benefit");
        equal(NumberFormatting.Count(long.MaxValue),"9,223,372,036,854,775,807","Large stack counts are never coerced through double");
        equal(NumberFormatting.Count(null),"Unavailable","Missing count remains unknown");
        equal(NumberFormatting.Count(-1),"Unavailable","Invalid count remains unknown");
        equal(NumberFormatting.Display(1e15),"1e+15","Extreme magnitudes remain bounded on rows");
        var oldCulture=Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture=new CultureInfo("fr-FR");
            equal(NumberFormatting.Display(12500000.125),"12.5M","Machine culture does not change display convention");
            equal(NumberFormatting.Exact(12500000.125),"12,500,000.125","Machine culture does not change detailed precision");
        }
        finally { Thread.CurrentThread.CurrentCulture=oldCulture; }

        var comparison=ComparisonPresentation.Create(new ComparisonInput {Current=10,Next=25,Unit=ValueUnit.Percent,
            Evidence=PredictionEvidence.VerifiedComponent,CurrentBasis=ComparisonBasis.Component,NextBasis=ComparisonBasis.Component});
        assert(comparison.Direction==ValueDirection.Increase,"Verified equal-basis increase is identified");
        assert(comparison.Difference.Contains("+15 percentage points"),"Percent delta is labeled points, not relative percent");
        assert(comparison.Confidence.Contains("final benefit can differ"),"Component increase never promises a final stat increase");
        comparison=ComparisonPresentation.Create(new ComparisonInput {Current=12500000.125,Next=12500000.625,Unit=ValueUnit.Health,
            Evidence=PredictionEvidence.ConditionalEstimate,CurrentBasis=ComparisonBasis.Total,NextBasis=ComparisonBasis.Total,
            Conditions="same active modifiers"});
        assert(comparison.Current.Contains("12.5M"),"Large comparison rows are compact");
        assert(comparison.CurrentExact.Contains("12,500,000.125"),"Large comparison details retain precision");
        assert(comparison.DifferenceExact.Contains("+0.5 health"),"Small gains beside millions remain visible in detail");
        var comparisonTooltip=TooltipContent.FromComparison("Synthetic comparison","Provided values only",comparison);
        assert(TooltipPresentation.Full(comparisonTooltip).Contains("12,500,000.125"),"Comparison factory carries exact values into detailed tooltips");
        assert(TooltipPresentation.Short(comparisonTooltip).Contains("12.5M"),"Comparison factory carries compact values into short tooltips");
        assert(comparison.Confidence.Contains("same active modifiers"),"Assumptions survive comparison rendering");
        comparison=ComparisonPresentation.Create(new ComparisonInput {Current=2,Next=1,Evidence=PredictionEvidence.VerifiedComponent});
        assert(comparison.Direction==ValueDirection.Decrease&&comparison.Difference.Contains("-1"),"A next-copy decrease is not called a gain");
        comparison=ComparisonPresentation.Create(new ComparisonInput {Current=2,Next=2,Evidence=PredictionEvidence.VerifiedComponent});
        assert(comparison.Direction==ValueDirection.NoImmediateChange&&comparison.Difference.Contains("under these conditions"),"Zero component change is contextual");
        comparison=ComparisonPresentation.Create(new ComparisonInput {Current=2,Next=0,Evidence=PredictionEvidence.Unavailable,UnavailableReason="formula absent"});
        assert(comparison.Direction==ValueDirection.Unknown&&comparison.Next.Contains("Unavailable")&&!comparison.Next.Contains("0"),"Absent formula does not render a zero next-copy value");
        assert(comparison.Confidence.Contains("formula absent"),"Unknown prediction explains its reason");
        comparison=ComparisonPresentation.Create(new ComparisonInput {Current=null,Next=2,Evidence=PredictionEvidence.VerifiedComponent});
        assert(comparison.Direction==ValueDirection.Unknown&&comparison.Difference.Contains("current value unknown"),"Unknown current value prevents a delta");
        comparison=ComparisonPresentation.Create(new ComparisonInput {Current=1,Next=2,Evidence=PredictionEvidence.VerifiedComponent,SameConditions=false});
        assert(comparison.Direction==ValueDirection.Unknown&&comparison.Difference.Contains("conditions differ"),"Different conditions cannot yield a benefit promise");
        comparison=ComparisonPresentation.Create(new ComparisonInput {Current=100,Next=10,Evidence=PredictionEvidence.VerifiedComponent,
            CurrentBasis=ComparisonBasis.Total,NextBasis=ComparisonBasis.Component});
        assert(comparison.Direction==ValueDirection.Unknown&&comparison.Difference.Contains("component and total differ"),"Component terms cannot subtract from live totals");
        comparison=ComparisonPresentation.Create(new ComparisonInput {Current=100,Next=101,Evidence=PredictionEvidence.VerifiedComponent,
            CurrentBasis=ComparisonBasis.Total,NextBasis=ComparisonBasis.Total});
        assert(comparison.Direction==ValueDirection.Unknown&&comparison.Next.Contains("Unavailable")&&comparison.Confidence.StartsWith("Unverified"),
            "Inconsistent confidence metadata must not present a verified total or numeric next-copy prediction");
        comparison=ComparisonPresentation.Create(new ComparisonInput {Current=1,Next=double.NaN,Evidence=PredictionEvidence.ConditionalEstimate});
        assert(comparison.Direction==ValueDirection.Unknown&&comparison.Next.Contains("Unavailable"),"Nonfinite next estimates remain unknown");
        comparison=ComparisonPresentation.Create(new ComparisonInput {Current=-double.MaxValue,Next=double.MaxValue,Evidence=PredictionEvidence.ConditionalEstimate});
        assert(comparison.Direction==ValueDirection.Unknown&&comparison.Difference.Contains("numeric range"),"Overflow in subtraction is disclosed");

        var attribution=AttributionPresentation.Create(100,100,false);
        assert(attribution.Quality==AttributionQuality.Partial,"Coincidental subtotal match does not complete declared-incomplete attribution");
        attribution=AttributionPresentation.Create(100,100,true);
        assert(attribution.Quality==AttributionQuality.Complete,"Explicit completeness with matching values is preserved");
        attribution=AttributionPresentation.Create(1000000,999950,true);
        assert(attribution.Quality==AttributionQuality.Partial&&attribution.Difference.Contains("+50"),"Millions do not hide a meaningful discrepancy under an automatic relative tolerance");
        attribution=AttributionPresentation.Create(100,100.00001,true,.001);
        assert(attribution.Quality==AttributionQuality.Complete,"Adapters can supply an explicit finite tolerance");
        attribution=AttributionPresentation.Create(100,null,true);
        assert(attribution.Quality==AttributionQuality.Unavailable&&attribution.Live.Contains("100"),"Missing attribution does not discard the live total");
        attribution=AttributionPresentation.Create(null,100,true);
        assert(attribution.Quality==AttributionQuality.Unavailable,"Missing live value cannot claim completeness");
        attribution=AttributionPresentation.Create(double.MaxValue,-double.MaxValue,true);
        assert(attribution.Quality==AttributionQuality.Partial&&attribution.Status.Contains("numeric range"),"Residual overflow does not fake complete attribution");
        bool invalid=false;try { AttributionPresentation.Create(1,1,true,double.NaN); }catch(ArgumentOutOfRangeException){invalid=true;}
        assert(invalid,"Invalid tolerance must be rejected");

        var inputGroups=new List<EffectGroup>{EffectGroup.Defense,EffectGroup.OnKill,EffectGroup.Defense};
        var inactive=new InventoryEntry("item:i","Inactive",9,0,1,4,inputGroups,LuckApplicability.Mixed,searchText:"omega");
        inputGroups.Clear();
        assert(inactive.Groups.Count==2&&inactive.Groups.Contains(EffectGroup.Defense),"Entry owns a copy of mixed-effect metadata");
        assert(inactive.CountLabel.Contains("Owned: 9 | Effective: 0 (inactive)"),"Owned but inactive stacks stay distinguishable");
        assert(inactive.GroupLabel.Contains("On kill")&&inactive.GroupLabel.Contains("Defense"),"Mixed groups retain both classifications");
        var source=new List<InventoryEntry>{
            new InventoryEntry("item:b","Beta",2,2,1,0,new[]{EffectGroup.Defense},LuckApplicability.Unaffected),
            new InventoryEntry("item:a","Alpha",1,5,0,2,new[]{EffectGroup.OnHit},LuckApplicability.Affected),
            new InventoryEntry("item:z","alpha",1,5,0,1,new[]{EffectGroup.Utility},LuckApplicability.Mixed),
            new InventoryEntry("item:h","Hidden",1,20,hidden:true),
            new InventoryEntry("item:g","Absent",0,0),
            new InventoryEntry("item:u","Zeta",null,null,2,3),inactive};
        string originalOrder=string.Join(",",source.Select(e=>e.Id));
        var visible=InventoryPresentation.Select(source);
        equal(string.Join(",",visible.Select(e=>e.Id)),"item:a,item:z,item:b,item:i,item:u","Rarity/name sort has deterministic row-ID ties and excludes hidden/absent entries");
        equal(string.Join(",",source.Select(e=>e.Id)),originalOrder,"Display sorting leaves the source order untouched");
        assert(source[0].OwnedCount==2&&source[0].EffectiveCount==2,"Display sorting leaves source counts untouched");
        visible=InventoryPresentation.Select(source,new InventoryFilter {Sort=InventorySort.EffectiveCount});
        equal(string.Join(",",visible.Select(e=>e.Id)),"item:a,item:z,item:b,item:i,item:u","Effective-count sorting places unknown counts last");
        visible=InventoryPresentation.Select(source,new InventoryFilter {Sort=InventorySort.Acquisition});
        equal(string.Join(",",visible.Select(e=>e.Id)),"item:b,item:z,item:a,item:u,item:i","Acquisition sort uses supplied presentation metadata");
        visible=InventoryPresentation.Select(source,new InventoryFilter {Group=EffectGroup.Defense});
        equal(string.Join(",",visible.Select(e=>e.Id)),"item:b,item:i","Group filters include matching mixed-effect entries");
        visible=InventoryPresentation.Select(source,new InventoryFilter {Luck=LuckApplicability.Mixed});
        equal(string.Join(",",visible.Select(e=>e.Id)),"item:z,item:i","Mixed luck remains separately filterable");
        visible=InventoryPresentation.Select(source,new InventoryFilter {Sort=InventorySort.EffectGroup});
        equal(string.Join(",",visible.Select(e=>e.Id)),"item:a,item:i,item:b,item:z,item:u","Effect-group sort orders mixed entries by their first supplied category");
        visible=InventoryPresentation.Select(source,new InventoryFilter {Query="inactive",Group=EffectGroup.Defense,Luck=LuckApplicability.Mixed});
        equal(string.Join(",",visible.Select(e=>e.Id)),"item:i","Search/group/luck filters intersect rather than replace each other");
        var contrasting=new[]{new InventoryEntry("common","Zeta",1,1,rarity:0),new InventoryEntry("rare","Alpha",1,100,rarity:2)};
        equal(InventoryPresentation.Select(contrasting)[0].Id,"common","Default rarity order can differ from alphabetical order");
        equal(InventoryPresentation.Select(contrasting,new InventoryFilter {Sort=InventorySort.Name})[0].Id,"rare","Name sort overrides rarity grouping");
        equal(InventoryPresentation.Select(contrasting,new InventoryFilter {Sort=InventorySort.EffectiveCount})[0].Id,"rare","Effective-count sort overrides rarity grouping");
        visible=InventoryPresentation.Select(source,new InventoryFilter {ActiveOnly=true});
        equal(string.Join(",",visible.Select(e=>e.Id)),"item:a,item:z,item:b","Active-only filter excludes inactive and unconfirmed counts");
        visible=InventoryPresentation.Select(source,new InventoryFilter {Query="  BETA   defense "});
        equal(string.Join(",",visible.Select(e=>e.Id)),"item:b","Search is case-insensitive and requires all words");
        visible=InventoryPresentation.Select(source,new InventoryFilter {Query="omega"});
        equal(string.Join(",",visible.Select(e=>e.Id)),"item:i","Additional supplied search text is searchable");
        visible=InventoryPresentation.Select(source,new InventoryFilter {Query="unknownword"});
        assert(visible.Count==0,"No-match search is an empty presentation result");
        visible=InventoryPresentation.Select(source,new InventoryFilter {Sort=InventorySort.Name});
        equal(InventoryPresentation.ResolveSelection("item:z",visible),"item:z","Stable selection survives sort changes");
        equal(InventoryPresentation.ResolveSelection("item:h",visible),"item:a","Unavailable selection resolves to first visible row");
        assert(InventoryPresentation.ResolveSelection("item:z",new InventoryEntry[0])==null,"Empty results have no fake selected row");
        assert(InventoryPresentation.Select(source).Any(e=>e.Id=="item:u"),"Unknown counts stay visible rather than pretending ownership is zero");
        equal(InventoryPresentation.LuckLabel(LuckApplicability.Unknown),"Unverified","Unknown luck never defaults to unaffected");
        assert(InventoryPresentation.LuckLabel(LuckApplicability.Mixed).Contains("differs by effect"),"Mixed luck label explains why one yes/no answer is insufficient");
        invalid=false;try { InventoryPresentation.Select(new[]{source[0],source[0]}); }catch(ArgumentException){invalid=true;}
        assert(invalid,"Duplicate stable IDs are rejected instead of making hover selection ambiguous");
        invalid=false;try { new InventoryEntry("negative","Negative",-1,0); }catch(ArgumentOutOfRangeException){invalid=true;}
        assert(invalid,"Invalid known negative counts are rejected");
        invalid=false;try { new InventoryEntry(" ","Missing",1,1); }catch(ArgumentException){invalid=true;}
        assert(invalid,"Missing row identity is rejected");
        var many=Enumerable.Range(0,2000).Select(i=>new InventoryEntry("row:"+i,"Row "+i,1,i%7,rarity:i%4)).ToArray();
        assert(InventoryPresentation.Select(many).Count==2000,"Large inventories retain all supplied visible entries");
        equal(string.Join(",",InventoryPresentation.Select(many).Select(e=>e.Id)),
            string.Join(",",InventoryPresentation.Select(many.Reverse()).Select(e=>e.Id)),"Tie-breaking is deterministic across reversed input order");

        equal(TooltipText.Clean(" A\r\nB\0\u0001\tC "),"A\nB\tC","Control characters are cleaned without losing intentional lines");
        equal(TooltipText.SingleLine(" A\r\n B\t C "),"A B C","Row labels collapse whitespace");
        equal(TooltipText.Clip("abcdef",2),"..","Very small clipping budgets remain bounded");
        equal(TooltipText.Clip("abcdef",0),"","Zero clipping budget stays empty");
        equal(TooltipText.Clip("abcdef",1,"\ud83d\ude00"),"","Clipping cannot split a Unicode truncation marker");
        string unicode=TooltipText.Clip("abcd\ud83d\ude00tail",8);
        assert(unicode.Length<=8&&!unicode.Any(char.IsSurrogate),"Clipping cannot leave half a surrogate pair");
        var sectionList=new List<TooltipSection>{new TooltipSection(" Details ","Provided detail text")};
        var tooltip=new TooltipContent("Synthetic item","Summary","Current: 1","Next: 2","Change: +1","Unverified: example data",sectionList);
        sectionList.Clear();
        assert(tooltip.Sections.Count==1,"Tooltip content snapshots its sections");
        assert(TooltipPresentation.Short(tooltip).Contains("Unverified"),"Short tooltip preserves uncertainty");
        assert(TooltipPresentation.Full(tooltip).Contains("Provided detail text"),"Full tooltip retains detail sections");
        equal(TooltipPresentation.Select(tooltip,false),TooltipPresentation.Short(tooltip),"Collapsed selection uses short formatting");
        equal(TooltipPresentation.Select(tooltip,true),TooltipPresentation.Full(tooltip),"Detailed selection uses full formatting");
        tooltip=new TooltipContent("Long synthetic item",new string('q',10000),"Current: 1","Next copy: Unavailable","Benefit unknown",
            "Unverified: no supported prediction",new[]{new TooltipSection("More",new string('d',10000))});
        string shortTip=TooltipPresentation.Short(tooltip);
        assert(shortTip.Length<=420&&shortTip.Contains("Unverified"),"Long summary cannot crowd out the uncertainty label");
        string fullTip=TooltipPresentation.Full(tooltip);
        assert(fullTip.Length<=8192&&fullTip.EndsWith("[Details truncated]"),"Bounded full detail clearly announces truncation");
        assert(fullTip.IndexOf("Unverified",StringComparison.Ordinal)<200,"Full detail puts uncertainty near the top");
        tooltip=new TooltipContent("Missing metadata","","","","","");
        assert(TooltipPresentation.Short(tooltip).Contains("Unverified")&&TooltipPresentation.Full(tooltip).Contains("Unverified"),"Absent confidence never becomes an assurance");
        tooltip=new TooltipContent("Millions","","Current: 12.5M","Next: 12.5M","Change: +0.5","Conditional estimate",
            detailedCurrent:"Current: 12,500,000.125",detailedNext:"Next: 12,500,000.625",detailedDifference:"Change: +0.5");
        assert(TooltipPresentation.Full(tooltip).Contains("12,500,000.125")&&TooltipPresentation.Short(tooltip).Contains("12.5M"),"Detailed and short tooltips use their separate precision representations");
        tooltip=new TooltipContent("<b>Literal title</b>","<size=999>Literal text</size>","","","","Unverified");
        assert(TooltipPresentation.Full(tooltip).Contains("<size=999>Literal text</size>"),"Plain tooltip model does not execute or reinterpret supplied markup");
        invalid=false;try { TooltipPresentation.Short(tooltip,10); }catch(ArgumentOutOfRangeException){invalid=true;}
        assert(invalid,"Insufficient tooltip budget is rejected instead of silently dropping uncertainty");
    }
}
