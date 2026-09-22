using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UIPilot.Editor.Modules.ClickDebug
{
    // A click reached something that handles clicks: says what that is and what
    // the click will do there.
    internal static class ClickHandlers
    {
        internal static ClickReport Explain(GameObject top, GameObject handler, string order, Vector2 position)
        {
            var details = new List<string>();
            var verdict = Describe(handler, details);
            if (top != handler)
                details.Add(string.Format(ClickDebugContent.Messages.LandedOnChild, top.name));

            return new ClickReport(verdict, handler.name, details.ToArray(),
                new Object[] { handler }, order, position);
        }

        private static ClickVerdict Describe(GameObject handler, List<string> details)
        {
            var selectable = handler.GetComponent<Selectable>();
            if (selectable != null && !selectable.IsInteractable())
            {
                details.Add(WhyOff(selectable));
                return ClickVerdict.NotInteractable;
            }

            if (selectable is Button button) return DescribeButton(button, details);
            if (selectable is Toggle toggle) return DescribeToggle(toggle, details);

            details.Add(string.Format(ClickDebugContent.Messages.HandledBy, HandlerNames(handler)));
            return ClickVerdict.Handled;
        }

        private static ClickVerdict DescribeButton(Button button, List<string> details)
        {
            var count = button.onClick.GetPersistentEventCount();
            if (count == 0)
            {
                details.Add(ClickDebugContent.Messages.NoListeners);
                return ClickVerdict.NoListeners;
            }

            return AddListeners(button.onClick, ClickDebugContent.Messages.Calls, details) > 0
                ? ClickVerdict.Handled
                : ClickVerdict.NoListeners;
        }

        // A Toggle switches whether or not anything listens, so it always counts as handled.
        private static ClickVerdict DescribeToggle(Toggle toggle, List<string> details)
        {
            if (toggle.onValueChanged.GetPersistentEventCount() == 0)
                details.Add(ClickDebugContent.Messages.ToggleNoListeners);
            else
                AddListeners(toggle.onValueChanged, ClickDebugContent.Messages.ValueCalls, details);

            return ClickVerdict.Handled;
        }

        // One line per entry set in the Inspector. Returns how many of them will run.
        private static int AddListeners(UnityEventBase unityEvent, string callFormat, List<string> details)
        {
            var working = 0;
            for (var i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            {
                var line = DescribeEntry(unityEvent, i, callFormat, out var runs);
                details.Add(line);
                if (runs) working++;
            }

            return working;
        }

        private static string DescribeEntry(UnityEventBase unityEvent, int index, string callFormat, out bool runs)
        {
            runs = false;
            var target = unityEvent.GetPersistentTarget(index);
            if (target == null)
                return string.Format(ClickDebugContent.Messages.EntryNoObject, index + 1);

            var method = unityEvent.GetPersistentMethodName(index);
            if (string.IsNullOrEmpty(method))
                return string.Format(ClickDebugContent.Messages.EntryNoMethod, index + 1);

            // The Inspector shows a component by its type ("GameManager.StartGame").
            var owner = target is Component component ? component.GetType().Name : target.name;
            if (unityEvent.GetPersistentListenerState(index) == UnityEventCallState.Off)
                return string.Format(ClickDebugContent.Messages.CallsOff, owner, method);

            runs = true;
            return string.Format(callFormat, owner, method);
        }

        private static string WhyOff(Selectable selectable)
        {
            var kind = selectable.GetType().Name;
            if (!selectable.interactable)
                return string.Format(ClickDebugContent.Messages.InteractableOff, kind);

            var group = ClickDiagnosis.FindGroup(selectable.transform, g => !g.interactable);
            return group != null
                ? string.Format(ClickDebugContent.Messages.GroupOff, kind, group.name)
                : string.Format(ClickDebugContent.Messages.InteractableOff, kind);
        }

        // The scripts on the object that take clicks, for a handler that is not a
        // Button or Toggle: a custom IPointerClickHandler.
        private static string HandlerNames(GameObject handler)
        {
            var names = new List<string>();
            foreach (var component in handler.GetComponents<Component>())
                if (component is IPointerClickHandler && component is Behaviour behaviour && behaviour.isActiveAndEnabled)
                    names.Add(component.GetType().Name);

            return string.Join(ClickDebugContent.Messages.ListSeparator, names);
        }
    }
}
