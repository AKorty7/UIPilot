namespace UIPilot.Editor.Modules.ClickDebug
{
    // What became of one click, worst last.
    internal enum ClickVerdict
    {
        Missed,          // nothing clickable under the pointer: a background, empty space
        Handled,         // a control took it and has something to call
        NoListeners,     // a Button took it, but nothing is set under On Click
        NotInteractable, // a control took it, but it is switched off
        Blocked,         // something drawn over a control took it instead
        Unreachable,     // a control is under the pointer but cannot receive clicks
        NoEventSystem,   // nothing can receive clicks at all
    }
}
