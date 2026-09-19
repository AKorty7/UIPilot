using UnityEditor;
using UnityEngine;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.Health;

namespace UIPilot.Editor
{
    // A lamp at the right of each Hierarchy row that has a UI Health issue, so the
    // problem is marked where the developer is already looking. A parent whose
    // children have issues gets a smaller lamp, so collapsed rows still show it.
    [InitializeOnLoad]
    internal static class UIPilotHierarchyLamps
    {
        private const float RightInset = 30f; // clear of the prefab arrow at the row's right edge
        private const float ChildLamp  = 4f;

        static UIPilotHierarchyLamps()
        {
            EditorApplication.hierarchyWindowItemOnGUI += DrawRow;
            UIPilotHealthMonitor.Changed               += EditorApplication.RepaintHierarchyWindow;
        }

        private static void DrawRow(int instanceId, Rect row)
        {
            var own = UIPilotHealthMonitor.IssueOn(instanceId);
            if (own != null)
            {
                DrawLamp(row, own, UIPilotStyles.LampSize);
                return;
            }

            var below = UIPilotHealthMonitor.IssueBelow(instanceId);
            if (below != null)
                DrawLamp(row, below, ChildLamp);
        }

        private static void DrawLamp(Rect row, HealthIssue issue, float size)
        {
            var lamp = new Rect(row.xMax - RightInset + (UIPilotStyles.LampSize - size) * 0.5f,
                row.y + (row.height - size) * 0.5f, size, size);

            EditorGUI.DrawRect(lamp, issue.Severity == HealthSeverity.Broken
                ? UIPilotStyles.LampFault
                : UIPilotStyles.LampCaution);

            // The detail as a tooltip, on a slightly larger area than the lamp.
            GUI.Label(new Rect(lamp.x - 3f, row.y, size + 6f, row.height),
                new GUIContent(string.Empty, string.Format(UIPilotLabels.Health.LampTooltip, issue.Label, issue.Detail)));
        }
    }
}
