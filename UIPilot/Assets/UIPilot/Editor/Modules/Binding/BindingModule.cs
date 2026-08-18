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

        internal static void ApplyBindings()
        {
            var buttons    = FindUIPilotButtons();
            var actions    = ActionDiscoveryModule.Scan();
            var selections = BuildAutoSelections(buttons, actions);
            ApplyBindings(selections, actions);
        }

        private static Dictionary<string, int> BuildAutoSelections(
            List<Button> buttons, List<DiscoveredAction> actions)
        {
            var selections = new Dictionary<string, int>();

            foreach (var btn in buttons)
            {
                if (btn == null) continue;

                var label = btn.name.StartsWith(UIGeneratorContent.GameObjects.ButtonPrefix, System.StringComparison.Ordinal)
                    ? btn.name.Substring(UIGeneratorContent.GameObjects.ButtonPrefix.Length)
                    : btn.name;

                var expectedMethod = "On" + label + "Pressed";
                var matched        = false;

                for (var i = 0; i < actions.Count; i++)
                {
                    if (actions[i].MethodName == expectedMethod)
                    {
                        selections[btn.name] = i + 1;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                    selections[btn.name] = 0;
            }

            return selections;
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

                var serializedButton = new SerializedObject(button);
                var onClickProp      = serializedButton.FindProperty("m_OnClick");
                var callsProp        = onClickProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
                var lastCall         = callsProp.GetArrayElementAtIndex(callsProp.arraySize - 1);
                lastCall.FindPropertyRelative("m_CallState").intValue = 2;
                serializedButton.ApplyModifiedPropertiesWithoutUndo();

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
            var canvas = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);
            if (canvas == null) return null;

            foreach (var btn in canvas.GetComponentsInChildren<Button>(true))
                if (btn.name == buttonName)
                    return btn;

            return null;
        }
    }
}
