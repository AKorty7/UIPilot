using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UIPilot.Editor.Modules.ActionDiscovery;

namespace UIPilot.Editor.Modules.Binding
{
    internal static class BindingModule
    {
        private const string CanvasName   = "UIPilot_Canvas";
        private const string ButtonPrefix = "UIPilot_Btn_";

        // ── Public entry points ──────────────────────────────────────────────

        internal static List<Button> FindUIPilotButtons()
        {
            var results = new List<Button>();

            var canvas = GameObject.Find(CanvasName);
            if (canvas == null) return results;

            foreach (var btn in canvas.GetComponentsInChildren<Button>(true))
                if (btn.name.StartsWith(ButtonPrefix, System.StringComparison.Ordinal))
                    results.Add(btn);

            return results;
        }

        internal static void ApplyBindings(
            Dictionary<string, int> selections,
            List<DiscoveredAction>  actions)
        {
            var allBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsSortMode.None);

            foreach (var kvp in selections)
            {
                if (kvp.Value <= 0) continue;

                var action = actions[kvp.Value - 1];

                var target = FindBehaviour(allBehaviours, action.ClassName);
                if (target == null)
                {
                    Debug.LogWarning(BindingContent.Messages.ComponentNotFound + action.ClassName);
                    continue;
                }

                var method = target.GetType().GetMethod(
                    action.MethodName,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (method == null)
                {
                    Debug.LogWarning(BindingContent.Messages.MethodNotFound + action.FullLabel);
                    continue;
                }

                var button = FindButtonByName(kvp.Key);
                if (button == null) continue;

                Undo.RecordObject(button, BindingContent.Undo.Action);

                // Remove all existing persistent listeners before adding the new one.
                var eventCount = button.onClick.GetPersistentEventCount();
                for (var i = eventCount - 1; i >= 0; i--)
                    UnityEventTools.RemovePersistentListener(button.onClick, i);

                var call = System.Delegate.CreateDelegate(
                    typeof(UnityEngine.Events.UnityAction), target, method)
                    as UnityEngine.Events.UnityAction;

                UnityEventTools.AddPersistentListener(button.onClick, call);

                EditorUtility.SetDirty(button);
            }

            EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static MonoBehaviour FindBehaviour(MonoBehaviour[] all, string className)
        {
            foreach (var mb in all)
                if (mb.GetType().Name == className)
                    return mb;
            return null;
        }

        private static Button FindButtonByName(string buttonName)
        {
            var canvas = GameObject.Find(CanvasName);
            if (canvas == null) return null;

            foreach (var btn in canvas.GetComponentsInChildren<Button>(true))
                if (btn.name == buttonName)
                    return btn;

            return null;
        }
    }
}
