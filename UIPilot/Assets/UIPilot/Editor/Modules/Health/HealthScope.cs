using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIPilot.Editor.Modules.Health
{
    // Everything the checks read, gathered once per run from the scenes being
    // checked. Inactive objects are included: a hidden panel's broken button
    // breaks the moment the panel is shown. Read-only: nothing here changes a scene.
    internal sealed class HealthScope
    {
        internal readonly List<Canvas>      Canvases     = new List<Canvas>();
        internal readonly List<Selectable>  Selectables  = new List<Selectable>();
        internal readonly List<Graphic>     Graphics     = new List<Graphic>();
        internal readonly List<EventSystem> EventSystems = new List<EventSystem>();

        internal static HealthScope Gather(IEnumerable<Scene> scenes)
        {
            var scope = new HealthScope();

            foreach (var scene in scenes)
            {
                if (!scene.IsValid() || !scene.isLoaded) continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    scope.Canvases.AddRange(root.GetComponentsInChildren<Canvas>(true));
                    scope.Selectables.AddRange(root.GetComponentsInChildren<Selectable>(true));
                    scope.Graphics.AddRange(root.GetComponentsInChildren<Graphic>(true));
                    scope.EventSystems.AddRange(root.GetComponentsInChildren<EventSystem>(true));
                }
            }

            return scope;
        }

        // A control a player could use right now: shown, enabled, interactable,
        // and not switched off by a CanvasGroup.
        internal static bool IsLive(Selectable selectable)
        {
            return selectable.isActiveAndEnabled && selectable.IsInteractable();
        }

        internal bool HasLiveControls()
        {
            foreach (var selectable in Selectables)
                if (IsLive(selectable)) return true;
            return false;
        }

        // The canvas a graphic registers with, and so the one whose raycaster must
        // hit it: the nearest active, enabled Canvas above it (as Graphic does).
        internal static Canvas NearestCanvas(Transform transform)
        {
            for (var t = transform; t != null; t = t.parent)
            {
                var canvas = t.GetComponent<Canvas>();
                if (canvas != null && canvas.isActiveAndEnabled) return canvas;
            }

            return null;
        }

        // The top-most Canvas above an object, active or not.
        internal static Canvas RootCanvas(Transform transform)
        {
            Canvas root = null;
            for (var t = transform; t != null; t = t.parent)
            {
                var canvas = t.GetComponent<Canvas>();
                if (canvas != null) root = canvas;
            }

            return root;
        }

        internal static bool IsOverlay(Canvas root)
        {
            return root.renderMode == RenderMode.ScreenSpaceOverlay
                || (root.renderMode == RenderMode.ScreenSpaceCamera && root.worldCamera == null);
        }

        internal static Camera EventCamera(Canvas root)
        {
            return IsOverlay(root) ? null : root.worldCamera;
        }

        // "Play", "Play and Quit", "Play, Settings and Quit", "Play, Settings, Quit and 2 more".
        internal static string NameList(IList<string> names)
        {
            const int shown = 3;
            if (names.Count == 1) return names[0];

            if (names.Count <= shown)
            {
                var head = string.Join(HealthContent.Messages.ListSeparator, Slice(names, names.Count - 1));
                return head + HealthContent.Messages.ListLastJoin + names[names.Count - 1];
            }

            return string.Format(HealthContent.Messages.ListMore,
                string.Join(HealthContent.Messages.ListSeparator, Slice(names, shown)), names.Count - shown);
        }

        private static string[] Slice(IList<string> names, int count)
        {
            var slice = new string[count];
            for (var i = 0; i < count; i++) slice[i] = names[i];
            return slice;
        }
    }
}
