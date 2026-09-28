using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UIPilot.Editor.Modules.ActionDiscovery;
using UIPilot.Editor.Modules.UIGenerator;

namespace UIPilot.Editor.Modules.Binding
{
    internal static class BindingModule
    {
        // ── Public entry points ──────────────────────────────────────────────

        internal static List<Button> FindUIPilotButtons()
        {
            var results = new List<Button>();

            var canvas = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);
            if (canvas == null) return results;

            foreach (var btn in canvas.GetComponentsInChildren<Button>(true))
                if (btn.name.StartsWith(UIGeneratorContent.GameObjects.ButtonPrefix, System.StringComparison.Ordinal))
                    results.Add(btn);

            return results;
        }

        internal static void ApplyBindings(
            Dictionary<string, int> selections,
            List<DiscoveredAction>  actions)
        {
            var allBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsSortMode.None);
            var allButtons    = FindUIPilotButtons();

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

                // Menus share button names (Settings and Quit exist in both the
                // Main and Pause menus), so a selection applies to every button
                // carrying that name — not just the first one found.
                foreach (var button in allButtons)
                    if (button.name == kvp.Key)
                        BindButton(button, target, method);
            }

            EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void BindButton(
            Button button, MonoBehaviour target, System.Reflection.MethodInfo method)
        {
            Undo.RecordObject(button, BindingContent.Undo.Action);

            // Remove all existing persistent listeners before adding the new one.
            var eventCount = button.onClick.GetPersistentEventCount();
            for (var i = eventCount - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(button.onClick, i);

            var call = System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), target, method)
                as UnityEngine.Events.UnityAction;

            UnityEventTools.AddPersistentListener(button.onClick, call);

            var serializedButton = new SerializedObject(button);
            var onClickProp      = serializedButton.FindProperty("m_OnClick");
            var callsProp        = onClickProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
            var lastCall         = callsProp.GetArrayElementAtIndex(callsProp.arraySize - 1);
            lastCall.FindPropertyRelative("m_CallState").intValue = 2;
            serializedButton.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(button);
        }

        private static MonoBehaviour FindBehaviour(MonoBehaviour[] all, string className)
        {
            foreach (var mb in all)
                if (mb.GetType().Name == className)
                    return mb;
            return null;
        }
    }
}
