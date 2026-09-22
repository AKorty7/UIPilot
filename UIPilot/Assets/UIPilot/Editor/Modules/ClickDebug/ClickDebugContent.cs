namespace UIPilot.Editor.Modules.ClickDebug
{
    internal static class ClickDebugContent
    {
        internal static class Messages
        {
            // What took the click
            internal const string Calls           = "On Click calls {0}.{1}().";
            internal const string CallsOff        = "{0}.{1}() is set, but switched Off, so it does not run.";
            internal const string EntryNoObject   = "Entry {0} has no object, so it calls nothing.";
            internal const string EntryNoMethod   = "Entry {0} has no function selected, so it calls nothing.";
            internal const string ValueCalls      = "On Value Changed calls {0}.{1}().";
            internal const string NoListeners     = "Button with nothing under On Click, so the click does nothing. Listeners added from code still run, but are not listed here.";
            internal const string ToggleNoListeners = "Toggle. It switches, but nothing is set under On Value Changed.";
            internal const string HandledBy       = "{0} receives the click.";
            internal const string LandedOnChild   = "Landed on {0}, which passes clicks up to it.";

            internal const string InteractableOff = "{0} with Interactable off, so it ignores the click.";
            internal const string GroupOff        = "{0} inside CanvasGroup \"{1}\", which has Interactable off, so it ignores the click.";

            internal const string Blocking        = "{0} with Raycast Target on, drawn over {1}, so it takes the click and {1} never gets it.";
            internal const string BlockingFix     = "If {0} is only decoration, turn off its Raycast Target.";
            internal const string NotClickable    = "{0}. Nothing on it or its parents responds to clicks, which is normal for backgrounds and decoration.";

            // Why nothing was hit
            internal const string NothingHere     = "Nothing under the pointer";
            internal const string NothingDetail   = "No UI here receives clicks.";
            internal const string RaycastOff      = "Its {0} \"{1}\" has Raycast Target off, so clicks pass through.";
            internal const string NoRaycaster     = "Canvas \"{0}\" has no Graphic Raycaster, so nothing on it can be clicked.";
            internal const string NestedNoRaycaster = "Nested canvas \"{0}\" has no Graphic Raycaster of its own, so nothing on it can be clicked.";
            internal const string GroupBlocks     = "CanvasGroup \"{0}\" has Blocks Raycasts off, so clicks pass through everything in it.";
            internal const string Masked          = "It is cut off by a mask at this point.";
            internal const string SeeThrough      = "Its Image ignores clicks on see-through pixels (Alpha Hit Test Minimum Threshold {0:0.##}).";

            internal const string NoEventSystem       = "No EventSystem";
            internal const string NoEventSystemDetail = "There is no active EventSystem, so no UI can receive clicks. UI Health adds one in Edit mode.";
            internal const string NoInputModule       = "The EventSystem has no working input module, so clicks never reach the UI.";

            internal const string HitOrder      = "Under the pointer, top first: {0}";
            internal const string HitSeparator  = " › ";
            internal const string HitMore       = " › {0} more";
            internal const string ListSeparator = ", ";
        }

        // The word beside each click, 52 px wide at most.
        internal static class Words
        {
            internal const string Handled     = "OK";
            internal const string NoListeners = "Empty";
            internal const string Off         = "Off";
            internal const string Blocked     = "Blocked";
            internal const string Broken      = "Broken";
            internal const string Missed      = "—";
        }

        // Per project and per user, in UserSettings/EditorUserSettings.asset.
        internal static class Settings
        {
            internal const string WatchKey  = "UIPilot.ClickDebug.Watch";
            internal const string LogKey    = "UIPilot.ClickDebug.Log";
            internal const string SelectKey = "UIPilot.ClickDebug.Select";
            internal const string On        = "1";
            internal const string Off       = "0";
        }

        internal static class Console
        {
            internal const string Line = "UIPilot Click: {0} ({1}). {2}";
            internal const string DetailSeparator = " ";
        }
    }
}
