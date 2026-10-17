using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    public static class IndividualItemDetails
    {
        static ItemTooltipView[] Known(IEnumerable<ItemTooltipView> terms)=>
            (terms??Enumerable.Empty<ItemTooltipView>()).Where(t=>t!=null&&t.CurrentValue.HasValue&&NumberFormatting.IsFinite(t.CurrentValue.Value)).ToArray();
        public static string Create(IEnumerable<ItemTooltipView> singleSource,IEnumerable<ItemTooltipView> currentSource,
            long effective,long permanent,string conditions="")
        {
            var single=Known(singleSource);var current=Known(currentSource);
            if(current.Length==0&&single.Length==0)return "";
            var lines=new List<string>();
            // At one copy the unchanged native description already supplies the individual effect.
            // A multi-copy comparison needs an explicit numeric reference before its combined current-stack block.
            if(effective>1)
            {
                if(single.Length>0)
                {
                    lines.Add(TooltipMarkup.Style("Single-copy components","cStack"));
                    foreach(var term in single)lines.Add(TooltipMarkup.Style(term.CompactValue,term.SemanticStyle));
                }
                if(current.Length>0)
                {
                    lines.Add(TooltipMarkup.Style("Current stack ("+NumberFormatting.Count(effective)+" effective)","cStack"));
                    foreach(var term in current)
                        lines.Add(TooltipMarkup.Style(term.CompactValue,term.SemanticStyle)+
                            (term.StackHint.Length>0?" "+TooltipMarkup.Style(term.StackHint,"cStack"):""));
                }
            }
            if(effective!=permanent)lines.Add(TooltipMarkup.Style(NumberFormatting.Count(permanent)+" owned; "+NumberFormatting.Count(effective)+" effective","cStack"));
            foreach(var term in current)
                if(term.NextValue.HasValue)lines.Add(TooltipMarkup.Style(term.NextLabel,term.SemanticStyle));
            foreach(string confidence in current.Select(t=>t.Comparison.Confidence).Distinct(StringComparer.Ordinal))
                lines.Add(TooltipMarkup.Literal(confidence));
            if(!string.IsNullOrWhiteSpace(conditions))lines.Add(TooltipMarkup.Literal(conditions));
            return string.Join("\n",lines);
        }
    }
}
