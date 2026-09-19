using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIPilot.Editor.Core;
using Object = UnityEngine.Object;

namespace UIPilot.Editor.Modules.Health
{
    // Checks that a click has somewhere to go: event listeners that still resolve,
    // and one EventSystem with an input module the project can actually run.
    internal static class HealthEventChecks
    {
        private static readonly Dictionary<Type, PropertyInfo[]> EventPropertiesByType =
            new Dictionary<Type, PropertyInfo[]>();

        private static readonly Dictionary<(Type, string), bool> MethodExistsCache =
            new Dictionary<(Type, string), bool>();

        // ── Click events ─────────────────────────────────────────────────────

        internal static void CheckClickEvents(HealthScope scope, List<HealthIssue> issues)
        {
            foreach (var selectable in scope.Selectables)
                foreach (var property in EventPropertiesOf(selectable.GetType()))
                    CheckEvent(selectable, property, issues);
        }

        private static void CheckEvent(Selectable owner, PropertyInfo property, List<HealthIssue> issues)
        {
            object value;
            try { value = property.GetValue(owner); }
            catch (TargetInvocationException) { return; } // a custom control's getter that throws

            if (!(value is UnityEventBase unityEvent)) return;

            var eventName = ObjectNames.NicifyVariableName(property.Name);
            for (var i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            {
                var problem = DescribeBrokenListener(unityEvent, i, eventName);
                if (problem == null) continue;

                issues.Add(new HealthIssue(HealthCheck.ClickEvents, HealthSeverity.Broken,
                    owner.name, problem, new Object[] { owner.gameObject }));
            }
        }

        // Null when the listener resolves. A listener switched Off is left alone:
        // that is a deliberate choice in the Inspector, not a broken reference.
        private static string DescribeBrokenListener(UnityEventBase unityEvent, int index, string eventName)
        {
            if (unityEvent.GetPersistentListenerState(index) == UnityEventCallState.Off) return null;

            var target = unityEvent.GetPersistentTarget(index);
            if (target == null)
                return string.Format(HealthContent.Messages.ListenerNoTarget, eventName, index + 1);

            var method = unityEvent.GetPersistentMethodName(index);
            if (string.IsNullOrEmpty(method))
                return string.Format(HealthContent.Messages.ListenerNoMethod, eventName, index + 1);

            var type = target.GetType();
            return MethodExists(type, method)
                ? null
                : string.Format(HealthContent.Messages.ListenerMissingMethod, eventName, type.Name, method);
        }

        // Every public UnityEvent a control exposes: Button.onClick,
        // Toggle.onValueChanged, TMP_InputField.onEndEdit, and custom controls' own.
        // Obsolete aliases are skipped so one event is never reported twice.
        private static PropertyInfo[] EventPropertiesOf(Type type)
        {
            if (EventPropertiesByType.TryGetValue(type, out var cached)) return cached;

            var found = new List<PropertyInfo>();
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                if (IsEventProperty(property))
                    found.Add(property);

            return EventPropertiesByType[type] = found.ToArray();
        }

        private static bool IsEventProperty(PropertyInfo property)
        {
            return property.CanRead
                && property.GetIndexParameters().Length == 0
                && typeof(UnityEventBase).IsAssignableFrom(property.PropertyType)
                && !property.IsDefined(typeof(ObsoleteAttribute), true);
        }

        // Only a method that is gone entirely is reported. A same-named method
        // with a changed signature is rare, and Unity reports that one itself.
        private static bool MethodExists(Type type, string methodName)
        {
            var key = (type, methodName);
            if (MethodExistsCache.TryGetValue(key, out var exists)) return exists;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                                     | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (var t = type; t != null && !exists; t = t.BaseType)
                foreach (var method in t.GetMethods(flags))
                    if (method.Name == methodName && method.GetParameters().Length <= 1)
                        exists = true;

            return MethodExistsCache[key] = exists;
        }

        // ── EventSystem ──────────────────────────────────────────────────────

        internal static void CheckEventSystems(HealthScope scope, List<HealthIssue> issues)
        {
            var live = scope.EventSystems.FindAll(es => es.isActiveAndEnabled);

            if (live.Count == 0)
            {
                if (scope.HasLiveControls())
                    issues.Add(new HealthIssue(HealthCheck.EventSystem, HealthSeverity.Warning,
                        HealthContent.Messages.EventSystemLabel, HealthContent.Messages.NoEventSystem, null,
                        () => UIPilotEventSystem.Create(HealthContent.Undo.Fix)));
                return;
            }

            if (live.Count > 1)
                issues.Add(new HealthIssue(HealthCheck.EventSystem, HealthSeverity.Warning,
                    live[1].name, string.Format(HealthContent.Messages.ManyEventSystems, live.Count),
                    live.ConvertAll(es => (Object)es.gameObject).ToArray()));

            foreach (var eventSystem in live)
                CheckInputModule(eventSystem, issues);
        }

        private static void CheckInputModule(EventSystem eventSystem, List<HealthIssue> issues)
        {
            var go       = eventSystem.gameObject;
            var anyLive  = false;

            foreach (var module in go.GetComponents<BaseInputModule>())
            {
                if (!module.enabled) continue;
                anyLive = true;

                if (module is StandaloneInputModule legacy && !UIPilotEventSystem.LegacyInputAvailable)
                    issues.Add(new HealthIssue(HealthCheck.EventSystem, HealthSeverity.Broken,
                        go.name, HealthContent.Messages.LegacyModule, new Object[] { go },
                        CanReplaceLegacyModule() ? () => ReplaceLegacyModule(legacy) : (Action)null));
            }

            if (!anyLive)
                issues.Add(new HealthIssue(HealthCheck.EventSystem, HealthSeverity.Broken,
                    go.name, HealthContent.Messages.NoInputModule, new Object[] { go },
                    () => Undo.AddComponent(go, UIPilotEventSystem.InputModuleType())));
        }

        // Only when the Input System module can actually be found; otherwise the
        // replacement would be StandaloneInputModule again.
        private static bool CanReplaceLegacyModule()
        {
            return UIPilotEventSystem.InputModuleType() != typeof(StandaloneInputModule);
        }

        private static void ReplaceLegacyModule(StandaloneInputModule legacy)
        {
            var go = legacy.gameObject;
            Undo.DestroyObjectImmediate(legacy);

            var moduleType = UIPilotEventSystem.InputModuleType();
            if (go.GetComponent(moduleType) == null)
                Undo.AddComponent(go, moduleType);
        }
    }
}
