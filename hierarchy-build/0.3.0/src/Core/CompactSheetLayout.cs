using System;
using System.Collections.Generic;
namespace Alexander.CharacterSheet.Core
{
    public readonly struct CompactSheetMetrics
    {
        public readonly double RowHeight,HeaderHeight,ControlHeight,Spacing,Padding,Width,MinWidth,MinHeight;
        public CompactSheetMetrics(double fontSize,double scale)
        {
            if(!NumberFormatting.IsFinite(fontSize)||!NumberFormatting.IsFinite(scale)||scale<.7-1e-6||scale>1.2+1e-6)
                throw new ArgumentOutOfRangeException();
            scale=Math.Max(.7,Math.Min(1.2,scale));
            fontSize=Math.Max(18,Math.Min(24,fontSize));
            RowHeight=(fontSize+8)*scale;HeaderHeight=(fontSize+12)*scale;ControlHeight=RowHeight;
            Spacing=2*scale;Padding=10*scale;Width=Math.Max(340,fontSize*18)*scale;
            MinWidth=Math.Max(280,fontSize*14)*scale;MinHeight=140*scale;
        }
        public double ContentHeight(int statRows,int effectRows)
        {
            if(statRows<0||effectRows<0)throw new ArgumentOutOfRangeException();
            double rows=(double)statRows+effectRows;
            return HeaderHeight+ControlHeight+rows*RowHeight+(rows+1)*Spacing+2*Padding;
        }
        public double PanelHeight(double canvasHeight,int statRows,int effectRows,bool expanded)
        {
            double content=ContentHeight(statRows,effectRows);
            return expanded?Math.Min(content,Math.Max(MinHeight,canvasHeight*.45)):content;
        }
    }
    public static class CompactSheetPlacement
    {
        public static bool TryBelowOwn(double width,double height,UiRect own,IEnumerable<UiRect> occupied,
            double desiredWidth,double desiredHeight,double minWidth,double minHeight,double gap,out UiRect dock)
        {
            dock=default(UiRect);
            if(!NumberFormatting.IsFinite(width)||!NumberFormatting.IsFinite(height)||!own.Valid||occupied==null||
                !NumberFormatting.IsFinite(desiredWidth)||!NumberFormatting.IsFinite(desiredHeight)||
                !NumberFormatting.IsFinite(minWidth)||!NumberFormatting.IsFinite(minHeight)||!NumberFormatting.IsFinite(gap)||
                width<=0||height<=0||minWidth<=0||minHeight<=0||gap<0||desiredWidth<minWidth||desiredHeight<minHeight||
                own.Top<=0||own.Y>=height||own.Right<=0||own.X>=width)return false;
            double w=Math.Min(desiredWidth,Math.Min(own.Width,width-2*gap));
            if(w<minWidth)return false;
            double x=Math.Max(gap,Math.Min(own.X,width-gap-w));
            double top=Math.Min(height-gap,own.Y-gap);
            if(top-gap<minHeight)return false;
            var spaces=new List<UiRect>{new UiRect(x,gap,w,top-gap)};
            foreach(var obstacle in occupied)
            {
                if(!obstacle.Valid)return false;
                if(obstacle.Right+gap<=x||obstacle.X-gap>=x+w)continue;
                double low=obstacle.Y-gap,high=obstacle.Top+gap;
                var next=new List<UiRect>();
                foreach(var space in spaces)
                {
                    if(high<=space.Y||low>=space.Top){next.Add(space);continue;}
                    if(low>space.Y)next.Add(new UiRect(x,space.Y,w,Math.Min(low,space.Top)-space.Y));
                    if(high<space.Top)next.Add(new UiRect(x,Math.Max(high,space.Y),w,space.Top-Math.Max(high,space.Y)));
                }
                spaces=next;
            }
            double highest=double.NegativeInfinity;
            foreach(var space in spaces)
            {
                if(!space.Valid||space.Height<minHeight||space.Top<=highest)continue;
                double h=Math.Min(desiredHeight,space.Height);
                dock=new UiRect(x,space.Top-h,w,h);highest=space.Top;
            }
            return dock.Valid;
        }
    }
}
