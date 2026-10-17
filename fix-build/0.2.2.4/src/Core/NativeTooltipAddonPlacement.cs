using System;
namespace Alexander.CharacterSheet.Core
{
    public static class NativeTooltipAddonPlacement
    {
        public static bool TryBelow(double canvasWidth,double canvasHeight,UiRect native,double desiredHeight,
            double minimumHeight,double scale,out UiRect result)
        {
            result=default(UiRect);
            foreach(double v in new[]{canvasWidth,canvasHeight,desiredHeight,minimumHeight,scale})
                if(!NumberFormatting.IsFinite(v)||v<=0)return false;
            if(!native.Valid||scale<.69||scale>1.21)return false;
            double margin=12*scale,gap=4*scale;
            double available=native.Y-gap-margin;
            double width=Math.Min(native.Width,canvasWidth-2*margin);
            double height=Math.Min(desiredHeight,available);
            if(width<100*scale||height<minimumHeight||native.Y>canvasHeight-margin)return false;
            double x=Math.Max(margin,Math.Min(native.X,canvasWidth-width-margin));
            result=new UiRect(x,Math.Max(margin,native.Y-gap-height),width,height);
            return result.Valid;
        }
    }
}
