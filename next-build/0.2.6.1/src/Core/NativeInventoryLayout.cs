using System;
namespace Alexander.CharacterSheet.Core
{
    public readonly struct NativeInventoryPlan
    {
        public readonly int Columns,Rows;
        public readonly double IconWidth,ViewportHeight,ContentCeiling;
        public NativeInventoryPlan(int columns,int rows,double icon,double viewport,double ceiling)
        {Columns=columns;Rows=rows;IconWidth=icon;ViewportHeight=viewport;ContentCeiling=ceiling;}
    }
    public static class NativeInventoryLayout
    {
        public static bool TryPlan(double width,int visibleCount,double originalMaxIcon,double verticalMargin,
            double scale,double originalHeight,out NativeInventoryPlan plan)
        {
            plan=default(NativeInventoryPlan);
            if(!NumberFormatting.IsFinite(width)||!NumberFormatting.IsFinite(originalMaxIcon)||!NumberFormatting.IsFinite(verticalMargin)||
               !NumberFormatting.IsFinite(scale)||!NumberFormatting.IsFinite(originalHeight)||width<64||visibleCount<0||visibleCount>5000||
               verticalMargin<0||verticalMargin>100||scale<.7||scale>1.2||originalHeight<0)return false;
            double icon=Math.Max(18,Math.Min(originalMaxIcon>0?originalMaxIcon:36*scale,36*scale));
            int columns=Math.Max(1,(int)Math.Floor((width-14)/icon));
            int rows=Math.Max(1,(visibleCount+columns-1)/columns);
            double viewport=Math.Max(originalHeight,4*(icon+verticalMargin));
            // Conservative room for the game's own layout. Native preferredHeight determines actual content height.
            double ceiling=Math.Max(viewport,(visibleCount+1)*(icon+verticalMargin)+8);
            plan=new NativeInventoryPlan(columns,rows,icon,viewport,ceiling);return true;
        }
        public static bool OwnsParent(object expected,object current)=>expected!=null&&ReferenceEquals(expected,current);
    }
}
