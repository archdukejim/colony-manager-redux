using System.Reflection;

namespace ColonyManagerRedux.Foreman;

// Applies this assembly's Harmony patches at startup. Each satellite assembly patches itself (the base
// mod's PatchAll only covers its own assembly), mirroring ColonyManagerRedux.Managers' Managers_Patches.
[StaticConstructorOnStartup]
internal static class Foreman_Patches
{
    static Foreman_Patches()
    {
        var harmony = new Harmony("ColonyManagerRedux.Foreman");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}

// Isolation harness: force a suppressed work type's effective priority to 0 so the vanilla job system
// treats it as disabled (WorkIsActive == GetPriority > 0). With isolation on, a pawn then does only the
// allowed work types or wanders, regardless of its Work tab. Near-zero cost when isolation is off anywhere
// (the AnyIsolationActive short-circuit). Leaves drafted/forced/emergency behaviour untouched — those
// don't route through work priorities — which is what we want during a test (fires still get fought).
[HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.GetPriority))]
internal static class Pawn_WorkSettings_GetPriority_Patch
{
    private static void Postfix(WorkTypeDef w, Pawn ___pawn, ref int __result)
    {
        if (!WorkforceComp.AnyIsolationActive || __result == 0)
        {
            return;
        }

        var map = ___pawn?.Map;
        if (map == null)
        {
            return;
        }

        var comp = Manager.For(map)?.CompOfType<WorkforceComp>();
        if (comp != null && comp.IsSuppressed(w))
        {
            __result = 0;
        }
    }
}
