using System;
using Alexander.CharacterSheet.Core;
static class CompactSheetLayoutTests
{
    public static void Run(Action<bool,string> assert)
    {
        var sample=new CompactSheetMetrics(18,1);var own=new UiRect(462,717,995,74);
        var skills=new UiRect(1260,0,650,150);UiRect dock;
        assert(CompactSheetPlacement.TryBelowOwn(1920,1080,own,new[]{own,skills},sample.Width,
            sample.PanelHeight(1080,8,0,false),sample.MinWidth,sample.MinHeight,12,out dock),"Screenshot geometry supports compact below-own placement");
        assert(dock.X==462&&Math.Abs(dock.Top-705)<1e-9&&dock.Height<340,"Sample card aligns with own strip and removes detached 510-unit height");
        assert(!dock.Overlaps(own)&&!dock.Overlaps(skills),"Sample card never covers native scoreboard or bottom-right skills");
        foreach(var resolution in new[]{new[]{1280,720},new[]{1366,768},new[]{1920,1080},new[]{2560,1440},new[]{3440,1440}})
        foreach(double scale in new[]{.7,.85,1,1.2})
        foreach(double font in new[]{18.0,24.0})
        foreach(bool expanded in new[]{false,true})
        {
            double w=resolution[0],h=resolution[1];var metrics=new CompactSheetMetrics(font,scale);
            var row=new UiRect(w*.24,h*.664,w*.52,h*.069);var skill=new UiRect(w*.75,0,w*.25,h*.14);
            var health=new UiRect(w*.28,0,w*.44,h*.13);
            double height=metrics.PanelHeight(h,8,expanded?60:0,expanded);
            bool fits=CompactSheetPlacement.TryBelowOwn(w,h,row,new[]{row,skill,health},metrics.Width,height,
                metrics.MinWidth,metrics.MinHeight,12*scale,out dock);
            assert(fits,"Common resolution, typography and UI scale supports a safe compact card");
            assert(dock.X>=0&&dock.Y>=0&&dock.Right<=w&&dock.Top<=h,"Common-scale card stays inside owned canvas");
            assert(!dock.Overlaps(row)&&!dock.Overlaps(skill)&&!dock.Overlaps(health)&&dock.Top<row.Y,"Common-scale card stays below scoreboard and away from skills and health HUD");
            assert(Math.Abs(dock.X-row.X)<1e-9,"Common-scale card stays in the central native composition");
            assert(!expanded||dock.Height<=h*.45+1e-9,"Large effect list has bounded scrolling height");
        }
        var second=new UiRect(462,617,995,74);
        assert(CompactSheetPlacement.TryBelowOwn(1920,1080,own,new[]{own,second,skills},sample.Width,
            sample.ContentHeight(8,0),sample.MinWidth,sample.MinHeight,12,out dock)&&dock.Top<=second.Y-12,
            "Another native player row displaces the card below it without overlap");
        var bottomHud=new UiRect(450,0,420,480);
        assert(CompactSheetPlacement.TryBelowOwn(1920,1080,own,new[]{own,bottomHud},sample.Width,
            sample.ContentHeight(8,0),sample.MinWidth,sample.MinHeight,12,out dock)&&dock.Y>=bottomHud.Top+12&&dock.Height<sample.ContentHeight(8,0),
            "Available gap clips viewport height without covering bottom HUD");
        assert(!CompactSheetPlacement.TryBelowOwn(1920,1080,own,new[]{own,new UiRect(450,0,420,700)},sample.Width,
            sample.ContentHeight(8,0),sample.MinWidth,sample.MinHeight,12,out dock),"No central gap declines card instead of docking into a HUD corner");
        assert(!CompactSheetPlacement.TryBelowOwn(1920,1080,new UiRect(462,717,200,74),new[]{own},sample.Width,
            sample.ContentHeight(8,0),sample.MinWidth,sample.MinHeight,12,out dock),"Narrow native owner region cannot create an overlapping wider card");
        assert(!CompactSheetPlacement.TryBelowOwn(double.NaN,1080,own,new[]{own},sample.Width,300,280,140,12,out dock)&&
            !CompactSheetPlacement.TryBelowOwn(1920,1080,default(UiRect),new[]{own},sample.Width,300,280,140,12,out dock),
            "Unknown owner or nonfinite canvas fails closed");
        assert(!CompactSheetPlacement.TryBelowOwn(1920,1080,own,new[]{new UiRect(0,0,-1,10)},sample.Width,300,280,140,12,out dock),
            "Invalid occupied geometry is not ignored");
        assert(sample.ContentHeight(8,0)<sample.ContentHeight(8,10)&&sample.PanelHeight(1080,8,80,true)==486,
            "Collapsed height fits rows and expanded content is capped independently of effect count");
        assert(new CompactSheetMetrics(40,1).RowHeight==new CompactSheetMetrics(24,1).RowHeight,
            "Unexpected native heading font does not make all body rows oversized");
        assert(Math.Abs(new CompactSheetMetrics(18,(double).7f).RowHeight-18.2)<1e-5&&
            Math.Abs(new CompactSheetMetrics(18,(double)1.2f).RowHeight-31.2)<1e-5,
            "Native float UI-scale endpoints remain valid after conversion to geometry doubles");
    }
}
