namespace ColonyManagerRedux.Foreman;

// The always-on, free, per-map heart of the Workforce feature. As a ManagerComp it is ticked by
// Manager.MapComponentTick every game tick with no dependency on a manager pawn, desk, power, or the
// WorkGiver_Manage gating that manager jobs require — so the planner runs even with no manager assigned
// (the design decision for the testing build; a desk-tier cost is a deferred wrapper). Retrieve it with
// Manager.CompOfType<WorkforceComp>(). Ledger + hourly controller logic will live here.
internal sealed class WorkforceComp : ManagerComp
{
    public override void CompTick()
    {
        // Skeleton: no work yet. This proves the always-on tick surface exists.
    }
}
