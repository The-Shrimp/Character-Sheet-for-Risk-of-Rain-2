using System;
namespace Alexander.CharacterSheet.Core
{
    // Native build 21587608. These are component terms, not a replacement stat engine.
    public static class VerifiedMath
    {
        public static double BonusHealthScale(int copies) => copies > 0 ? 1.5 + .15 * (copies - 1) : 1;
        public static int NectarCount(int copies, int qualifyingTypes) => (int)Math.Min(Math.Max(0L, 4L * copies), Math.Max(0, qualifyingTypes));
        public static double NectarMultiplier(int activeCounter) => 1 + .04 * Math.Max(0, activeCounter);
        public static double NectarNextDelta(int copies, int qualifyingTypes) => .04 * (NectarCount(copies + 1, qualifyingTypes) - NectarCount(copies, qualifyingTypes));
        public static double BeadPayout(double levelGrowth, int storedLevels, int cachedCopies) => cachedCopies > 0 ? levelGrowth * Math.Max(0, storedLevels) * (.2 + .05 * (cachedCopies - 1)) : 0;
        public static double FalseSonBonusHealth(double preCurseSnapshot, double baseline, double perLevel, double level) => preCurseSnapshot - (baseline + perLevel * (level - 1));
        public static double RemainingGrowth(double bonusHealth, double nextCost) => Math.Max(0, nextCost - bonusHealth);
        public static double ArmorMultiplier(double armor) => armor >= 0 ? 100 / (100 + armor) : 2 - 100 / (100 - armor);
    }
}
