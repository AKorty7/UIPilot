using UnityEngine;
using UIPilot.Editor.Core;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UIPilot.Editor.Modules.ClickDebug
{
    // Reads the pointer the way the running game does: the Input System package
    // when the project uses it, the legacy Input class otherwise. Call it from the
    // game's own frame (the player loop). Read from an editor callback, the Input
    // System answers with the editor's input, not the game's.
    internal static class ClickPointer
    {
        // True on the frame the pointer's main button (left mouse, pen tip, a
        // finger) goes down, with where it went down, in Game view pixels.
        internal static bool WentDown(out Vector2 position)
        {
#if ENABLE_INPUT_SYSTEM
            // The mouse, pen or touchscreen, whichever was used last.
            var pointer = Pointer.current;
            if (pointer != null)
            {
                position = pointer.position.ReadValue();
                return pointer.press.wasPressedThisFrame;
            }
#endif
            if (UIPilotEventSystem.LegacyInputAvailable)
            {
                position = Input.mousePosition;
                return Input.GetMouseButtonDown(0);
            }

            position = default;
            return false;
        }
    }
}
