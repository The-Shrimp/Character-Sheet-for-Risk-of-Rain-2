using System;
using Alexander.CharacterSheet.Core;
static class ItemTooltipTests
{
    static ItemScalingInput Example(long? copies) => new ItemScalingInput {
        Name="Prayer Beads",Verb="Stores",ValueLabel="of level-up stat gains",CopySingular="bead",CopyPlural="beads",
        OwnedCopies=copies,BaseValue=20,PerAdditionalCopy=5,Unit=ValueUnit.Percent,Evidence=PredictionEvidence.VerifiedComponent,
        Conditions="Supplied design example; all copies assumed to count.",Luck=LuckApplicability.Unaffected,
        LuckDetail="The shown additive scaling term is not a random roll; other effects are unverified."
    };
    public static void Run(Action<bool,string> assert)
    {
        var item=ItemTooltipPresentation.Create(Example(4));
        assert(item.CurrentValue==35,"Four copies use three additional copies:20+5*3, not20+5*4");
        assert(item.AdditionalCopies==3&&item.CountLabel.Contains("4 total beads | 3 additional"),"Total and additional counts are explicitly distinct");
        assert(item.Headline=="Stores 35% (20 + 5 \u00d7 each additional item) of level-up stat gains","Scaling stays inline with the current amount and additional-item wording");
        assert(item.Equation=="20% base + 5% \u00d7 3 additional beads","Scaling equation includes the base and exact additional count");
        assert(item.NextValue==40&&item.NextLabel.Contains("40% (+5 percentage points)"),"Next-copy total and marginal points stay separate");
        assert(item.NextEquation.Contains("5% \u00d7 4 additional beads = 40%"),"Next copy uses four additional copies rather than five");
        assert(item.FullMath.Contains("4 - 1 = 3")&&item.FullMath.Contains("15%"),"Expanded detail exposes the off-by-one and contribution");
        assert(item.LuckMark=="x"&&item.LuckText.Contains("Unaffected")&&item.LuckText.Contains("shown additive"),"Luck badge has text and applies only to the shown term");
        assert(TooltipPresentation.Full(item.Content).Contains("20% base"),"Detailed tooltip includes the separate per-copy breakdown");
        assert(TooltipPresentation.Short(item.Content).Contains("Stores 35%"),"Short tooltip preserves the prominent current amount");
        item=ItemTooltipPresentation.Create(Example(1));
        assert(item.CurrentValue==20&&item.AdditionalCopies==0&&item.NextValue==25,"First copy supplies base alone");
        assert(item.Equation.Contains("0 additional beads"),"First-copy equation does not invent an extra copy");
        item=ItemTooltipPresentation.Create(Example(2));
        assert(item.CurrentValue==25&&item.Equation.Contains("1 additional bead"),"Single additional copy is counted and labeled correctly");
        item=ItemTooltipPresentation.Create(Example(0));
        assert(item.CurrentValue==0&&item.NextValue==20&&item.Headline=="No owned copies","An absent item does not activate the first-copy base");
        assert(item.FullMath.Contains("no active base"),"Zero-copy expanded detail explains why base is inactive");
        item=ItemTooltipPresentation.Create(Example(null));
        assert(item.CurrentValue==null&&item.NextValue==null&&item.CountLabel.Contains("Unavailable"),"Unknown copies cannot predict a current or next value");
        var input=Example(4);input.Evidence=PredictionEvidence.Unavailable;
        item=ItemTooltipPresentation.Create(input);
        assert(item.CurrentValue==null&&item.NextValue==null&&item.Equation.Contains("unverified"),"Supplied numbers alone do not certify a formula");
        input=Example(4);input.BaseValue=double.NaN;
        item=ItemTooltipPresentation.Create(input);
        assert(item.CurrentValue==null&&item.NextValue==null,"Nonfinite coefficients cannot render effect claims");
        input=Example(-1);
        item=ItemTooltipPresentation.Create(input);
        assert(item.CurrentValue==null&&item.CountLabel.Contains("Unavailable"),"Invalid count remains unavailable rather than fabricated zero");
        input=Example(long.MaxValue);
        item=ItemTooltipPresentation.Create(input);
        assert(item.NextValue==null&&item.NextLabel.Contains("Unavailable")&&item.Comparison.Confidence.Contains("range"),"Next-copy count overflow is disclosed");
        input=Example(4);input.BaseValue=double.MaxValue;input.PerAdditionalCopy=double.MaxValue;
        item=ItemTooltipPresentation.Create(input);
        assert(item.CurrentValue==null&&item.NextValue==null,"Arithmetic overflow does not render infinity");
        input=Example(4);input.Luck=LuckApplicability.Mixed;
        item=ItemTooltipPresentation.Create(input);
        assert(item.LuckMark=="mix"&&item.LuckText.Contains("Mixed"),"Mixed luck is explicit rather than color-only");
        input=Example(4);input.Luck=LuckApplicability.Unknown;
        item=ItemTooltipPresentation.Create(input);
        assert(item.LuckMark=="?"&&item.LuckText.Contains("Unverified"),"Unknown luck stays unverified");
        input=Example(4);input.Evidence=PredictionEvidence.ConditionalEstimate;
        item=ItemTooltipPresentation.Create(input);
        assert(item.Comparison.Confidence.Contains("Conditional estimate"),"Conditional input is not promoted to verified");
    }
}
