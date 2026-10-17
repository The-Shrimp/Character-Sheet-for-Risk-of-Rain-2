using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Alexander.CharacterSheet.Core
{
    public static class TooltipText
    {
        public static string Clean(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var result=new StringBuilder(text.Length);
            foreach(char c in text.Replace("\r\n","\n").Replace('\r','\n'))
                if (!char.IsControl(c)||c=='\n'||c=='\t') result.Append(c);
            return result.ToString().Trim();
        }
        public static string SingleLine(string text) => string.Join(" ",Clean(text).Split((char[])null,StringSplitOptions.RemoveEmptyEntries));
        public static string Clip(string text,int limit,string marker="...")
        {
            if (limit<0) throw new ArgumentOutOfRangeException(nameof(limit));
            text=text??""; marker=marker??"";
            if (text.Length<=limit) return text;
            if (limit<=marker.Length)
            {
                int end=limit;
                if(end>0&&end<marker.Length&&char.IsHighSurrogate(marker[end-1])&&char.IsLowSurrogate(marker[end])) end--;
                return marker.Substring(0,end);
            }
            int prefix=limit-marker.Length;
            if(prefix>0&&prefix<text.Length&&char.IsHighSurrogate(text[prefix-1])&&char.IsLowSurrogate(text[prefix])) prefix--;
            return text.Substring(0,prefix)+marker;
        }
    }
    public sealed class TooltipSection
    {
        public readonly string Label, Text;
        public TooltipSection(string label,string text) { Label=TooltipText.SingleLine(label);Text=TooltipText.Clean(text); }
    }
    public sealed class TooltipContent
    {
        public readonly string Title, Summary, Current, Next, Difference, Confidence, DetailedCurrent, DetailedNext, DetailedDifference;
        public readonly IReadOnlyList<TooltipSection> Sections;
        public TooltipContent(string title,string summary,string current,string next,string difference,string confidence,IEnumerable<TooltipSection> sections=null,string detailedCurrent=null,string detailedNext=null,string detailedDifference=null)
        {
            Title=TooltipText.SingleLine(title);Summary=TooltipText.Clean(summary);
            Current=TooltipText.SingleLine(current);Next=TooltipText.SingleLine(next);Difference=TooltipText.SingleLine(difference);
            Confidence=TooltipText.SingleLine(confidence);
            DetailedCurrent=TooltipText.SingleLine(detailedCurrent??current);DetailedNext=TooltipText.SingleLine(detailedNext??next);DetailedDifference=TooltipText.SingleLine(detailedDifference??difference);
            Sections=Array.AsReadOnly((sections??Enumerable.Empty<TooltipSection>()).Where(s=>s!=null).ToArray());
        }
        public static TooltipContent FromComparison(string title,string summary,ComparisonView comparison,IEnumerable<TooltipSection> sections=null)
        {
            if(comparison==null)throw new ArgumentNullException(nameof(comparison));
            return new TooltipContent(title,summary,comparison.Current,comparison.Next,comparison.Difference,comparison.Confidence,sections,
                comparison.CurrentExact,comparison.NextExact,comparison.DifferenceExact);
        }
    }
    public static class TooltipPresentation
    {
        // Plain-text result: renderer must disable rich-text interpretation rather than execute embedded markup.
        public static string Short(TooltipContent content,int characterLimit=420)
        {
            if(content==null)throw new ArgumentNullException(nameof(content));
            if(characterLimit<96)throw new ArgumentOutOfRangeException(nameof(characterLimit));
            string confidence=TooltipText.Clip(content.Confidence.Length>0 ? content.Confidence : "Unverified",Math.Min(140,characterLimit/3));
            string body=string.Join("\n",new[]{TooltipText.Clip(content.Title,72),TooltipText.SingleLine(content.Summary),
                content.Current,content.Next,content.Difference}.Where(s=>s.Length>0));
            int bodyBudget=characterLimit-confidence.Length-1;
            return TooltipText.Clip(body,bodyBudget)+"\n"+confidence;
        }
        public static string Full(TooltipContent content,int characterLimit=8192)
        {
            if(content==null)throw new ArgumentNullException(nameof(content));
            if(characterLimit<192)throw new ArgumentOutOfRangeException(nameof(characterLimit));
            // Uncertainty is near the top so even bounded details cannot conceal it.
            var parts=new List<string>{TooltipText.Clip(content.Title,96),
                TooltipText.Clip(content.Confidence.Length>0 ? content.Confidence : "Unverified",160),
                content.Summary,content.DetailedCurrent,content.DetailedNext,content.DetailedDifference};
            parts.AddRange(content.Sections.Select(s=>s.Label.Length>0 ? s.Label+"\n"+s.Text : s.Text));
            return TooltipText.Clip(string.Join("\n\n",parts.Where(s=>s.Length>0)),characterLimit,"\n[Details truncated]");
        }
        public static string Select(TooltipContent content,bool detailed) => detailed ? Full(content) : Short(content);
    }
}
