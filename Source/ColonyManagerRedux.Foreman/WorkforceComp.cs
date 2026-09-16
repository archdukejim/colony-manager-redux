namespace ColonyManagerRedux.Foreman;

// The always-on, free, per-map heart of the Workforce feature. As a ManagerComp it is ticked by
// Manager.MapComponentTick every game tick with no dependency on a manager pawn, desk, power, or the
// WorkGiver_Manage gating that manager jobs require — so the planner runs even with no manager assigned
// (the design decision for the testing build; a desk-tier cost is a deferred wrapper). Retrieve it with
// Manager.CompOfType<WorkforceComp>(). Ledger + hourly controller logic will live here.
//
// It also owns the per-activity enable registry (keyed by WorkGiver defName). Everything ships OFF; each
// module gates itself with IsEnabled(...) so unfinished/unverified code is inert until a row is switched on
// in the Workforce tab. This is what makes it safe to build many activities ahead of in-game testing.
internal sealed class WorkforceComp : ManagerComp
{
    private HashSet<string> _enabled = [];

    public bool IsEnabled(string workGiver) => _enabled.Contains(workGiver);

    public void SetEnabled(string workGiver, bool enabled)
    {
        if (enabled)
        {
            _enabled.Add(workGiver);
        }
        else
        {
            _enabled.Remove(workGiver);
        }
    }

    public override void CompTick()
    {
        // Skeleton: no work yet. Enabled activities will drive the ledger/controller from here.
    }

    public override void PostExposeData()
    {
        Scribe_Collections.Look(ref _enabled, "foremanEnabledWorkGivers", LookMode.Value);
        _enabled ??= [];
    }
}
