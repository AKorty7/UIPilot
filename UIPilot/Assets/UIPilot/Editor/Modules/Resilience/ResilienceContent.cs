namespace UIPilot.Editor.Modules.Resilience
{
    internal static class ResilienceContent
    {
        internal const string UndoLabel = "UIPilot Repair";

        internal static class Labels
        {
            internal const string Canvas             = "Canvas";
            internal const string CanvasScaler       = "Canvas Scaler";
            internal const string EventSystem        = "EventSystem";
            internal const string MainMenuPanel      = "Main Menu Panel";
            internal const string PauseMenuPanel     = "Pause Menu Panel";
            internal const string SettingsPanel      = "Settings Panel";
            internal const string GameManagerGO      = "GameManager (Scene)";
            internal const string GameManagerScript  = "GameManager (Script)";
            internal const string ButtonListeners    = "Button Listeners";
        }

        internal static class Results
        {
            internal const string Skipped           = "Skipped — no issue reported";
            internal const string CanvasRepaired    = "Canvas created and configured";
            internal const string CanvasScalerFixed = "CanvasScaler set to ScaleWithScreenSize 1920×1080";
            internal const string EventSystemFixed  = "EventSystem created";
            internal const string PanelRegenerated  = "Panel regenerated via UIGeneratorModule";
            internal const string GameManagerFixed  = "GameManager script and GameObject created";
            internal const string ListenersFixed    = "Button listeners re-applied";
            internal const string UnknownLabel      = "Unknown audit label — no repair dispatched";
            internal const string ValidationSuffix  = " — post-repair validation: {0} issue(s) remain";
        }

        internal static class Console
        {
            internal const string RepairStart    = "UIPilot Resilience: starting repair pass…";
            internal const string RepairComplete = "UIPilot Resilience: repair complete — {0} item(s) attempted, {1} succeeded.";
            internal const string ValidationLog  = "UIPilot Resilience: post-repair validation — {0} issue(s) remain.";
        }
    }
}
