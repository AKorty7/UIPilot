using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIPilot.Editor.Modules.Binding;
using UIPilot.Editor.Modules.SceneAudit;
using UIPilot.Editor.Modules.ScriptSetup;
using UIPilot.Editor.Modules.UIGenerator;
using UIPilot.Editor.Modules.Validation;

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

        // ── Public entry point ───────────────────────────────────────────────

        public static List<ResilienceResult> Repair(List<SceneAuditResult> auditResults)
        {
            Debug.Log(ResilienceContent.Console.RepairStart);

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(ResilienceContent.UndoLabel);

            var results = new List<ResilienceResult>();

            foreach (var audit in auditResults)
            {
                if (audit.Severity == SceneAuditSeverity.OK)
                    continue;

                var result = Dispatch(audit);
                results.Add(result);
            }

            // ── Post-repair validation ───────────────────────────────────────
            var validationResults = ValidationModule.Validate();
            var remainingIssues   = 0;
            foreach (var v in validationResults)
                if (v.Severity == ValidationSeverity.Error || v.Severity == ValidationSeverity.Warning)
                    remainingIssues++;

            Debug.Log(string.Format(ResilienceContent.Console.ValidationLog, remainingIssues));

            // ── Collapse entire repair into one undo step ────────────────────
            Undo.CollapseUndoOperations(undoGroup);

            var succeeded = 0;
            foreach (var r in results)
                if (r.Success) succeeded++;

            Debug.Log(string.Format(
                ResilienceContent.Console.RepairComplete, results.Count, succeeded));

            return results;
        }

        // ── Dispatch ─────────────────────────────────────────────────────────

        private static ResilienceResult Dispatch(SceneAuditResult audit)
        {
            var label = audit.Label;

            // Canvas
            if (label == SceneAuditContent.Labels.Canvas)
                return RepairCanvas(audit);

            // EventSystem
            if (label == SceneAuditContent.Labels.EventSystem)
                return RepairEventSystem();

            // Panels
            if (label == SceneAuditContent.Labels.MainMenuPanel)
                return RepairPanel(ResilienceContent.Labels.MainMenuPanel, MenuType.MainMenu);

            if (label == SceneAuditContent.Labels.PauseMenuPanel)
                return RepairPanel(ResilienceContent.Labels.PauseMenuPanel, MenuType.PauseMenu);

            if (label == SceneAuditContent.Labels.SettingsPanel)
                return RepairPanel(ResilienceContent.Labels.SettingsPanel, MenuType.SettingsMenu);

            // GameManager GO or script on disk
            if (label == SceneAuditContent.Labels.GameManagerGO ||
                label == SceneAuditContent.Labels.GameManagerScript)
                return RepairGameManager(label);

            // Button listeners
            if (label == SceneAuditContent.Labels.ButtonListeners)
                return RepairButtonListeners();

            return new ResilienceResult(label, ResilienceContent.Results.UnknownLabel, false);
        }

        // ── Canvas repair ────────────────────────────────────────────────────

        private static ResilienceResult RepairCanvas(SceneAuditResult audit)
        {
            var canvasGO = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);

            // Missing: generate any menu so the canvas is (re)created with
            // the correct CanvasScaler.  UIGeneratorModule handles idempotency.
            if (canvasGO == null)
            {
                UIGeneratorModule.Generate(MenuType.MainMenu);

                MarkSceneDirty();
                return new ResilienceResult(
                    ResilienceContent.Labels.Canvas,
                    ResilienceContent.Results.CanvasRepaired,
                    GameObject.Find(UIGeneratorContent.GameObjects.Canvas) != null);
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

            return new ResilienceResult(
                ResilienceContent.Labels.CanvasScaler,
                ResilienceContent.Results.CanvasScalerFixed,
                true);
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

        // ── Panel repair ─────────────────────────────────────────────────────

        private static ResilienceResult RepairPanel(string resultLabel, MenuType menuType)
        {
            try
            {
                UIGeneratorModule.Generate(menuType);
                MarkSceneDirty();
                return new ResilienceResult(resultLabel, ResilienceContent.Results.PanelRegenerated, true);
            }
            catch (Exception ex)
            {
                return new ResilienceResult(resultLabel, ex.Message, false);
            }
        }

        // ── GameManager repair ───────────────────────────────────────────────

        private static ResilienceResult RepairGameManager(string auditLabel)
        {
            var allMenus = new[]
            {
                MenuType.MainMenu,
                MenuType.PauseMenu,
                MenuType.SettingsMenu
            };

            try
            {
                ScriptSetupModule.GenerateGameManager(allMenus);
                MarkSceneDirty();
                return new ResilienceResult(auditLabel, ResilienceContent.Results.GameManagerFixed, true);
            }
            catch (Exception ex)
            {
                return new ResilienceResult(auditLabel, ex.Message, false);
            }
        }

        // ── Button listener repair ───────────────────────────────────────────

        private static ResilienceResult RepairButtonListeners()
        {
            try
            {
                BindingModule.FindUIPilotButtons();
                BindingModule.ApplyBindings();
                MarkSceneDirty();

                return new ResilienceResult(
                    ResilienceContent.Labels.ButtonListeners,
                    ResilienceContent.Results.ListenersFixed,
                    true);
            }
            catch (Exception ex)
            {
                return new ResilienceResult(
                    ResilienceContent.Labels.ButtonListeners,
                    ex.Message,
                    false);
            }
        }

        // ── Utility ──────────────────────────────────────────────────────────

        private static void MarkSceneDirty()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
