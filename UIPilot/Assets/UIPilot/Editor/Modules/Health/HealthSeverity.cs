namespace UIPilot.Editor.Modules.Health
{
    internal enum HealthSeverity
    {
        Warning,   // may be intended, or only wrong in the state the scene was saved in
        Broken     // fails in Play mode, every time
    }
}
