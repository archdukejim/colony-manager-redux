using ilyvion.Laboratory.UI;

namespace ColonyManagerRedux.Foreman;

// Foreman's colony-wide work planner, surfaced in Colony Manager Redux as the "Workforce" tab.
// Jobless tab (derives from the non-generic ManagerTab): it has no per-desk manager job — the planning
// runs for free in WorkforceComp, which ticks every game tick regardless of any manager pawn or station.
//
// For now the tab is the per-activity control surface: every manageable activity (at vanilla WorkGiver
// granularity) with an enable switch. Disabled rows are activities whose steering isn't written yet — visible
// but not switchable — so you can watch the feature fill in and test each piece in isolation as it lands.
[HotSwappable]
internal sealed class ManagerTab_Workforce(Manager manager) : ManagerTab(manager)
{
    private readonly ScrollViewStatus _scrollViewStatus = new();

    // No auto-created job: this tab manages the colony-wide plan, not a per-station manager job.
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

        var introRect = new Rect(canvas.x, canvas.y, canvas.width, Constants.ListEntryHeight);
        Widgets.Label(introRect,
            "Enable the activities Foreman should manage. Disabled rows aren't implemented yet.");

        var listRect = new Rect(
            canvas.x,
            introRect.yMax + Constants.Margin,
            canvas.width,
            canvas.height - introRect.height - Constants.Margin);

        using var scrollView = GUIScope.ScrollView(listRect, _scrollViewStatus);
        using var _ = GUIScope.TextAnchor(TextAnchor.MiddleLeft);

        var width = scrollView.ViewRect.width;
        var cur = Vector2.zero;

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

        if (Event.current.type == EventType.Layout)
        {
            scrollView.Height = cur.y;
        }
    }
}
