using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Alexander.CharacterSheet.Core;
using RoR2;
using UnityEngine;

namespace Alexander.CharacterSheet
{
    internal sealed class SheetRow
    {
        public string Id, Title, Detail, ShortTooltip, FullTooltip, ShortRichTooltip, FullRichTooltip, LuckTooltip, Group="Utility / passive", LuckLabel="Unverified";
        public bool FormulaRegistered;
        public Sprite Icon;
        public Color Color = Color.white;
        public string LuckMark = "?";
    }

    internal static class SheetData
    {

        static string F(double value) => NumberFormatting.Display(value);
        static string NativeText(string token)
        {
            string text = Language.GetString(token);
            return TooltipText.Clean(Regex.Replace(text ?? "", "<[^>]*>", ""));
        }
        static string Friendly(string name)
        {
            string value=name??"Unknown";if(value.StartsWith("bd",StringComparison.Ordinal))value=value.Substring(2);
            return Regex.Replace(value,"([a-z])([A-Z])","$1 $2").Trim();
        }

        public static List<SheetRow> Character(CharacterBody body)
        {
            float levels = Mathf.Max(0, body.level - 1);
            var rows = new List<SheetRow>();
            AddStat(rows, "Health", body.maxHealth, body.baseMaxHealth, body.levelMaxHealth * levels,
                "Maximum health. Current health: " + F(body.healthComponent.health), body, false);
            AddStat(rows, "Damage", body.damage, body.baseDamage, body.levelDamage * levels,
                "Damage coefficient basis; conditional hit damage is separate.", body, false);
            AddStat(rows, "Regeneration", body.regen, body.baseRegen, body.levelRegen * levels,
                "Health per second before heal-event rules.", body, false);
            AddStat(rows, "Armor", body.armor, body.baseArmor, body.levelArmor * levels,
                "Damage multiplier from armor alone: " + F(body.armor >= 0 ? 100.0 / (100 + body.armor) : 2 - 100.0 / (100 - body.armor)), body, false);
            AddStat(rows, "Critical chance", body.crit, body.baseCrit, body.levelCrit * levels,
                "Raw crit stat: " + F(body.crit) + "%. Standard roll after luck: " +
                F(100 * LuckMath.EffectiveChance(body.crit / 100.0, body.master ? body.master.luck : 0)) + "%.", body, true);
            AddStat(rows, "Attack speed", body.attackSpeed, body.baseAttackSpeed, body.levelAttackSpeed * levels,
                "Live attack-speed multiplier.", body, false);
            AddStat(rows, "Movement speed", body.moveSpeed, body.baseMoveSpeed, body.levelMoveSpeed * levels,
                "Live movement speed in meters per second.", body, false);
            rows.Add(new SheetRow { Id = "barrier", Title = "Shield / barrier",
                Detail = F(body.healthComponent.shield) + " / " + F(body.healthComponent.barrier),
                ShortTooltip = "Shield capacity: " + F(body.maxShield) + ". Barrier is temporary protection, not maximum health.",
                FullTooltip = "Shield capacity: " + F(body.maxShield) + ". Current barrier: " + F(body.healthComponent.barrier) +
                    ". Separate pools; no hidden or enemy information is read.", LuckMark = "×", LuckTooltip = "These live pools are not luck rolls." });
            return rows;
        }

