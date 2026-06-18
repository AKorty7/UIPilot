namespace UIPilot.Editor.Modules.SceneAudit
{
    internal sealed class SceneAuditResult
    {
        internal string            Label    { get; }
        internal string            Detail   { get; }
        internal SceneAuditSeverity Severity { get; }

        internal SceneAuditResult(string label, string detail, SceneAuditSeverity severity)
        {
            Label    = label;
            Detail   = detail;
            Severity = severity;
        }
    }
}
