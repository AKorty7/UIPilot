using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace UIPilot.Editor.Modules.ClickDebug
{
    // Explains one click in Play mode. It runs the raycast the EventSystem runs at
    // that point, then follows the click up to whatever would handle it. Read-only:
    // it never changes the scene.
    internal static class ClickDebugModule
    {
        private const int HitOrderShown = 4;
        private static readonly List<RaycastResult> Hits = new List<RaycastResult>();

        internal static ClickReport Inspect(Vector2 position)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null || !eventSystem.isActiveAndEnabled)
                return new ClickReport(ClickVerdict.NoEventSystem, ClickDebugContent.Messages.NoEventSystem,
                    new[] { ClickDebugContent.Messages.NoEventSystemDetail }, null, null, position);

            if (eventSystem.currentInputModule == null)
                return new ClickReport(ClickVerdict.NoEventSystem, eventSystem.name,
                    new[] { ClickDebugContent.Messages.NoInputModule },
                    new Object[] { eventSystem.gameObject }, null, position);

            Hits.Clear();
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = position }, Hits);
            if (Hits.Count == 0)
                return ClickDiagnosis.FindUnreachable(position) ?? ClickDiagnosis.Nothing(position);

            var top     = Hits[0].gameObject;
            var order   = HitOrder();
            var handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(top);

            return handler != null
                ? ClickHandlers.Explain(top, handler, order, position)
                : Unhandled(top, order, position);
        }

        // The Console line for a click.
        internal static string Describe(ClickReport report)
        {
            return string.Format(ClickDebugContent.Console.Line,
                report.Headline, Word(report.Verdict),
                string.Join(ClickDebugContent.Console.DetailSeparator, report.Details));
        }

        internal static string Word(ClickVerdict verdict)
        {
            switch (verdict)
            {
                case ClickVerdict.Handled:         return ClickDebugContent.Words.Handled;
                case ClickVerdict.NoListeners:     return ClickDebugContent.Words.NoListeners;
                case ClickVerdict.NotInteractable: return ClickDebugContent.Words.Off;
                case ClickVerdict.Blocked:         return ClickDebugContent.Words.Blocked;
                case ClickVerdict.Missed:          return ClickDebugContent.Words.Missed;
                default:                           return ClickDebugContent.Words.Broken;
            }
        }

        // Something took the click but nothing on it responds. A control further
        // down means it is in that control's way; otherwise it is just scenery,
        // unless a control under it cannot take clicks at all.
        private static ClickReport Unhandled(GameObject top, string order, Vector2 position)
        {
            var kind = KindOf(top);
            for (var i = 1; i < Hits.Count; i++)
            {
                var below = ExecuteEvents.GetEventHandler<IPointerClickHandler>(Hits[i].gameObject);
                if (below == null) continue;

                return new ClickReport(ClickVerdict.Blocked, top.name, new[]
                {
                    string.Format(ClickDebugContent.Messages.Blocking, kind, below.name),
                    string.Format(ClickDebugContent.Messages.BlockingFix, top.name),
                }, new Object[] { top, below }, order, position);
            }

            return ClickDiagnosis.FindUnreachable(position)
                ?? new ClickReport(ClickVerdict.Missed, top.name,
                       new[] { string.Format(ClickDebugContent.Messages.NotClickable, kind) },
                       new Object[] { top }, order, position);
        }

        private static string KindOf(GameObject go)
        {
            var graphic = go.GetComponent<UnityEngine.UI.Graphic>();
            return graphic != null ? graphic.GetType().Name : go.name;
        }

        private static string HitOrder()
        {
            var names = new List<string>();
            for (var i = 0; i < Hits.Count && i < HitOrderShown; i++)
                names.Add(Hits[i].gameObject.name);

            var order = string.Join(ClickDebugContent.Messages.HitSeparator, names);
            if (Hits.Count > HitOrderShown)
                order += string.Format(ClickDebugContent.Messages.HitMore, Hits.Count - HitOrderShown);

            return string.Format(ClickDebugContent.Messages.HitOrder, order);
        }
    }
}
