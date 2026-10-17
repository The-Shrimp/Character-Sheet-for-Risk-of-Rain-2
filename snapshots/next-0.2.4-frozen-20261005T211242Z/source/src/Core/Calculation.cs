using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Alexander.CharacterSheet.Core
{
    public enum CalculationStage { Baseline, Flat, AdditiveMultiplier, Multiplier, LateFlat, Conversion, LateMultiplier, Final, FinalConversion }
    public sealed class CalculationTerm
    {
        public string Source;
        public CalculationStage Stage;
        public double Operand;
        public double Before, After;
    }

    public sealed class Calculation
    {
        readonly List<CalculationTerm> terms = new List<CalculationTerm>();
        public IReadOnlyList<CalculationTerm> Terms => terms;
        public double Predicted { get; private set; }
        double additiveBase, additiveAmount;

        public Calculation(string source, double baseline)
        {
            Predicted = baseline;
            terms.Add(new CalculationTerm { Source = source, Stage = CalculationStage.Baseline,
                Operand = baseline, After = baseline });
        }

        public void Apply(string source, CalculationStage stage, double operand)
        {
            if (stage < terms.Last().Stage) throw new InvalidOperationException("Calculation stages must follow the verified game order.");
            double before = Predicted;
            if (stage == CalculationStage.Flat || stage == CalculationStage.LateFlat || stage == CalculationStage.Final) Predicted += operand;
            else if (stage == CalculationStage.AdditiveMultiplier)
            {
                if (terms.Last().Stage != CalculationStage.AdditiveMultiplier) { additiveBase = before; additiveAmount = 0; }
                additiveAmount += operand;
                Predicted = additiveBase * (1 + additiveAmount);
            }
            else if (stage == CalculationStage.Multiplier || stage == CalculationStage.Conversion || stage == CalculationStage.LateMultiplier || stage == CalculationStage.FinalConversion) Predicted *= operand;
            else throw new ArgumentException("Baseline is only supplied to the constructor.");
            terms.Add(new CalculationTerm { Source = source, Stage = stage, Operand = operand, Before = before, After = Predicted });
        }

        public string Describe(double live, bool complete, bool detailed)
        {
            Func<double,string> format=value=>detailed?NumberFormatting.Exact(value):NumberFormatting.Display(value);
            string text=string.Join("\n",terms.Select(t=>
                t.Source+": "+format(t.After)+(detailed&&t.Stage!=CalculationStage.Baseline
                    ? " ("+format(t.Before)+" -> "+format(t.After)+"; "+t.Stage+")" : "")));
            var attribution=AttributionPresentation.Create(live,Predicted,complete);
            if(attribution.Quality!=AttributionQuality.Complete)
                text+="\nIncomplete attribution: unmodeled effects/conversions/hooks.\nLive \u2212 modeled: "+(live>Predicted?"+":"")+format(live-Predicted);
            return text+"\nAuthoritative live total: "+format(live);
        }
    }

    public static class LuckMath
    {
        // Verified Util.CheckRoll uses CeilToInt(Abs(luck)), not fractional interpolation.
        public static double EffectiveChance(double chance, double luck)
        {
            if (double.IsNaN(chance) || double.IsNaN(luck) || double.IsInfinity(luck))
                throw new ArgumentOutOfRangeException();
            double p = Math.Max(0, Math.Min(1, chance));
            double rolls = 1 + Math.Ceiling(Math.Abs(luck));
            return luck >= 0 ? 1 - Math.Pow(1 - p, rolls) : Math.Pow(p, rolls);
        }
    }
}
