namespace UIPilot.Editor.Modules.ActionDiscovery
{
    internal sealed class DiscoveredAction
    {
        private const string LabelSeparator = "/";

        internal string ClassName  { get; }
        internal string MethodName { get; }
        internal string FullLabel  { get; }

        internal DiscoveredAction(string className, string methodName)
        {
            ClassName  = className;
            MethodName = methodName;
            FullLabel  = className + LabelSeparator + methodName;
        }
    }
}