        static void AddStat(List<SheetRow> rows, string name, double live, double baseline, double growth,
            string explanation, CharacterBody body, bool critical)
        {
            var calculation = new Calculation("Survivor baseline", baseline);
            calculation.Apply("Level growth (level − 1)", CalculationStage.Flat, growth);
            if (critical && body.inventory)
            {
                ItemIndex index = ItemCatalog.FindItemIndex("CritGlasses");
                if (index != ItemIndex.None)
                {
                    int count = body.inventory.GetItemCountEffective(index);
                    if (count > 0) calculation.Apply("Lens-Maker's Glasses (" + count + " effective)", CalculationStage.Flat, count * 10);
                }
            }
            string compactStat=CompactTooltipPresentation.Stat(explanation,live,calculation,
                name=="Damage"||name=="Critical chance"||name=="Attack speed"?"cIsDamage":
                name=="Health"||name=="Regeneration"?"cIsHealing":"cIsUtility",
                critical?"%":name=="Regeneration"?" /s":name=="Movement speed"?" m/s":name=="Attack speed"?"x":"",name);
            rows.Add(new SheetRow { Id = name, Title = name, Detail = F(live) + (critical ? "%" : ""),
                ShortTooltip=TooltipMarkup.Render(compactStat,null),ShortRichTooltip=compactStat,
                FullTooltip = explanation + "\n" + calculation.Describe(live, false, true) +
                    "\nBaseline is an explanation reference, not a replacement stat calculation. Later buffs, items, conversions, and other mods may remain unattributed.",
                LuckMark=critical?"mix":"x", LuckLabel=critical?"Mixed scope":"Unaffected total",
                LuckTooltip = critical ? "Clover affects standard critical rolls. The crit stat itself stays unchanged; guaranteed crit and converted crit require context." :
                    "This total is not itself a luck roll. Conditional item effects are listed separately." });
        }

        public static List<SheetRow> Inventory(CharacterBody body, bool effects)
        {
            var rows = new List<SheetRow>();
            if (!body.inventory) return rows;
            for (int i = 0; i < ItemCatalog.itemCount; i++)
            {
                var def = ItemCatalog.GetItemDef((ItemIndex)i);
                if (!def || def.hidden) continue;
                int owned = body.inventory.GetItemCountPermanent(def);
                int effective = body.inventory.GetItemCountEffective(def);
                if (owned <= 0 && effective <= 0) continue;
                string name = NativeText(def.nameToken);
                string group = Group(def);
                string current = "Current: " + effective + " effective";
                string next = "Next copy: formula unavailable";
                string formula = "No verified quantitative stack formula is registered for this item/build. Native description follows.";
                string luck = "Luck applicability is unverified for this item's individual effects.";
                string mark = "?";
                var definition=OriginalItemDefinitions.Find(def.name);
                IReadOnlyList<ItemTooltipView> terms=definition?.CreateTerms(name,effective,definition.Id=="BoostAllStats"?OwnEligibleBuffTypes(body):null);
                ItemTooltipView scaling=terms!=null?terms[0]:null;
                string fullText=formula;
                string luckLabel="Unverified";
                if(scaling!=null)
                {
                    group=definition.Group;
                    fullText=string.Join("\n\n",terms.Select(t=>t.CompactValue+"\n"+t.NextLabel+"\n"+t.FullMath+"\n"+t.Comparison.Confidence))+
                        "\n"+definition.TriggerContext+"\nDefinition source: "+definition.Source+
                        "\nReference revision: "+definition.Revision+
                        (definition.SupplementarySource.Length>0?"\nSupplementary public mechanics (reviewed 2026-10-05): "+definition.SupplementarySource:"");
                    if(definition.Id=="BoostAllStats")fullText+="\nLive qualifying-type count: "+body.currentGrowthNectarBuffCount+" / "+body.maxGrowthNectarBuffCount+"; observed metadata and model remain separate.";
                    luck=scaling.LuckText;mark=scaling.LuckMark;
                    luckLabel=mark=="x"?"Unaffected term":mark=="mix"?"Mixed scope":mark=="\u2713"?"Affected roll":"Unverified";
                }
                else if(def.name=="LowerPricedChests")
                {
                    mark="!";luckLabel=LuckEvidence.Label(LuckEvidenceState.ReportedBug);
                    luck=LuckEvidence.SaleStarReport+"\nReport source: "+LuckEvidence.SaleStarReportSource+
                        "\nReported "+LuckEvidence.SaleStarReportDate+"; installed build not verified.";
                    fullText+="\n"+luck;
                }
                string compact=CompactTooltipPresentation.Item(terms,Language.GetString(def.pickupToken),effective,owned);
                string detailed=Language.GetString(def.descriptionToken)+"\n\n"+TooltipMarkup.Literal(fullText+
                    "\nOwned: "+owned+" | Effective: "+effective);
                rows.Add(new SheetRow { FormulaRegistered=scaling!=null,ShortRichTooltip=compact,FullRichTooltip=detailed,Id = "item:" + i, Title = (effects ? group + " · " : "") + name + " ×" + effective,
                    Detail = scaling!=null ? scaling.NextLabel : current+" - "+next, Icon = def.pickupIconSprite,
                    Color = ItemTierCatalog.GetItemTierDef(def.tier) ? ColorCatalog.GetColor(ItemTierCatalog.GetItemTierDef(def.tier).colorIndex) : Color.white, ShortTooltip = TooltipText.Clean(Regex.Replace(compact,"<[^>]*>","")),
                    FullTooltip = NativeText(def.descriptionToken)+"\n\n"+fullText+"\nOwned: "+owned+" | Effective: "+effective,
                    Group=group, LuckMark=mark, LuckLabel=luckLabel, LuckTooltip=luck });
            }
            return rows.OrderBy(r => effects ? r.Group : "")
                .ThenBy(r => r.Title, StringComparer.OrdinalIgnoreCase).ToList();
        }

