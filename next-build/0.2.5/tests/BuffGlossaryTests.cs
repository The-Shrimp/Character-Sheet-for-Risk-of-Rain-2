using System;
using System.Linq;
using Alexander.CharacterSheet.Core;
static class BuffGlossaryTests
{
    static BuffObservation Observe(string publicId,long? stacks=1)=>new BuffObservation {
        PublicId=publicId,InternalId="observedInternalId",DisplayName="Observed buff",Stacks=stacks,
        Hidden=false,DefinitionIgnored=false,CatalogIgnored=false,IsNectarCounter=false,CanStack=false,IsDebuff=false,IsCooldown=false
    };
    public static void Run(Action<bool,string> assert)
    {
        string[] expected={
            "RoR2Content.Buffs.FullCrit","DLC2Content.Buffs.BoostAllStatsBuff","DLC2Content.Buffs.ExtraStatsOnLevelUpBuff",
            "DLC2Content.Buffs.ElusiveAntlersBuff","DLC3Content.Buffs.SpeedOnPickup","DLC1Content.Buffs.ImmuneToDebuffReady",
            "DLC1Content.Buffs.ImmuneToDebuffCooldown","RoR2Content.Buffs.Energized","DLC1Content.Buffs.BearVoidReady",
            "DLC1Content.Buffs.BearVoidCooldown","DLC1Content.Buffs.OutOfCombatArmorBuff","RoR2Content.Buffs.Warbanner",
            "DLC3Content.Buffs.JumpDamageStrikeCharge","DLC2Content.Buffs.KnockBackAvailable","DLC2Content.Buffs.KnockBackUnavailable",
            "RoR2Content.Buffs.TeslaField","RoR2Content.Buffs.MedkitHeal","RoR2Content.Buffs.WhipBoost","RoR2Content.Buffs.AttackSpeedOnCrit"
        };
        assert(expected.OrderBy(x=>x).SequenceEqual(OriginalBuffDefinitions.SupportedDefinitions.Select(d=>d.PublicId).OrderBy(x=>x)),
            "Glossary exact supported public identities match the bounded requested coverage");
        foreach(string id in expected)
        {
            var definition=OriginalBuffDefinitions.Find(id);
            var positive=BuffGlossaryPresentation.Create(Observe(id,1));
            assert(definition!=null&&positive.Definition==definition&&definition.Source.Contains(definition.Revision)&&
                definition.StackMeaning.Length>0&&definition.Conditions.Length>0&&positive.FullTooltip.Contains("installed behavior unverified")&&
                positive.FullTooltip.Contains(BuffGlossaryPresentation.RemainingDuration)&&
                definition.Describe(0).Contains("State inactive")&&!definition.Describe(0).Contains("Observed active-stack")&&
                BuffGlossaryPresentation.Create(Observe(id,0))==null,id+" has bounded source/meaning/conditions and zero stacks imply no active row/component");
        }
        var pred=OriginalBuffDefinitions.Find("RoR2Content.Buffs.AttackSpeedOnCrit");
        assert(pred.Describe(1).Contains("12%")&&pred.Describe(4).Contains("48%")&&pred.Describe(10).Contains("120%"),
            "Predatory active-stack components at 1/4/10 are independent of owned item count or crit points");
        var faraday=OriginalBuffDefinitions.Find("DLC3Content.Buffs.JumpDamageStrikeCharge");
        assert(faraday.Describe(5).Contains("movement bonus component: 8%")&&faraday.Describe(5).Contains("jump-height bonus component: 10%"),
            "Five observed Faraday charges have two independent bounded component references");
        var horn=OriginalBuffDefinitions.Find("RoR2Content.Buffs.Energized");
        assert(horn.Describe(4).Contains("70%")&&!horn.Describe(4).Contains("280%")&&!horn.Describe(4).Contains("Observed active-stack"),
            "War Horn active-state count never multiplies its fixed public magnitude");
        var banner=OriginalBuffDefinitions.Find("RoR2Content.Buffs.Warbanner");
        assert(banner.Describe(4).Contains("30%")&&!banner.Describe(4).Contains("120%"),"Warbanner fixed references are not per observed stack");
        assert(OriginalBuffDefinitions.Find("DLC1Content.Buffs.OutOfCombatArmorBuff").StackMeaning.Contains("not the number")&&
            OriginalBuffDefinitions.Find("RoR2Content.Buffs.WhipBoost").StackMeaning.Contains("not effective item count"),
            "Opal and Red Whip status count does not fabricate owned-copy magnitude");
        assert(OriginalBuffDefinitions.Find("RoR2Content.Buffs.MedkitHeal").Effect.Contains("no immediate healing")&&
            OriginalBuffDefinitions.Find("RoR2Content.Buffs.MedkitHeal").Kind=="Pending heal",
            "Medkit pending state is neither restored health nor raw regeneration");
        assert(OriginalBuffDefinitions.Find("RoR2Content.Buffs.TeslaField").StackMeaning.Contains("not shocks dealt")&&
            !OriginalBuffDefinitions.Find("RoR2Content.Buffs.TeslaField").Describe(10).Contains("Observed active-stack"),
            "Tesla active-state count does not predict shock count or DPS");
        var rain=Observe("DLC1Content.Buffs.ImmuneToDebuffReady",2);
        var ready=BuffGlossaryPresentation.Create(rain);
        assert(ready.Kind=="Ready charges"&&ready.Nectar.Disposition==NectarDisposition.Eligible,"Raincoat ready charges use live eligible flags");
        rain.Stacks=0;assert(BuffGlossaryPresentation.Create(rain)==null,"Consumed zero-ready state cannot retain a ready row");
        rain.PublicId="DLC1Content.Buffs.ImmuneToDebuffCooldown";rain.Stacks=1;rain.IsCooldown=true;rain.CatalogIgnored=true;
        var recharging=BuffGlossaryPresentation.Create(rain);
        assert(recharging.Kind=="Recharge state"&&recharging.Nectar.Disposition==NectarDisposition.NotCounted&&
            recharging.FullTooltip.Contains("live catalog exclusion")&&recharging.ShortTooltip.Contains("Remaining duration unavailable"),
            "Ready-to-recharge transition replaces meaning and respects that state's catalog exclusion without a timer");
        rain.CatalogIgnored=false;
        assert(BuffGlossaryPresentation.Create(rain).Nectar.Disposition==NectarDisposition.Eligible,
            "Cooldown name/flag cannot override qualifying live Nectar metadata");
        rain.PublicId="DLC1Content.Buffs.ImmuneToDebuffReady";rain.IsCooldown=false;
        assert(BuffGlossaryPresentation.Create(rain).Kind=="Ready charges","Recharge-to-ready transition has no stale state");
        foreach(var pair in new[]{new[]{"DLC1Content.Buffs.BearVoidReady","DLC1Content.Buffs.BearVoidCooldown"},
            new[]{"DLC2Content.Buffs.KnockBackAvailable","DLC2Content.Buffs.KnockBackUnavailable"}})
        {
            var a=BuffGlossaryPresentation.Create(Observe(pair[0]));
            var state=Observe(pair[1]);state.IsCooldown=true;state.DefinitionIgnored=true;
            var b=BuffGlossaryPresentation.Create(state);
            assert(a.Kind=="Ready state"&&b.Kind=="Cooldown state"&&a.Title!=b.Title&&b.Nectar.Disposition==NectarDisposition.NotCounted,
                pair[0]+" ready/cooldown meanings and Nectar flags are independent");
        }
        var unknown=Observe(null,3);unknown.InternalId="HiddenInvulnerability";unknown.DisplayName="Hidden Invulnerability";
        var unfamiliar=BuffGlossaryPresentation.Create(unknown);
        assert(unfamiliar.Definition==null&&unfamiliar.Title.Contains("Unverified")&&
            unfamiliar.ShortTooltip.Contains("Internal definition: HiddenInvulnerability")&&unfamiliar.FullTooltip.Contains("Effect evidence: Unverified"),
            "Unfamiliar visible HiddenInvulnerability has an internal ID and explicit unknown effect");
        unknown.Hidden=true;
        assert(BuffGlossaryPresentation.Create(unknown)==null&&BuffGlossaryPresentation.AssessNectar(3,true,false,false,false).Disposition==NectarDisposition.NotCounted,
            "Actual hidden metadata excludes both display and Nectar without exposing a hidden effect");
        unknown.Hidden=null;assert(BuffGlossaryPresentation.Create(unknown)==null,"Unknown visibility cannot expose a potentially hidden state");
        unknown.Hidden=false;unknown.InternalId="Warbanner";unknown.DisplayName="Warbanner";
        assert(BuffGlossaryPresentation.Create(unknown).Definition==null,"Familiar name without matched public object identity remains unknown");
        unknown.CatalogIgnored=null;
        assert(BuffGlossaryPresentation.Create(unknown).Nectar.Disposition==NectarDisposition.Unknown,
            "Missing catalog context is unknown rather than guessed qualifying");
        unknown.DefinitionIgnored=true;
        assert(BuffGlossaryPresentation.Create(unknown).Nectar.Disposition==NectarDisposition.NotCounted,
            "A known definition exclusion remains decisive despite missing catalog context");
        unknown.DefinitionIgnored=false;unknown.CatalogIgnored=false;unknown.IsDebuff=true;unknown.IsCooldown=true;
        assert(BuffGlossaryPresentation.Create(unknown).Nectar.Disposition==NectarDisposition.Eligible,
            "Debuff/cooldown semantics do not invent a Nectar exclusion when live flags qualify");
        var counter=Observe("DLC2Content.Buffs.BoostAllStatsBuff",4);counter.IsNectarCounter=true;
        var types=BuffGlossaryPresentation.Create(counter);
        assert(types.Kind=="Type counter"&&types.Nectar.Disposition==NectarDisposition.NotCounted&&types.FullTooltip.Contains("excludes itself"),
            "Observed Nectar self-counter is distinct from eligible ordinary buff types");
        var beads=BuffGlossaryPresentation.Create(Observe("DLC2Content.Buffs.ExtraStatsOnLevelUpBuff",6));
        assert(beads.Kind=="Stored level counter"&&beads.ShortTooltip.Contains("not a live percent-stat buff"),
            "Bead stored counter remains distinct from active stat stacks and certified removal payout");
        var antlers=Observe("DLC2Content.Buffs.ElusiveAntlersBuff",4);antlers.CanStack=true;
        var oneType=BuffGlossaryPresentation.Create(antlers);
        antlers.Stacks=10;
        assert(oneType.Nectar.Disposition==NectarDisposition.Eligible&&
            BuffGlossaryPresentation.Create(antlers).Nectar.Reason==oneType.Nectar.Reason,
            "More stacks do not become more Nectar types");
        assert(BuffGlossaryPresentation.AssessNectar(0,false,false,false,false).Disposition==NectarDisposition.NotCounted&&
            BuffGlossaryPresentation.AssessNectar(null,false,false,false,false).Disposition==NectarDisposition.Unknown&&
            BuffGlossaryPresentation.AssessNectar(-1,false,false,false,false).Disposition==NectarDisposition.Unknown,
            "Inactive, missing and invalid counts retain distinct safe Nectar states");
        var missingCount=Observe("RoR2Content.Buffs.AttackSpeedOnCrit",null);
        var missing=BuffGlossaryPresentation.Create(missingCount);
        assert(missing.Nectar.Disposition==NectarDisposition.Unknown&&missing.ShortTooltip.Contains("observed count missing")&&
            !missing.ShortTooltip.Contains("Observed active-stack"),"Missing stack count cannot fabricate a numeric active component");
        assert(OriginalBuffDefinitions.Find("HiddenInvulnerability")==null&&OriginalBuffDefinitions.Find("RoR2Content.Buffs.HiddenInvincibility")==null&&
            OriginalBuffDefinitions.Find("mod.Warbanner")==null,"Unverified/ambiguous immunity or namespace names have no confident glossary match");
        assert(OriginalBuffDefinitions.Find("bdBearVoidReady").PublicId=="DLC1Content.Buffs.BearVoidReady"&&
            OriginalBuffDefinitions.Find("DLC2Content.Buffs.ElusiveAntlersBuff").Describe(4).Contains("48%"),
            "Public qualified identities and legacy API aliases map consistently without runtime name inference");
        assert(unfamiliar.FullTooltip.Contains("\n")&&!unfamiliar.FullTooltip.Contains("\\n")&&unfamiliar.Detail.Contains("Timer unavailable"),
            "Unknown fallback is readable and explicitly lacks a timer");
    }
}
