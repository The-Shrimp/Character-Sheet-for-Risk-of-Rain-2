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
        public string Id, Title, Detail, ShortTooltip, FullTooltip, LuckTooltip, Group="Utility / passive", LuckLabel="Unverified";
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
            var snapshot=CoreSnapshot(body);
            var core=CoreStatContributions.Build(snapshot);
            AddCoreStat(rows,"Health",core[CoreStatKind.Health],
                "Current health: "+F(body.healthComponent.health)+". Maximum health excludes shield/barrier.",body);
            AddCoreStat(rows,"Damage",core[CoreStatKind.Damage],
                "Raw damage coefficient basis; skill coefficients and conditional hit/proc damage are separate.",body);
            AddCoreStat(rows,"Regeneration",core[CoreStatKind.Regen],
                "Raw regen per second; heal-event efficiency is not inferred.",body);
            AddCoreStat(rows,"Armor",core[CoreStatKind.Armor],
                "Armor-only damage multiplier: "+F(VerifiedMath.ArmorMultiplier(body.armor))+". Armor Plate is a separate flat reduction.",body);
            AddCoreStat(rows,"Critical chance",core[CoreStatKind.Crit],
                "Raw crit: "+F(body.crit)+" points. Standard capped roll at observed luck: "+
                (body.master?F(100*LuckMath.EffectiveChance(body.crit/100.0,body.master.luck))+"%":"Unavailable (luck source missing)")+"; not converted/guaranteed special-roll behavior.",body);
            AddStat(rows, "Attack speed", body.attackSpeed, body.baseAttackSpeed, body.levelAttackSpeed * levels,
                "Live attack-speed multiplier.", body, false);
            AddStat(rows, "Movement speed", body.moveSpeed, body.baseMoveSpeed, body.levelMoveSpeed * levels,
                "Live movement speed in meters per second.", body, false);
            rows.Add(new SheetRow { Id = "barrier", Title = "Shield / barrier",
                Detail = F(body.healthComponent.shield) + " / " + F(body.healthComponent.barrier),
                ShortTooltip = snapshot.Pools.Describe(),
                FullTooltip = snapshot.Pools.Describe()+"\n"+
                    (core[CoreStatKind.Health].ModeledFinal.HasValue?
                        "Shield-source attribution and conversion order remain incomplete.":
                        core[CoreStatKind.Health].FinalUnavailableReason)+
                    "\nLive shield is an observation, not a predicted partition of the reviewed health subtotal. No hidden or enemy information is read.", LuckMark = "×", LuckTooltip = "These live pools are not luck rolls." });
            return rows;
        }

        static CoreStatSnapshot CoreSnapshot(CharacterBody body)
        {
            var snapshot=new CoreStatSnapshot {
                Level=body.level,IsSprinting=body.isSprinting,OutOfDanger=body.outOfDanger,
                OpalActive=body.HasBuff(DLC1Content.Buffs.OutOfCombatArmorBuff),
                FullCritActive=body.HasBuff(RoR2Content.Buffs.FullCrit),
                NectarActive=body.HasBuff(DLC2Content.Buffs.BoostAllStatsBuff),
                NectarCounter=body.currentGrowthNectarBuffCount,HealthCurseDivisor=body.cursePenalty,
                // Effective healing factors from difficulty are not substituted for raw regen scaling.
                RawRegenItemScale=null,
                Pools=new ProtectionPools {
                    CurrentHealth=body.healthComponent.health,MaxHealth=body.maxHealth,
                    CurrentShield=body.healthComponent.shield,MaxShield=body.maxShield,Barrier=body.healthComponent.barrier
                }
            };
            snapshot.Stats[CoreStatKind.Health]=new StatObservation(body.baseMaxHealth,body.levelMaxHealth,body.maxHealth);
            snapshot.Stats[CoreStatKind.Damage]=new StatObservation(body.baseDamage,body.levelDamage,body.damage);
            snapshot.Stats[CoreStatKind.Regen]=new StatObservation(body.baseRegen,body.levelRegen,body.regen);
            snapshot.Stats[CoreStatKind.Armor]=new StatObservation(body.baseArmor,body.levelArmor,body.armor);
            snapshot.Stats[CoreStatKind.Crit]=new StatObservation(body.baseCrit,body.levelCrit,body.crit);
            if(body.inventory)
            {
                snapshot.InfusionBonus=body.inventory.infusionBonus;
                foreach(string id in CoreStatContributions.RelevantItemIds)
                {
                    ItemIndex index=ItemCatalog.FindItemIndex(id);
                    var def=index!=ItemIndex.None?ItemCatalog.GetItemDef(index):null;
                    snapshot.Items[id]=def?new ObservedItemCount(body.inventory.GetItemCountPermanent(def),body.inventory.GetItemCountEffective(def)):
                        new ObservedItemCount(null,null);
                }
            }
            return snapshot;
        }

        static void AddCoreStat(List<SheetRow> rows,string name,ContributionResult result,string explanation,CharacterBody body)
        {
            bool critical=result.Kind==CoreStatKind.Crit;
            rows.Add(new SheetRow {
                Id=name,Title=name,Detail=NumberFormatting.Display(result.Live)+(critical?"%":""),
                ShortTooltip=explanation+"\n"+result.Describe(false),
                FullTooltip=explanation+"\n"+result.Describe(true)+
                    (result.Kind==CoreStatKind.Health?"\n"+new ProtectionPools {
                        CurrentHealth=body.healthComponent.health,MaxHealth=body.maxHealth,
                        CurrentShield=body.healthComponent.shield,MaxShield=body.maxShield,Barrier=body.healthComponent.barrier
                    }.Describe():"")+
                    "\nPublic component snapshot: "+OriginalItemDefinitions.Revision+
                    "\nStage/API documentation: "+CoreStatContributions.StatApiDocumentation+
                    "\nThis is original presentation-only explanation; it never recalculates or writes gameplay stats.",
                LuckMark=critical?"mix":"x",LuckLabel=critical?"Mixed scope":"Unaffected total",
                LuckTooltip=critical?"Luck affects eligible standard crit rolls, not the raw crit-stat points. Caps and conversion/special-roll rules require separate scope.":
                    "This live stat is not itself a luck roll; trigger chances and conditional effects are separate."
            });
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
            rows.Add(new SheetRow { Id = name, Title = name, Detail = F(live) + (critical ? "%" : ""),
                ShortTooltip = explanation + "\n" + calculation.Describe(live, false, false),
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
                string shortText=NativeText(def.pickupToken)+"\n"+current+"\n"+next;
                string fullText=NativeText(def.descriptionToken)+"\n\n"+formula;
                string luckLabel="Unverified";
                if(scaling!=null)
                {
                    group=definition.Group;
                    shortText=string.Join("\n\n",terms.Select(t=>t.Headline+"\n"+t.NextLabel))+
                        "\n"+scaling.CountLabel+"\nPublic snapshot component estimate; installed build unverified. Shift shows conditions.";
                    if(definition.TriggerContext.Length>0)shortText+="\n"+definition.TriggerContext;
                    if(definition.Id=="BoostAllStats")shortText+="\nAuthoritative live Nectar counter: "+body.currentGrowthNectarBuffCount+" / "+body.maxGrowthNectarBuffCount+"; modeled terms use the observed metadata set.";
                    fullText=string.Join("\n\n",terms.Select(t=>t.Headline+"\n"+t.FullMath+"\n"+t.Comparison.Confidence))+
                        "\n"+definition.TriggerContext+"\nDefinition source: "+definition.Source+
                        "\nReference revision: "+definition.Revision+
                        (definition.SupplementarySource.Length>0?"\nSupplementary public mechanics (reviewed 2026-10-05): "+definition.SupplementarySource:"")+
                        "\nNative description: "+NativeText(def.descriptionToken);
                    luck=scaling.LuckText;mark=scaling.LuckMark;
                    luckLabel=mark=="x"?"Unaffected term":mark=="mix"?"Mixed scope":mark=="\u2713"?"Affected roll":"Unverified";
                }
                else if(def.name=="LowerPricedChests")
                {
                    mark="!";luckLabel=LuckEvidence.Label(LuckEvidenceState.ReportedBug);
                    luck=LuckEvidence.SaleStarReport+"\nReport source: "+LuckEvidence.SaleStarReportSource+
                        "\nReported "+LuckEvidence.SaleStarReportDate+"; installed build not verified.";
                    shortText+="\nLuck: "+luckLabel;fullText+="\n"+luck;
                }
                rows.Add(new SheetRow { Id = "item:" + i, Title = (effects ? group + " · " : "") + name + " ×" + effective,
                    Detail = scaling!=null ? scaling.NextLabel : current+" - "+next, Icon = def.pickupIconSprite,
                    Color = ItemTierCatalog.GetItemTierDef(def.tier) ? ColorCatalog.GetColor(ItemTierCatalog.GetItemTierDef(def.tier).colorIndex) : Color.white, ShortTooltip = shortText,
                    FullTooltip = fullText +
                        "\nPermanent count: " + owned + ". Effective count: " + effective +
                        ". Effective includes the game's currently active temporary/channeled/suppressed rules. No inventory order or contents are changed.",
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
            var rows=new List<SheetRow>();
            for(int i=0;i<BuffCatalog.buffCount;i++)
            {
                var index=(BuffIndex)i;
                int stacks=body.GetBuffCount(index);
                if(stacks<=0)continue;
                var def=BuffCatalog.GetBuffDef(index);
                if(!def||def.isHidden)continue;
                bool counter=def==DLC2Content.Buffs.BoostAllStatsBuff;
                bool beadCounter=def==DLC2Content.Buffs.ExtraStatsOnLevelUpBuff;
                var exclusions=BuffCatalog.ignoreGrowthNectarIndices;
                bool? catalogIgnored=exclusions==null?(bool?)null:exclusions.Contains(index);
                var view=BuffGlossaryPresentation.Create(new BuffObservation {
                    PublicId=PublicBuffId(def),InternalId=def.name,DisplayName=Friendly(def.name),Stacks=stacks,
                    Hidden=def.isHidden,DefinitionIgnored=def.ignoreGrowthNectar,CatalogIgnored=catalogIgnored,
                    IsNectarCounter=counter,CanStack=def.canStack,IsDebuff=def.isDebuff,IsCooldown=def.isCooldown
                });
                if(view==null)continue;
                string extra="";
                if(counter)extra+="\nAuthoritative live qualifying-type counter: "+body.currentGrowthNectarBuffCount+" / "+body.maxGrowthNectarBuffCount+
                    ". Public active-counter bonus component reference: "+F(4.0*body.currentGrowthNectarBuffCount)+"%; not complete final stat attribution.";
                if(beadCounter)
                {
                    double? units=null;
                    if(body.inventory)
                    {
                        ItemIndex beadIndex=ItemCatalog.FindItemIndex("ExtraStatsOnLevelUp");
                        if(beadIndex!=ItemIndex.None)units=OriginalBuffDefinitions.BeadStoredLevelUnits(stacks,body.inventory.GetItemCountEffective(beadIndex));
                    }
                    extra+="\nStored level-gain units at current copies (conditional reference): "+NumberFormatting.Display(units)+
                        ". Not a percent of current stats or a certified cached payout.";
                }
                rows.Add(new SheetRow {
                    Id="buff:"+i,Title=view.Title,Detail=view.Detail,Icon=def.iconSprite,Color=def.buffColor,LuckMark="?",
                    LuckTooltip="Buff effects have separate scopes; no generic luck claim is made.",
                    ShortTooltip=view.ShortTooltip+extra,
                    FullTooltip=view.FullTooltip+extra+
                        "\nNectar's authoritative live count: "+body.currentGrowthNectarBuffCount+" / "+body.maxGrowthNectarBuffCount
                });
            }
            return rows.OrderBy(r=>r.Title).ToList();
        }

        static string PublicBuffId(BuffDef def)
        {
            // Match public object identities, not similar internal names. Unmatched definitions keep the unknown-effect fallback.
            if(def==RoR2Content.Buffs.FullCrit)return "RoR2Content.Buffs.FullCrit";
            if(def==DLC2Content.Buffs.BoostAllStatsBuff)return "DLC2Content.Buffs.BoostAllStatsBuff";
            if(def==DLC2Content.Buffs.ExtraStatsOnLevelUpBuff)return "DLC2Content.Buffs.ExtraStatsOnLevelUpBuff";
            if(def==DLC2Content.Buffs.ElusiveAntlersBuff)return "DLC2Content.Buffs.ElusiveAntlersBuff";
            if(def==DLC3Content.Buffs.SpeedOnPickup)return "DLC3Content.Buffs.SpeedOnPickup";
            if(def==DLC1Content.Buffs.ImmuneToDebuffReady)return "DLC1Content.Buffs.ImmuneToDebuffReady";
            if(def==DLC1Content.Buffs.ImmuneToDebuffCooldown)return "DLC1Content.Buffs.ImmuneToDebuffCooldown";
            if(def==RoR2Content.Buffs.Energized)return "RoR2Content.Buffs.Energized";
            if(def==DLC1Content.Buffs.BearVoidReady)return "DLC1Content.Buffs.BearVoidReady";
            if(def==DLC1Content.Buffs.BearVoidCooldown)return "DLC1Content.Buffs.BearVoidCooldown";
            if(def==DLC1Content.Buffs.OutOfCombatArmorBuff)return "DLC1Content.Buffs.OutOfCombatArmorBuff";
            if(def==RoR2Content.Buffs.Warbanner)return "RoR2Content.Buffs.Warbanner";
            if(def==DLC3Content.Buffs.JumpDamageStrikeCharge)return "DLC3Content.Buffs.JumpDamageStrikeCharge";
            if(def==DLC2Content.Buffs.KnockBackAvailable)return "DLC2Content.Buffs.KnockBackAvailable";
            if(def==DLC2Content.Buffs.KnockBackUnavailable)return "DLC2Content.Buffs.KnockBackUnavailable";
            if(def==RoR2Content.Buffs.TeslaField)return "RoR2Content.Buffs.TeslaField";
            if(def==RoR2Content.Buffs.MedkitHeal)return "RoR2Content.Buffs.MedkitHeal";
            if(def==RoR2Content.Buffs.WhipBoost)return "RoR2Content.Buffs.WhipBoost";
            if(def==RoR2Content.Buffs.AttackSpeedOnCrit)return "RoR2Content.Buffs.AttackSpeedOnCrit";
            return null;
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
                var exclusions=BuffCatalog.ignoreGrowthNectarIndices;
                var assessment=BuffGlossaryPresentation.AssessNectar(body.GetBuffCount(index),def.isHidden,def.ignoreGrowthNectar,
                    exclusions==null?(bool?)null:exclusions.Contains(index),def==DLC2Content.Buffs.BoostAllStatsBuff);
                if(assessment.Disposition==NectarDisposition.Unknown)return null;
                if(assessment.Disposition==NectarDisposition.Eligible)types++;
            }
            return types;
        }

    }
}
