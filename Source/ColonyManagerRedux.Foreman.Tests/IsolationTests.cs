using RimTestRedux;

namespace ColonyManagerRedux.Foreman.Tests;

// The isolation suppression decision is pure logic, covered here without a running game. The in-game half
// (the GetPriority patch actually making pawns wander) is what the live validation session confirms.
[TestSuite]
internal static class IsolationTests
{
    private static System.Collections.Generic.HashSet<string> Allowed(params string[] names) => [.. names];

    [Test]
    public static void NotSuppressedWhenIsolationOff() =>
        Assert.That(WorkforceComp.ShouldSuppress(false, Allowed(), "Cleaning")).Is.False();

    [Test]
    public static void NotSuppressedWhenIsolationOffEvenIfNotAllowed() =>
        Assert.That(WorkforceComp.ShouldSuppress(false, Allowed("Mining"), "Cleaning")).Is.False();

    [Test]
    public static void SuppressedWhenIsolationOnAndNotAllowed() =>
        Assert.That(WorkforceComp.ShouldSuppress(true, Allowed(), "Cleaning")).Is.True();

    [Test]
    public static void NotSuppressedWhenAllowed() =>
        Assert.That(WorkforceComp.ShouldSuppress(true, Allowed("Cleaning"), "Cleaning")).Is.False();

    [Test]
    public static void OtherTypesSuppressedWhenOneAllowed() =>
        Assert.That(WorkforceComp.ShouldSuppress(true, Allowed("Cleaning"), "Mining")).Is.True();
}
