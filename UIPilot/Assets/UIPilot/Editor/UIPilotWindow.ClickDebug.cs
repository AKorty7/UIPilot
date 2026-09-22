using UnityEditor;
using UnityEngine;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.ClickDebug;

namespace UIPilot.Editor
{
    // The Click Debugger section: the last clicks in Play mode, what took each one
    // and why. Watching happens in UIPilotClickMonitor, whether this is open or not.
    public sealed partial class UIPilotWindow
    {
        private const int EarlierDetailLines = 1;

        private bool _clickFoldout = true;

        private static readonly GUIContent ContentClickWatch  = new GUIContent(UIPilotLabels.ClickDebug.Watch,        UIPilotLabels.ClickDebug.WatchTip);
        private static readonly GUIContent ContentClickLog    = new GUIContent(UIPilotLabels.ClickDebug.LogToConsole, UIPilotLabels.ClickDebug.LogToConsoleTip);
        private static readonly GUIContent ContentClickSelect = new GUIContent(UIPilotLabels.ClickDebug.SelectTarget, UIPilotLabels.ClickDebug.SelectTargetTip);

        private void DrawClickSection()
        {
            if (!DrawSectionFoldout(ref _clickFoldout, UIPilotLabels.ClickDebug.Section,
                    UIPilotLabels.ClickDebug.EditorPrefsOpen))
                return;

            using (new EditorGUILayout.VerticalScope(UIPilotStyles.Card))
            {
                var clicks = UIPilotClickMonitor.Clicks;
                if (clicks.Count == 0)
                    DrawClickStatus();
                else
                    DrawClicks();

                EditorGUILayout.Space(8f);
                DrawClickSettings();
            }
        }

        // Before the first click: what to do to get one.
        private static void DrawClickStatus()
        {
            string label, detail;
            if (!EditorApplication.isPlaying)
            {
                label  = UIPilotLabels.ClickDebug.Idle;
                detail = UIPilotLabels.ClickDebug.IdleDetail;
            }
            else if (UIPilotClickMonitor.Watching)
            {
                label  = UIPilotLabels.ClickDebug.Watching;
                detail = UIPilotLabels.ClickDebug.WatchingDetail;
            }
            else
            {
                label  = UIPilotLabels.ClickDebug.Off;
                detail = UIPilotLabels.ClickDebug.OffDetail;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLamp(UIPilotStyles.LampIdle);
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Label(label, EditorStyles.boldLabel);
                    GUILayout.Label(detail, UIPilotStyles.RowDetail);
                }
            }
        }

        // The last click in full, the ones before it in a line each.
        private static void DrawClicks()
        {
            var clicks = UIPilotClickMonitor.Clicks;
            GUILayout.Label(UIPilotLabels.ClickDebug.LastClick, UIPilotStyles.Description);
            DrawClickRow(clicks[0], int.MaxValue, true);

            if (clicks.Count > 1)
            {
                EditorGUILayout.Space(8f);
                GUILayout.Label(UIPilotLabels.ClickDebug.Earlier, UIPilotStyles.Description);
                for (var i = 1; i < clicks.Count; i++)
                {
                    EditorGUILayout.Space(4f);
                    DrawClickRow(clicks[i], EarlierDetailLines, false);
                }
            }

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.Space(4f);
                GUILayout.Label(UIPilotLabels.ClickDebug.AfterPlay, UIPilotStyles.Description);
            }
        }

        private static void DrawClickRow(ClickReport report, int detailLines, bool showHitOrder)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLamp(ClickLamp(report.Verdict));

                using (new EditorGUILayout.VerticalScope())
                {
                    DrawClickLabel(report);
                    for (var i = 0; i < report.Details.Length && i < detailLines; i++)
                        GUILayout.Label(report.Details[i], UIPilotStyles.RowDetail);
                    if (showHitOrder && report.HitOrder != null)
                        GUILayout.Label(report.HitOrder, UIPilotStyles.RowDetail);
                }

                GUILayout.Label(ClickDebugModule.Word(report.Verdict),
                    UIPilotStyles.StatusWord, UIPilotStyles.StatusWordSize);
            }
        }

        // A link to what took the click while it still exists; after Play mode,
        // objects the game created are gone and the name is plain text.
        private static void DrawClickLabel(ClickReport report)
        {
            if (report.Targets.Length == 0 || report.Targets[0] == null)
            {
                GUILayout.Label(report.Headline, UIPilotStyles.RowLabel);
                return;
            }

            var content = new GUIContent(report.Headline, UIPilotLabels.Health.RowTooltip);
            var clicked = GUILayout.Button(content, UIPilotStyles.RowLink);
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            if (!clicked) return;

            Selection.objects = report.Targets;
            EditorGUIUtility.PingObject(report.Targets[0]);
        }

        private static Color ClickLamp(ClickVerdict verdict)
        {
            switch (verdict)
            {
                case ClickVerdict.Handled:         return UIPilotStyles.LampOk;
                case ClickVerdict.NoListeners:
                case ClickVerdict.NotInteractable: return UIPilotStyles.LampCaution;
                case ClickVerdict.Missed:          return UIPilotStyles.LampIdle;
                default:                           return UIPilotStyles.LampFault;
            }
        }

        private static void DrawClickSettings()
        {
            var watch = ClickDebugSettings.Watch;
            if (EditorGUILayout.ToggleLeft(ContentClickWatch, watch) != watch)
            {
                ClickDebugSettings.Watch = !watch;
                UIPilotClickMonitor.Refresh();
            }

            var log = ClickDebugSettings.LogToConsole;
            if (EditorGUILayout.ToggleLeft(ContentClickLog, log) != log)
                ClickDebugSettings.LogToConsole = !log;

            var select = ClickDebugSettings.SelectTarget;
            if (EditorGUILayout.ToggleLeft(ContentClickSelect, select) != select)
                ClickDebugSettings.SelectTarget = !select;

            GUILayout.Label(UIPilotLabels.Health.SettingsNote, UIPilotStyles.Description);
        }
    }
}
