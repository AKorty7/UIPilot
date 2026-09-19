using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.Health;

namespace UIPilot.Editor
{
    // The UI Health lamp on Unity's main toolbar: always in view, so problems are
    // seen without opening anything. Green and quiet when all is clear; amber or
    // red with a count otherwise. Clicking it opens the UI Health section.
    [InitializeOnLoad]
    internal static class UIPilotToolbarLamp
    {
        static UIPilotToolbarLamp()
        {
            UIPilotHealthMonitor.Changed += () => MainToolbar.Refresh(UIPilotLabels.Health.ToolbarPath);
        }

        [MainToolbarElement(UIPilotLabels.Health.ToolbarPath, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement Create()
        {
            return new MainToolbarButton(Content(), UIPilotWindow.OpenHealth);
        }

        private static MainToolbarContent Content()
        {
            if (!UIPilotHealthMonitor.HasResults)
                return new MainToolbarContent(UIPilotLabels.Health.ToolbarText,
                    UIPilotStyles.LampIcon(UIPilotStyles.LampIdle), UIPilotLabels.Health.ToolbarIdle);

            var issues = UIPilotHealthMonitor.Issues;
            if (issues.Count == 0)
                return new MainToolbarContent(UIPilotLabels.Health.ToolbarText,
                    UIPilotStyles.LampIcon(UIPilotStyles.LampOk), UIPilotLabels.Health.ToolbarOk);

            var lamp = UIPilotHealthMonitor.WorstSeverity() == HealthSeverity.Broken
                ? UIPilotStyles.LampFault
                : UIPilotStyles.LampCaution;

            return new MainToolbarContent(
                string.Format(UIPilotLabels.Health.ToolbarCount, issues.Count),
                UIPilotStyles.LampIcon(lamp),
                string.Format(UIPilotLabels.Health.ToolbarIssues, UIPilotWindow.IssueCount(issues.Count)));
        }
    }
}
