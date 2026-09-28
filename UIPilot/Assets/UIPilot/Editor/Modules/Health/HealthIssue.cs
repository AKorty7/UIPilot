using System;
using UnityEngine;
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

        // What the Fix button says when a plain "Fix" would not tell the developer
        // what happens. Null for the plain one.
        internal GUIContent     FixButton { get; }

        // Deletes the object at fault. Offered only when that is certainly safe
        // (a leftover nothing uses), beside the Fix, never instead of it.
        internal Action         Remove   { get; }

        internal HealthIssue(HealthCheck check, HealthSeverity severity, string label, string detail,
            Object[] targets, Action fix = null, GUIContent fixButton = null, Action remove = null)
        {
            Check     = check;
            Severity  = severity;
            Label     = label;
            Detail    = detail;
            Targets   = targets ?? Array.Empty<Object>();
            Fix       = fix;
            FixButton = fixButton;
            Remove    = remove;
        }
    }
}
