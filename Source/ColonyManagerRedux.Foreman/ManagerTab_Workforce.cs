namespace ColonyManagerRedux.Foreman;

// Foreman's colony-wide work planner, surfaced in Colony Manager Redux as the "Workforce" tab.
// Jobless tab (derives from the non-generic ManagerTab): it has no per-desk manager job — the planning
// runs for free in WorkforceComp, which ticks every game tick regardless of any manager pawn or station.
// This is the skeleton: it just renders a placeholder. The ledger/controller UI comes later.
internal sealed class ManagerTab_Workforce(Manager manager) : ManagerTab(manager)
{
    // No auto-created job: this tab manages the colony-wide plan, not a per-station manager job.
    protected override bool CreateNewSelectedJobOnMake => false;

    protected override void DoTabContents(Rect canvas)
    {
        Widgets.Label(canvas, "Workforce — Foreman work planner (skeleton)");
    }
}
