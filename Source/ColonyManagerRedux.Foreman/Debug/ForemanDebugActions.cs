using System.Text;
using LudeonTK;

namespace ColonyManagerRedux.Foreman;

// Dev-mode diagnostics for validating the isolation harness headlessly (no UI automation): flip isolation
// via the actions, then "Log isolation effect" dumps each colonist's effective work priorities + current
// job. With isolation on and nothing allowed, every GetPriority should read 0 (the Harmony patch working);
// with one work type allowed, only that type stays non-zero. Category "Foreman" in the debug menu.
internal static class ForemanDebugActions
{
    private const string Category = "Foreman";

    private static readonly string[] SampleWorkTypes =
        ["PlantCutting", "Mining", "Cleaning", "Construction", "Research", "Hauling"];

    private static WorkforceComp? Comp =>
        Find.CurrentMap is { } map ? Manager.For(map)?.CompOfType<WorkforceComp>() : null;

    [DebugAction(Category, "Log isolation effect", actionType = DebugActionType.Action,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void LogIsolationEffect()
    {
        var map = Find.CurrentMap;
        var comp = Comp;
        var sb = new StringBuilder();
        if (comp == null)
        {
            sb.AppendLine("Foreman: no WorkforceComp on this map.");
            Log.Message(sb.ToString());
            return;
        }

        sb.AppendLine($"Foreman isolation — mode={comp.IsolationMode}  anyActive={WorkforceComp.AnyIsolationActive}");
        var sample = new List<WorkTypeDef>();
        foreach (var defName in SampleWorkTypes)
        {
            var wt = DefDatabase<WorkTypeDef>.GetNamedSilentFail(defName);
            if (wt != null)
            {
                sample.Add(wt);
            }
        }

        foreach (var pawn in map.mapPawns.FreeColonistsSpawned)
        {
            sb.Append($"  {pawn.LabelShort,-12}");
            foreach (var wt in sample)
            {
                sb.Append($" {wt.defName}={pawn.workSettings?.GetPriority(wt)}");
            }
            sb.AppendLine($"  | job={pawn.CurJob?.def?.defName ?? "none"}");
        }
        Log.Message(sb.ToString());
    }

    [DebugAction(Category, "Isolation: toggle (allow nothing)", actionType = DebugActionType.Action,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ToggleIsolation()
    {
        var comp = Comp;
        if (comp == null)
        {
            return;
        }
        comp.IsolationMode = !comp.IsolationMode;
        Messages.Message($"Foreman isolation mode = {comp.IsolationMode}", MessageTypeDefOf.TaskCompletion, false);
    }

    [DebugAction(Category, "Isolation: allow only PlantCutting", actionType = DebugActionType.Action,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void IsolatePlantCuttingOnly()
    {
        var comp = Comp;
        if (comp == null)
        {
            return;
        }
        foreach (var wt in DefDatabase<WorkTypeDef>.AllDefsListForReading)
        {
            comp.SetWorkTypeAllowed(wt.defName, false);
        }
        comp.SetWorkTypeAllowed("PlantCutting", true);
        comp.IsolationMode = true;
        Messages.Message("Foreman: isolating PlantCutting only", MessageTypeDefOf.TaskCompletion, false);
    }
}
