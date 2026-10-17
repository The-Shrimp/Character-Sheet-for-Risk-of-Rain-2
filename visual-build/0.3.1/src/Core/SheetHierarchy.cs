using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    public enum SheetGlyph { None,Shield,Sword,Runner,Clover,Burst,Skull,CrossedSwords,Misc }
    public sealed class SheetNode
    {
        public readonly string Id,Parent,Title,Payload;
        public readonly bool Header;
        public readonly int Depth;
        public readonly SheetGlyph Glyph;
        public SheetNode(string id,string parent,string title,bool header,int depth,string payload="",SheetGlyph glyph=SheetGlyph.None)
        {Id=id;Parent=parent??"";Title=title;Header=header;Depth=depth;Payload=payload;Glyph=glyph;}
    }
    public static class SheetHierarchy
    {
        public const string Base="base",Defense="defense",Offense="offense",Movement="movement",Luck="luck",
            Damage="damage-effects",Hit="on-hit",Kill="on-kill",Combat="out-of-combat",Sprint="sprinting",Other="other";
        public static readonly IReadOnlyList<SheetNode> Headers=Array.AsReadOnly(new[]{
            new SheetNode(Base,"","Base Stats",true,0),
            new SheetNode(Defense,Base,"Defense",true,1,glyph:SheetGlyph.Shield),
            new SheetNode(Offense,Base,"Offense",true,1,glyph:SheetGlyph.Sword),
            new SheetNode(Movement,Base,"Movement",true,1,glyph:SheetGlyph.Runner),
            new SheetNode(Luck,Base,"Luck",true,1,glyph:SheetGlyph.Clover),
            new SheetNode(Damage,"","General Damage Modifiers",true,0,glyph:SheetGlyph.Sword),
            new SheetNode(Hit,"","On-Hit",true,0,glyph:SheetGlyph.Burst),
            new SheetNode(Kill,"","On-Kill",true,0,glyph:SheetGlyph.Skull),
            new SheetNode(Combat,"","Out of Combat",true,0),
            new SheetNode(Sprint,"","While Sprinting",true,0,glyph:SheetGlyph.Runner),
            new SheetNode(Other,"","Other items & buffs",true,0)
        });
        public static readonly IReadOnlyDictionary<string,string[]> Stats=new Dictionary<string,string[]>{
            [Defense]=new[]{"Health","Shield","Regeneration","Armor","Dmg Reduction"},
            [Offense]=new[]{"Damage","Attack speed","Cooldown Reduction","Critical chance"},
            [Movement]=new[]{"Movement speed","Sprint Speed Modifier","Maximum jumps"},
            [Luck]=new[]{"Luck"}};
        public static IReadOnlyList<SheetNode> Visible(IEnumerable<SheetNode> source,SheetExpansionState state)
        {
            if(source==null||state==null)throw new ArgumentNullException();
            var nodes=source.ToArray();var map=new Dictionary<string,SheetNode>(StringComparer.Ordinal);
            foreach(var node in nodes)
            {
                if(node==null||string.IsNullOrEmpty(node.Id)||map.ContainsKey(node.Id))throw new ArgumentException("Node identities must be unique.");
                map.Add(node.Id,node);
            }
            var result=new List<SheetNode>();
            foreach(var node in nodes)
            {
                string parent=node.Parent;var seen=new HashSet<string>();bool visible=true;
                while(parent.Length>0)
                {
                    if(!seen.Add(parent))throw new ArgumentException("Hierarchy cycle.");
                    SheetNode ancestor;if(!map.TryGetValue(parent,out ancestor)||!ancestor.Header)throw new ArgumentException("Missing section parent.");
                    if(!state.IsOpen(parent))visible=false;
                    parent=ancestor.Parent;
                }
                if(visible)result.Add(node);
            }
            return result;
        }
        public static IReadOnlyList<T> UniqueRows<T>(IEnumerable<T> rows,Func<T,string> id)
        {
            if(rows==null||id==null)throw new ArgumentNullException();
            var seen=new HashSet<string>(StringComparer.Ordinal);var result=new List<T>();
            foreach(var row in rows){string key=id(row);if(!string.IsNullOrEmpty(key)&&seen.Add(key))result.Add(row);}
            return result;
        }
        public static string RestoreFocus(string previous,IEnumerable<SheetNode> all,IEnumerable<SheetNode> visible)
        {
            var available=new HashSet<string>(visible.Select(n=>n.Id));var map=all.ToDictionary(n=>n.Id);
            string current=previous;var seen=new HashSet<string>();
            while(!string.IsNullOrEmpty(current)&&seen.Add(current))
            {if(available.Contains(current))return current;SheetNode node;if(!map.TryGetValue(current,out node))break;current=node.Parent;}
            return available.Contains(Base)?Base:visible.Select(n=>n.Id).FirstOrDefault();
        }
    }
    public sealed class SheetExpansionState
    {
        readonly Dictionary<string,bool> values=new Dictionary<string,bool>(StringComparer.Ordinal);
        readonly bool referenceDefaults;
        public SheetExpansionState(bool referenceDefaults=false){this.referenceDefaults=referenceDefaults;}
        public bool IsOpen(string id)
        {
            bool value;if(values.TryGetValue(id??"",out value))return value;
            return id==SheetHierarchy.Defense||(!referenceDefaults&&(id==SheetHierarchy.Base||id==SheetHierarchy.Offense||id==SheetHierarchy.Movement||id==SheetHierarchy.Luck));
        }
        public bool Set(string id,bool value)
        {if(!SheetHierarchy.Headers.Any(n=>n.Id==id))return false;values[id]=value;return true;}
        public bool Toggle(string id)=>Set(id,!IsOpen(id));
    }
    public static class HierarchySheetSizing
    {
        public const double BackgroundOpacity=.8;
        public static double PanelHeight(double canvasHeight,int visibleRows,double rowHeight,double headerHeight,double spacing,double padding,double minHeight)
        {
            if(new[]{canvasHeight,rowHeight,headerHeight,spacing,padding,minHeight}.Any(v=>!NumberFormatting.IsFinite(v))||canvasHeight<=0||visibleRows<0||rowHeight<=0||headerHeight<=0||spacing<0||padding<0||minHeight<=0)throw new ArgumentOutOfRangeException();
            double content=headerHeight+visibleRows*rowHeight+Math.Max(0,visibleRows-1)*spacing+2*padding;
            return Math.Max(minHeight,Math.Min(content,Math.Max(minHeight,canvasHeight*.5)));
        }
        public static double EnsureVisible(double top,double bottom,double viewportHeight,double offset,double contentHeight)
        {
            if(new[]{top,bottom,viewportHeight,offset,contentHeight}.Any(v=>!NumberFormatting.IsFinite(v))||viewportHeight<=0||contentHeight<0)return 0;
            if(top>viewportHeight)offset-=top-viewportHeight;
            else if(bottom<0)offset-=bottom;
            return Math.Max(0,Math.Min(Math.Max(0,contentHeight-viewportHeight),offset));
        }
    }
}
