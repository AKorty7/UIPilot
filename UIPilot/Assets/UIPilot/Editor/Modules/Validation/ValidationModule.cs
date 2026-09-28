using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.UIGenerator;

namespace UIPilot.Editor.Modules.Validation
{
    internal static class ValidationModule
    {
        // ── Public entry points ──────────────────────────────────────────────

        internal static List<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();

            CheckEventSystem(results);
            CheckCanvas(results);
            CheckButtons(results);

            return results;
        }

        internal static void RunAutoFix(ValidationResult result)
        {
            if (result.AutoFix == null) return;

            result.AutoFix();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        // ── Check: EventSystem ───────────────────────────────────────────────

        private static void CheckEventSystem(List<ValidationResult> results)
        {
            var es = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                results.Add(new ValidationResult(
                    ValidationContent.Messages.NoEventSystem,
                    ValidationSeverity.Error,
                    () => UIPilotEventSystem.Create(ValidationContent.Undo.AutoFix)));
            }
            else
            {
                results.Add(new ValidationResult(
                    ValidationContent.Messages.EventSystemOk,
                    ValidationSeverity.Info));
            }
        }

        // ── Check: Canvas / CanvasScaler ────────────────────────────────────

        private static void CheckCanvas(List<ValidationResult> results)
        {
            var canvasGO = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);
            if (canvasGO == null)
            {
                results.Add(new ValidationResult(
                    ValidationContent.Messages.NoCanvas,
                    ValidationSeverity.Warning));
                return;
            }

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                results.Add(new ValidationResult(
                    ValidationContent.Messages.NoCanvasScaler,
                    ValidationSeverity.Error,
                    () =>
                    {
                        Undo.RecordObject(canvasGO, ValidationContent.Undo.AutoFix);
                        var s = Undo.AddComponent<CanvasScaler>(canvasGO);
                        ConfigureScaler(s);
                    }));
                return;
            }

            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                results.Add(new ValidationResult(
                    ValidationContent.Messages.WrongScaleMode,
                    ValidationSeverity.Warning,
                    () =>
                    {
                        Undo.RecordObject(scaler, ValidationContent.Undo.AutoFix);
                        ConfigureScaler(scaler);
                    }));
            }
            else
            {
                results.Add(new ValidationResult(
                    ValidationContent.Messages.CanvasScalerOk,
                    ValidationSeverity.Info));
            }
        }

        // ── Check: Buttons ───────────────────────────────────────────────────

        private static void CheckButtons(List<ValidationResult> results)
        {
            var canvasGO = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);
            if (canvasGO == null) return;

            var buttons       = canvasGO.GetComponentsInChildren<Button>(true);
            var allWired      = true;
            var anySwitchedOff = false;

            foreach (var btn in buttons)
            {
                if (!btn.name.StartsWith(UIGeneratorContent.GameObjects.ButtonPrefix, System.StringComparison.Ordinal))
                    continue;

                var onClick       = btn.onClick;
                var persistentCount = onClick.GetPersistentEventCount();

                if (persistentCount == 0)
                {
                    allWired = false;
                    results.Add(new ValidationResult(
                        string.Format(ValidationContent.Messages.ButtonNoListener, btn.name),
                        ValidationSeverity.Error));
                    continue;
                }

                // A listener set to Off never fires. Runtime Only is not a problem:
                // it is Unity's default and the state UIPilot itself writes.
                for (var i = 0; i < persistentCount; i++)
                {
                    if (onClick.GetPersistentListenerState(i) == UnityEngine.Events.UnityEventCallState.Off)
                    {
                        anySwitchedOff = true;
                        results.Add(new ValidationResult(
                            string.Format(ValidationContent.Messages.ButtonListenerOff, btn.name),
                            ValidationSeverity.Warning));
                        break;
                    }
                }
            }

            if (allWired && !anySwitchedOff && buttons.Length > 0)
            {
                results.Add(new ValidationResult(
                    ValidationContent.Messages.AllButtonsWired,
                    ValidationSeverity.Info));
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void ConfigureScaler(CanvasScaler scaler)
        {
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;
        }
    }
}
