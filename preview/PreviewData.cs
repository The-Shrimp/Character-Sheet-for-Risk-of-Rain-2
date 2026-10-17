using System;
using System.Text;
using System.Web.Script.Serialization;
using Alexander.CharacterSheet.Core;
static class PreviewData
{
    static void Main(string[] args)
    {
        Console.OutputEncoding=new UTF8Encoding(false);
        var view=ItemTooltipPresentation.Create(new ItemScalingInput {
            Name="Prayer Beads",Verb="Stores",ValueLabel="of level-up stat gains",CopySingular="bead",CopyPlural="beads",
            OwnedCopies=4,BaseValue=20,PerAdditionalCopy=5,Unit=ValueUnit.Percent,Evidence=PredictionEvidence.VerifiedComponent,
            Conditions="User-supplied design equation; all four sample copies assumed to count. This preview does not verify game mechanics.",
            Luck=LuckApplicability.Unaffected,LuckDetail="The shown additive scaling term is not a random roll. Other item effects remain unverified."
        });
        string json=new JavaScriptSerializer().Serialize(new {
            title=view.Title,headline=view.Headline,equation=view.Equation,countLabel=view.CountLabel,nextLabel=view.NextLabel,
            nextEquation=view.NextEquation,fullMath=view.FullMath,luckMark=view.LuckMark,luckText=view.LuckText,
            current=view.CurrentValue,next=view.NextValue,additional=view.AdditionalCopies,
            scope="Design preview — sample data, not an in-game capture",
            character=new {name="Alexander",level=18,health=1248,damage=72,regen=5.8,armor=20,crit=32},
            items=new object[] {
                new {id="beads",name="Prayer Beads",count=4,group="Stat growth",symbol="◎",color="violet",selected=true},
                new {id="coat",name="Reinforced Coat",count=8,group="Defense",symbol="◇",color="blue",selected=false},
                new {id="module",name="Cooling Module",count=3,group="On hit",symbol="⊙",color="amber",selected=false},
                new {id="token",name="Fortune Token",count=1,group="Luck",symbol="✧",color="green",selected=false},
                new {id="relay",name="Echo Relay",count=6,group="On kill",symbol="≋",color="rose",selected=false},
                new {id="patch",name="Field Patch",count=2,group="Defense",symbol="+",color="blue",selected=false}
            }
        });
        System.IO.File.WriteAllText(args[0],json,new UTF8Encoding(false));
        Console.WriteLine("Wrote synthetic preview data.");
    }
}
