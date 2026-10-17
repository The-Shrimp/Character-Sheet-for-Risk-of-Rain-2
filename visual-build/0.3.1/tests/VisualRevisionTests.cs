using System;
using System.Collections.Generic;
using System.Linq;
using Alexander.CharacterSheet.Core;
static class VisualRevisionTests
{
    static List<SheetNode> Example(bool shield=true)
    {
        var nodes=SheetHierarchy.Headers.ToList();
        foreach(var group in SheetHierarchy.Stats)
        foreach(string stat in group.Value)
            if(shield||stat!="Shield")nodes.Add(new SheetNode("stat:"+stat,group.Key,stat,false,2,stat));
        nodes.Add(new SheetNode("effect:crowbar",SheetHierarchy.Damage,"Crowbar",false,1,"item:crowbar"));
        nodes.Add(new SheetNode("effect:slug",SheetHierarchy.Combat,"Slug",false,1,"item:slug"));
        nodes.Add(new SheetNode("effect:quail",SheetHierarchy.Sprint,"Quail",false,1,"item:quail"));
        nodes.Add(new SheetNode("effect:buff",SheetHierarchy.Other,"Visible buff",false,1,"buff:1"));
        return nodes;
    }
    static string Plain(string text)=>TooltipMarkup.Render(text,s=>false);
    static NectarBuffObservation Buff(string id,long count=1,bool? hidden=false,bool? ignored=false,bool? catalog=false,bool? self=false)=>
        new NectarBuffObservation{Id=id,Stacks=count,Hidden=hidden,DefinitionIgnored=ignored,CatalogIgnored=catalog,SelfCounter=self};
    static bool Throws(Action action){try{action();return false;}catch(ArgumentOutOfRangeException){return true;}}
    public static void Run(Action<bool,string> assert)
    {
        var projected=VisualSheetStructure.Project(Example()).ToArray();
        string[] order={"defense","offense","movement","on-hit","on-kill","out-of-combat","sprinting","other"};
        assert(projected.Where(n=>n.Header).Select(n=>n.Id).SequenceEqual(order),"Flat category order follows the final user grouping");
        assert(projected.Where(n=>n.Header).All(n=>n.Parent==""&&n.Depth==0),"No outer accordion or hidden nested category");
        assert(!projected.Any(n=>n.Id==SheetHierarchy.Base||n.Id==SheetHierarchy.Damage||n.Id==SheetHierarchy.Luck),"Removed presentation headers do not survive the projection");
        assert(projected.Single(n=>n.Id==SheetHierarchy.Other).Title=="Misc Effects","Final scroll section is Misc Effects");
        assert(projected.Single(n=>n.Id==SheetHierarchy.Hit).Title=="On-Hit Effects"&&projected.Single(n=>n.Id==SheetHierarchy.Kill).Title=="On-Kill Effects","Effect section labels follow the reference");
        assert(projected.Single(n=>n.Id==SheetHierarchy.Offense).Glyph==SheetGlyph.CrossedSwords,"Offense uses crossed swords");
        assert(projected.Single(n=>n.Id==SheetHierarchy.Movement).Glyph==SheetGlyph.Runner,"Movement uses running-person geometry");
        assert(projected.Last().Id==VisualSheetStructure.FooterId&&VisualSheetStructure.IsFooter(projected.Last()),"Luck is a dedicated final focus target");
        assert(!projected.Last().Header&&projected.Last().Parent==""&&projected.Last().Glyph==SheetGlyph.Clover,"Luck remains independent of all collapse states");
        assert(projected.Single(n=>n.Id=="effect:crowbar").Parent==SheetHierarchy.Other,"Damage-item information is retained without a separate damage category");
        assert(projected.Single(n=>n.Id=="effect:slug").Parent==SheetHierarchy.Combat&&projected.Single(n=>n.Id=="effect:quail").Parent==SheetHierarchy.Sprint,"Conditional effects stay standalone before Misc Effects");
        assert(projected.Where(n=>!n.Header).Select(n=>n.Id).OrderBy(n=>n).SequenceEqual(Example().Where(n=>!n.Header).Select(n=>n.Id).OrderBy(n=>n)),"Every original functional row is retained exactly once");
        var defaults=new SheetExpansionState(true);
        var initial=SheetHierarchy.Visible(projected,defaults);
        assert(defaults.IsOpen(SheetHierarchy.Defense)&&!defaults.IsOpen(SheetHierarchy.Offense)&&!defaults.IsOpen(SheetHierarchy.Movement),"Reference defaults expand Defense only");
        assert(initial.Any(n=>n.Id=="stat:Health")&&!initial.Any(n=>n.Id=="stat:Damage")&&initial.Any(VisualSheetStructure.IsFooter),"Defense totals and Luck are visible initially");
        assert(VisualSheetStructure.ToggleLabel(true)=="Collapse"&&VisualSheetStructure.ToggleLabel(false)=="Expand","Explicit controls replace visual chevrons");
        for(int mask=0;mask<256;mask++)
        {
            var state=new SheetExpansionState(true);
            for(int i=0;i<order.Length;i++)state.Set(order[i],(mask&(1<<i))!=0);
            var visible=SheetHierarchy.Visible(projected,state);
            assert(visible.Count(n=>n.Header)==8&&visible.Count(VisualSheetStructure.IsFooter)==1,"Headers and fixed footer survive every collapse combination");
            foreach(var row in projected.Where(n=>!n.Header&&!VisualSheetStructure.IsFooter(n)))
                assert(visible.Any(n=>n.Id==row.Id)==state.IsOpen(row.Parent),"Collapse affects only its own rows");
            assert(visible.Last().Id==VisualSheetStructure.FooterId,"Navigation ends at fixed Luck for every collapse combination");
        }
        defaults.Set(SheetHierarchy.Defense,false);
        assert(SheetHierarchy.RestoreFocus("stat:Shield",projected,SheetHierarchy.Visible(projected,defaults))==SheetHierarchy.Defense,"Collapsing a focused stat restores its visible category");
        var noShield=VisualSheetStructure.Project(Example(false));
        assert(!noShield.Any(n=>n.Id=="stat:Shield")&&noShield.Any(VisualSheetStructure.IsFooter),"Conditional shield removal cannot remove or duplicate Luck");
        int sizing=0;
        foreach(double canvas in new[]{640d,720d,1080d,1440d})
        foreach(double scale in new[]{.7,1,1.2})
        foreach(double font in new[]{18d,22d,24d})
        foreach(int rows in new[]{0,32,128})
        {
            var metrics=new CompactSheetMetrics(font,scale);
            double footer=metrics.RowHeight+10*scale;
            double minimum=Math.Max(metrics.MinHeight,metrics.HeaderHeight+footer+2*metrics.Padding+metrics.RowHeight);
            double height=VisualSheetSizing.PanelHeight(canvas,8,rows,metrics.RowHeight,metrics.RowHeight+12*scale,
                metrics.HeaderHeight,footer,metrics.Padding,minimum);
            var regions=new VisualSheetRegions(height,metrics.HeaderHeight,footer,metrics.Padding);
            assert(height>=minimum&&height<=Math.Max(minimum,canvas*.5),"Long content remains bounded across scales and resolutions");
            assert(regions.ViewportBottom>regions.FooterHeight&&regions.ViewportHeight>0,"Masked content never extends behind the fixed footer");
            assert(regions.ViewportTop<height&&regions.ViewportTop>regions.ViewportBottom,"Title and footer leave a positive scroll region");
            assert(VisualSheetSizing.BackgroundOpacity==.8,"Body and green footer backgrounds use the accepted opacity");
            double offset=HierarchySheetSizing.EnsureVisible(regions.ViewportHeight+30,-20,regions.ViewportHeight,400,800);
            assert(offset>=0&&offset<=Math.Max(0,800-regions.ViewportHeight),"Scroll-to-focus remains bounded to scroll content");
            sizing++;
        }
        assert(Throws(()=>new VisualSheetRegions(20,30,30,4))&&Throws(()=>new VisualSheetRegions(double.NaN,30,30,4)),"Insufficient/nonfinite footer layouts are rejected");
        assert(Throws(()=>VisualSheetSizing.PanelHeight(1080,-1,0,28,40,32,38,10,140)),"Negative section counts cannot create layout geometry");
        var regen=new Calculation("Base",1);regen.Apply("Level growth (level - 1)",CalculationStage.Flat,(double)(float).2);
        double live=(double)(float)1.2,prediction=regen.Predicted;
        assert(live-prediction!=0&&DisplayResidual.ForDisplay(live,prediction)==0,"Actual float32 regen discrepancy is suppressed only for display");
        string hover=Plain(StatTooltipPresentation.Create(live,.2,regen,"cIsHealing"," /s"));
        assert(hover.EndsWith("Bonuses: None")&&!hover.Contains("e-8"),"Ordinary Regen hover does not show roundoff as an unknown bonus");
        assert(regen.Predicted==prediction&&regen.Describe(live,false,true).Contains(NumberFormatting.Exact(live)),"Authoritative value and exact math trace remain intact");
        assert(DisplayResidual.ForDisplay(1+1e-7,1)!=0&&DisplayResidual.ForDisplay(1-1e-7,1)!=0,"Real small positive and negative discrepancies above float precision remain visible");
        assert(DisplayResidual.ForDisplay(1e-10,0)==1e-10,"Real tiny values near zero are not erased by a broad absolute threshold");
        var tiny=new Calculation("Base",0);tiny.Apply("Known small bonus",CalculationStage.Flat,1e-10);
        assert(Plain(StatTooltipPresentation.Create(1e-10,0,tiny,"cIsHealing")).Contains("Known small bonus"),"Known small bonuses are never passed through residual filtering");
        assert(double.IsNaN(DisplayResidual.ForDisplay(double.NaN,1))&&double.IsInfinity(DisplayResidual.ForDisplay(double.PositiveInfinity,1)),"Unavailable/nonfinite observations do not become zero bonuses");
        foreach(double magnitude in new[]{1e-6,.1,1,1e3,1e6})
        {
            assert(DisplayResidual.ForDisplay(magnitude*(1+2e-8),magnitude)==0,"Relative float noise is filtered at each scale");
            assert(DisplayResidual.ForDisplay(magnitude*(1+1e-5),magnitude)!=0,"Material relative residuals remain at each scale");
        }
        var active=new[]{Buff("a",50),Buff("b",1),Buff("c",7)};
        assert(NectarCounterPresentation.EligibleTypes(active)==3,"Stacks do not inflate distinct eligible types");
        assert(NectarCounterPresentation.EligibleTypes(active.Concat(new[]{Buff("a",50)}))==3,"Repeated observations of the same type count once");
        assert(NectarCounterPresentation.EligibleTypes(active.Concat(new[]{Buff("a",51)}))==null,"Contradictory duplicate observations are unknown");
        var exclusions=new[]{Buff("hidden",4,true,null,null,null),Buff("definition",3,false,true,null,null),
            Buff("catalog",2,false,false,true,null),Buff("self",12,false,false,false,true),Buff("inactive",0,null,null,null,null)};
        assert(NectarCounterPresentation.EligibleTypes(active.Concat(exclusions))==3,"Hidden/ignored/self counters and inactive types are excluded");
        assert(NectarCounterPresentation.EligibleTypes(new[]{Buff("unknown",2,false,false,null,false)})==null,"Missing active eligibility metadata is unknown");
        assert(NectarCounterPresentation.EligibleTypes(null)==null&&NectarCounterPresentation.EligibleTypes(Array.Empty<NectarBuffObservation>())==0,"Missing snapshot is distinguished from a verified empty set");
        var seven=Enumerable.Range(0,7).Select(i=>Buff("type:"+i,i+1)).ToArray();
        var one=NectarCounterPresentation.Create(1,seven,4,4,true,true);
        assert(one.EligibleTypes==7&&one.Used==4&&one.ReferenceCap==4&&one.BonusPercent==16,"One copy uses four eligible types for its active reference component");
        assert(one.Compact.Contains("Effective buffs: 4 / 4")&&one.Compact.Contains("Eligible types: 7 (over cap)")&&one.Compact.Contains("+16%"),"Compact counter includes over-cap eligibility and known active bonus");
        var two=NectarCounterPresentation.Create(2,seven,7,8,true,true);
        assert(two.Used==7&&two.ReferenceCap==8&&two.BonusPercent==28&&!two.Discrepancy,"Capacity grows four per effective copy");
        var inactive=NectarCounterPresentation.Create(1,active,3,4,false,true);
        assert(inactive.BonusPercent==0&&inactive.Compact.Contains("(inactive)"),"An absent self counter cannot claim its active bonus");
        var unavailable=NectarCounterPresentation.Create(null,null,null,null,null,false);
        assert(unavailable.Used==null&&unavailable.ReferenceCap==null&&unavailable.BonusPercent==null&&unavailable.Compact.Contains("Unknown"),"Unavailable formula/counter/metadata never produce a guessed total");
        var capMismatch=NectarCounterPresentation.Create(1,seven,6,6,true,true);
        assert(capMismatch.Discrepancy&&capMismatch.Used==6&&capMismatch.BonusPercent==null,"Unexpected live cap preserves the observation and declines reference bonus");
        var countMismatch=NectarCounterPresentation.Create(1,active,4,4,true,true);
        assert(countMismatch.Discrepancy&&countMismatch.Used==4&&countMismatch.Compact.Contains("game counter shown"),"Count/model discrepancies retain the game counter");
        var bad=NectarCounterPresentation.Create(-1,active,-1,-2,null,true);
        assert(bad.ReferenceCap==null&&bad.Used==null&&bad.BonusPercent==null,"Invalid counts remain unknown");
        assert(NectarCounterPresentation.Create(long.MaxValue,active,1,4,true,true).ReferenceCap==null,"Unsupported cap arithmetic cannot overflow");
        var source=OriginalItemDefinitions.Find("BleedOnHit");
        foreach(long copies in new[]{1L,4L,10L})
        {
            var single=source.CreateTerms("Original native title",1);var current=source.CreateTerms("Original native title",copies);
            string details=Plain(IndividualItemDetails.Create(single,current,copies,copies,source.TriggerContext));
            assert(details.Contains("Next copy:")&&!details.Contains("Original native title"),"Native title/description are not repeated and next-copy benefit remains");
            assert(details.Contains("Current stack")== (copies>1),"Combined stack block appears only above one effective copy");
            assert(details.Contains("Single-copy components")== (copies>1),"At one copy the native individual description is retained without a redundant added block");
            if(copies>1)assert(details.IndexOf("Single-copy components")<details.IndexOf("Current stack"),"Individual components precede combined stack components");
        }
        assert(IndividualItemDetails.Create(null,null,2,2)==""&&IndividualItemDetails.Create(null,null,1,1)=="","Unregistered math leaves the native description intact");
        var distinct=Plain(IndividualItemDetails.Create(source.CreateTerms("Item",1),source.CreateTerms("Item",3),3,1));
        assert(distinct.Contains("1 owned; 3 effective"),"Stack math uses effective copies while preserving ownership distinction");
        var nectarDefinition=OriginalItemDefinitions.Find("BoostAllStats");
        var nectarTerms=nectarDefinition.CreateTerms("Nectar",1,7);
        assert(nectarTerms[1].CurrentValue==16&&nectarTerms[1].NextValue==28,"Existing next-copy model uses the same distinct type set and new capacity");
        var interaction=new DetailsInteraction();object identity=new object();
        interaction.Enter(identity,"item:nectar");interaction.PopupEnter();interaction.Toggle(true,true);
        assert(!interaction.Enter(identity,"item:nectar")&&interaction.Expanded,"Dynamic same-item counter updates preserve open Details");
        Console.WriteLine("Visual revision: 256 collapse states; "+sizing+" footer/scroll sizing cases; float residuals; Nectar eligibility/cap/unknowns; individual-before-stack Details.");
    }
}

