using System;
using System.Collections.Generic;
using Alexander.CharacterSheet.Core;

class CoreTests
{
    static int assertions;
    static void Assert(bool value, string message)
    {
        assertions++;
        if (!value) throw new Exception(message);
    }
    static void Near(double actual, double expected, string message) => Assert(Math.Abs(actual - expected) < 1e-9, message);
    static int Main()
    {
        object first = new object(), replacement = new object();
        var requests = new List<bool>();
        var lease = new PauseLease();
        lease.Open(false, false, first, requests.Add);
        Assert(!lease.IsOpen && requests.Count == 0, "Multiplayer must never request pause.");
        lease.Open(true, true, first, requests.Add);
        Assert(lease.PreserveExternalPause, "Borrowed pause must protect native close.");
        lease.Close(true, first, requests.Add);
        Assert(requests.Count == 0, "Existing pause must not be released.");

        lease.Open(true, false, first, requests.Add);
        Assert(lease.Ownership == PauseOwnership.Pending && requests[0], "Acquire must request native pause once.");
        lease.Open(true, false, first, requests.Add);
        Assert(requests.Count == 1, "Repeated open must not overwrite native old time scale.");
        lease.Observe(true);
        Assert(lease.Ownership == PauseOwnership.Owned, "Pause acknowledgment acquires ownership.");
        lease.Close(true, first, requests.Add);
        lease.Close(true, first, requests.Add);
        Assert(requests.Count == 2 && !requests[1], "Cleanup must release once.");
        requests.Clear();
        lease.Open(true, false, first, requests.Add);
        lease.Close(true, first, requests.Add);
        Assert(requests.Count == 2 && !requests[1], "Close before acknowledgment must cancel pending pause.");

        requests.Clear();
        lease.Open(true, false, first, requests.Add);
        lease.Observe(true);
        lease.ExternalPauseRequest();
        lease.Close(true, first, requests.Add);
        Assert(requests.Count == 1, "An external takeover must retain pause.");
        requests.Clear();
        lease.Open(true, false, first, requests.Add);
        lease.Close(true, replacement, requests.Add);
        Assert(requests.Count == 1, "Scene/controller replacement must not unpause another controller.");
        requests.Clear();
        lease.Open(true, false, first, requests.Add);
        lease.Close(false, first, requests.Add);
        Assert(requests.Count == 1, "Mode change must not send a multiplayer unpause.");

        var calculation = new Calculation("Baseline", 100);
        calculation.Apply("Flat", CalculationStage.Flat, 25);
        calculation.Apply("Additive", CalculationStage.AdditiveMultiplier, 0.2);
        calculation.Apply("Second additive term", CalculationStage.AdditiveMultiplier, 0.3);
        Near(calculation.Predicted, 187.5, "Same-bucket percentage bonuses must add before multiplication.");
        calculation.Apply("Separate multiplier", CalculationStage.Multiplier, 2);
        Near(calculation.Predicted, 375, "Flat must be included before multiplicative contributions.");
        Assert(calculation.Describe(385, false, true).Contains("Live − modeled: +10"), "Mismatch must be disclosed.");
        bool rejected = false;
        try { calculation.Apply("Wrong order", CalculationStage.Flat, 1); } catch (InvalidOperationException) { rejected = true; }
        Assert(rejected, "Order-changing additions must be rejected.");
        Near(LuckMath.EffectiveChance(.1, 0), .1, "No luck.");
        Near(LuckMath.EffectiveChance(.1, 1), .19, "One positive reroll.");
        Near(LuckMath.EffectiveChance(.1, -1), .01, "One negative reroll.");
        Near(LuckMath.EffectiveChance(.1, .2), .19, "Fractional luck rounds upward in the verified game.");
        Near(LuckMath.EffectiveChance(2, 2), 1, "Chance caps at certainty.");
        Near(LuckMath.EffectiveChance(-1, 2), 0, "Chance cannot be negative.");
        Near(VerifiedMath.BonusHealthScale(0), 1, "No bonus-health item must retain unit scaling.");
        Near(VerifiedMath.BonusHealthScale(1), 1.5, "First bonus-health copy scales applicable component terms by 1.5.");
        Near(VerifiedMath.BonusHealthScale(3), 1.8, "Additional bonus-health copies add 0.15 to their shared factor.");
        Near(VerifiedMath.BeadPayout(10, 3, 2), 7.5, "Stored bead levels use the cached copy payout rate.");
        Near(VerifiedMath.BeadPayout(10, 3, 0), 0, "An absent cached bead item must not promise a payout.");
        Near(VerifiedMath.NectarCount(1, 9), 4, "One Nectar copy caps qualifying types at four.");
        Near(VerifiedMath.NectarCount(2, 9), 8, "A second copy expands capacity by four types.");
        Near(VerifiedMath.NectarCount(2, 3), 3, "Nectar counts types once, independently of their stack sizes.");
        Near(VerifiedMath.NectarCount(-1, 3), 0, "Missing or invalid item counts have no Nectar capacity.");
        Near(VerifiedMath.NectarMultiplier(4), 1.16, "Four active types add a separate sixteen-percent multiplier.");
        Near(VerifiedMath.NectarNextDelta(1, 3), 0, "A next copy below existing capacity has no immediate benefit at fixed buff types.");
        Near(VerifiedMath.NectarNextDelta(1, 9), .16, "A next copy can enable four more qualifying types at fixed buff types.");
        Near(VerifiedMath.FalseSonBonusHealth(400, 150, 50, 4), 100, "False Son Growth uses the pre-curse snapshot rather than final cursed health.");
        Near(VerifiedMath.RemainingGrowth(100, 130), 30, "Growth threshold distance uses the actual next cumulative cost.");
        Near(VerifiedMath.RemainingGrowth(140, 130), 0, "An already met threshold must not show negative remaining health.");
        Near(VerifiedMath.ArmorMultiplier(100), .5, "Positive armor damage curve.");
        Near(VerifiedMath.ArmorMultiplier(-100), 1.5, "Negative armor damage curve.");
        var armorOrder = new Calculation("Base armor", 100);
        armorOrder.Apply("Early pearl factor", CalculationStage.Multiplier, 1.1);
        armorOrder.Apply("Early buff factor", CalculationStage.Multiplier, 1.07);
        armorOrder.Apply("Conditional late armor", CalculationStage.LateFlat, 30);
        armorOrder.Apply("Late Nectar", CalculationStage.LateMultiplier, 1.16);
        Near(armorOrder.Predicted, 171.332, "Early armor factors must not amplify a later flat armor term; Nectar applies later.");
        var critConversion = new Calculation("Crit points", 70);
        critConversion.Apply("Convert away crit", CalculationStage.Conversion, 0);
        critConversion.Apply("Late Nectar", CalculationStage.LateMultiplier, 1.16);
        Near(critConversion.Predicted, 0, "A later multiplier must not restore converted-away crit chance.");
        var healthCurse = new Calculation("Pre-curse health", 100);
        healthCurse.Apply("Late Nectar", CalculationStage.LateMultiplier, 1.16);
        healthCurse.Apply("Final curse divisor", CalculationStage.FinalConversion, .5);
        Near(healthCurse.Predicted, 58, "Final curse conversion follows the late Nectar multiplier.");
        PresentationTests.Run(Assert);
        ItemTooltipTests.Run(Assert);
        NativeTabTests.Run(Assert);
        NativeTabRegressionTests.Run(Assert);
        CompactSheetLayoutTests.Run(Assert);
        LifecycleCoverageTests.Run(Assert);
        CoverageFormulaTests.Run(Assert);
        Console.WriteLine("PASS " + assertions + " assertions: lifecycle/ownership, ordered stat math, n=1/4/10 item components, caps/proc/luck, Nectar and buff uncertainty.");
        return 0;
    }
}
