using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIPilot.Editor.Modules.SceneAudit;
using UIPilot.Editor.Modules.UIGenerator;

namespace UIPilot.Editor.Modules.Resilience
{
    internal static class ResilienceModule
    {
        // ── Inline result type ───────────────────────────────────────────────

        public sealed class ResilienceResult
        {
            public string Label   { get; }
            public string Detail  { get; }
            public bool   Success { get; }

            internal ResilienceResult(string label, string detail, bool success)
            {
                Label   = label;
                Detail  = detail;
                Success = success;
            }
        }

        // Follow-up work that requires another module. Resilience decides *what*
        // is needed; UIPilotWindow performs the cross-module calls.
        internal enum RepairFollowUpKind
        {
            None = 0,
            GeneratePanel,
            GenerateGameManager,
            ReapplyBindings
        }

        internal sealed class RepairFollowUp
        {
            internal RepairFollowUpKind Kind            { get; }
            internal string             ResultLabel     { get; }
            internal string             SuccessDetail   { get; }
            internal MenuType           MenuType        { get; }
            internal MenuType[]         Menus           { get; }
            internal ResilienceResult   CompletedResult { get; }

            internal bool NeedsWindowAction => Kind != RepairFollowUpKind.None;

            private RepairFollowUp(
                RepairFollowUpKind kind,
                string             resultLabel,
                string             successDetail,
                MenuType           menuType,
                MenuType[]         menus,
                ResilienceResult   completedResult)
            {
                Kind            = kind;
                ResultLabel     = resultLabel;
                SuccessDetail   = successDetail;
                MenuType        = menuType;
                Menus           = menus;
                CompletedResult = completedResult;
            }

            internal static RepairFollowUp Completed(ResilienceResult result)
            {
                return new RepairFollowUp(
                    RepairFollowUpKind.None, null, null, default, null, result);
            }

            internal static RepairFollowUp GeneratePanel(
                string resultLabel, string successDetail, MenuType menuType)
            {
                return new RepairFollowUp(
                    RepairFollowUpKind.GeneratePanel,
                    resultLabel,
                    successDetail,
                    menuType,
                    null,
                    null);
            }

            internal static RepairFollowUp GenerateGameManager(string resultLabel)
            {
                return new RepairFollowUp(
                    RepairFollowUpKind.GenerateGameManager,
                    resultLabel,
                    ResilienceContent.Results.GameManagerFixed,
                    default,
                    new[] { MenuType.MainMenu, MenuType.PauseMenu, MenuType.SettingsMenu },
                    null);
            }

            internal static RepairFollowUp ReapplyBindings(string resultLabel)
            {
                return new RepairFollowUp(
                    RepairFollowUpKind.ReapplyBindings,
                    resultLabel,
                    ResilienceContent.Results.ListenersFixed,
                    default,
                    null,
                    null);
            }
        }

        // ── Public entry point ───────────────────────────────────────────────
        // Walks the audit list, applies repairs that Resilience can do itself
        // (Canvas scaler, EventSystem), and records follow-ups for the window
        // to run against other modules. Result order matches audit order.

        internal static List<RepairFollowUp> Repair(List<SceneAuditResult> auditResults)
        {
            var followUps = new List<RepairFollowUp>();

            foreach (var audit in auditResults)
            {
                if (audit.Severity == SceneAuditSeverity.OK)
                    continue;

                followUps.Add(Dispatch(audit));
            }

            return followUps;
        }

        internal static ResilienceResult MakeResult(string label, string detail, bool success)
        {
            return new ResilienceResult(label, detail, success);
        }

        // ── Dispatch ─────────────────────────────────────────────────────────

        private static RepairFollowUp Dispatch(SceneAuditResult audit)
        {
            var label = audit.Label;

            // Canvas
            if (label == SceneAuditContent.Labels.Canvas)
                return RepairCanvas();

            // EventSystem
            if (label == SceneAuditContent.Labels.EventSystem)
                return RepairFollowUp.Completed(RepairEventSystem());

            // Panels
            if (label == SceneAuditContent.Labels.MainMenuPanel)
                return RepairFollowUp.GeneratePanel(
                    ResilienceContent.Labels.MainMenuPanel,
                    ResilienceContent.Results.PanelRegenerated,
                    MenuType.MainMenu);

            if (label == SceneAuditContent.Labels.PauseMenuPanel)
                return RepairFollowUp.GeneratePanel(
                    ResilienceContent.Labels.PauseMenuPanel,
                    ResilienceContent.Results.PanelRegenerated,
                    MenuType.PauseMenu);

            if (label == SceneAuditContent.Labels.SettingsPanel)
                return RepairFollowUp.GeneratePanel(
                    ResilienceContent.Labels.SettingsPanel,
                    ResilienceContent.Results.PanelRegenerated,
                    MenuType.SettingsMenu);

            // GameManager GO or script on disk
            if (label == SceneAuditContent.Labels.GameManagerGO ||
                label == SceneAuditContent.Labels.GameManagerScript)
                return RepairFollowUp.GenerateGameManager(label);

            // Button listeners
            if (label == SceneAuditContent.Labels.ButtonListeners)
                return RepairFollowUp.ReapplyBindings(ResilienceContent.Labels.ButtonListeners);

            return RepairFollowUp.Completed(
                new ResilienceResult(label, ResilienceContent.Results.UnknownLabel, false));
        }

        // ── Canvas repair ────────────────────────────────────────────────────

        private static RepairFollowUp RepairCanvas()
        {
            var canvasGO = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);

            // Missing: window must generate a menu so the canvas is (re)created
            // with the correct CanvasScaler. UIGeneratorModule handles idempotency.
            if (canvasGO == null)
            {
                return RepairFollowUp.GeneratePanel(
                    ResilienceContent.Labels.Canvas,
                    ResilienceContent.Results.CanvasRepaired,
                    MenuType.MainMenu);
            }

            // Present but misconfigured scaler
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = Undo.AddComponent<CanvasScaler>(canvasGO);

            Undo.RecordObject(scaler, ResilienceContent.UndoLabel);
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;

            EditorUtility.SetDirty(canvasGO);
            MarkSceneDirty();

            return RepairFollowUp.Completed(new ResilienceResult(
                ResilienceContent.Labels.CanvasScaler,
                ResilienceContent.Results.CanvasScalerFixed,
                true));
        }

        // ── EventSystem repair ───────────────────────────────────────────────

        private static ResilienceResult RepairEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
                return new ResilienceResult(
                    ResilienceContent.Labels.EventSystem,
                    ResilienceContent.Results.EventSystemFixed,
                    true);

            var esGO = new GameObject(UIGeneratorContent.GameObjects.EventSystem);
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(esGO, ResilienceContent.UndoLabel);

            MarkSceneDirty();

            return new ResilienceResult(
                ResilienceContent.Labels.EventSystem,
                ResilienceContent.Results.EventSystemFixed,
                true);
        }

        // ── Utility ──────────────────────────────────────────────────────────

        private static void MarkSceneDirty()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
