namespace ColonyManagerRedux.Foreman;

// Pure hysteresis band for "keep N of X" resource targets — the shared core every resource-gathering
// activity uses to decide whether to gather right now. Gather while stock is at/below the low mark; once it
// reaches the high mark, stop; between the two the previous decision holds (the deadband), so labor doesn't
// thrash at the edge of the target. No game state — unit-tested directly (see ResourceBand tests).
internal static class ResourceBand
{
    // Whether gathering should be active now.
    //   stock        current amount on hand
    //   low, high    the target band: gather at/below low, stop at/above high, hold in between
    //   wasGathering the previous decision — the hysteresis state that fills the deadband
    // Degenerate bands (high <= low) collapse to a single threshold at low, which is still well-defined.
    public static bool ShouldGather(int stock, int low, int high, bool wasGathering)
    {
        if (stock <= low)
        {
            return true;
        }
        if (stock >= high)
        {
            return false;
        }
        return wasGathering;
    }
}
