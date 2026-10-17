using System;
namespace Alexander.CharacterSheet.Core
{
    // Only the unattributed compact display residual is filtered. Live values, known terms and traces are untouched.
    public static class DisplayResidual
    {
        public const double AbsoluteTolerance=1e-12,RelativeTolerance=6e-8;
        public static double ForDisplay(double live,double modeled)
        {
            double residual=live-modeled;
            if(!NumberFormatting.IsFinite(live)||!NumberFormatting.IsFinite(modeled)||!NumberFormatting.IsFinite(residual))return residual;
            double tolerance=Math.Max(AbsoluteTolerance,RelativeTolerance*Math.Max(Math.Abs(live),Math.Abs(modeled)));
            return Math.Abs(residual)<=tolerance?0:residual;
        }
    }
}
