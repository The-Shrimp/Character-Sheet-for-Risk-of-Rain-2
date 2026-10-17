using System;
using System.Collections.Generic;
namespace Alexander.CharacterSheet.Core
{
    public static class GeneralCooldownPresentation
    {
        public static IReadOnlyList<string> Components(long alienHead,long lightFlux,long purity)
        {
            var lines=new List<string>();
            if(alienHead>0)lines.Add("Alien Head reference: "+NumberFormatting.Display(100*(1-Math.Pow(.75,alienHead)))+"% cooldown reduction.");
            if(lightFlux>0)lines.Add("Light Flux reference: "+NumberFormatting.Display(100*(1-Math.Pow(.5,lightFlux)))+"% cooldown reduction.");
            if(purity>0)lines.Add("Purity reference: -"+NumberFormatting.Display(1.0+purity)+" seconds.");
            return lines;
        }
        public const string UnknownTotal="Total: Unknown\nBase: Unknown\nBonuses: Other/unknown";
    }
}
