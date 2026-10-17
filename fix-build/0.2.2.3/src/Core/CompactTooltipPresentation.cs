using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace Alexander.CharacterSheet.Core
{
    // Only native named styles and native description formatting can survive this boundary.
    // Own strings are literal before being placed into markup. No generated RGB palette.
    public static class TooltipMarkup
    {
        static readonly Regex StyleTag=new Regex(@"^style\s*=\s*[""']?([A-Za-z][A-Za-z0-9_]{0,63})[""']?$",RegexOptions.IgnoreCase);
        static readonly Regex ColorTag=new Regex(@"^color\s*=\s*[""']?(#[0-9a-fA-F]{6}(?:[0-9a-fA-F]{2})?)[""']?$",RegexOptions.IgnoreCase);
        static string Escape(string value)=>(value??"").Replace("<","\u2039").Replace(">","\u203a");
        public static string Literal(string value)=>Escape(TooltipText.Clean(value));
        public static string Style(string value,string style)=>"<style="+style+">"+Literal(value)+"</style>";
        public static string Render(string value,Func<string,bool> availableStyle)
        {
            var result=new StringBuilder();var scopes=new Stack<KeyValuePair<string,string>>();
            value=TooltipText.Clean(value);int start=0;
            foreach(Match tag in Regex.Matches(value,"<[^>]*>"))
            {
                result.Append(Escape(value.Substring(start,tag.Index-start)));start=tag.Index+tag.Length;
                string token=tag.Value.Substring(1,tag.Length-2).Trim(),lower=token.ToLowerInvariant();
                Match match=StyleTag.Match(token);
                if(match.Success)
                {
                    bool accepted=availableStyle!=null&&availableStyle(match.Groups[1].Value);
                    if(accepted)result.Append("<style="+match.Groups[1].Value+">");
                    scopes.Push(new KeyValuePair<string,string>("style",accepted?"</style>":""));continue;
                }
                match=ColorTag.Match(token);
                if(match.Success){result.Append("<color="+match.Groups[1].Value+">");scopes.Push(new KeyValuePair<string,string>("color","</color>"));continue;}
                if(lower=="b"||lower=="i"||lower=="u"){result.Append("<"+lower+">");scopes.Push(new KeyValuePair<string,string>(lower,"</"+lower+">"));continue;}
                if(lower=="br"||lower=="br/"){result.Append("\n");continue;}
                if(lower.StartsWith("/",StringComparison.Ordinal)&&scopes.Count>0&&scopes.Peek().Key==lower.Substring(1))
                    result.Append(scopes.Pop().Value);
                // Sprite/link/font/size/material tags and unsupported attributes are discarded.
            }
            result.Append(Escape(value.Substring(start)));
            while(scopes.Count>0)result.Append(scopes.Pop().Value);
            return result.ToString();
        }
    }
    public static class CompactTooltipPresentation
    {
        public static string Item(IEnumerable<ItemTooltipView> source,string nativePickup,long effective,long permanent)
        {
            var terms=(source??Enumerable.Empty<ItemTooltipView>()).Where(t=>t!=null).ToArray();
            var lines=new List<string>();
            if(!string.IsNullOrWhiteSpace(nativePickup))lines.Add(nativePickup);
            foreach(var term in terms)
            {
                lines.Add(TooltipMarkup.Style(term.CompactValue,term.SemanticStyle)+
                    (term.StackHint.Length>0?" "+TooltipMarkup.Style(term.StackHint,"cStack"):""));
            }
            if(effective!=permanent)lines.Add(TooltipMarkup.Style(effective+" effective / "+permanent+" owned","cStack"));
            lines.Add(TooltipMarkup.Style(terms.Length==0?"Scaling unverified":"Reference components; conditions in Shift","cStack"));
            return string.Join("\n",lines);
        }
        public static string Stat(string explanation,double live,Calculation calculation,string semanticStyle,string suffix="",string statName="stat")
        {
            if(calculation==null)throw new ArgumentNullException(nameof(calculation));
            string name=TooltipText.SingleLine(statName).ToLowerInvariant();
            var lines=new List<string>{TooltipMarkup.Style("Total "+name+": "+NumberFormatting.Display(live)+suffix,semanticStyle)};
            var baseline=calculation.Terms.FirstOrDefault(t=>t.Stage==CalculationStage.Baseline);
            if(baseline!=null)lines.Add(TooltipMarkup.Style("Base "+name+": "+NumberFormatting.Display(baseline.After)+suffix,"cStack"));
            foreach(var term in calculation.Terms.Where(t=>t.Stage==CalculationStage.Flat&&t.Source.StartsWith("Level",StringComparison.Ordinal)))
                if(term.Operand!=0)lines.Add(TooltipMarkup.Style("Level growth: "+NumberFormatting.Signed(term.Operand)+suffix,"cStack"));
            // Compact categories never reorder or recalculate the underlying ordered term list.
            foreach(var term in calculation.Terms.Where(t=>t.Stage==CalculationStage.Flat||t.Stage==CalculationStage.LateFlat||t.Stage==CalculationStage.Final))
                if(!term.Source.StartsWith("Level",StringComparison.Ordinal)&&term.Operand!=0)
                    lines.Add(TooltipMarkup.Literal("Flat / "+term.Source+": ")+TooltipMarkup.Style(NumberFormatting.Signed(term.Operand)+suffix,semanticStyle));
            foreach(var term in calculation.Terms.Where(t=>t.Stage!=CalculationStage.Baseline&&t.Stage!=CalculationStage.Flat&&t.Stage!=CalculationStage.LateFlat&&t.Stage!=CalculationStage.Final))
            {
                string value=term.Stage==CalculationStage.AdditiveMultiplier?NumberFormatting.Signed(term.Operand*100)+"%":
                    "x"+NumberFormatting.Display(term.Operand);
                lines.Add(TooltipMarkup.Literal("Modifier / "+term.Source+": ")+TooltipMarkup.Style(value,semanticStyle));
            }
            double residual=live-calculation.Predicted;
            if(residual!=0)lines.Add(TooltipMarkup.Style("Unattributed: "+NumberFormatting.Signed(residual)+suffix+" (partial)","cStack"));
            return string.Join("\n",lines);
        }
    }
    public struct CompactTooltipSize
    {
        public readonly double Width,Height,ViewportHeight;
        public CompactTooltipSize(double width,double height,double viewport){Width=width;Height=height;ViewportHeight=viewport;}
    }
    public static class CompactTooltipSizing
    {
        public static bool TryCreate(double canvasWidth,double canvasHeight,double preferredWidth,double bodyHeight,
            double headerHeight,double footerHeight,double scale,bool detailed,out CompactTooltipSize size)
        {
            size=default(CompactTooltipSize);
            foreach(double value in new[]{canvasWidth,canvasHeight,preferredWidth,bodyHeight,headerHeight,footerHeight,scale})
                if(!NumberFormatting.IsFinite(value)||value<0)return false;
            if(scale<.69||scale>1.21)return false;
            double width=Math.Min(preferredWidth,canvasWidth-24*scale);
            double maxHeight=Math.Min((detailed?580:420)*scale,canvasHeight-24*scale);
            double chrome=headerHeight+footerHeight+16*scale;
            if(width<100*scale||maxHeight<chrome+24*scale)return false;
            double height=Math.Min(maxHeight,Math.Max(chrome+24*scale,bodyHeight+chrome));
            size=new CompactTooltipSize(width,height,height-chrome);return true;
        }
    }
}
