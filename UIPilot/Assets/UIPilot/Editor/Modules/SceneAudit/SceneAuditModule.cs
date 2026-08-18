using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIPilot.Editor.Modules.ScriptSetup;
using UIPilot.Editor.Modules.UIGenerator;

namespace UIPilot.Editor.Modules.SceneAudit
{
    internal static class SceneAuditModule
    {
        private const int CallStatePersistent = 2;

        // ── Public entry point ───────────────────────────────────────────────

        internal static List<SceneAuditResult> Scan()
        {
            var results = new List<SceneAuditResult>();

            CheckCanvas(results);
            CheckEventSystem(results);
            CheckPanel(results, SceneAuditContent.Labels.MainMenuPanel,
                UIGeneratorContent.GameObjects.MainMenuPrefix + UIGeneratorContent.GameObjects.PanelSuffix,
                UIGeneratorContent.ButtonSets.MainMenu);
            CheckPanel(results, SceneAuditContent.Labels.PauseMenuPanel,
                UIGeneratorContent.GameObjects.PauseMenuPrefix + UIGeneratorContent.GameObjects.PanelSuffix,
                UIGeneratorContent.ButtonSets.PauseMenu);
            CheckPanel(results, SceneAuditContent.Labels.SettingsPanel,
                UIGeneratorContent.GameObjects.SettingsMenuPrefix + UIGeneratorContent.GameObjects.PanelSuffix,
                UIGeneratorContent.ButtonSets.SettingsMenu);
            CheckGameManagerGO(results);
            CheckGameManagerScript(results);
            CheckButtonListeners(results);

            return results;
        }

        // ── Check 1: Canvas ──────────────────────────────────────────────────

        private static void CheckCanvas(List<SceneAuditResult> results)
        {
            var canvasGO = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);
            if (canvasGO == null)
            {
                results.Add(new SceneAuditResult(
                    SceneAuditContent.Labels.Canvas,
                    SceneAuditContent.Details.CanvasMissing,
                    SceneAuditSeverity.Missing));
                return;
            }

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            if (scaler == null
                || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                || scaler.referenceResolution != new Vector2(1920f, 1080f))
            {
                results.Add(new SceneAuditResult(
                    SceneAuditContent.Labels.Canvas,
                    SceneAuditContent.Details.CanvasScalerMisconfigured,
                    SceneAuditSeverity.Warning));
                return;
            }

            results.Add(new SceneAuditResult(
                SceneAuditContent.Labels.Canvas,
                string.Empty,
                SceneAuditSeverity.OK));
        }

        // ── Check 2: EventSystem ─────────────────────────────────────────────

        private static void CheckEventSystem(List<SceneAuditResult> results)
        {
            var es = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                results.Add(new SceneAuditResult(
                    SceneAuditContent.Labels.EventSystem,
                    SceneAuditContent.Details.EventSystemMissing,
                    SceneAuditSeverity.Warning));
                return;
            }

            results.Add(new SceneAuditResult(
                SceneAuditContent.Labels.EventSystem,
                string.Empty,
                SceneAuditSeverity.OK));
        }

        // ── Check 3: Panels ──────────────────────────────────────────────────

        private static void CheckPanel(List<SceneAuditResult> results,
            string label, string panelName, string[] expectedButtons)
        {
            var panelGO = GameObject.Find(panelName);
            if (panelGO == null)
            {
                results.Add(new SceneAuditResult(
                    label,
                    SceneAuditContent.Details.PanelMissing,
                    SceneAuditSeverity.Missing));
                return;
            }

            foreach (var btnLabel in expectedButtons)
            {
                var expectedName = UIGeneratorContent.GameObjects.ButtonPrefix + btnLabel;
                var found        = false;

                foreach (Transform child in panelGO.transform)
                    if (child.name == expectedName) { found = true; break; }

                if (!found)
                {
                    results.Add(new SceneAuditResult(
                        label,
                        string.Format(SceneAuditContent.Details.PanelBroken, expectedName),
                        SceneAuditSeverity.Broken));
                    return;
                }
            }

            results.Add(new SceneAuditResult(
                label,
                string.Empty,
                SceneAuditSeverity.OK));
        }

        // ── Check 4: GameManager GO ──────────────────────────────────────────

        private static void CheckGameManagerGO(List<SceneAuditResult> results)
        {
            var go = GameObject.Find(ScriptSetupContent.GameObjects.ManagerName);
            if (go == null)
            {
                results.Add(new SceneAuditResult(
                    SceneAuditContent.Labels.GameManagerGO,
                    SceneAuditContent.Details.GameManagerGOMissing,
                    SceneAuditSeverity.Warning));
                return;
            }

            results.Add(new SceneAuditResult(
                SceneAuditContent.Labels.GameManagerGO,
                string.Empty,
                SceneAuditSeverity.OK));
        }

        // ── Check 5: GameManager Script on disk ──────────────────────────────

        private static void CheckGameManagerScript(List<SceneAuditResult> results)
        {
            var guids = AssetDatabase.FindAssets(SceneAuditGameObjects.ManagerScriptSearch);
            if (guids.Length == 0)
            {
                results.Add(new SceneAuditResult(
                    SceneAuditContent.Labels.GameManagerScript,
                    SceneAuditContent.Details.GameManagerScriptMissing,
                    SceneAuditSeverity.Warning));
                return;
            }

            results.Add(new SceneAuditResult(
                SceneAuditContent.Labels.GameManagerScript,
                string.Empty,
                SceneAuditSeverity.OK));
        }

        // ── Check 6: Button Listeners ────────────────────────────────────────

        private static void CheckButtonListeners(List<SceneAuditResult> results)
        {
            var canvasGO = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);
            if (canvasGO == null) return;

            var allPersistent = true;

            foreach (var btn in canvasGO.GetComponentsInChildren<Button>(true))
            {
                if (!btn.name.StartsWith(UIGeneratorContent.GameObjects.ButtonPrefix,
                        System.StringComparison.Ordinal))
                    continue;

                if (!HasPersistentListener(btn))
                {
                    allPersistent = false;
                    results.Add(new SceneAuditResult(
                        SceneAuditContent.Labels.ButtonListeners,
                        string.Format(SceneAuditContent.Details.ListenerNotPersistent, btn.name),
                        SceneAuditSeverity.Warning));
                }
            }

            if (allPersistent)
            {
                results.Add(new SceneAuditResult(
                    SceneAuditContent.Labels.ButtonListeners,
                    string.Empty,
                    SceneAuditSeverity.OK));
            }
        }

        private static bool HasPersistentListener(Button button)
        {
            var so        = new SerializedObject(button);
            var onClickProp = so.FindProperty("m_OnClick");
            var callsProp   = onClickProp.FindPropertyRelative("m_PersistentCalls.m_Calls");

            for (var i = 0; i < callsProp.arraySize; i++)
            {
                var callState = callsProp
                    .GetArrayElementAtIndex(i)
                    .FindPropertyRelative("m_CallState")
                    .intValue;

                if (callState == CallStatePersistent) return true;
            }

            return false;
        }
    }

    // ── Internal name constants (not exposed outside this module) ────────────

    internal static class SceneAuditGameObjects
    {
        internal const string ManagerScriptSearch = ScriptSetupContent.GameObjects.ManagerName + " t:Script";
    }
}
