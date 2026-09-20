using ilyvion.Laboratory.UI;

namespace ColonyManagerRedux.Foreman;

// Foreman's colony-wide work planner, surfaced in Colony Manager Redux as the "Workforce" tab.
// Jobless tab (derives from the non-generic ManagerTab): the planning runs for free in WorkforceComp,
// which ticks every game tick regardless of any manager pawn or station.
//
// Two things live here today:
//  - Isolation mode (testing): a master switch + a list of allowed work types. When on, all other work
//    types are suppressed, so a pawn does only the selected work or wanders — the harness for testing one
//    work group at a time, and the first proof Foreman can override vanilla work selection.
//  - The per-activity control surface (WorkGiver granularity, finer than the vanilla Work tab): each
//    activity with an enable switch; not-implemented rows are shown greyed and non-switchable.
[HotSwappable]
internal sealed class ManagerTab_Workforce(Manager manager) : ManagerTab(manager)
{
    private readonly ScrollViewStatus _scrollViewStatus = new();

    protected override bool CreateNewSelectedJobOnMake => false;

    private WorkforceComp? Comp => Manager.CompOfType<WorkforceComp>();

    protected override void DoTabContents(Rect canvas)
    {
        var comp = Comp;
        if (comp == null)
        {
            Widgets.Label(canvas, "Workforce: WorkforceComp missing from this map's manager.");
            return;
        }

        // Master isolation toggle.
        var toggleRect = new Rect(canvas.x, canvas.y, canvas.width, Constants.ListEntryHeight);
        var isolation = comp.IsolationMode;
        var isolationBefore = isolation;
        Widgets.CheckboxLabeled(
            toggleRect,
            "Isolation mode (testing) — suppress all work except the allowed work types below",
            ref isolation);
        if (isolation != isolationBefore)
        {
            comp.IsolationMode = isolation;
        }

        var listRect = new Rect(
            canvas.x,
            toggleRect.yMax + Constants.Margin,
            canvas.width,
            canvas.yMax - toggleRect.yMax - Constants.Margin);

        using var scrollView = GUIScope.ScrollView(listRect, _scrollViewStatus);
        using var _ = GUIScope.TextAnchor(TextAnchor.MiddleLeft);

        var width = scrollView.ViewRect.width;
        var cur = Vector2.zero;

        if (comp.IsolationMode)
        {
            DrawIsolationSection(comp, ref cur, width);
        }

        DrawCatalog(comp, ref cur, width);

        if (Event.current.type == EventType.Layout)
        {
            scrollView.Height = cur.y;
        }
    }

    private static void DrawIsolationSection(WorkforceComp comp, ref Vector2 cur, float width)
    {
        Widgets.ListSeparator(ref cur.y, width, "Allowed work types (isolation)");

        var workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading
            .Where(wt => wt.visible)
            .OrderByDescending(wt => wt.naturalPriority);

        var i = 0;
        foreach (var wt in workTypes)
        {
            var row = new Rect(cur.x, cur.y, width, Constants.ListEntryHeight);
            if (i++ % 2 == 0)
            {
                Widgets.DrawAltRect(row);
            }

            var allowed = comp.IsWorkTypeAllowed(wt.defName);
            var allowedBefore = allowed;
            var label = wt.labelShort.NullOrEmpty() ? wt.defName : wt.labelShort.CapitalizeFirst();
            Widgets.CheckboxLabeled(row.ContractedBy(Constants.Margin, 0f), label, ref allowed);
            if (allowed != allowedBefore)
            {
                comp.SetWorkTypeAllowed(wt.defName, allowed);
            }

            cur.y += Constants.ListEntryHeight;
        }

        cur.y += Constants.Margin;
    }

    private static void DrawCatalog(WorkforceComp comp, ref Vector2 cur, float width)
    {
        foreach (var section in WorkforceCatalog.Sections)
        {
            Widgets.ListSeparator(ref cur.y, width, section.Label);

            var i = 0;
            foreach (var item in section.Items)
            {
                var row = new Rect(0f, cur.y, width, Constants.ListEntryHeight);
                if (i++ % 2 == 0)
                {
                    Widgets.DrawAltRect(row);
                }
                Widgets.DrawHighlightIfMouseover(row);

                var labelRect = new Rect(
                    row.x + Constants.Margin,
                    row.y,
                    row.width - (2 * Constants.Margin) - 30f,
                    row.height);

                if (!item.Implemented)
                {
                    GUI.color = Color.gray;
                }
                Widgets.Label(labelRect, item.Implemented ? item.Label : item.Label + "  (not implemented)");
                GUI.color = Color.white;

                var on = comp.IsEnabled(item.WorkGiver);
                var newOn = on;
                Widgets.Checkbox(
                    new Vector2(row.xMax - 24f - Constants.Margin, row.y + ((row.height - 24f) / 2f)),
                    ref newOn,
                    disabled: !item.Implemented);
                if (newOn != on)
                {
                    comp.SetEnabled(item.WorkGiver, newOn);
                }

                cur.y += Constants.ListEntryHeight;
            }

            cur.y += Constants.Margin;
        }
    }
}
