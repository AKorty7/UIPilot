using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIPilot.Editor.Modules.Health
{
    // UI Health: checks ANY UI in the scene, not only what UIPilot built, for the
    // mistakes that make a control silently fail. Checking never changes a scene;
    // a fix runs only when the developer clicks it, as one undoable step.
    internal static class HealthModule
    {
        internal static readonly HealthCheck[] AllChecks =
            (HealthCheck[])Enum.GetValues(typeof(HealthCheck));

        private static readonly Dictionary<HealthCheck, Action<HealthScope, List<HealthIssue>>> Runners =
            new Dictionary<HealthCheck, Action<HealthScope, List<HealthIssue>>>
            {
                { HealthCheck.ClickEvents,       HealthEventChecks.CheckClickEvents },
                { HealthCheck.EventSystem,       HealthEventChecks.CheckEventSystems },
                { HealthCheck.Raycasters,        HealthRaycastChecks.CheckRaycasters },
                { HealthCheck.BlockedButtons,    HealthRaycastChecks.CheckBlockedButtons },
                { HealthCheck.GamepadNavigation, HealthHandheldChecks.CheckGamepadNavigation },
                { HealthCheck.TextSize,          HealthHandheldChecks.CheckTextSize },
            };

        // ── Entry points ─────────────────────────────────────────────────────

        // Every loaded scene, together: what Play mode starts with.
        internal static List<HealthIssue> CheckOpenScenes()
        {
            var scenes = new List<Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
                scenes.Add(SceneManager.GetSceneAt(i));

            return Check(scenes, includeEventSystem: true);
        }

        // One scene as a build processes it. The EventSystem check is left out:
        // in a build, another scene or a prefab often provides it at runtime.
        internal static List<HealthIssue> CheckSceneForBuild(Scene scene)
        {
            return Check(new[] { scene }, includeEventSystem: false);
        }

        internal static void RunFix(HealthIssue issue)
        {
            if (issue.Fix == null) return;

            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(HealthContent.Undo.Fix);

            issue.Fix();

            Undo.CollapseUndoOperations(group);
            MarkScenesDirty(issue);
        }

        // "- Play: On Click calls Hud.Resume, which no longer exists." one per line,
        // for the Console. Past the limit, a count of the rest.
        internal static string Describe(IList<HealthIssue> issues, int limit)
        {
            var lines = new List<string>();
            for (var i = 0; i < issues.Count && i < limit; i++)
                lines.Add(string.Format(HealthContent.Console.IssueLine, issues[i].Label, issues[i].Detail));

            if (issues.Count > limit)
                lines.Add(string.Format(HealthContent.Console.MoreLines, issues.Count - limit));

            return string.Join("\n", lines);
        }

        // ── Internals ────────────────────────────────────────────────────────

        private static List<HealthIssue> Check(IEnumerable<Scene> scenes, bool includeEventSystem)
        {
            var scope  = HealthScope.Gather(scenes);
            var issues = new List<HealthIssue>();

            foreach (var check in AllChecks)
            {
                if (!HealthSettings.IsEnabled(check)) continue;
                if (check == HealthCheck.EventSystem && !includeEventSystem) continue;

                Runners[check](scope, issues);
            }

            // Broken first, then warnings; within each, check order. OrderBy is stable.
            return issues.OrderByDescending(issue => issue.Severity).ToList();
        }

        private static void MarkScenesDirty(HealthIssue issue)
        {
            if (issue.Targets.Length == 0)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                return;
            }

            foreach (var target in issue.Targets)
                if (target is GameObject go && go != null && go.scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(go.scene);
        }
    }
}
