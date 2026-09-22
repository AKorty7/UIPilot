namespace UIPilot.Editor.Core
{
    internal static class UIPilotLabels
    {
        internal static class Menu
        {
            internal const string WindowPath = "Tools/UIPilot";
        }

        internal static class Theme
        {
            internal const string CreateMenuPath = "Assets/Create/UIPilot/Theme";
            internal const string NewAssetFile   = "New UIPilot Theme.asset";
            internal const string AssetFilter    = " t:UIPilotTheme";
            internal const string AssetExtension = ".asset";
            internal const string EditorPrefsKey = "UIPilot_ThemeGuid";

            // The presets UIPilot ships, by asset name, and where Restore puts them.
            internal const string DefaultAsset = "Soft Club";
            internal const string NightAsset   = "Soft Club Night";
            internal const string InkAsset     = "Ink";
            internal const string FantasyAsset = "Fantasy RPG";
            internal const string JrpgAsset    = "JRPG Window";
            internal const string PixelAsset   = "Pixel Retro";
            internal const string SciFiAsset   = "Sci-Fi HUD";
            internal const string PresetFolder = "Assets/UIPilot/Themes";

            internal const string FieldLabel   = "Theme";
            internal const string FieldTooltip =
                "The look Build UI generates. Pick a preset from Assets/UIPilot/Themes, or duplicate one and make it yours. Empty = built-in Soft Club.";
            internal const string ApplyButton  = "Apply Theme to Existing Menus";
            internal const string ApplyTooltip =
                "Restyles the menus already in the scene with the selected theme. Layout, names and button wiring are untouched. Undo is supported.";
            internal const string BuiltInHint  =
                "No theme selected: Build UI uses the built-in Soft Club look.";

            internal const string UndoApply      = "Apply UIPilot Theme";
            internal const string ConsoleApplied = "UIPilot: Theme \"{0}\" applied to {1} menu(s).";
            internal const string ConsoleNoMenus = "UIPilot: No generated menus in the scene to restyle. Click Build UI first.";
            internal const string BuiltInName    = "Soft Club (built-in)";

            internal const string MissingHint    = "Missing: {0}.";
            internal const string ListSeparator  = ", ";
            internal const string RestoreButton  = "Restore Default Themes";
            internal const string RestoreTooltip =
                "Recreates the missing default themes in Assets/UIPilot/Themes, exactly as shipped. Themes that are still there are never changed.";
            internal const string ConsoleRestored = "UIPilot: Restored default theme(s): {0}";
        }

        internal static class Health
        {
            internal const string Section          = "UI Health";
            internal const string CheckNowButton   = "Check Now";
            internal const string CheckNowTooltip  = "Check every UI in the open scenes again. UIPilot also checks by itself after you edit, save, open a scene or press Play.";
            internal const string FixButton        = "Fix";
            internal const string FixTooltip       = "Make the change described, as one step you can undo with Ctrl+Z.";
            internal const string RowTooltip       = "Click to select it in the Hierarchy.";
            internal const string LampTooltip      = "{0}: {1}";

            internal const string NotChecked       = "Not checked yet";
            internal const string NotCheckedDetail = "UI Health runs in Edit mode. Stop Play mode, or click Check Now.";
            internal const string AllClear         = "All clear";
            internal const string AllClearDetail   = "{0} of {1} checks on, across every UI in: {2}. Checked again after edits, on save, on Play and on build.";
            internal const string IssuesSummary    = "{0} to look at in: {1}";
            internal const string IssueCountOne    = "1 issue";
            internal const string IssueCountMany   = "{0} issues";
            internal const string SceneSeparator   = ", ";
            internal const string UntitledScene    = "Untitled";

            internal const string ChecksFoldout    = "Checks";
            internal const string WarnOnPlay       = "Warn in the Console when entering Play mode";
            internal const string WarnOnPlayTip    = "One summary line, only when there is something to look at.";
            internal const string WarnOnBuild      = "Warn in the Console for each scene in a build";
            internal const string WarnOnBuildTip   = "Checks every scene the build includes, even ones not open. The build always continues.";
            internal const string SettingsNote     = "Saved for you in this project only. Teammates keep their own.";

            internal const string EditorPrefsOpen       = "UIPilot_HealthOpen";
            internal const string EditorPrefsChecksOpen = "UIPilot_HealthChecksOpen";
            internal const string EditorPrefsToolbarTip = "UIPilot_HealthToolbarTipDone";

            // Unity hides toolbar items added by packages until they are switched on.
            internal const string ToolbarTip           = "Keep this in view: put the UI Health lamp on Unity's main toolbar, above every window.";
            internal const string ShowOnToolbar        = "Show on Toolbar";
            internal const string ShowOnToolbarTip     = "Adds the UI Health lamp to Unity's main toolbar. Take it off again under Checks, or from the toolbar's right-click menu.";
            internal const string NotNow               = "Not now";
            internal const string NotNowTip            = "Hide this tip. The switch under Checks adds the lamp later.";
            internal const string ToolbarLampToggle    = "Lamp on Unity's main toolbar";
            internal const string ToolbarLampToggleTip = "The UI Health lamp above every window: green when all is clear, amber or red with a count.";

            // When the toolbar cannot be reached from code (a later Unity changed it).
            internal const string ToolbarTipManual  = "Keep this in view: right-click an empty part of Unity's main toolbar (or click its ⋮ menu) and tick UIPilot > UI Health. A lamp then shows the result above every window.";
            internal const string ToolbarTipDismiss = "Got it";

            // Main toolbar
            internal const string ToolbarPath      = "UIPilot/UI Health";
            internal const string ToolbarText      = "UI";
            internal const string ToolbarCount     = "UI  {0}";
            internal const string ToolbarOk        = "UIPilot UI Health: all clear.";
            internal const string ToolbarIssues    = "UIPilot UI Health: {0}. Click to see them.";
            internal const string ToolbarIdle      = "UIPilot UI Health checks the open scenes in Edit mode.";
        }

        internal static class ClickDebug
        {
            internal const string Section         = "Click Debugger";
            internal const string Idle            = "Ready for Play mode";
            internal const string IdleDetail      = "Press Play, then click anywhere in the Game view. Each click shows here: what took it, and why.";
            internal const string Watching        = "Watching clicks";
            internal const string WatchingDetail  = "Click anywhere in the Game view.";
            internal const string Off             = "Not watching";
            internal const string OffDetail       = "Turn on Watch clicks in Play mode, below.";
            internal const string LastClick       = "Last click";
            internal const string Earlier         = "Earlier";
            internal const string AfterPlay       = "From the last Play session. Objects the game created are gone now.";

            internal const string Watch           = "Watch clicks in Play mode";
            internal const string WatchTip        = "Looks at a click only when the mouse button goes down in Play mode. Costs nothing otherwise.";
            internal const string LogToConsole    = "Log each click to the Console";
            internal const string LogToConsoleTip = "One line per click, and clicking the line selects the object. Works with this window closed.";
            internal const string SelectTarget    = "Select what took each click";
            internal const string SelectTargetTip = "Selects it in the Hierarchy and Inspector as you play.";

            internal const string EditorPrefsOpen = "UIPilot_ClickDebugOpen";
        }

        internal static class EventSystem
        {
            internal const string ObjectName = "EventSystem";
            internal const string InputSystemModuleType =
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem";
        }

        internal static class Window
        {
            internal const string Title         = "UIPilot";
            internal const string WorkflowGuide =
                "① Generate a menu\n" +
                "② Scan Project to find your scripts\n" +
                "③ Load Buttons & Actions in Wire, then assign methods\n" +
                "④ Validate to check your scene";

            internal const string HelperText =
                "UI Health checks every UI in your open scenes as you work. To make menus, tick them under Build and click Build UI.";
            internal const string HelpToggle       = "?";
            internal const string CollapseAllButton = "−";
            internal const string ExpandAllButton   = "+";

            internal const string TooltipHelp     = "Show or hide the short description under the title";
            internal const string TooltipCollapse = "Collapse every section";
            internal const string TooltipExpand   = "Restore the sections you had open";

            internal const string EditorPrefsHelperText = "UIPilot_ShowHelperText";
            internal const string EditorPrefsBuildOpen  = "UIPilot_BuildOpen";
            internal const string EditorPrefsScanOpen   = "UIPilot_ScanOpen";
            internal const string EditorPrefsManualOpen = "UIPilot_ManualOpen";
        }

        // The word shown beside every status lamp, so a row's state never
        // depends on telling green from amber from red.
        internal static class Status
        {
            internal const string Ok      = "OK";
            internal const string Warning = "Warning";
            internal const string Missing = "Missing";
            internal const string Broken  = "Broken";
            internal const string Error   = "Error";
            internal const string Fixed   = "Fixed";
            internal const string Failed  = "Failed";
            internal const string Working = "Working";
            internal const string Ready   = "Ready";
            internal const string Stalled = "Stalled";
        }

        internal static class Sections
        {
            internal const string Build       = "Build";
            internal const string ScanRepair  = "Scan & Repair";
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
            internal const string ClearRowLabel  = "Remove one panel from the scene:";
            internal const string ClearMainMenu  = "Main Menu";
            internal const string ClearPauseMenu = "Pause Menu";
            internal const string ClearSettings  = "Settings";

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

        internal static class Manual
        {
            internal const string GenerateDesc =
                "Manually create or remove individual menu panels in the scene.";
            internal const string DiscoverDesc =
                "Scans your project for public void methods you can assign to buttons.";
            internal const string WireDesc =
                "Assign a script method to each button's click event.";
            internal const string WireGuidance =
                "Select methods from UIPilot_GameManager unless you are using a custom script.";
            internal const string ApplyBindingsNote =
                "Quick Build wires buttons automatically. Use this only if you have changed assignments manually.";
        }

        internal static class Validate
        {
            internal const string RunButton   = "Run Validation";
            internal const string FixButton   = "Fix";
            internal const string AllClear    = "No errors or warnings found.";
            internal const string EmptyHint   =
                "Run Validation to check the EventSystem, CanvasScaler and every button's click listener.";

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
            internal const string ConsoleSkipped =
                "UIPilot: All selected menus already intact — nothing to regenerate.";

            internal const string ConsoleWaitingForCompile =
                "UIPilot: Waiting for Unity to compile UIPilot_GameManager.cs — buttons will be wired automatically when it finishes.";

            // SessionState key: a Quick Build is waiting on a domain reload to finish wiring.
            internal const string SessionPendingWire = "UIPilot_PendingQuickBuildWire";
            // SessionState key: editor time at which that wait began.
            internal const string SessionPendingSince = "UIPilot_PendingQuickBuildSince";
            // SessionState key: the last Quick Build finished wiring (survives the reload).
            internal const string SessionBuildDone   = "UIPilot_QuickBuildDone";

            // Shown inside the Build card, so the outcome is visible without the Console.
            internal const string StatusWaiting =
                "Unity is compiling the new script. The buttons are wired automatically when it finishes.";
            internal const string StatusStalled =
                "Unity stopped compiling before the script was ready. Fix any Console compile errors, then click Build UI again.";
            internal const string StatusDone =
                "Menus built and wired. Press Play.";

            internal const string WarnTypeNotResolved =
                "UIPilot: UIPilot_GameManager type could not be resolved after compiling — fix any Console compile errors, then click Build UI again.";
            internal const string WarnGameObjectNotFound =
                "UIPilot: UIPilot_GameManager GameObject not found in scene.";

            internal const string ClearButton     = "Quick Clear";
            internal const string TooltipClear    = "Removes UIPilot canvas and GameManager from the scene. Leaves your scripts intact.";
            internal const string DialogTitle     = "Quick Clear";
            internal const string DialogMessage   = "This will remove UIPilot_Canvas and UIPilot_GameManager from the scene. Your UIPilot_GameManager.cs script will not be deleted. Continue?";
            internal const string DialogConfirm   = "Clear Scene";
            internal const string DialogCancel    = "Cancel";
            internal const string ConsoleCleared  = "UIPilot: Scene cleared. UIPilot_GameManager.cs left intact — delete manually if needed.";
        }

        internal static class SceneAudit
        {
            internal const string ConsoleSummary =
                "UIPilot: Scan complete — {0} items checked, {1} issues found.";
            internal const string EmptyHint =
                "Scan Scene checks every object UIPilot generated and lists anything missing or broken.";
            internal const string SummaryIssues   = "{0} checked, {1} need attention";
            internal const string SummaryAllClear = "{0} checked, all clear";
            internal const string TooltipScanScene =
                "Audit every UIPilot object in the scene and report missing or broken components";
        }

        internal static class Resilience
        {
            internal const string RepairSceneButton  = "Repair Scene";
            internal const string RepairSceneTooltip = "Fix all issues found by the last scan";
            internal const string RepairResultsHeader = "Repair Results";
        }
    }
}
