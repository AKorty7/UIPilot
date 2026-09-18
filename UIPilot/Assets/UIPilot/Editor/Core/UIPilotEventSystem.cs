using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIPilot.Editor.Core
{
    // The one place UIPilot creates an EventSystem, so generation, validation
    // and repair all pick the input module that matches the project.
    internal static class UIPilotEventSystem
    {
        internal static GameObject Create(string undoLabel)
        {
            var go = new GameObject(UIPilotLabels.EventSystem.ObjectName);
            go.AddComponent<EventSystem>();
            AddInputModule(go);
            Undo.RegisterCreatedObjectUndo(go, undoLabel);
            return go;
        }

        // StandaloneInputModule reads the legacy Input class, which throws every
        // frame when Active Input Handling is "Input System Package (New)" — the
        // default for new Unity 6 projects. The Input System module is resolved
        // by name so UIPilot compiles whether or not that package is installed.
        private static void AddInputModule(GameObject go)
        {
#if ENABLE_INPUT_SYSTEM
            var moduleType = Type.GetType(UIPilotLabels.EventSystem.InputSystemModuleType);
            if (moduleType != null)
            {
                go.AddComponent(moduleType);
                return;
            }
#endif
            go.AddComponent<StandaloneInputModule>();
        }
    }
}
