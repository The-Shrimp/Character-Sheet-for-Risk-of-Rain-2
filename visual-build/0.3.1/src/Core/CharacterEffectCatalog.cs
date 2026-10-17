using System;
using System.Collections.Generic;
using System.Linq;
namespace Alexander.CharacterSheet.Core
{
    public enum EffectOrigin { Item,Passive,Ability,Unknown }
    public enum AttackProcScope { None,RandomRoll,ShieldHit,EligibleHit }
    public enum EffectCondition { None,TargetHealth,Nearby,Unbroken,OutOfDanger,OutOfCombat,Sprinting,SprintJump,Shield }
    public sealed class CharacterEffectRule
    {
        public readonly string Id,Section;
        public readonly AttackProcScope Proc;
        public readonly EffectCondition Condition;
        internal CharacterEffectRule(string id,string section,AttackProcScope proc,EffectCondition condition)
        {Id=id;Section=section;Proc=proc;Condition=condition;}
        public string HoverNote(bool outOfDanger,bool outOfCombat,bool sprinting)
        {
            string note=Proc==AttackProcScope.RandomRoll?"Trigger chance depends on the attack's proc coefficient; attack context varies.":
                Proc==AttackProcScope.ShieldHit?"Requires shield and an eligible nonzero-proc hit; this trigger is not a random chance.":
                Proc==AttackProcScope.EligibleHit?"Eligible-hit effect; attack and proc-chain conditions apply.":"";
            string condition=Condition==EffectCondition.OutOfDanger?"Out of danger: "+(outOfDanger?"active":"inactive")+". Separate from out of combat.":
                Condition==EffectCondition.OutOfCombat?"Out of combat: "+(outOfCombat?"active":"inactive")+". Separate from out of danger.":
                Condition==EffectCondition.SprintJump?"Triggers on a jump while sprinting; sprinting alone does not trigger it.":
                Condition==EffectCondition.Sprinting?"While sprinting: "+(sprinting?"active":"inactive")+".":"";
            return string.Join("\n",new[]{note,condition}.Where(s=>s.Length>0));
        }
    }
    public static class CharacterEffectCatalog
    {
        // Original routing facts; quantitative item definitions remain a separate maintained boundary.
        static readonly Dictionary<string,CharacterEffectRule> items=Build();
        static Dictionary<string,CharacterEffectRule> Build()
        {
            var d=new Dictionary<string,CharacterEffectRule>(StringComparer.Ordinal);
            Action<string,string,AttackProcScope,EffectCondition> add=(id,section,proc,condition)=>d.Add(id,new CharacterEffectRule(id,section,proc,condition));
            foreach(string id in new[]{"Crowbar","FragileDamageBonus","NearbyDamageBonus","BossDamageBonus","RandomDamageZone","IncreaseDamageOnMultiKill"})
                add(id,SheetHierarchy.Damage,AttackProcScope.None,id=="Crowbar"?EffectCondition.TargetHealth:id=="NearbyDamageBonus"?EffectCondition.Nearby:id=="FragileDamageBonus"?EffectCondition.Unbroken:EffectCondition.None);
            foreach(string id in new[]{"Missile","ChainLightning","BleedOnHit","BleedOnHitVoid","StunChanceOnHit","BounceNearby","StickyBomb","ChainLightningVoid"})
                add(id,SheetHierarchy.Hit,AttackProcScope.RandomRoll,EffectCondition.None);
            add("MissileVoid",SheetHierarchy.Hit,AttackProcScope.ShieldHit,EffectCondition.Shield);
            foreach(string id in new[]{"Behemoth","IceRing","FireRing","ElementalRingVoid","SlowOnHit","SlowOnHitVoid","AttackSpeedOnCrit","HealOnCrit","CritHeal","LightningStrikeOnHit","ArmorReductionOnHit","PermanentDebuffOnHit"})
                add(id,SheetHierarchy.Hit,AttackProcScope.EligibleHit,EffectCondition.None);
            foreach(string id in new[]{"IgniteOnKill","ExplodeOnDeath","Dagger","Tooth","Infusion","Bandolier","MoveSpeedOnKill","KillsToStats"})
                add(id,SheetHierarchy.Kill,AttackProcScope.None,EffectCondition.None);
            add("HealWhileSafe",SheetHierarchy.Combat,AttackProcScope.None,EffectCondition.OutOfDanger);
            add("SprintOutOfCombat",SheetHierarchy.Combat,AttackProcScope.None,EffectCondition.OutOfCombat);
            foreach(string id in new[]{"SprintArmor","SprintBonus","SprintWisp","SprintHealing","MushroomVoid","SprintMoney"})
                add(id,SheetHierarchy.Sprint,AttackProcScope.None,EffectCondition.Sprinting);
            add("JumpBoost",SheetHierarchy.Sprint,AttackProcScope.None,EffectCondition.SprintJump);
            return d;
        }
        public static CharacterEffectRule Item(string id){CharacterEffectRule rule;return id!=null&&items.TryGetValue(id,out rule)?rule:null;}
        public static string Route(string id,EffectOrigin origin)
        {
            if(origin==EffectOrigin.Ability||origin==EffectOrigin.Unknown)return null;
            if(origin==EffectOrigin.Item)return Item(id)?.Section??SheetHierarchy.Other;
            // Passive rows require the actual native passive metadata; no active skill entries are queried here.
            return new[]{"BANDIT2_PASSIVE_NAME","CROCO_PASSIVE_NAME","TREEBOT_PASSIVE_NAME","RAILGUNNER_PASSIVE_NAME"}.Contains(id)?SheetHierarchy.Hit:SheetHierarchy.Other;
        }
        public static bool GeneralCooldownItem(string id)=>id=="AlienHead"||id=="HalfAttackSpeedHalfCooldowns"||id=="LunarBadLuck";
        public static bool ShieldVisible(double capacity)=>NumberFormatting.IsFinite(capacity)&&capacity>0;
        public static double ArmorReduction(double armor)=>100*(1-VerifiedMath.ArmorMultiplier(armor));
        public static double FlatArmorPlate(long copies)=>copies>0?5.0*copies:0;
        public static string ProcNote(string id)=>Item(id)?.HoverNote(false,false,false)??"";
        public static IReadOnlyList<string> SupportedItemIds=>Array.AsReadOnly(items.Keys.OrderBy(s=>s,StringComparer.Ordinal).ToArray());
    }
}