        static string Group(ItemDef def)
        {
            if (def.tags != null && def.tags.Contains(ItemTag.OnKillEffect)) return "On kill";
            if (def == RoR2Content.Items.CritGlasses) return "On hit";
            if (def.tags != null && def.tags.Contains(ItemTag.Healing)) return "Defense / sustain";
            if (def.tags != null && def.tags.Contains(ItemTag.Damage)) return "Damage · trigger unverified";
            return "Utility / passive";
        }

        public static List<SheetRow> Buffs(CharacterBody body)
        {
            var rows = new List<SheetRow>();
            for (int i = 0; i < BuffCatalog.buffCount; i++)
            {
                var index = (BuffIndex)i;
                int stacks = body.GetBuffCount(index);
                if (stacks <= 0) continue;
                var def = BuffCatalog.GetBuffDef(index);
                if (!def || def.isHidden) continue;
                bool counter=def==DLC2Content.Buffs.BoostAllStatsBuff;
                bool beadCounter=def==DLC2Content.Buffs.ExtraStatsOnLevelUpBuff;
                bool nectar=BuffGlossaryRules.NectarEligible(def.isHidden,def.ignoreGrowthNectar,BuffCatalog.ignoreGrowthNectarIndices.Contains(index),counter);
                string kind=beadCounter?"Counter (stored level gains)":BuffGlossaryRules.Kind(def.isDebuff,def.isCooldown,counter);
                string duration = Remaining(body, index);
                string publicId=def==RoR2Content.Buffs.FullCrit?"RoR2Content.Buffs.FullCrit":
                    counter?"DLC2Content.Buffs.BoostAllStatsBuff":beadCounter?"DLC2Content.Buffs.ExtraStatsOnLevelUpBuff":null;
                var glossary=OriginalBuffDefinitions.Find(publicId??def.name);
                if(glossary?.Id=="ImmuneToDebuff")kind="Charges (ready)";
                else if(glossary?.Id=="ImmuneToDebuffConsumed")kind="Recharge / charge state";
                string effect=glossary?.Describe(stacks)??"Effect formula is not yet verified; authoritative totals are shown in the live stats panel.";
                if(counter)effect+="\nLive qualifying-type counter: "+body.currentGrowthNectarBuffCount+" / "+body.maxGrowthNectarBuffCount+
                    ". Public bonus component reference: "+F(4.0*body.currentGrowthNectarBuffCount)+"%; not complete final stat attribution.";
                if(beadCounter&&body.inventory)
                {
                    ItemIndex beadIndex=ItemCatalog.FindItemIndex("ExtraStatsOnLevelUp");
                    if(beadIndex!=ItemIndex.None)
                    {
                        double? units=OriginalBuffDefinitions.BeadStoredLevelUnits(stacks,body.inventory.GetItemCountEffective(beadIndex));
                        effect+="\nStored level-gain units at current copies (conditional reference): "+NumberFormatting.Display(units)+
                            ". Not a percent of current stats or a certified cached payout.";
                    }
                }
                rows.Add(new SheetRow { Id = "buff:" + i, Title = (glossary?.Name??Friendly(def.name)) + " ×" + stacks,
                    Detail = kind + " · " + duration + " · Nectar: " + (nectar ? "eligible type" : "excluded"),
                    Icon = def.iconSprite, Color = def.buffColor, LuckMark = "?",
                    LuckTooltip = "Buff effects are context dependent; no generic luck claim is made.",
                    ShortTooltip = CompactBuff(glossary?.Id)+"\n"+kind+" | Nectar: "+(nectar?"eligible":"excluded")+"\nTimer unavailable",
                    ShortRichTooltip=TooltipMarkup.Literal(CompactBuff(glossary?.Id))+"\n"+
                        TooltipMarkup.Style(kind+" | Nectar: "+(nectar?"eligible":"excluded"),"cStack")+"\n"+
                        TooltipMarkup.Style("Timer unavailable"+(glossary!=null?" | Reference effect":""),"cStack"),
                    FullTooltip = effect + "\nInternal definition: " + def.name + "\nStacks: " + stacks +
                        (def.canStack ? " (stackable)" : " (does not stack normally)") + "\n" + kind +
                        "\n" + duration + "\nGrowth Nectar counts qualifying active types, not individual stacks." +
                        "\nNectar's authoritative count: " + body.currentGrowthNectarBuffCount + " / " + body.maxGrowthNectarBuffCount+
                        (glossary!=null?"\nEffect evidence: "+glossary.Confidence+"\nSource: "+glossary.Source:"\nEffect evidence: Unverified") });
            }
            return rows.OrderBy(r => r.Title).ToList();
        }

