using System;
namespace Alexander.CharacterSheet.Core
{
    public enum ValueUnit { None, Health, PerSecond, Percent, PercentagePoints, Multiplier, Seconds, Meters }
    public enum ComparisonBasis { Component, Total }
    public enum PredictionEvidence { Unavailable, VerifiedComponent, ConditionalEstimate }
    public enum ValueDirection { Unknown, Increase, Decrease, NoImmediateChange }
    public sealed class ComparisonInput
    {
        public double? Current, Next;
        public ValueUnit Unit;
        public ComparisonBasis CurrentBasis, NextBasis;
        public PredictionEvidence Evidence;
        public bool SameConditions = true;
        public string Conditions, UnavailableReason;
    }
    public sealed class ComparisonView
    {
        public readonly string Current, Next, Difference, Confidence, CurrentExact, NextExact, DifferenceExact;
        public readonly ValueDirection Direction;
        internal ComparisonView(string current, string next, string difference, string confidence, ValueDirection direction,
            string currentExact=null,string nextExact=null,string differenceExact=null)
        { Current=current; Next=next; Difference=difference; Confidence=confidence; Direction=direction;
          CurrentExact=currentExact??current;NextExact=nextExact??next;DifferenceExact=differenceExact??difference; }
    }
    public static class ComparisonPresentation
    {
        public static string UnitSuffix(ValueUnit unit, bool difference)
        {
            switch (unit)
            {
                case ValueUnit.Health: return " health";
                case ValueUnit.PerSecond: return " /s";
                case ValueUnit.Percent: return difference ? " percentage points" : "%";
                case ValueUnit.PercentagePoints: return " percentage points";
                case ValueUnit.Multiplier: return "x";
                case ValueUnit.Seconds: return " s";
                case ValueUnit.Meters: return " m";
                default: return "";
            }
        }
        static bool Known(double? value) => value.HasValue && NumberFormatting.IsFinite(value.Value);
        public static ComparisonView Create(ComparisonInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            string current=Known(input.Current) ? "Current: " + NumberFormatting.Display(input.Current) + UnitSuffix(input.Unit,false) : "Current: Unavailable";
            string currentExact=Known(input.Current) ? "Current: "+NumberFormatting.Exact(input.Current)+UnitSuffix(input.Unit,false) : current;
            string reason=TooltipText.SingleLine(input.UnavailableReason);
            if (input.Evidence==PredictionEvidence.Unavailable || !Known(input.Next))
                return new ComparisonView(current,"Next copy: Unavailable","Benefit unknown",
                    "Unverified" + (reason.Length>0 ? ": "+reason : ": no supported prediction"),ValueDirection.Unknown,currentExact);
            if (input.Evidence==PredictionEvidence.VerifiedComponent && input.NextBasis!=ComparisonBasis.Component)
                return new ComparisonView(current,"Next copy: Unavailable","Comparison unavailable: inconsistent evidence",
                    "Unverified: component evidence cannot describe a total prediction",ValueDirection.Unknown,currentExact);
            string confidence=input.Evidence==PredictionEvidence.VerifiedComponent ? "Verified component; final benefit can differ" : "Conditional estimate; final benefit can differ";
            string conditions=TooltipText.SingleLine(input.Conditions);
            if (conditions.Length>0) confidence += ". Conditions: " + conditions;
            string next="Next copy ("+(input.Evidence==PredictionEvidence.VerifiedComponent ? "component" : "estimate")+"): "+
                NumberFormatting.Display(input.Next)+UnitSuffix(input.Unit,false);
            string nextExact="Next copy ("+(input.Evidence==PredictionEvidence.VerifiedComponent ? "component" : "estimate")+"): "+
                NumberFormatting.Exact(input.Next)+UnitSuffix(input.Unit,false);
            if (!Known(input.Current))
                return new ComparisonView(current,next,"Comparison unavailable: current value unknown",confidence,ValueDirection.Unknown,currentExact,nextExact);
            if (!input.SameConditions)
                return new ComparisonView(current,next,"Comparison unavailable: conditions differ",confidence,ValueDirection.Unknown,currentExact,nextExact);
            if (input.CurrentBasis!=input.NextBasis)
                return new ComparisonView(current,next,"Comparison unavailable: component and total differ",confidence,ValueDirection.Unknown,currentExact,nextExact);
            double delta=input.Next.Value-input.Current.Value;
            if (!NumberFormatting.IsFinite(delta))
                return new ComparisonView(current,next,"Comparison unavailable: numeric range exceeded",confidence,ValueDirection.Unknown,currentExact,nextExact);
            ValueDirection direction=delta>0 ? ValueDirection.Increase : delta<0 ? ValueDirection.Decrease : ValueDirection.NoImmediateChange;
            string difference=delta==0 ? "No immediate change under these conditions" :
                "Change ("+(input.NextBasis==ComparisonBasis.Component ? "component" : "estimate")+"): "+
                NumberFormatting.Signed(delta)+UnitSuffix(input.Unit,true);
            string differenceExact=delta==0 ? difference : "Change ("+(input.NextBasis==ComparisonBasis.Component ? "component" : "estimate")+"): "+
                (delta>0 ? "+" : "")+NumberFormatting.Exact(delta)+UnitSuffix(input.Unit,true);
            return new ComparisonView(current,next,difference,confidence,direction,currentExact,nextExact,differenceExact);
        }
    }
    public enum AttributionQuality { Complete, Partial, Unavailable }
    public sealed class AttributionView
    {
        public readonly string Live, Modeled, Difference, Status;
        public readonly AttributionQuality Quality;
        internal AttributionView(string live,string modeled,string difference,string status,AttributionQuality quality)
        { Live=live; Modeled=modeled; Difference=difference; Status=status; Quality=quality; }
    }
    public static class AttributionPresentation
    {
        public static AttributionView Create(double? live, double? modeled, bool declaredComplete, double tolerance=0)
        {
            if (!NumberFormatting.IsFinite(tolerance)||tolerance<0) throw new ArgumentOutOfRangeException(nameof(tolerance));
            string liveText="Live total: "+NumberFormatting.Display(live);
            if (!live.HasValue || !NumberFormatting.IsFinite(live.Value))
                return new AttributionView(liveText,"Modeled subtotal: "+NumberFormatting.Display(modeled),
                    "Difference unavailable","Live value unavailable",AttributionQuality.Unavailable);
            if (!modeled.HasValue || !NumberFormatting.IsFinite(modeled.Value))
                return new AttributionView(liveText,"Modeled subtotal: Unavailable","Difference unavailable",
                    "Attribution unavailable",AttributionQuality.Unavailable);
            double residual=live.Value-modeled.Value;
            if (!NumberFormatting.IsFinite(residual))
                return new AttributionView(liveText,"Modeled subtotal: "+NumberFormatting.Display(modeled),
                    "Difference unavailable","Incomplete attribution: numeric range exceeded",AttributionQuality.Partial);
            bool matches=Math.Abs(residual)<=tolerance;
            bool complete=declaredComplete&&matches;
            return new AttributionView(liveText,"Modeled subtotal: "+NumberFormatting.Display(modeled),
                "Live - modeled: "+NumberFormatting.Signed(residual),
                complete ? "Attribution marked complete; totals match within tolerance" : "Incomplete attribution; live total remains authoritative",
                complete ? AttributionQuality.Complete : AttributionQuality.Partial);
        }
    }
}
