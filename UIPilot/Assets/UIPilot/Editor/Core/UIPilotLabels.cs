namespace UIPilot.Editor.Core
{
    internal static class UIPilotLabels
    {
        internal static class Menu
        {
            internal const string WindowPath = "Tools/UIPilot";
        }

        internal static class Window
        {
            internal const string Title         = "UIPilot";
            internal const string WorkflowGuide =
                "① Generate a menu\n" +
                "② Scan Project to find your scripts\n" +
                "③ Load Buttons & Actions in Wire, then assign methods\n" +
                "④ Validate to check your scene";
        }

        internal static class Sections
        {
            internal const string Generate    = "Generate";
            internal const string Discover    = "Discover";
            internal const string Wire        = "Wire";
            internal const string Validate    = "Validate";
            internal const string Placeholder = "— not yet implemented —";
        }

        internal static class Generate
        {
            internal const string MenuTypeLabel  = "Menu Type";
            internal const string ButtonLabel    = "Generate UI";
            internal const string ClearMainMenu  = "Clear Main Menu";
            internal const string ClearPauseMenu = "Clear Pause Menu";
            internal const string ClearSettings  = "Clear Settings";

            internal const string TooltipGenerate    =
                "Generates the selected menu type in your scene inside a single UIPilot_Canvas";
            internal const string TooltipClearPanel  =
                "Removes only this menu panel from the scene. Undo is supported.";
        }

        internal static class Discover
        {
            internal const string ScanButton = "Scan Project";
            internal const string EmptyList  = "No public actions found.";

            internal const string TooltipScan =
                "Scans all MonoBehaviour scripts in your project for public methods with no parameters";
        }

        internal static class Wire
        {
            internal const string RefreshButton  = "Load Buttons & Actions";
            internal const string ApplyButton    = "Apply Bindings";
            internal const string NoneOption     = "None";
            internal const string NoButtons      = "No UIPilot buttons found.";
            internal const string NoActions      = "No actions found. Add scripts with public methods.";
            internal const string AssignHint     = "Assign a method to each button, then click Apply Bindings";

            internal const string TooltipRefresh =
                "Loads all UIPilot buttons from the scene and available actions from your scripts";
            internal const string TooltipApply   =
                "Wires each button to its selected method. Check button OnClick in Inspector to verify.";
            internal const string HelpRefreshNeeded =
                "Click Load Buttons & Actions to load your UIPilot buttons and available script methods";
            internal const string HelpNoActions  =
                "No public methods found. Add a MonoBehaviour script with public void methods to a GameObject in your scene, then Refresh";
        }

        internal static class Validate
        {
            internal const string RunButton   = "Run Validation";
            internal const string FixButton   = "Fix";
            internal const string AllClear    = "✓ No errors or warnings found.";

            internal const string TooltipValidate =
                "Checks your scene for common UI setup mistakes";
            internal const string TooltipFix =
                "Automatically fix this issue";
        }

        internal static class QuickBuild
        {
            internal const string SectionLabel       = "Quick Build";
            internal const string ManualSectionLabel = "Manual";
            internal const string BuildButton        = "Build UI";
            internal const string MainMenuToggle     = "Main Menu";
            internal const string PauseMenuToggle    = "Pause Menu";
            internal const string SettingsMenuToggle = "Settings Menu";

            internal const string TooltipBuild =
                "Generates selected menus, creates a GameManager script, and wires all buttons automatically";

            internal const string ConsoleStart =
                "UIPilot: Building your UI — please wait...";
            internal const string ConsoleComplete =
                "UIPilot: Setup complete. Your buttons are wired and ready.";
            internal const string WarnTypeNotResolved =
                "UIPilot: UIPilot_GameManager type not resolved yet — Unity may still be compiling. Try again shortly.";
            internal const string WarnGameObjectNotFound =
                "UIPilot: UIPilot_GameManager GameObject not found in scene.";
        }
    }
}
