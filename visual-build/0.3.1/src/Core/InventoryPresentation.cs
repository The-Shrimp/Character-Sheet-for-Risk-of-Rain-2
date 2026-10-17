using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    public enum EffectGroup { OnHit, OnKill, Defense, Utility, Unverified }
    public enum InventorySort { RarityThenName, Name, EffectiveCount, Acquisition, EffectGroup }
    public enum LuckApplicability { Affected, Unaffected, Mixed, Unknown }
    public sealed class InventoryEntry
    {
        public string Id { get; }
        public string Name { get; }
        public string SearchText { get; }
        public int RarityOrder { get; }
        public int AcquisitionOrder { get; }
        public long? OwnedCount { get; }
        public long? EffectiveCount { get; }
        public bool Hidden { get; }
        public IReadOnlyList<EffectGroup> Groups { get; }
        public LuckApplicability Luck { get; }
        public InventoryEntry(string id,string name,long? owned,long? effective,int rarity=0,int acquisition=0,
            IEnumerable<EffectGroup> groups=null,LuckApplicability luck=LuckApplicability.Unknown,bool hidden=false,string searchText=null)
        {
            if(string.IsNullOrWhiteSpace(id))throw new ArgumentException("A stable row ID is required.",nameof(id));
            if(owned<0||effective<0)throw new ArgumentOutOfRangeException("Counts must be nonnegative or unknown.");
            Id=id;Name=TooltipText.SingleLine(name);if(Name.Length==0)Name="Unknown item";
            OwnedCount=owned;EffectiveCount=effective;RarityOrder=rarity;AcquisitionOrder=acquisition;Hidden=hidden;Luck=luck;
            EffectGroup[] copied=(groups??Enumerable.Empty<EffectGroup>()).Distinct().OrderBy(g=>g).ToArray();
            Groups=Array.AsReadOnly(copied.Length>0 ? copied : new[]{EffectGroup.Unverified});
            SearchText=TooltipText.SingleLine(searchText);
        }
        public string CountLabel => "Owned: "+NumberFormatting.Count(OwnedCount)+" | Effective: "+NumberFormatting.Count(EffectiveCount)+
            (OwnedCount>0&&EffectiveCount==0 ? " (inactive)" : "");
        public string GroupLabel => string.Join(" / ",Groups.Select(InventoryPresentation.GroupLabel));
    }
    public sealed class InventoryFilter
    {
        public string Query="";
        public InventorySort Sort=InventorySort.RarityThenName;
        public EffectGroup? Group;
        public LuckApplicability? Luck;
        public bool ActiveOnly;
    }
    public static class InventoryPresentation
    {
        public static string GroupLabel(EffectGroup group)
        {
            switch(group)
            {
                case EffectGroup.OnHit:return "On hit";
                case EffectGroup.OnKill:return "On kill";
                case EffectGroup.Defense:return "Defense / sustain";
                case EffectGroup.Utility:return "Utility / passive";
                default:return "Trigger unverified";
            }
        }
        public static string LuckLabel(LuckApplicability luck)
        {
            switch(luck)
            {
                case LuckApplicability.Affected:return "Affected";
                case LuckApplicability.Unaffected:return "Unaffected";
                case LuckApplicability.Mixed:return "Mixed: applicability differs by effect";
                default:return "Unverified";
            }
        }
        public static IReadOnlyList<InventoryEntry> Select(IEnumerable<InventoryEntry> source,InventoryFilter filter=null)
        {
            if(source==null)throw new ArgumentNullException(nameof(source));
            filter=filter??new InventoryFilter();
            InventoryEntry[] all=source.Where(e=>e!=null).ToArray();
            if(all.GroupBy(e=>e.Id,StringComparer.Ordinal).Any(g=>g.Count()>1))
                throw new ArgumentException("Duplicate stable row IDs would make UI selection ambiguous.",nameof(source));
            string[] terms=TooltipText.SingleLine(filter.Query).Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<InventoryEntry> visible=all.Where(e=>!e.Hidden&&
                (!e.OwnedCount.HasValue||!e.EffectiveCount.HasValue||e.OwnedCount>0||e.EffectiveCount>0));
            if(filter.ActiveOnly)visible=visible.Where(e=>e.EffectiveCount>0);
            if(filter.Group.HasValue)visible=visible.Where(e=>e.Groups.Contains(filter.Group.Value));
            if(filter.Luck.HasValue)visible=visible.Where(e=>e.Luck==filter.Luck.Value);
            if(terms.Length>0)visible=visible.Where(e=>{
                string text=e.Name+" "+e.GroupLabel+" "+e.SearchText;
                return terms.All(term=>text.IndexOf(term,StringComparison.OrdinalIgnoreCase)>=0);
            });
            IOrderedEnumerable<InventoryEntry> sorted;
            switch(filter.Sort)
            {
                case InventorySort.Name:sorted=visible.OrderBy(e=>e.Name,StringComparer.OrdinalIgnoreCase);break;
                case InventorySort.EffectiveCount:sorted=visible.OrderByDescending(e=>e.EffectiveCount??-1);break;
                case InventorySort.Acquisition:sorted=visible.OrderBy(e=>e.AcquisitionOrder);break;
                case InventorySort.EffectGroup:sorted=visible.OrderBy(e=>e.Groups.Min());break;
                default:sorted=visible.OrderBy(e=>e.RarityOrder);break;
            }
            return Array.AsReadOnly(sorted.ThenBy(e=>e.Name,StringComparer.OrdinalIgnoreCase).ThenBy(e=>e.Id,StringComparer.Ordinal).ToArray());
        }
        public static string ResolveSelection(string previousId,IReadOnlyList<InventoryEntry> visible)
        {
            if(visible==null)throw new ArgumentNullException(nameof(visible));
            return visible.Any(e=>e.Id==previousId) ? previousId : visible.Count>0 ? visible[0].Id : null;
        }
    }
}
