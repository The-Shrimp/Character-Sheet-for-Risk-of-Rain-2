using System;
using System.Collections.Generic;
using System.Linq;
using Alexander.CharacterSheet.Core;
using RoR2;
using UnityEngine;
namespace Alexander.CharacterSheet
{
    internal sealed class SheetStructureEntry
    {
        internal readonly SheetNode Node;
        internal readonly SheetRow Row;
        internal SheetStructureEntry(SheetNode node,SheetRow row=null){Node=node;Row=row;}
    }
    internal static class SheetStructureData
    {
        internal static long Count(CharacterBody body,string id)
        {
            if(!body||!body.inventory)return 0;var index=ItemCatalog.FindItemIndex(id);
            return index==ItemIndex.None?0:Math.Max(0,body.inventory.GetItemCountEffective(index));
        }
        static SheetRow Numeric(string id,string title,double value,double basis,double growth,string suffix="",
            Action<Calculation> terms=null,string note="",bool signed=false)
        {
            var calculation=new Calculation("Base",basis);
            calculation.Apply("Level growth (level - 1)",CalculationStage.Flat,growth);
            terms?.Invoke(calculation);
            string compact=StatTooltipPresentation.Create(value,0,calculation,"cIsUtility",suffix);
            if(signed)compact=compact.Replace("Total: "+TooltipMarkup.Style(NumberFormatting.Display(value)+suffix,"cIsUtility"),
                "Total: "+TooltipMarkup.Style(NumberFormatting.Signed(value)+suffix,"cIsUtility"));
            if(note.Length>0)compact+="\n"+TooltipMarkup.Style(note,"cStack");
            string full=compact+"\n\n"+TooltipMarkup.Literal(calculation.Describe(value,false,true));
            return new SheetRow{Id=id,Title=title,Detail=(signed?NumberFormatting.Signed(value):NumberFormatting.Display(value))+suffix,
                ShortRichTooltip=compact,ShortTooltip=TooltipMarkup.Render(compact,null),FullRichTooltip=full,
                FullTooltip=TooltipMarkup.Render(full,null),LuckMark="x",LuckTooltip="This value is not itself a probability."};
        }
        internal static IReadOnlyList<SheetStructureEntry> Build(CharacterBody body,List<SheetRow> inventory)
        {
            var character=SheetData.Character(body).ToDictionary(r=>r.Id);
            character["Health"].Title="Health";character["Regeneration"].Title="Regen";
            character["Attack speed"].Title="Attack Speed";character["Critical chance"].Title="Crit Chance";
            character["Movement speed"].Title="Movement Speed";
            character["Regeneration"].Detail=NumberFormatting.Display(body.regen)+" /s";
            character["Attack speed"].Detail=NumberFormatting.Display(body.attackSpeed)+"x";
            character["Movement speed"].Detail=NumberFormatting.Display(body.moveSpeed)+" m/s";
            string armorNote="Reduction from armor: "+NumberFormatting.Display(CharacterEffectCatalog.ArmorReduction(body.armor))+"%.";
            character["Armor"].ShortRichTooltip+="\n"+TooltipMarkup.Style(armorNote,"cStack");
            character["Armor"].ShortTooltip+="\n"+armorNote;
            character["Armor"].FullRichTooltip+="\n"+TooltipMarkup.Literal(armorNote);
            if(CharacterEffectCatalog.ShieldVisible(body.maxShield))
            {
                var row=Numeric("Shield","Shield",body.maxShield,body.baseMaxShield,
                    body.levelMaxShield*Mathf.Max(0,body.level-1),note:"Capacity; temporary depletion does not remove this row.");
                row.ShortRichTooltip=StatTooltipPresentation.Create(body.maxShield,body.levelMaxShield,
                    ShieldCalculation(body),"cIsHealing")+"\n"+TooltipMarkup.Style("Shield capacity; current shield is in Details.","cStack");
                row.FullRichTooltip=row.ShortRichTooltip+"\n\n"+TooltipMarkup.Literal("Current shield: "+NumberFormatting.Display(body.healthComponent.shield)+"\n"+ShieldCalculation(body).Describe(body.maxShield,false,true));
                character["Shield"]=row;
            }
            long plates=Count(body,"ArmorPlate");
            character["Dmg Reduction"]=Numeric("Dmg Reduction","Dmg Reduction",CharacterEffectCatalog.FlatArmorPlate(plates),0,0,
                terms:c=>{if(plates>0)c.Apply("Repulsion Armor Plate",CalculationStage.Flat,CharacterEffectCatalog.FlatArmorPlate(plates));},
                note:"Repulsion Armor Plate per-hit reduction; hit conditions and minimum damage limit the benefit.");
            character["Sprint Speed Modifier"]=new SheetRow{Id="Sprint Speed Modifier",Title="Sprint Speed Modifier",
                Detail=NumberFormatting.Signed((body.sprintingSpeedMultiplier-1)*100)+"%",
                ShortRichTooltip="Total: "+TooltipMarkup.Style(NumberFormatting.Signed((body.sprintingSpeedMultiplier-1)*100)+"%","cIsUtility")+"\nBase: Unknown\nBonuses: Other/unknown",
                FullTooltip="Live sprint speed multiplier: "+NumberFormatting.Exact(body.sprintingSpeedMultiplier)+". Displayed modifier is (multiplier - 1) x 100. The survivor base coefficient and complete per-source attribution are unavailable.",
                LuckMark="x",LuckTooltip="Sprint speed is not itself a luck roll."};
            character["Maximum jumps"]=Numeric("Maximum jumps","Maximum jumps",body.maxJumpCount,body.baseJumpCount,0,
                terms:c=>{long feathers=Count(body,"Feather");if(feathers>0)c.Apply("Hopoo Feather",CalculationStage.Flat,feathers);},
                note:"Maximum jump count; this is not jumps remaining.");
            long clovers=Count(body,"Clover"),purity=Count(body,"LunarBadLuck");
            character["Luck"]=Numeric("Luck","Luck",body.master?body.master.luck:double.NaN,0,0,
                terms:c=>{if(clovers>0)c.Apply("57 Leaf Clover",CalculationStage.Flat,clovers);if(purity>0)c.Apply("Purity",CalculationStage.Flat,-purity);},signed:true);
            character["Luck"].LuckMark="mix";character["Luck"].LuckTooltip="Signed luck value, not a probability. Only eligible random rolls receive luck adjustment.";
            character["Cooldown Reduction"]=new SheetRow{Id="Cooldown Reduction",Title="Cooldown Reduction",Detail="Unknown",
                ShortRichTooltip=TooltipMarkup.Literal(GeneralCooldownPresentation.UnknownTotal),
                FullTooltip="A single live general cooldown total is unavailable. Per-skill cooldown values are excluded because they can include slot-specific modifiers such as Hardlight Afterburner.\n"+
                    string.Join("\n",GeneralCooldownPresentation.Components(Count(body,"AlienHead"),Count(body,"HalfAttackSpeedHalfCooldowns"),purity)),
                LuckMark="x",LuckTooltip="Cooldown modifiers are separate from luck rolls."};

            var routed=new Dictionary<string,List<SheetRow>>(StringComparer.Ordinal);
            foreach(var header in SheetHierarchy.Headers)routed[header.Id]=new List<SheetRow>();
            foreach(var row in SheetHierarchy.UniqueRows(inventory,r=>r.Id))
            {
                int index;if(!int.TryParse(row.Id.Substring("item:".Length),out index))continue;
                var def=ItemCatalog.GetItemDef((ItemIndex)index);if(!def)continue;
                row.Title=System.Text.RegularExpressions.Regex.Replace(Language.GetString(def.nameToken)??"","<[^>]*>","");
                row.Detail="x"+NumberFormatting.Count(Count(body,def.name));
                var rule=CharacterEffectCatalog.Item(def.name);
                string effectNote=rule?.HoverNote(body.outOfDanger,body.outOfCombat,body.isSprinting)??"";
                if(rule!=null&&rule.Section==SheetHierarchy.Damage){row.ItemHoverNote="";row.ExpandedItemNote=TooltipMarkup.Literal(GeneralDamageNote(def.name));}
                else row.ItemHoverNote=effectNote;
                routed[CharacterEffectCatalog.Route(def.name,EffectOrigin.Item)].Add(row);
            }
            var passive=body.skillLocator?body.skillLocator.passiveSkill:default(SkillLocator.PassiveSkill);
            if(body.skillLocator&&passive.enabled&&!string.IsNullOrWhiteSpace(passive.skillNameToken))
            {
                string name=Language.GetString(passive.skillNameToken),description=Language.GetString(passive.skillDescriptionToken);
                var row=new SheetRow{Id="passive:"+passive.skillNameToken,Title=name,Detail="Passive",Icon=passive.icon,
                    ShortRichTooltip=description,ShortTooltip=description,FullRichTooltip=description,
                    FullTooltip=description+"\nOwn native passive metadata only; no active ability effects are included.",
                    LuckMark="?",LuckTooltip="Passive luck applicability depends on the specific effect."};
                routed[CharacterEffectCatalog.Route(passive.skillNameToken,EffectOrigin.Passive)].Add(row);
            }
            var buffs=SheetData.Buffs(body);routed[SheetHierarchy.Other].AddRange(buffs);
            if(body.healthComponent.barrier>0)
                routed[SheetHierarchy.Other].Insert(0,Numeric("Barrier","Barrier",body.healthComponent.barrier,0,0,note:"Temporary barrier; separate from shield capacity."));
            var entries=new List<SheetStructureEntry>();
            foreach(var header in SheetHierarchy.Headers)
            {
                entries.Add(new SheetStructureEntry(header));
                string[] names;if(SheetHierarchy.Stats.TryGetValue(header.Id,out names))
                    foreach(string key in names){SheetRow row;if(character.TryGetValue(key,out row))entries.Add(new SheetStructureEntry(new SheetNode("stat:"+key,header.Id,row.Title,false,2,key),row));}
                foreach(var row in routed[header.Id])
                    entries.Add(new SheetStructureEntry(new SheetNode("effect:"+row.Id,header.Id,row.Title,false,1,row.Id),row));
            }
            return entries;
        }
        internal static IReadOnlyList<SheetStructureEntry> VisualBuild(CharacterBody body,List<SheetRow> inventory)
        {
            var original=Build(body,inventory);var rows=original.ToDictionary(e=>e.Node.Id,e=>e.Row);
            return VisualSheetStructure.Project(original.Select(e=>e.Node)).Select(n=>new SheetStructureEntry(n,rows[n.Id])).ToArray();
        }
        static string GeneralDamageNote(string id)
        {
            switch(id)
            {
                case "Crowbar":return "Conditional hit bonus against high-health targets. The native tooltip gives the threshold; no enemy health is inspected.";
                case "NearbyDamageBonus":return "Requires the native item proximity condition. Applies to eligible hits, not a universal damage-stat multiplier.";
                case "FragileDamageBonus":return "The bonus belongs to an unbroken watch; the native item description gives its break condition.";
                case "BossDamageBonus":return "Damage component applies to the native item's specified enemy category; no hidden enemy state is read.";
                case "RandomDamageZone":return "Conditional zone effect. Position, activation and eligible-hit conditions remain separate from the live Damage total.";
                case "IncreaseDamageOnMultiKill":return "Conditional multi-kill effect. Native trigger requirements and active state are separate from a universal damage multiplier.";
                default:return "Conditional damage component; native trigger and target requirements apply.";
            }
        }
        static Calculation ShieldCalculation(CharacterBody body)
        {
            var c=new Calculation("Base",body.baseMaxShield);c.Apply("Level growth (level - 1)",CalculationStage.Flat,body.levelMaxShield*Mathf.Max(0,body.level-1));return c;
        }
    }
}
