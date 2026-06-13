using System;

namespace UIPilot.Editor.Modules.Validation
{
    internal sealed class ValidationResult
    {
        internal string             Message  { get; }
        internal ValidationSeverity Severity { get; }
        internal Action             AutoFix  { get; }

        internal ValidationResult(string message, ValidationSeverity severity, Action autoFix = null)
        {
            Message  = message;
            Severity = severity;
            AutoFix  = autoFix;
        }
    }
}
