using RoR2;
public static class PublicHierarchyApiProbe {
 public static void Check(CharacterBody b) {
  int max=b.maxJumpCount;int basis=b.baseJumpCount;
  float baseShield=b.baseMaxShield;float levelShield=b.levelMaxShield;
  float sprint=b.sprintingSpeedMultiplier;
  bool danger=b.outOfDanger;bool combat=b.outOfCombat;
  bool enabled=b.skillLocator.passiveSkill.enabled;
  string token=b.skillLocator.passiveSkill.skillNameToken;
  string description=b.skillLocator.passiveSkill.skillDescriptionToken;
 }
}