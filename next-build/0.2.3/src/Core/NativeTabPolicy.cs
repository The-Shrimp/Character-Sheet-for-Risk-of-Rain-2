using System;
using System.Collections.Generic;
namespace Alexander.CharacterSheet.Core
{
    public readonly struct UiRect
    {
        public readonly double X,Y,Width,Height;
        public double Right=>X+Width;
        public double Top=>Y+Height;
        public bool Valid=>NumberFormatting.IsFinite(X)&&NumberFormatting.IsFinite(Y)&&NumberFormatting.IsFinite(Width)&&NumberFormatting.IsFinite(Height)&&Width>0&&Height>0;
        public UiRect(double x,double y,double width,double height){X=x;Y=y;Width=width;Height=height;}
        public bool Overlaps(UiRect other)=>X<other.Right&&Right>other.X&&Y<other.Top&&Top>other.Y;
    }
    public static class NativeTabPolicy
    {
        public static bool OwnVisibleItem(object localMaster,object stripMaster,bool hidden,int permanent,int effective)=>
            localMaster!=null&&ReferenceEquals(localMaster,stripMaster)&&!hidden&&permanent>=0&&effective>=0&&(permanent>0||effective>0);
        // Geometry only. Never alter a native panel to manufacture room for an addition.
        public static bool TryDock(double width,double height,IEnumerable<UiRect> occupied,double desiredWidth,double desiredHeight,out UiRect dock,
            double minWidth=240,double minHeight=180,double margin=12)
        {
            dock=default(UiRect);
            if(!NumberFormatting.IsFinite(width)||!NumberFormatting.IsFinite(height)||!NumberFormatting.IsFinite(desiredWidth)||
               !NumberFormatting.IsFinite(desiredHeight)||!NumberFormatting.IsFinite(minWidth)||!NumberFormatting.IsFinite(minHeight)||
               !NumberFormatting.IsFinite(margin)||width<=0||height<=0||minWidth<=0||minHeight<=0||margin<0||
               desiredWidth<minWidth||desiredHeight<minHeight||occupied==null)return false;
            double left=width,right=0,bottom=height,top=0;
            bool any=false;
            foreach(var area in occupied)
            {
                if(!area.Valid)return false;
                if(area.Right<=0||area.Top<=0||area.X>=width||area.Y>=height)continue;
                any=true;left=Math.Min(left,Math.Max(0,area.X));right=Math.Max(right,Math.Min(width,area.Right));
                bottom=Math.Min(bottom,Math.Max(0,area.Y));top=Math.Max(top,Math.Min(height,area.Top));
            }
            if(!any)return false;
            var free=new[]{
                new UiRect(right+margin,margin,width-right-2*margin,height-2*margin),
                new UiRect(margin,margin,left-2*margin,height-2*margin),
                new UiRect(margin,margin,width-2*margin,bottom-2*margin),
                new UiRect(margin,top+margin,width-2*margin,height-top-2*margin)
            };
            double best=-1;
            foreach(var space in free)
            {
                if(!space.Valid||space.Width<minWidth||space.Height<minHeight)continue;
                double w=Math.Min(desiredWidth,space.Width),h=Math.Min(desiredHeight,space.Height);
                double score=w*h;
                if(score<=best)continue;
                best=score;dock=new UiRect(space.X,space.Y,w,h);
            }
            return best>=0;
        }
    }
    // Cleanup cannot overwrite a later, distinguishable UI change.
    public sealed class UiOverrideLease<T>
    {
        T original,written;
        bool held;
        public bool Held=>held;
        public void Capture(T before,T after)
        {if(held)throw new InvalidOperationException("Override already held.");original=before;written=after;held=true;}
        public bool Release(T current,out T restore)
        {
            restore=original;
            bool matches=held&&EqualityComparer<T>.Default.Equals(current,written);
            held=false;original=default(T);written=default(T);return matches;
        }
    }
}
