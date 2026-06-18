namespace UIPilot.Editor.Modules.Validation
{
    internal static class ValidationContent
    {
        internal static class GameObjects
        {
            internal const string EventSystem = "EventSystem";
        }

        internal static class Messages
        {
            internal const string NoEventSystem    = "No EventSystem found in scene.";
            internal const string EventSystemOk    = "EventSystem is present.";
            internal const string NoCanvas         = "No UIPilot_Canvas found. Generate a menu first.";
            internal const string NoCanvasScaler   = "UIPilot_Canvas is missing a CanvasScaler component.";
            internal const string WrongScaleMode   = "CanvasScaler is not set to Scale With Screen Size.";
            internal const string CanvasScalerOk   = "CanvasScaler is correctly configured.";
            internal const string ButtonNoListener = "{0} has no method assigned.";
            internal const string ButtonRuntimeOnly = "{0} has a Runtime Only listener. Re-apply bindings via Wire.";
            internal const string AllButtonsWired  = "All buttons have persistent listeners.";
        }

        internal static class Undo
        {
            internal const string AutoFix = "UIPilot AutoFix";
        }
    }
}
