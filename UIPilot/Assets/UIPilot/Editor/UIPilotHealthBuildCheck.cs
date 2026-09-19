using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIPilot.Editor.Modules.Health;

namespace UIPilot.Editor
{
    // Runs UI Health on every scene a build includes, whether or not it is open,
    // and warns in the Console. It never fails the build and never changes a scene.
    public sealed class UIPilotHealthBuildCheck : IProcessSceneWithReport
    {
        private const int ConsoleLines = 8;

        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            // Unity also calls this for each scene entering Play mode, with no report.
            // Play mode has its own warning, from UIPilotHealthMonitor.
            if (report == null || !HealthSettings.WarnOnBuild) return;

            var issues = HealthModule.CheckSceneForBuild(scene);
            if (issues.Count == 0) return;

            Debug.LogWarning(string.Format(HealthContent.Console.BuildSummary,
                issues.Count, scene.name, HealthModule.Describe(issues, ConsoleLines)));
        }
    }
}
