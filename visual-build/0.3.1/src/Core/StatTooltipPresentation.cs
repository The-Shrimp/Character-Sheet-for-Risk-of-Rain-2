using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    // Display groups never recalculate or reorder Calculation.Terms.
    public static class StatTooltipPresentation
    {
        static bool Flat(CalculationStage stage)=>stage==CalculationStage.Flat||stage==CalculationStage.LateFlat||stage==CalculationStage.Final;
        public static string Create(double live,double perLevelGrowth,Calculation calculation,string semanticStyle,
            string suffix="",string growthSource="Level growth (level - 1)")
        {
            if(calculation==null)throw new ArgumentNullException(nameof(calculation));
            var baseline=calculation.Terms.First(t=>t.Stage==CalculationStage.Baseline);
            string growth=(perLevelGrowth<0?" - ":" + ")+NumberFormatting.Display(Math.Abs(perLevelGrowth))+suffix+" per level";
            var lines=new List<string>{
                "Total: "+TooltipMarkup.Style(NumberFormatting.Display(live)+suffix,semanticStyle),
                "Base: "+TooltipMarkup.Style(NumberFormatting.Display(baseline.Operand)+suffix+growth,"cStack")
            };
            var bonuses=new List<string>();
            var terms=calculation.Terms.Where(t=>t.Stage!=CalculationStage.Baseline&&
                !(t.Stage==CalculationStage.Flat&&string.Equals(t.Source,growthSource,StringComparison.Ordinal))).ToArray();
            foreach(var term in terms.Where(t=>Flat(t.Stage)&&t.Operand!=0))
                bonuses.Add(TooltipMarkup.Literal(term.Source)+": "+TooltipMarkup.Style(NumberFormatting.Signed(term.Operand)+suffix,semanticStyle));
            foreach(var term in terms.Where(t=>!Flat(t.Stage)))
            {
                double percent=(term.Stage==CalculationStage.AdditiveMultiplier?term.Operand:term.Operand-1)*100;
                if(percent!=0)bonuses.Add(TooltipMarkup.Literal(term.Source)+": "+TooltipMarkup.Style(NumberFormatting.Signed(percent)+"%",semanticStyle));
            }
            // This is a difference after the known operations, not an inferred early flat source.
            double residual=DisplayResidual.ForDisplay(live,calculation.Predicted);
            if(!NumberFormatting.IsFinite(residual)||residual!=0)
                bonuses.Add("Other/unknown: "+TooltipMarkup.Style(NumberFormatting.Signed(residual)+suffix,"cStack"));
            if(bonuses.Count==0)lines.Add("Bonuses: None");
            else {lines.Add("Bonuses:");lines.AddRange(bonuses);}
            return string.Join("\n",lines);
        }
    }
}