        static long? OwnEligibleBuffTypes(CharacterBody body)
        {
            if(!body||BuffCatalog.buffCount<=0)return null;
            long types=0;
            for(int i=0;i<BuffCatalog.buffCount;i++)
            {
                var index=(BuffIndex)i;
                if(body.GetBuffCount(index)<=0)continue;
                var def=BuffCatalog.GetBuffDef(index);
                if(!def)return null;
                if(def.isHidden)continue;
                if(BuffGlossaryRules.NectarEligible(def.isHidden,def.ignoreGrowthNectar,
                    BuffCatalog.ignoreGrowthNectarIndices.Contains(index),def==DLC2Content.Buffs.BoostAllStatsBuff))types++;
            }
            return types;
        }

        static string CompactBuff(string id)
        {
            switch(id)
            {
                case "RoR2Content.Buffs.FullCrit":return "+100 raw crit points before conversions.";
                case "DLC2Content.Buffs.BoostAllStatsBuff":return "Qualifying buff TYPES, not stacks. Reference: 4% per type.";
                case "DLC2Content.Buffs.ExtraStatsOnLevelUpBuff":return "Stored level gains; removal payout depends on cached state.";
                case "ElusiveAntlersSpeedBuff":return "Reference: +12% movement bonus per active stack.";
                case "SpeedOnPickup":return "Reference: +3% stat-bonus component per active stack.";
                case "ImmuneToDebuff":return "Ready charges prevent eligible temporary debuffs.";
                case "ImmuneToDebuffConsumed":return "Consumed charges recharging; no active defense multiplier.";
                case "Energized":return "Equipment-triggered attack-speed buff; magnitude unverified.";
                default:return "Effect unverified";
            }
        }
        static string Remaining(CharacterBody body, BuffIndex index)
        {
            return "Remaining duration unavailable: public per-stack timer adapter pending";
        }
    }
}
