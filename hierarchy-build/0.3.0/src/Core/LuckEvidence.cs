using System;
namespace Alexander.CharacterSheet.Core
{
    public enum LuckEvidenceState { Unsupported,Unverified,Confirmed,ReportedBug }
    public static class LuckEvidence
    {
        public const string SaleStarEffectId="LowerPricedChests.ExtraItemRoll";
        public const string SaleStarReportSource="https://riskofrain2.wiki.gg/wiki/57_Leaf_Clover";
        public const string SaleStarReportDate="2026-10-05";
        public const string SaleStarReport="Reported issue: Clover/Sale Star extra-item interaction is reported as not working. Installed build behavior is unverified; no luck-adjusted probability is claimed.";
        public static string Label(LuckEvidenceState state)=>state==LuckEvidenceState.ReportedBug?"Reported bug / unverified":
            state==LuckEvidenceState.Confirmed?"Confirmed scope":state==LuckEvidenceState.Unverified?"Unverified scope":"Unsupported effect";
        public static double? AdjustedProbability(double baseChance,double luck,LuckApplicability applicability,LuckEvidenceState state)
        {
            if(state!=LuckEvidenceState.Confirmed||!NumberFormatting.IsFinite(baseChance)||!NumberFormatting.IsFinite(luck))return null;
            if(applicability==LuckApplicability.Affected)return LuckMath.EffectiveChance(baseChance,luck);
            if(applicability==LuckApplicability.Unaffected)return Math.Max(0,Math.Min(1,baseChance));
            return null; // Mixed scopes need a separately identified roll.
        }
    }
}
