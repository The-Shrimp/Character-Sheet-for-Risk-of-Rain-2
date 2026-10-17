using System;
namespace Alexander.CharacterSheet.Core
{
    public enum NectarDisposition { Eligible, NotCounted, Unknown }
    public sealed class NectarAssessment
    {
        public readonly NectarDisposition Disposition;
        public readonly string Reason;
        internal NectarAssessment(NectarDisposition disposition,string reason){Disposition=disposition;Reason=reason;}
        public string Label=>Disposition==NectarDisposition.Eligible?"eligible type":Disposition==NectarDisposition.NotCounted?"not counted":"unknown";
        public string Describe()=>Label+": "+Reason;
    }
    public sealed class BuffObservation
    {
        public string PublicId,InternalId,DisplayName;
        public long? Stacks;
        public bool? Hidden,DefinitionIgnored,CatalogIgnored,IsNectarCounter,CanStack,IsDebuff,IsCooldown;
    }
    public sealed class BuffGlossaryView
    {
        public readonly OriginalBuffDefinition Definition;
        public readonly NectarAssessment Nectar;
        public readonly string Title,Kind,Detail,ShortTooltip,FullTooltip;
        internal BuffGlossaryView(OriginalBuffDefinition definition,NectarAssessment nectar,string title,string kind,string detail,string shortText,string fullText)
        {Definition=definition;Nectar=nectar;Title=title;Kind=kind;Detail=detail;ShortTooltip=shortText;FullTooltip=fullText;}
    }
    public static class BuffGlossaryPresentation
    {
        public const string RemainingDuration="Remaining duration unavailable: no public per-instance/per-stack timer adapter";
        public static NectarAssessment AssessNectar(long? stacks,bool? hidden,bool? definitionIgnored,bool? catalogIgnored,bool? nectarCounter)
        {
            if(stacks==0)return new NectarAssessment(NectarDisposition.NotCounted,"inactive (0 observed stacks)");
            if(hidden==true)return new NectarAssessment(NectarDisposition.NotCounted,"live hidden flag");
            if(definitionIgnored==true)return new NectarAssessment(NectarDisposition.NotCounted,"live definition ignoreGrowthNectar flag");
            if(catalogIgnored==true)return new NectarAssessment(NectarDisposition.NotCounted,"live catalog exclusion");
            if(nectarCounter==true)return new NectarAssessment(NectarDisposition.NotCounted,"observed Nectar counter excludes itself");
            if(!stacks.HasValue||stacks.Value<0||!hidden.HasValue||!definitionIgnored.HasValue||!catalogIgnored.HasValue||!nectarCounter.HasValue)
                return new NectarAssessment(NectarDisposition.Unknown,"active count or eligibility metadata unavailable; no name-based inference");
            return new NectarAssessment(NectarDisposition.Eligible,"one observed qualifying active type, independently of stack count");
        }
        static string Flag(bool? value)=>value.HasValue?(value.Value?"true":"false"):"Unavailable";
        public static BuffGlossaryView Create(BuffObservation input)
        {
            if(input==null)throw new ArgumentNullException(nameof(input));
            // Unresolved visibility cannot expose a potentially hidden state.
            if(input.Hidden!=false||(input.Stacks.HasValue&&input.Stacks.Value<=0))return null;
            // Runtime passes only a public object identity verified by its adapter; a similar name alone is insufficient.
            var definition=OriginalBuffDefinitions.Find(input.PublicId);
            string internalId=TooltipText.SingleLine(input.InternalId);
            if(internalId.Length==0)internalId="Unavailable";
            string title=definition?.Name;
            if(title==null)
            {
                string display=TooltipText.SingleLine(input.DisplayName??input.InternalId);
                title="Unverified buff"+(display.Length>0?": "+display:"");
            }
            title+=" x"+NumberFormatting.Count(input.Stacks);
            string kind=definition?.Kind??
                (!input.IsDebuff.HasValue||!input.IsCooldown.HasValue?"Unclassified status (metadata unavailable)":
                    BuffGlossaryRules.Kind(input.IsDebuff.Value,input.IsCooldown.Value,input.IsNectarCounter==true));
            var nectar=AssessNectar(input.Stacks,input.Hidden,input.DefinitionIgnored,input.CatalogIgnored,input.IsNectarCounter);
            string effect=definition==null?
                "Effect unverified for this observed definition. No immunity, stat, proc or timer claim is inferred from its name. Live totals remain authoritative.":
                input.Stacks.HasValue?definition.Describe(input.Stacks.Value):
                    "Reference effect: "+definition.Effect+"\nActive-stack components unavailable: observed count missing.";
            string shortText=effect+"\nInternal definition: "+internalId+"\nNectar: "+nectar.Describe()+"\n"+RemainingDuration;
            string fullText=shortText+"\nObserved stacks: "+NumberFormatting.Count(input.Stacks)+
                "\nLive metadata: canStack="+Flag(input.CanStack)+", isDebuff="+Flag(input.IsDebuff)+", isCooldown="+Flag(input.IsCooldown)+
                ", isHidden="+Flag(input.Hidden)+", ignoreGrowthNectar="+Flag(input.DefinitionIgnored)+", catalogExcluded="+Flag(input.CatalogIgnored)+
                "\nNectar assessment uses live metadata and an observed self-counter identity; charges/cooldowns/debuff names do not override these flags.";
            if(definition!=null)fullText+="\nMatched public identity: "+definition.PublicId+"\nStack meaning: "+definition.StackMeaning+
                "\nConditions: "+definition.Conditions+"\nEffect evidence: "+definition.Confidence+
                "\nSource: "+definition.Source+"\nSource revision: "+definition.Revision;
            else fullText+="\nEffect evidence: Unverified; no registered public identity matched.";
            return new BuffGlossaryView(definition,nectar,title,kind,kind+" \u00b7 Timer unavailable \u00b7 Nectar: "+nectar.Label,shortText,fullText);
        }
    }
}
