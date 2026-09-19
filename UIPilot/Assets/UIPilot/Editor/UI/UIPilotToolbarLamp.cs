using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Overlays;
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

        // ── Showing the lamp ─────────────────────────────────────────────────
        // Unity 6.3 hides toolbar items that packages add, and has no public way
        // to show one: only MainToolbar.Refresh is public. Each item is an Overlay,
        // and Overlay.displayed is public, but finding ours takes the internal
        // MainToolbar.TryGetOverlay. It is looked up by name and every use is
        // guarded, so if a later Unity changes it, IsShown returns null and the
        // window falls back to telling the developer where to click.
        //
        // This is the only place UIPilot touches Unity internals.
        private static readonly MethodInfo TryGetOverlayMethod = typeof(MainToolbar).GetMethod(
            "TryGetOverlay", BindingFlags.NonPublic | BindingFlags.Static, null,
            new[] { typeof(string), typeof(Overlay).MakeByRefType() }, null);

        // Whether the lamp is on the toolbar; null when that cannot be known from code.
        internal static bool? IsShown()
        {
            var overlay = FindOverlay();
            return overlay == null ? (bool?)null : overlay.displayed;
        }

        // False when the toolbar cannot be reached from code.
        internal static bool SetShown(bool shown)
        {
            var overlay = FindOverlay();
            if (overlay == null) return false;

            overlay.displayed = shown;
            return true;
        }

        private static Overlay FindOverlay()
        {
            if (TryGetOverlayMethod == null) return null;

            // Any failure inside Unity's internal code means "not reachable",
            // never an error in the developer's Console.
            try
            {
                var args = new object[] { UIPilotLabels.Health.ToolbarPath, null };
                return (bool)TryGetOverlayMethod.Invoke(null, args) ? args[1] as Overlay : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // ── Content ──────────────────────────────────────────────────────────

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
