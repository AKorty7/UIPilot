using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.Health;

namespace UIPilot.Editor
{
    // The UI Health section: what UIPilotHealthMonitor found, one row per issue,
    // each with its fix. Checking happens in the monitor, whether this is open or not.
    public sealed partial class UIPilotWindow
    {
        private bool _healthFoldout = true;
        private bool _healthChecksFoldout;

        private static readonly GUIContent ContentCheckNow    = new GUIContent(UIPilotLabels.Health.CheckNowButton, UIPilotLabels.Health.CheckNowTooltip);
        private static readonly GUIContent ContentHealthFix   = new GUIContent(UIPilotLabels.Health.FixButton,      UIPilotLabels.Health.FixTooltip);
        private static readonly GUIContent ContentWarnOnPlay  = new GUIContent(UIPilotLabels.Health.WarnOnPlay,     UIPilotLabels.Health.WarnOnPlayTip);
        private static readonly GUIContent ContentWarnOnBuild = new GUIContent(UIPilotLabels.Health.WarnOnBuild,    UIPilotLabels.Health.WarnOnBuildTip);

        private static readonly Dictionary<HealthCheck, GUIContent> CheckToggles = new Dictionary<HealthCheck, GUIContent>
        {
            { HealthCheck.ClickEvents,       new GUIContent(HealthContent.Checks.ClickEvents,    HealthContent.Checks.ClickEventsTip) },
            { HealthCheck.EventSystem,       new GUIContent(HealthContent.Checks.EventSystem,    HealthContent.Checks.EventSystemTip) },
            { HealthCheck.Raycasters,        new GUIContent(HealthContent.Checks.Raycasters,     HealthContent.Checks.RaycastersTip) },
            { HealthCheck.BlockedButtons,    new GUIContent(HealthContent.Checks.BlockedButtons, HealthContent.Checks.BlockedButtonsTip) },
            { HealthCheck.GamepadNavigation, new GUIContent(HealthContent.Checks.Gamepad,        HealthContent.Checks.GamepadTip) },
            { HealthCheck.TextSize,          new GUIContent(HealthContent.Checks.TextSize,       HealthContent.Checks.TextSizeTip) },
        };

        // The toolbar lamp's click: the window, on its UI Health section.
        internal static void OpenHealth()
        {
            EditorPrefs.SetBool(UIPilotLabels.Health.EditorPrefsOpen, true);
            Open();

            var window = GetWindow<UIPilotWindow>();
            window._healthFoldout   = true;
            window._allCollapsed    = false;
            window._windowScrollPos = Vector2.zero; // UI Health is the first section
        }

        internal static string IssueCount(int count)
        {
            return count == 1
                ? UIPilotLabels.Health.IssueCountOne
                : string.Format(UIPilotLabels.Health.IssueCountMany, count);
        }

        private void DrawHealthSection()
        {
            if (!DrawSectionFoldout(ref _healthFoldout, UIPilotLabels.Health.Section,
                    UIPilotLabels.Health.EditorPrefsOpen))
                return;

            using (new EditorGUILayout.VerticalScope(UIPilotStyles.Card))
            {
                DrawHealthSummary();

                foreach (var issue in UIPilotHealthMonitor.Issues)
                {
                    EditorGUILayout.Space(4f);
                    DrawHealthRow(issue);
                }

                DrawToolbarTip();

                EditorGUILayout.Space(8f);
                DrawHealthSettings();
            }
        }

        // Once, until dismissed: how to put the lamp on the main toolbar. Unity
        // hides toolbar items that packages add, and offers no public way to show one.
        private static void DrawToolbarTip()
        {
            if (EditorPrefs.GetBool(UIPilotLabels.Health.EditorPrefsToolbarTip, false)) return;

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(UIPilotLabels.Health.ToolbarTip, UIPilotStyles.Description);

                if (GUILayout.Button(UIPilotLabels.Health.ToolbarTipDismiss, EditorStyles.miniButton, UIPilotStyles.FitWidth))
                    EditorPrefs.SetBool(UIPilotLabels.Health.EditorPrefsToolbarTip, true);
            }
        }

        // One line that answers "is my UI OK?", and the button to check again.
        private static void DrawHealthSummary()
        {
            GetHealthSummary(out var lamp, out var label, out var detail);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLamp(lamp);

                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Label(label, EditorStyles.boldLabel);
                    if (detail != null)
                        GUILayout.Label(detail, UIPilotStyles.RowDetail);
                }

