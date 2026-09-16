namespace ColonyManagerRedux.Foreman;

// One manageable activity in the Workforce tab, at vanilla WorkGiver granularity (a "sub-job"). We deliberately
// break work down finer than the vanilla Work tab: e.g. Construction's frame-building is separate from delivering
// materials, so Foreman can hand the skilled part and the grunt part to different pawns. Keyed by the vanilla
// WorkGiverDef defName so a module can gate itself with WorkforceComp.IsEnabled(WorkGiver).
internal sealed class WorkforceItem(string workGiver, string label, bool implemented)
{
    public string WorkGiver { get; } = workGiver;
    public string Label { get; } = label;
    // Until a piece's steering logic exists its row is shown but disabled — you can see it's planned, but can't
    // switch it on. Flipping this to true (once the logic lands) makes the row toggleable for isolated testing.
    public bool Implemented { get; } = implemented;
}

internal sealed class WorkforceSection(string label, IReadOnlyList<WorkforceItem> items)
{
    public string Label { get; } = label;
    public IReadOnlyList<WorkforceItem> Items { get; } = items;
}

// The catalog of activities the Workforce tab exposes. Built out section by section as modules are written; every
// entry ships disabled by default. Resource gathering first (the cleanest "keep N of X" band cases to look at).
internal static class WorkforceCatalog
{
    public static readonly IReadOnlyList<WorkforceSection> Sections =
    [
        new WorkforceSection("Resource gathering",
        [
            new WorkforceItem("Mine", "Mine rock & ore", implemented: false),
            new WorkforceItem("Drill", "Deep drill", implemented: false),
            new WorkforceItem("PlantsCut", "Chop wood & cut plants", implemented: false),
            new WorkforceItem("ExtractTree", "Extract trees", implemented: false),
            new WorkforceItem("GrowerHarvest", "Harvest crops & forage", implemented: false),
            new WorkforceItem("HunterHunt", "Hunt", implemented: false),
        ]),
    ];
}
