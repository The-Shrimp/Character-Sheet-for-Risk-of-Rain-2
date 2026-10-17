using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    // Only added calculations. Native title/description never pass through this renderer.
    public static class ItemDetailsPresentation
    {
        public static string Create(IEnumerable<ItemTooltipView> source,long effective,long permanent,string triggerContext="")
        {
            var terms=(source??Enumerable.Empty<ItemTooltipView>()).Where(t=>t!=null).ToArray();
            var lines=new List<string>{TooltipMarkup.Style("Current stack ("+NumberFormatting.Count(effective)+" effective)","cStack")};
            if(effective!=permanent)lines.Add(TooltipMarkup.Style(NumberFormatting.Count(permanent)+" owned","cStack"));
            if(terms.Length==0)lines.Add("Scaling unverified; no quantitative next-copy prediction.");
            foreach(var term in terms)
            {
                lines.Add(TooltipMarkup.Style(term.CompactValue,term.SemanticStyle)+
                    (term.StackHint.Length>0?" "+TooltipMarkup.Style(term.StackHint,"cStack"):""));
                lines.Add(TooltipMarkup.Style(term.NextLabel,term.SemanticStyle));
                if(term.CurrentValue.HasValue)lines.Add(TooltipMarkup.Style(term.Equation,"cStack"));
            }
            foreach(string conditions in terms.Select(t=>t.Comparison.Confidence).Distinct(StringComparer.Ordinal))
                lines.Add(TooltipMarkup.Literal(conditions));
            if(!string.IsNullOrWhiteSpace(triggerContext))lines.Add(TooltipMarkup.Literal(triggerContext));
            return string.Join("\n",lines);
        }
    }
}
