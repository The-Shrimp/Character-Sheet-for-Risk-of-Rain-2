using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    public static class VisualSheetStructure
    {
        public const string FooterId="stat:Luck";
        public static bool IsFooter(SheetNode node)=>node!=null&&node.Id==FooterId;
        static readonly string[] order={SheetHierarchy.Defense,SheetHierarchy.Offense,SheetHierarchy.Movement,
            SheetHierarchy.Hit,SheetHierarchy.Kill,SheetHierarchy.Combat,SheetHierarchy.Sprint,SheetHierarchy.Other};
        public static IReadOnlyList<SheetNode> Project(IEnumerable<SheetNode> original)
        {
            if(original==null)throw new ArgumentNullException(nameof(original));
            var nodes=original.ToArray();var result=new List<SheetNode>();
            foreach(string id in order)
            {
                var header=nodes.Single(n=>n.Id==id&&n.Header);
                string parent="";
                string title=id==SheetHierarchy.Hit?"On-Hit Effects":id==SheetHierarchy.Kill?"On-Kill Effects":
                    id==SheetHierarchy.Other?"Misc Effects":header.Title;
                SheetGlyph glyph=id==SheetHierarchy.Offense?SheetGlyph.CrossedSwords:
                    id==SheetHierarchy.Other?SheetGlyph.Misc:header.Glyph;
                result.Add(new SheetNode(id,parent,title,true,0,glyph:glyph));
                foreach(var row in nodes.Where(n=>!n.Header&&(n.Parent==id||(id==SheetHierarchy.Other&&n.Parent==SheetHierarchy.Damage))))
                    result.Add(new SheetNode(row.Id,id,row.Title,false,1,row.Payload,row.Glyph));
            }
            var luck=nodes.Single(n=>n.Id==FooterId&&!n.Header);
            result.Add(new SheetNode(luck.Id,"",luck.Title,false,0,luck.Payload,SheetGlyph.Clover));
            return result;
        }
        public static string ToggleLabel(bool open)=>open?"Collapse":"Expand";
    }
    public readonly struct VisualSheetRegions
    {
        public readonly double FooterHeight,ViewportBottom,ViewportTop,ViewportHeight;
        public VisualSheetRegions(double panelHeight,double titleHeight,double footerHeight,double padding)
        {
            if(new[]{panelHeight,titleHeight,footerHeight,padding}.Any(v=>!NumberFormatting.IsFinite(v))||
                titleHeight<=0||footerHeight<=0||padding<0||panelHeight<=titleHeight+footerHeight+2*padding)
                throw new ArgumentOutOfRangeException();
            FooterHeight=footerHeight;ViewportBottom=footerHeight+padding;
            ViewportTop=panelHeight-titleHeight-padding;ViewportHeight=ViewportTop-ViewportBottom;
        }
    }
    public static class VisualSheetSizing
    {
        public const double BackgroundOpacity=.8;
        public static double PanelHeight(double canvasHeight,int headers,int rows,double rowHeight,double bandHeight,
            double titleHeight,double footerHeight,double padding,double minHeight)
        {
            if(new[]{canvasHeight,rowHeight,bandHeight,titleHeight,footerHeight,padding,minHeight}.Any(v=>!NumberFormatting.IsFinite(v))||
                canvasHeight<=0||headers<0||rows<0||rowHeight<=0||bandHeight<=0||titleHeight<=0||footerHeight<=0||padding<0||minHeight<=0)
                throw new ArgumentOutOfRangeException();
            double content=titleHeight+footerHeight+headers*bandHeight+rows*rowHeight+2*padding;
            return Math.Max(minHeight,Math.Min(content,Math.Max(minHeight,canvasHeight*.5)));
        }
    }
}