                if (GUILayout.Button(ContentCheckNow, EditorStyles.miniButton, UIPilotStyles.FitWidth))
                    UIPilotHealthMonitor.CheckNow();
            }
        }

        private static void GetHealthSummary(out Color lamp, out string label, out string detail)
        {
            var scenes = OpenSceneNames();

            if (!UIPilotHealthMonitor.HasResults)
            {
                lamp = UIPilotStyles.LampIdle;
                label = UIPilotLabels.Health.NotChecked;
                detail = UIPilotLabels.Health.NotCheckedDetail;
                return;
            }

            var issues = UIPilotHealthMonitor.Issues;
            if (issues.Count == 0)
            {
                lamp = UIPilotStyles.LampOk;
                label = UIPilotLabels.Health.AllClear;
                detail = string.Format(UIPilotLabels.Health.AllClearDetail,
                    EnabledCheckCount(), HealthModule.AllChecks.Length, scenes);
                return;
            }

            lamp = UIPilotHealthMonitor.WorstSeverity() == HealthSeverity.Broken
                ? UIPilotStyles.LampFault
                : UIPilotStyles.LampCaution;
            label = string.Format(UIPilotLabels.Health.IssuesSummary, IssueCount(issues.Count), scenes);
            detail = null;
        }

        // Lamp, what and where (click to select it), the fix, and the status in words.
        private static void DrawHealthRow(HealthIssue issue)
        {
            var broken = issue.Severity == HealthSeverity.Broken;

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLamp(broken ? UIPilotStyles.LampFault : UIPilotStyles.LampCaution);

                using (new EditorGUILayout.VerticalScope())
                {
                    DrawIssueLabel(issue);
                    GUILayout.Label(issue.Detail, UIPilotStyles.RowDetail);
                }

                if (issue.Fix != null
                    && GUILayout.Button(ContentHealthFix, EditorStyles.miniButton, UIPilotStyles.FixButtonSize))
                {
                    UIPilotHealthMonitor.Fix(issue);
                    GUIUtility.ExitGUI();
                }

                GUILayout.Label(broken ? UIPilotLabels.Status.Broken : UIPilotLabels.Status.Warning,
                    UIPilotStyles.StatusWord, UIPilotStyles.StatusWordSize);
            }
        }

        // The name is a link to the object(s): selecting is how every row can be acted
        // on, even the ones UIPilot will not fix for you.
        private static void DrawIssueLabel(HealthIssue issue)
        {
            if (issue.Targets.Length == 0)
            {
                GUILayout.Label(issue.Label, UIPilotStyles.RowLabel);
                return;
            }

            var content = new GUIContent(issue.Label, UIPilotLabels.Health.RowTooltip);
            var clicked = GUILayout.Button(content, UIPilotStyles.RowLink);
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

            if (!clicked) return;

            Selection.objects = issue.Targets;
            EditorGUIUtility.PingObject(issue.Targets[0]);
        }

        // Which checks run, and where they speak up. Folded away by default:
        // most people never need it.
        private void DrawHealthSettings()
        {
            var open = EditorGUILayout.Foldout(_healthChecksFoldout, UIPilotLabels.Health.ChecksFoldout, true);
            if (open != _healthChecksFoldout)
            {
                _healthChecksFoldout = open;
                EditorPrefs.SetBool(UIPilotLabels.Health.EditorPrefsChecksOpen, open);
            }

            if (!open) return;

            EditorGUI.BeginChangeCheck();

            using (new EditorGUI.IndentLevelScope())
            {
                // Written only when a box actually changes, never on every repaint.
                foreach (var check in HealthModule.AllChecks)
                {
                    var on = HealthSettings.IsEnabled(check);
                    if (EditorGUILayout.ToggleLeft(CheckToggles[check], on) != on)
                        HealthSettings.SetEnabled(check, !on);
                }

                EditorGUILayout.Space(4f);

                var warnOnPlay = HealthSettings.WarnOnPlay;
                if (EditorGUILayout.ToggleLeft(ContentWarnOnPlay, warnOnPlay) != warnOnPlay)
                    HealthSettings.WarnOnPlay = !warnOnPlay;

                var warnOnBuild = HealthSettings.WarnOnBuild;
                if (EditorGUILayout.ToggleLeft(ContentWarnOnBuild, warnOnBuild) != warnOnBuild)
                    HealthSettings.WarnOnBuild = !warnOnBuild;

                GUILayout.Label(UIPilotLabels.Health.SettingsNote, UIPilotStyles.Description);
            }

            if (EditorGUI.EndChangeCheck())
                UIPilotHealthMonitor.CheckNow();
        }

        private static int EnabledCheckCount()
        {
            var count = 0;
            foreach (var check in HealthModule.AllChecks)
                if (HealthSettings.IsEnabled(check)) count++;
            return count;
        }

        private static string OpenSceneNames()
        {
            var names = new List<string>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                    names.Add(string.IsNullOrEmpty(scene.name) ? UIPilotLabels.Health.UntitledScene : scene.name);
            }

            return string.Join(UIPilotLabels.Health.SceneSeparator, names);
        }
    }
}
