namespace UIPilot.Editor.Modules.Health
{
    internal static class HealthContent
    {
        // Each check's name and what it looks for, shown beside its switch in the window.
        internal static class Checks
        {
            internal const string ClickEvents        = "Click events";
            internal const string ClickEventsTip     = "Buttons, toggles and other controls whose events call a deleted object, or a method that no longer exists.";
            internal const string EventSystem        = "EventSystem";
            internal const string EventSystemTip     = "No EventSystem, more than one, or an input module that does not work with this project's Active Input Handling.";
            internal const string Raycasters         = "Graphic Raycasters";
            internal const string RaycastersTip      = "Canvases that contain buttons but cannot receive clicks. Nested canvases need their own.";
            internal const string BlockedButtons     = "Blocked buttons";
            internal const string BlockedButtonsTip  = "An image or text with Raycast Target on, drawn over a button, that takes its clicks.";
            internal const string Gamepad            = "Gamepad navigation";
            internal const string GamepadTip         = "Controls a gamepad or the arrow keys can never reach, or can never leave.";
            internal const string TextSize           = "Text size (Steam Deck)";
            internal const string TextSizeTip        = "Text under 9 px at 1280 x 800, Valve's minimum for Steam Deck Verified. 12 px is recommended.";
        }

        internal static class Messages
        {
            internal const string ListenerNoTarget      = "{0} entry {1} has no object: it was deleted, or never set. Nothing happens on click.";
            internal const string ListenerNoMethod      = "{0} entry {1} has no function selected.";
            internal const string ListenerMissingMethod = "{0} calls {1}.{2}, which no longer exists. Nothing happens on click.";

            internal const string EventSystemLabel = "EventSystem";
            internal const string NoEventSystem    = "No EventSystem in the open scenes, so no UI can be clicked or navigated. Ignore this if another scene or a prefab adds one at runtime.";
            internal const string ManyEventSystems = "{0} EventSystems are active. Unity uses one of them and warns about the rest.";
            internal const string NoInputModule    = "Has no input module, so nothing receives clicks or key presses.";
            internal const string LegacyModule     = "Uses StandaloneInputModule, which throws an error every frame: this project uses the Input System package only.";

            internal const string NoRaycaster       = "Has buttons but no Graphic Raycaster, so none of them can be clicked or tapped.";
            internal const string NestedNoRaycaster = "Nested canvas with buttons but no Graphic Raycaster of its own, so they cannot be clicked. The parent canvas's raycaster does not reach them.";

            internal const string Blocking = "{0} with Raycast Target on, drawn over {1}, so clicks there hit it instead.";

            internal const string NavigationNone  = "Navigation is None, so a gamepad or the arrow keys can never reach it.";
            internal const string NavigationStuck = "Explicit navigation with no neighbours set, so a gamepad that reaches it is stuck there.";

            internal const string TextTooSmallOne  = "{0:0.#} px tall at 1280 x 800. Steam Deck Verified needs at least 9 px; 12 px is recommended.";
            internal const string TextTooSmallMany = "Under 9 px at 1280 x 800, the Steam Deck Verified minimum. Smallest: {0}, {1:0.#} px. Click to select them all.";
            internal const string TextCountLabel   = "{0} texts";

            internal const string ListSeparator = ", ";
            internal const string ListLastJoin  = " and ";
            internal const string ListMore      = "{0} and {1} more";
        }

        internal static class Undo
        {
            internal const string Fix = "UIPilot UI Health Fix";
        }

        // Per project and per user, in UserSettings/EditorUserSettings.asset.
        internal static class Settings
        {
            internal const string CheckKeyPrefix = "UIPilot.Health.";
            internal const string WarnOnPlayKey  = "UIPilot.Health.WarnOnPlay";
            internal const string WarnOnBuildKey = "UIPilot.Health.WarnOnBuild";
            internal const string On             = "1";
            internal const string Off            = "0";
        }

        internal static class Console
        {
            internal const string PlaySummary  = "UIPilot UI Health: {0} issue(s) in the open scenes. Open Tools > UIPilot to see and fix them.\n{1}";
            internal const string BuildSummary = "UIPilot UI Health: {0} issue(s) in scene \"{1}\" (the build continues).\n{2}";
            internal const string IssueLine    = "- {0}: {1}";
            internal const string MoreLines    = "- and {0} more";
        }
    }
}
