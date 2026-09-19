using RimTestRedux;

namespace ColonyManagerRedux.Foreman.Tests;

// The resource "keep N of X" band is pure logic, so it's fully covered here without a running game.
// Guards the hysteresis contract: gather at/below low, stop at/above high, hold the prior decision between.
[TestSuite]
internal static class ResourceBandTests
{
    [Test]
    public static void BelowLowGathers() =>
        Assert.That(ResourceBand.ShouldGather(stock: 50, low: 100, high: 200, wasGathering: false)).Is.True();

    [Test]
    public static void AtLowGathers() =>
        Assert.That(ResourceBand.ShouldGather(stock: 100, low: 100, high: 200, wasGathering: false)).Is.True();

    [Test]
    public static void AtHighStops() =>
        Assert.That(ResourceBand.ShouldGather(stock: 200, low: 100, high: 200, wasGathering: true)).Is.False();

    [Test]
    public static void AboveHighStops() =>
        Assert.That(ResourceBand.ShouldGather(stock: 250, low: 100, high: 200, wasGathering: true)).Is.False();

    [Test]
    public static void InDeadbandHoldsWhenGathering() =>
        Assert.That(ResourceBand.ShouldGather(stock: 150, low: 100, high: 200, wasGathering: true)).Is.True();

    [Test]
    public static void InDeadbandHoldsWhenNotGathering() =>
        Assert.That(ResourceBand.ShouldGather(stock: 150, low: 100, high: 200, wasGathering: false)).Is.False();
}
