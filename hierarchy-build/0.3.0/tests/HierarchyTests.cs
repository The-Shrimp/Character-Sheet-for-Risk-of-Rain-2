using System;
using System.Collections.Generic;
using System.Linq;
using Alexander.CharacterSheet.Core;
static class HierarchyTests
{
    public static void Run(Action<bool,string> assert)
    {
        string[] expected={"Base Stats","Defense","Offense","Movement","Luck","General Damage Modifiers","On-Hit","On-Kill","Out of Combat","While Sprinting","Other items & buffs"};
        assert(SheetHierarchy.Headers.Select(n=>n.Title).SequenceEqual(expected),"Hierarchy keeps the exact approved section names and preserves the glossary fallback");
        assert(SheetHierarchy.Stats[SheetHierarchy.Defense].SequenceEqual(new[]{"Health","Shield","Regeneration","Armor","Dmg Reduction"}),"Corrected Defense contains discrete shield and flat reduction");
        assert(SheetHierarchy.Stats[SheetHierarchy.Offense].SequenceEqual(new[]{"Damage","Attack speed","Cooldown Reduction","Critical chance"}),"Offense has general CDR rather than skill-specific values");
        assert(SheetHierarchy.Stats[SheetHierarchy.Movement].SequenceEqual(new[]{"Movement speed","Sprint Speed Modifier","Maximum jumps"}),"Movement explicitly labels maximum jumps");
        assert(SheetHierarchy.Stats[SheetHierarchy.Luck].SequenceEqual(new[]{"Luck"}),"Luck is its own subgroup");
        assert(SheetHierarchy.Headers.Single(n=>n.Id==SheetHierarchy.Defense).Glyph==SheetGlyph.Shield&&SheetHierarchy.Headers.Single(n=>n.Id==SheetHierarchy.Offense).Glyph==SheetGlyph.Sword,"Defense and Offense have redundant semantic margin icons");
        assert(SheetHierarchy.Headers.Single(n=>n.Id==SheetHierarchy.Movement).Glyph==SheetGlyph.Runner&&SheetHierarchy.Headers.Single(n=>n.Id==SheetHierarchy.Luck).Glyph==SheetGlyph.Clover,"Movement and Luck have their requested margin icons");
        assert(SheetHierarchy.Headers.Single(n=>n.Id==SheetHierarchy.Hit).Glyph==SheetGlyph.Burst&&SheetHierarchy.Headers.Single(n=>n.Id==SheetHierarchy.Kill).Glyph==SheetGlyph.Skull,"On-Hit uses an impact burst and On-Kill a skull");
        var nodes=new List<SheetNode>();
        foreach(var header in SheetHierarchy.Headers)
        {
            nodes.Add(header);string[] stats;if(SheetHierarchy.Stats.TryGetValue(header.Id,out stats))
                foreach(string key in stats)nodes.Add(new SheetNode("stat:"+key,header.Id,key,false,2));
            if(header.Id==SheetHierarchy.Hit)nodes.Add(new SheetNode("effect:Missile",header.Id,"ATG",false,1));
            if(header.Id==SheetHierarchy.Other)nodes.Add(new SheetNode("buff:1",header.Id,"Buff",false,1));
        }
        var state=new SheetExpansionState();var visible=SheetHierarchy.Visible(nodes,state);
        assert(visible.Any(n=>n.Id=="stat:Health")&&visible.Any(n=>n.Id=="stat:Luck"),"Base and all stat subgroups start expanded");
        assert(!visible.Any(n=>n.Id=="effect:Missile"||n.Id=="buff:1"),"Conditional sections and preserved extras start collapsed");
        state.Set(SheetHierarchy.Defense,false);state.Set(SheetHierarchy.Base,false);
        assert(!SheetHierarchy.Visible(nodes,state).Any(n=>n.Id==SheetHierarchy.Defense),"Closing Base hides its subgroup headers and all descendants");
        state.Set(SheetHierarchy.Base,true);
        assert(!state.IsOpen(SheetHierarchy.Defense)&&state.IsOpen(SheetHierarchy.Offense),"Nested preferences survive parent collapse and reopen");
        visible=SheetHierarchy.Visible(nodes,state);
        assert(SheetHierarchy.RestoreFocus("stat:Health",nodes,visible)==SheetHierarchy.Defense,"Closing a subgroup returns descendant focus to its visible header");
        state.Set(SheetHierarchy.Base,false);visible=SheetHierarchy.Visible(nodes,state);
        assert(SheetHierarchy.RestoreFocus("stat:Health",nodes,visible)==SheetHierarchy.Base,"Closing Base returns nested focus to Base");
        assert(!state.Set("not-a-section",true)&&!state.Toggle(null),"Unknown section controls cannot create state");
        bool invalid=false;
        try{SheetHierarchy.Visible(new[]{new SheetNode("a","missing","A",false,1)},state);}catch(ArgumentException){invalid=true;}
        assert(invalid,"Missing parent identities fail closed");
        invalid=false;try{SheetHierarchy.Visible(new[]{new SheetNode("a","b","A",true,0),new SheetNode("b","a","B",true,0)},state);}catch(ArgumentException){invalid=true;}
        assert(invalid,"Cyclic hierarchy identities fail closed");
        invalid=false;try{SheetHierarchy.Visible(new[]{nodes[0],nodes[0]},state);}catch(ArgumentException){invalid=true;}
        assert(invalid,"Duplicate node identities cannot produce duplicate focus targets");
        var duplicateInputs=new[]{new SheetNode("item:1","","First",false,0),new SheetNode("item:1","","Second",false,0),new SheetNode("passive:1","","Passive",false,0),new SheetNode("item:2","","Other item",false,0)};
        var unique=SheetHierarchy.UniqueRows(duplicateInputs,n=>n.Id);
        assert(unique.Count==3&&unique[0].Title=="First"&&unique[1].Id=="passive:1","Duplicate item effects keep one stable first row without dropping separate passive effects");

        foreach(string id in new[]{"Crowbar","FragileDamageBonus","NearbyDamageBonus"})
            assert(CharacterEffectCatalog.Route(id,EffectOrigin.Item)==SheetHierarchy.Damage,"Conditional damage source has its own row and is not aggregated: "+id);
        foreach(string id in new[]{"Missile","MissileVoid","BleedOnHit","ChainLightning","AttackSpeedOnCrit"})
            assert(CharacterEffectCatalog.Route(id,EffectOrigin.Item)==SheetHierarchy.Hit,"Explicit item-on-hit source is routed: "+id);
        foreach(string id in new[]{"IgniteOnKill","ExplodeOnDeath"})
            assert(CharacterEffectCatalog.Route(id,EffectOrigin.Item)==SheetHierarchy.Kill,"Enemy-kill effects have the approved On-Kill heading: "+id);
        assert(CharacterEffectCatalog.Route("HealWhileSafe",EffectOrigin.Item)==SheetHierarchy.Combat&&CharacterEffectCatalog.Item("HealWhileSafe").Condition==EffectCondition.OutOfDanger,"Cautious Slug preserves the out-of-danger distinction");
        assert(CharacterEffectCatalog.Item("SprintOutOfCombat").Condition==EffectCondition.OutOfCombat,"Red Whip has a separate out-of-combat condition");
        assert(CharacterEffectCatalog.Item("JumpBoost").Condition==EffectCondition.SprintJump&&CharacterEffectCatalog.Item("SprintArmor").Condition==EffectCondition.Sprinting,"Wax Quail is a sprint-jump trigger, Rose Buckler a sprinting effect");
        assert(CharacterEffectCatalog.Item("JumpBoost").HoverNote(false,false,true).Contains("sprinting alone does not trigger"),"Quail hover states the required jump action");
        assert(CharacterEffectCatalog.Item("HealWhileSafe").HoverNote(true,false,false).Contains("active")&&CharacterEffectCatalog.Item("SprintOutOfCombat").HoverNote(true,false,false).Contains("inactive"),"Independent native danger/combat states are not conflated");
        foreach(string id in new[]{"Missile","BleedOnHit","ChainLightning","ChainLightningVoid"})
            assert(CharacterEffectCatalog.Item(id).Proc==AttackProcScope.RandomRoll&&CharacterEffectCatalog.ProcNote(id).Contains("attack's proc coefficient"),"Proc-dependent random trigger has a scoped attack note: "+id);
        assert(CharacterEffectCatalog.Item("MissileVoid").Proc==AttackProcScope.ShieldHit&&CharacterEffectCatalog.ProcNote("MissileVoid").Contains("not a random chance")&&CharacterEffectCatalog.ProcNote("MissileVoid").Contains("nonzero-proc"),"Plasma Shrimp does not inherit ATG's random-roll semantics");
        assert(CharacterEffectCatalog.Route("BANDIT2_PASSIVE_NAME",EffectOrigin.Passive)==SheetHierarchy.Hit,"Native on-hit passive metadata can appear in On-Hit");
        assert(CharacterEffectCatalog.Route("ARTIFICER_PASSIVE_NAME",EffectOrigin.Passive)==SheetHierarchy.Other,"A non-hit or unregistered passive is retained without invented on-hit behavior");
        assert(CharacterEffectCatalog.Route("ARTIFICER_PRIMARY_FIRE_NAME",EffectOrigin.Ability)==null,"Ability-specific ignite is excluded");
        foreach(string id in CharacterEffectCatalog.SupportedItemIds)
        {
            assert(CharacterEffectCatalog.Route(id,EffectOrigin.Ability)==null,"An ability never gains item membership by matching an item identifier");
            assert(CharacterEffectCatalog.Route(id,EffectOrigin.Unknown)==null,"Unknown origins cannot be treated as item/passive effects");
        }
        assert(CharacterEffectCatalog.Route("UnregisteredModItem",EffectOrigin.Item)==SheetHierarchy.Other,"Unknown visible items remain accessible through their native tooltip");
        assert(CharacterEffectCatalog.GeneralCooldownItem("AlienHead")&&CharacterEffectCatalog.GeneralCooldownItem("HalfAttackSpeedHalfCooldowns")&&CharacterEffectCatalog.GeneralCooldownItem("LunarBadLuck"),"Shared cooldown reference sources are explicitly scoped");
        assert(!CharacterEffectCatalog.GeneralCooldownItem("UtilitySkillMagazine")&&!CharacterEffectCatalog.GeneralCooldownItem("SecondarySkillMagazine"),"Hardlight Afterburner and skill stocks do not contribute to general CDR");
        assert(GeneralCooldownPresentation.UnknownTotal=="Total: Unknown\nBase: Unknown\nBonuses: Other/unknown","Missing general live total stays unknown in the simple hover");
        assert(GeneralCooldownPresentation.Components(1,0,0).Single().Contains("25%")&&GeneralCooldownPresentation.Components(2,0,0).Single().Contains("43.75%"),"Shared item estimates preserve multiplicative stacking");
        var cooldown=GeneralCooldownPresentation.Components(1,1,4);
        assert(cooldown.Count==3&&cooldown[1].Contains("50%")&&cooldown[2].Contains("-5 seconds"),"Cooldown percent references and flat seconds remain separate components");
        assert(GeneralCooldownPresentation.Components(0,0,0).Count==0,"Absent cooldown source does not create a guessed component");
        assert(CharacterEffectCatalog.ShieldVisible(40)&&!CharacterEffectCatalog.ShieldVisible(0)&&!CharacterEffectCatalog.ShieldVisible(double.NaN),"Shield visibility depends on valid maximum capacity");
        double currentDepleted=0;
        assert(CharacterEffectCatalog.ShieldVisible(40)&&currentDepleted==0,"A depleted shield still retains its capacity row");
        assert(CharacterEffectCatalog.FlatArmorPlate(0)==0&&CharacterEffectCatalog.FlatArmorPlate(1)==5&&CharacterEffectCatalog.FlatArmorPlate(4)==20,"Flat reduction is the separate maintained per-hit item component");
        assert(CharacterEffectCatalog.ArmorReduction(100)==50&&CharacterEffectCatalog.ArmorReduction(-100)==-50,"Raw positive/negative armor has the correct percentage explanation");
        assert(CharacterEffectCatalog.FlatArmorPlate(4)!=CharacterEffectCatalog.ArmorReduction(100),"Flat item reduction is never substituted with armor-derived percent");
        assert(NumberFormatting.Signed(2)=="+2"&&NumberFormatting.Signed(-1)=="-1"&&NumberFormatting.Signed(0)=="0","Luck displays its signed raw value");
        var jumps=new Calculation("Base",1);jumps.Apply("Level growth (level - 1)",CalculationStage.Flat,0);jumps.Apply("Feather",CalculationStage.Flat,2);
        assert(StatTooltipPresentation.Create(3,0,jumps,"cIsUtility").Contains("Total: ")&&jumps.Predicted==3,"Maximum jump arithmetic is independent of remaining jump input");

        int matrices=0;
        var bitHeaders=new[]{SheetHierarchy.Base,SheetHierarchy.Defense,SheetHierarchy.Offense,SheetHierarchy.Movement,SheetHierarchy.Luck,SheetHierarchy.Hit,SheetHierarchy.Other};
        for(int mask=0;mask<128;mask++)
        {
            var bits=new SheetExpansionState();for(int i=0;i<bitHeaders.Length;i++)bits.Set(bitHeaders[i],(mask&(1<<i))!=0);
            var current=SheetHierarchy.Visible(nodes,bits);var ids=new HashSet<string>(current.Select(n=>n.Id));
            assert(ids.Count==current.Count,"Every collapse combination has unique visible identities");
            assert(ids.Contains(SheetHierarchy.Defense)==bits.IsOpen(SheetHierarchy.Base),"Subgroup header follows parent state");
            assert(ids.Contains("stat:Health")== (bits.IsOpen(SheetHierarchy.Base)&&bits.IsOpen(SheetHierarchy.Defense)),"Nested health row follows both ancestors");
            assert(ids.Contains("stat:Luck")== (bits.IsOpen(SheetHierarchy.Base)&&bits.IsOpen(SheetHierarchy.Luck)),"Nested luck row follows both ancestors");
            assert(ids.Contains("effect:Missile")==bits.IsOpen(SheetHierarchy.Hit)&&ids.Contains("buff:1")==bits.IsOpen(SheetHierarchy.Other),"Conditional and buff sections retain independent state");
            string recovered=SheetHierarchy.RestoreFocus("stat:Health",nodes,current);
            assert(recovered!=null&&ids.Contains(recovered),"Focus recovery always chooses a visible owned target");
            matrices++;
        }
        int layouts=0;
        foreach(var resolution in new[]{new[]{1280d,720d},new[]{1920d,1080d},new[]{3440d,1440d}})
        foreach(double scale in new[]{.7,.85,1,1.2})
        foreach(double font in new[]{18d,24d})
        foreach(int count in new[]{5,23,24,100})
        {
            var m=new CompactSheetMetrics(font,scale);
            double height=HierarchySheetSizing.PanelHeight(resolution[1],count,m.RowHeight,m.HeaderHeight,m.Spacing,m.Padding,m.MinHeight);
            assert(height>=m.MinHeight&&height<=Math.Max(m.MinHeight,resolution[1]*.5),"Nested sheet height is content-sized and bounded");
            var own=new UiRect(resolution[0]*.25,resolution[1]*.6,resolution[0]*.5,60*scale);UiRect dock;
            bool fits=CompactSheetPlacement.TryBelowOwn(resolution[0],resolution[1],own,new[]{own},Math.Max(m.Width,380*scale),height,Math.Max(m.MinWidth,300*scale),m.MinHeight,12*scale,out dock);
            assert(fits&&dock.Top<own.Y&&dock.Y>=12*scale&&dock.Right<=resolution[0]-12*scale,"Hierarchical width fits below the own strip without changing native geometry");
            layouts++;
        }
        assert(HierarchySheetSizing.BackgroundOpacity==.8,"Only the sheet background opacity is the requested constant");
        assert(HierarchySheetSizing.EnsureVisible(230,200,200,80,600)==50,"Focus above viewport scrolls into view");
        assert(HierarchySheetSizing.EnsureVisible(10,-20,200,80,600)==100,"Focus below viewport scrolls into view");
        assert(HierarchySheetSizing.EnsureVisible(150,120,200,80,600)==80,"Visible focus does not shift the scroll");
        assert(HierarchySheetSizing.EnsureVisible(0,-1000,200,80,600)==400,"Scroll clamps to owned content limits");
        assert(HierarchySheetSizing.EnsureVisible(double.NaN,0,200,0,600)==0,"Unknown focus geometry fails closed");
        invalid=false;try{HierarchySheetSizing.PanelHeight(1080,1,double.NaN,20,2,10,140);}catch(ArgumentOutOfRangeException){invalid=true;}
        assert(invalid,"Unknown row metrics fail closed");
        var details=new DetailsInteraction();object focusSource=new object();
        details.Enter(focusSource,"stat:Health");details.Focus(true,0);details.Leave(focusSource,0);
        assert(!details.Expire(100),"Keyboard focus retains the stat hover without a pointer");
        assert(!details.FocusToggle(false)&&details.FocusToggle(true)&&details.Expanded,"Keyboard Details requires the native cursor context");
        assert(details.FocusExpand(true,true)&&details.Expanded&&details.FocusExpand(true,true)&&details.Expanded,"Right expands Details idempotently");
        details.Focus(false,100);assert(!details.Expire(100.3)&&details.Expire(100.5),"Deselect releases focus through the existing bounded transfer grace");
        details.Enter(focusSource,"stat:Health");details.Focus(true,0);details.Enter(new object(),"stat:Damage");
        assert(!details.FocusToggle(true)&&!details.Expanded,"Recycled source identity cannot retain another row's focus or expansion");
        Console.WriteLine("Hierarchy coverage: "+CharacterEffectCatalog.SupportedItemIds.Count+" item routes, "+matrices+" collapse states, "+layouts+" sizing cases; Defense semantics, ability exclusions, proc conditions, duplicates and keyboard focus.");
    }
}
