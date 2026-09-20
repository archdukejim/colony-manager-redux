namespace ColonyManagerRedux.Foreman;

// The always-on, free, per-map heart of the Workforce feature. As a ManagerComp it is ticked by
// Manager.MapComponentTick every game tick with no dependency on a manager pawn, desk, power, or the
// WorkGiver_Manage gating that manager jobs require — so the planner runs even with no manager assigned.
// Retrieve it with Manager.CompOfType<WorkforceComp>(). Ledger + hourly controller logic will live here.
//
// It owns two things today:
//  - the per-activity enable registry (keyed by WorkGiver defName); everything ships OFF, each module
//    gates itself with IsEnabled(...) so unfinished code is inert until switched on.
//  - Isolation mode (a testing harness): when on, all work types EXCEPT the allowed set are suppressed
//    (their effective priority forced to 0 via a Harmony patch on Pawn_WorkSettings.GetPriority), so a
//    pawn does only the selected work or wanders — regardless of its vanilla Work tab. This lets one work
//    group be tested in isolation, and is the embryo of the real Prioritize override (Foreman owning
//    effective work priority).
internal sealed class WorkforceComp : ManagerComp
{
    private HashSet<string> _enabled = [];

    private bool _isolationMode;
    private HashSet<string> _isolatedWorkTypes = [];

    #region Per-activity enable registry

    public bool IsEnabled(string workGiver) => _enabled.Contains(workGiver);

    public void SetEnabled(string workGiver, bool enabled)
    {
        if (enabled)
        {
            _ = _enabled.Add(workGiver);
        }
        else
        {
            _ = _enabled.Remove(workGiver);
        }
    }

    #endregion

    #region Isolation (testing harness)

    // A cheap global short-circuit so the GetPriority patch costs nothing when no map has isolation on
    // (the normal/shipped state). Biased to stay true rather than risk a false negative.
    internal static bool AnyIsolationActive { get; private set; }

    public bool IsolationMode
    {
        get => _isolationMode;
        set
        {
            _isolationMode = value;
            if (value)
            {
                AnyIsolationActive = true;
            }
            else
            {
                RecomputeAnyIsolation();
            }
        }
    }

    public bool IsWorkTypeAllowed(string workTypeDefName) => _isolatedWorkTypes.Contains(workTypeDefName);

    public void SetWorkTypeAllowed(string workTypeDefName, bool allowed)
    {
        if (allowed)
        {
            _ = _isolatedWorkTypes.Add(workTypeDefName);
        }
        else
        {
            _ = _isolatedWorkTypes.Remove(workTypeDefName);
        }
    }

    // Pure decision, unit-tested: during isolation a work type is suppressed unless it is in the allowed set.
    public static bool ShouldSuppress(bool isolationMode, ICollection<string> allowedWorkTypes, string workTypeDefName)
        => isolationMode && !allowedWorkTypes.Contains(workTypeDefName);

    public bool IsSuppressed(WorkTypeDef workType) =>
        workType != null && ShouldSuppress(_isolationMode, _isolatedWorkTypes, workType.defName);

    private static void RecomputeAnyIsolation()
    {
        if (Current.ProgramState != ProgramState.Playing || Find.Maps == null)
        {
            return;
        }
        foreach (var map in Find.Maps)
        {
            var comp = Manager.For(map)?.CompOfType<WorkforceComp>();
            if (comp != null && comp._isolationMode)
            {
                AnyIsolationActive = true;
                return;
            }
        }
        AnyIsolationActive = false;
    }

    #endregion

    public override void CompTick()
    {
        // Skeleton: no work yet. Enabled activities will drive the ledger/controller from here.
    }

    public override void PostExposeData()
    {
        Scribe_Collections.Look(ref _enabled, "foremanEnabledWorkGivers", LookMode.Value);
        Scribe_Values.Look(ref _isolationMode, "foremanIsolationMode", false);
        Scribe_Collections.Look(ref _isolatedWorkTypes, "foremanIsolatedWorkTypes", LookMode.Value);
        _enabled ??= [];
        _isolatedWorkTypes ??= [];
        if (_isolationMode)
        {
            AnyIsolationActive = true;
        }
    }
}
