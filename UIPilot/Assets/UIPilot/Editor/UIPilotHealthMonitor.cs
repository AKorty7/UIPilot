using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UIPilot.Editor.Modules.Health;

namespace UIPilot.Editor
{
    // The coordinator that keeps UI Health current while the UIPilot window is
    // closed. It runs the checks when the scene settles after an edit, on save,
    // when a scene opens, and just before Play, and tells the toolbar lamp, the
    // Hierarchy lamps and the window. It never changes a scene.
    //
    // This is the one place outside UIPilotWindow allowed to call a module.
    [InitializeOnLoad]
    internal static class UIPilotHealthMonitor
    {
        // Edits come in bursts (a drag, a paste): wait for a pause before checking.
        private const double SettleSeconds = 0.75;
        private const int    ConsoleLines  = 8;

        internal static event Action Changed;

        internal static IReadOnlyList<HealthIssue> Issues => _issues;

        // False in Play mode and before the first check of a session.
        internal static bool HasResults { get; private set; }

        private static List<HealthIssue> _issues = new List<HealthIssue>();
        // Keyed by GameObject instance ID, which is what the Hierarchy hands its row callback.
        private static readonly Dictionary<int, HealthIssue> OwnIssue   = new Dictionary<int, HealthIssue>();
        private static readonly Dictionary<int, HealthIssue> ChildIssue = new Dictionary<int, HealthIssue>();
        private static double _checkAt;
        private static bool   _leavingEditMode; // Play mode is starting, but the scene is still the edit-mode one

        static UIPilotHealthMonitor()
        {
            EditorApplication.hierarchyChanged     += Schedule;
            ObjectChangeEvents.changesPublished    += OnChangesPublished;
            EditorSceneManager.sceneOpened         += (scene, mode) => Schedule();
            EditorSceneManager.sceneClosed         += scene => Schedule();
            EditorSceneManager.sceneSaved          += scene => Schedule();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            Schedule();
        }

        // ── Queries ──────────────────────────────────────────────────────────

        // The worst issue on this object itself, if any.
        internal static HealthIssue IssueOn(int instanceId)
        {
            return OwnIssue.TryGetValue(instanceId, out var issue) ? issue : null;
        }

        // The worst issue somewhere below this object, for a collapsed Hierarchy row.
        internal static HealthIssue IssueBelow(int instanceId)
        {
            return ChildIssue.TryGetValue(instanceId, out var issue) ? issue : null;
        }

        internal static HealthSeverity WorstSeverity()
        {
            foreach (var issue in _issues)
                if (issue.Severity == HealthSeverity.Broken) return HealthSeverity.Broken;
            return HealthSeverity.Warning;
        }

        // ── Running ──────────────────────────────────────────────────────────

        internal static void CheckNow()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !_leavingEditMode) return;

            _issues    = HealthModule.CheckOpenScenes();
            HasResults = true;
            IndexByObject();
            Changed?.Invoke();
        }

        // Fixes go through here so every view updates the moment one is applied.
        internal static void Fix(HealthIssue issue)
        {
            HealthModule.RunFix(issue);
            CheckNow();
        }

        private static void Schedule()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            _checkAt = EditorApplication.timeSinceStartup + SettleSeconds;
            EditorApplication.update -= CheckWhenSettled;
            EditorApplication.update += CheckWhenSettled;
        }

        private static void CheckWhenSettled()
        {
            if (EditorApplication.timeSinceStartup < _checkAt) return;

            EditorApplication.update -= CheckWhenSettled;
            CheckNow();
        }

        private static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            Schedule();
        }

        // Checked once more on the way into Play mode, so the Console warning
        // describes exactly the scene that is about to run.
        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.ExitingEditMode:
                    _leavingEditMode = true;
                    CheckNow();
                    _leavingEditMode = false;
                    WarnBeforePlay();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    HasResults = false;
                    Changed?.Invoke();
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    Schedule();
                    break;
            }
        }

        // One summary, not one line per issue: a Console that fills up on every
        // Play gets ignored, and then the checks are worth nothing.
        private static void WarnBeforePlay()
        {
            if (_issues.Count == 0 || !HealthSettings.WarnOnPlay) return;

            Debug.LogWarning(string.Format(HealthContent.Console.PlaySummary,
                _issues.Count, HealthModule.Describe(_issues, ConsoleLines)));
        }

        // ── Index for the Hierarchy ──────────────────────────────────────────

        private static void IndexByObject()
        {
            OwnIssue.Clear();
            ChildIssue.Clear();

            foreach (var issue in _issues)
                foreach (var target in issue.Targets)
                    Index(target as GameObject, issue);
        }

        private static void Index(GameObject go, HealthIssue issue)
        {
            if (go == null) return;

            Keep(OwnIssue, go.GetInstanceID(), issue);
            for (var parent = go.transform.parent; parent != null; parent = parent.parent)
                Keep(ChildIssue, parent.gameObject.GetInstanceID(), issue);
        }

        // Keeps the worse of the two, so a red lamp is never hidden by an amber one.
        private static void Keep(Dictionary<int, HealthIssue> index, int instanceId, HealthIssue issue)
        {
            if (!index.TryGetValue(instanceId, out var existing) || issue.Severity > existing.Severity)
                index[instanceId] = issue;
        }
    }
}
