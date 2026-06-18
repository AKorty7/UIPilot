namespace UIPilot.Editor.Modules.SceneAudit
{
    internal static class SceneAuditContent
    {
        internal static class UI
        {
            internal const string SectionHeader = "Full Scene";
            internal const string ScanButton    = "Scan Scene";
        }

        internal static class Labels
        {
            internal const string Canvas          = "Canvas";
            internal const string EventSystem     = "EventSystem";
            internal const string MainMenuPanel   = "Main Menu Panel";
            internal const string PauseMenuPanel  = "Pause Menu Panel";
            internal const string SettingsPanel   = "Settings Panel";
            internal const string GameManagerGO   = "GameManager (Scene)";
            internal const string GameManagerScript = "GameManager (Script)";
            internal const string ButtonListeners = "Button Listeners";
        }

        internal static class Details
        {
            internal const string CanvasMissing          = "UIPilot_Canvas not found in scene.";
            internal const string CanvasScalerMisconfigured =
                "CanvasScaler is present but not set to Scale With Screen Size (1920×1080).";
            internal const string EventSystemMissing     = "No EventSystem found in scene.";
            internal const string PanelMissing           = "Panel not found in scene.";
            internal const string PanelBroken            = "Panel exists but is missing button: {0}";
            internal const string GameManagerGOMissing   = "UIPilot_GameManager GameObject not found in scene.";
            internal const string GameManagerScriptMissing =
                "Assets/UIPilot_GameManager.cs not found on disk.";
            internal const string ListenerNotPersistent  =
                "{0} has no persistent onClick listener assigned.";
        }
    }
}
