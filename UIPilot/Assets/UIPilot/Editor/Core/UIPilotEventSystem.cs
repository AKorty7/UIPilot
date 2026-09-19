using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIPilot.Editor.Core
{
    // The one place UIPilot creates an EventSystem or picks an input module, so
    // generation, validation, repair and UI Health all agree on what the project needs.
    internal static class UIPilotEventSystem
    {
        // Active Input Handling = "Input System Package (New)": the legacy Input
        // class is gone, and StandaloneInputModule throws every frame in Play mode.
        // A property, not a const, so callers do not compile to unreachable code.
        internal static bool LegacyInputAvailable
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                return false;
#else
                return true;
#endif
            }
        }

        internal static GameObject Create(string undoLabel)
        {
            var go = new GameObject(UIPilotLabels.EventSystem.ObjectName);
            go.AddComponent<EventSystem>();
            go.AddComponent(InputModuleType());
            Undo.RegisterCreatedObjectUndo(go, undoLabel);
            return go;
        }

        // StandaloneInputModule reads the legacy Input class, which throws every
        // frame when Active Input Handling is "Input System Package (New)", the
        // default for new Unity 6 projects. The Input System module is resolved
        // by name so UIPilot compiles whether or not that package is installed.
        internal static Type InputModuleType()
        {
#if ENABLE_INPUT_SYSTEM
            var moduleType = Type.GetType(UIPilotLabels.EventSystem.InputSystemModuleType);
            if (moduleType != null)
                return moduleType;
#endif
            return typeof(StandaloneInputModule);
        }
    }
}
