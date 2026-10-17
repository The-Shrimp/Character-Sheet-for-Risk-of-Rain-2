using System;
namespace Alexander.CharacterSheet.Core
{
    // Original presentation template for a caller-supplied additive scaling example.
    // It does not identify items, obtain game data, or verify any game formula.
    public sealed class ItemScalingInput
    {
        public string Name, Verb="Provides", ValueLabel="", CopySingular="copy", CopyPlural="copies", Conditions="";
        public long? OwnedCopies;
        public double? BaseValue, PerAdditionalCopy;
        public Func<long,double> ComponentAtCopies;
        public string ComponentFormula;
        public ValueUnit Unit=ValueUnit.Percent;
        public PredictionEvidence Evidence=PredictionEvidence.Unavailable;
        public LuckApplicability Luck=LuckApplicability.Unknown;
        public string LuckDetail="Applicability is unverified.";
    }
    public sealed class ItemTooltipView
    {
        public readonly string Title, Headline, Equation, CountLabel, NextLabel, NextEquation, FullMath, LuckMark, LuckText;
        public readonly double? CurrentValue, NextValue;
        public readonly long? AdditionalCopies;
        public readonly TooltipContent Content;
        public readonly ComparisonView Comparison;
        internal ItemTooltipView(string title,string headline,string equation,string countLabel,string nextLabel,string nextEquation,
            string fullMath,string luckMark,string luckText,double? current,double? next,long? additional,TooltipContent content,ComparisonView comparison)
        {Title=title;Headline=headline;Equation=equation;CountLabel=countLabel;NextLabel=nextLabel;NextEquation=nextEquation;
         FullMath=fullMath;LuckMark=luckMark;LuckText=luckText;CurrentValue=current;NextValue=next;AdditionalCopies=additional;Content=content;Comparison=comparison;}
    }
    public static class ItemTooltipPresentation
    {
        static bool Known(double? number) => number.HasValue&&NumberFormatting.IsFinite(number.Value);
        static string Noun(long count,ItemScalingInput input) => TooltipText.SingleLine(count==1 ? input.CopySingular : input.CopyPlural);
        static string Value(double value,ItemScalingInput input) => NumberFormatting.Exact(value)+ComparisonPresentation.UnitSuffix(input.Unit,false);
        public static ItemTooltipView Create(ItemScalingInput input)
        {
            if(input==null)throw new ArgumentNullException(nameof(input));
            string title=TooltipText.SingleLine(input.Name);
            if(title.Length==0)title="Unnamed item";
            bool knownCount=input.OwnedCopies.HasValue&&input.OwnedCopies.Value>=0;
            bool custom=input.ComponentAtCopies!=null;
            bool knownFormula=input.Evidence!=PredictionEvidence.Unavailable&&(custom||Known(input.BaseValue)&&Known(input.PerAdditionalCopy));
            long count=knownCount ? input.OwnedCopies.Value : 0;
            long? additional=knownCount ? (long?)Math.Max(0,count-1) : null;
            double? current=null,next=null;
            if(knownCount&&knownFormula)
            {
                double value=count==0 ? 0 : custom ? input.ComponentAtCopies(count) : input.BaseValue.Value+input.PerAdditionalCopy.Value*additional.Value;
                if(NumberFormatting.IsFinite(value))current=value;
                if(count<long.MaxValue)
                {
                    double valueNext=custom ? input.ComponentAtCopies(count+1) : input.BaseValue.Value+input.PerAdditionalCopy.Value*count;
                    if(NumberFormatting.IsFinite(valueNext))next=valueNext;
                }
            }
            var comparison=ComparisonPresentation.Create(new ComparisonInput {
                Current=current,Next=next,Unit=input.Unit,CurrentBasis=ComparisonBasis.Component,NextBasis=ComparisonBasis.Component,
                Evidence=next.HasValue ? input.Evidence : PredictionEvidence.Unavailable,Conditions=input.Conditions,
                UnavailableReason=!knownCount ? "copy count unavailable" : !knownFormula ? "scaling formula unverified" :
                    count==long.MaxValue ? "next copy count exceeds supported range" : "numeric range exceeded"
            });
            string suffix=ComparisonPresentation.UnitSuffix(input.Unit,false);
            string headline=current.HasValue ? count==0 ? "No owned copies" :
                TooltipText.SingleLine(input.Verb)+" "+NumberFormatting.Display(current)+suffix+
                    (custom ? " ("+TooltipText.SingleLine(input.ComponentFormula)+")" : " ("+NumberFormatting.Exact(input.BaseValue.Value)+
                    (input.PerAdditionalCopy.Value>=0 ? " + " : " - ")+NumberFormatting.Exact(Math.Abs(input.PerAdditionalCopy.Value))+
                    " \u00d7 each additional item)")+
                    (TooltipText.SingleLine(input.ValueLabel).Length>0 ? " "+TooltipText.SingleLine(input.ValueLabel) : "") : "Current effect: Unavailable";
            string equation="Scaling formula unverified",nextEquation="Next scaling unavailable",fullMath="No quantitative prediction is supported.";
            if(knownFormula&&custom)
            {
                equation=TooltipText.SingleLine(input.ComponentFormula);
                nextEquation=next.HasValue ? equation+" at "+NumberFormatting.Count(count+1)+" copies = "+Value(next.Value,input) : "Next scaling unavailable";
                fullMath="Component formula: "+equation+"\n"+(current.HasValue ? "At "+NumberFormatting.Count(count)+" copies: "+Value(current.Value,input) : "Current component unavailable")+
                    "\nNext-copy equation: "+nextEquation+"\n"+comparison.DifferenceExact;
            }
            else if(knownFormula)
            {
                string baseText=Value(input.BaseValue.Value,input);
                string increment=Value(input.PerAdditionalCopy.Value,input);
                string operation=input.PerAdditionalCopy.Value>=0 ? " + " : " - ";
                string magnitude=Value(Math.Abs(input.PerAdditionalCopy.Value),input);
                equation=knownCount&&count>0 ? baseText+" base"+operation+magnitude+" \u00d7 "+NumberFormatting.Count(additional)+" additional "+Noun(additional.Value,input) :
                    "First copy: "+baseText+" base; then "+increment+" per additional "+TooltipText.SingleLine(input.CopySingular);
                if(next.HasValue)nextEquation=baseText+operation+magnitude+" \u00d7 "+NumberFormatting.Count(count)+" additional "+Noun(count,input)+" = "+Value(next.Value,input);
                fullMath="Base (first copy): "+baseText+"\n"+
                    (knownCount ? "Additional copies: "+NumberFormatting.Count(count)+" - 1 = "+NumberFormatting.Count(additional)+
                        (count==0 ? " (clamped to zero; no active base)" : "") : "Additional copies: Unavailable");
                if(current.HasValue&&count>0)
                    fullMath+="\nAdditional contribution: "+increment+" \u00d7 "+NumberFormatting.Count(additional)+" = "+
                        Value(input.PerAdditionalCopy.Value*additional.Value,input)+"\nCurrent total: "+Value(current.Value,input);
                else if(knownCount&&count==0)fullMath+="\nCurrent component: 0; first copy activates the base.";
                fullMath+="\nNext-copy equation: "+nextEquation+"\n"+comparison.DifferenceExact;
            }
            string countLabel=knownCount ? NumberFormatting.Count(count)+" total "+Noun(count,input)+" | "+NumberFormatting.Count(additional)+" additional" : "Copy count: Unavailable";
            string nextLabel=next.HasValue ? "Next copy: "+Value(next.Value,input)+
                (comparison.Direction!=ValueDirection.Unknown ? " ("+NumberFormatting.Signed(next.Value-current.Value)+
                    ComparisonPresentation.UnitSuffix(input.Unit,true)+")" : "") : "Next copy: Unavailable";
            string luckMark=input.Luck==LuckApplicability.Affected ? "\u2713" : input.Luck==LuckApplicability.Unaffected ? "x" :
                input.Luck==LuckApplicability.Mixed ? "mix" : "?";
            string luckText="Luck: "+InventoryPresentation.LuckLabel(input.Luck)+"\n"+TooltipText.Clean(input.LuckDetail);
            var content=new TooltipContent(title,headline, countLabel,nextLabel,comparison.Difference,comparison.Confidence,
                new[]{new TooltipSection("Scaling",equation),new TooltipSection("Per-copy math",fullMath),
                    new TooltipSection("Conditions",input.Conditions),new TooltipSection("Luck",luckText)},
                detailedCurrent:comparison.CurrentExact,detailedNext:comparison.NextExact,detailedDifference:comparison.DifferenceExact);
            return new ItemTooltipView(title,headline,equation,countLabel,nextLabel,nextEquation,fullMath,luckMark,luckText,current,next,additional,content,comparison);
        }
    }
}
