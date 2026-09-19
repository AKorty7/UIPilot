using System;
using Object = UnityEngine.Object;

namespace UIPilot.Editor.Modules.Health
{
    internal sealed class HealthIssue
    {
        internal HealthCheck    Check    { get; }
        internal HealthSeverity Severity { get; }
        internal string         Label    { get; }
        internal string         Detail   { get; }

        // What the row selects in the Hierarchy. Empty when there is nothing
        // to point at, for example a missing EventSystem.
        internal Object[]       Targets  { get; }

        // Null when there is no fix safe to make without the developer: the row
        // then only selects its targets.
        internal Action         Fix      { get; }

        internal HealthIssue(HealthCheck check, HealthSeverity severity, string label, string detail,
            Object[] targets, Action fix = null)
        {
            Check    = check;
            Severity = severity;
            Label    = label;
            Detail   = detail;
            Targets  = targets ?? Array.Empty<Object>();
            Fix      = fix;
        }
    }
}
