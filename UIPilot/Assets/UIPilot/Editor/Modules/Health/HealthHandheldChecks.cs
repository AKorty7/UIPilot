using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UIPilot.Editor.Modules.Health
{
    // Checks for playing on a controller and a small screen: every control reachable
    // with a gamepad, and text large enough for Steam Deck Verified.
    internal static class HealthHandheldChecks
    {
        // Steam Deck's screen, and Valve's minimum text height on it.
        private const float DeckWidth     = 1280f;
        private const float DeckHeight    = 800f;
        private const float MinTextPixels = 9f;

        // ── Gamepad navigation ───────────────────────────────────────────────

        // Unity's automatic navigation skips a control whose own mode is None, so
        // nothing ever moves to it unless another control points at it explicitly.
        internal static void CheckGamepadNavigation(HealthScope scope, List<HealthIssue> issues)
        {
            var controls = LiveNavigableControls(scope);
            var targeted = ExplicitTargets(controls);
            var perRoot  = CountPerRootCanvas(controls);

            foreach (var control in controls)
            {
                var root = HealthScope.RootCanvas(control.transform);
                if (root == null || perRoot[root] < 2) continue; // a lone control has nowhere to go

                var problem = NavigationProblem(control.navigation, targeted.Contains(control));
                if (problem == null) continue;

                issues.Add(new HealthIssue(HealthCheck.GamepadNavigation, HealthSeverity.Warning,
                    control.name, problem, new Object[] { control.gameObject },
                    () => MakeNavigationAutomatic(control)));
            }
        }

        private static string NavigationProblem(Navigation navigation, bool isExplicitTarget)
        {
            if (navigation.mode == Navigation.Mode.None && !isExplicitTarget)
                return HealthContent.Messages.NavigationNone;

            var stuck = navigation.mode == Navigation.Mode.Explicit
                && navigation.selectOnUp == null && navigation.selectOnDown == null
                && navigation.selectOnLeft == null && navigation.selectOnRight == null;

            return stuck ? HealthContent.Messages.NavigationStuck : null;
        }

        // Scrollbars are left out: they are usually driven by their scroll view,
        // not selected on their own.
        private static List<Selectable> LiveNavigableControls(HealthScope scope)
        {
            var controls = new List<Selectable>();
            foreach (var selectable in scope.Selectables)
                if (HealthScope.IsLive(selectable) && !(selectable is Scrollbar))
                    controls.Add(selectable);
            return controls;
        }

        private static HashSet<Selectable> ExplicitTargets(List<Selectable> controls)
        {
            var targets = new HashSet<Selectable>();
            foreach (var control in controls)
            {
                var nav = control.navigation;
                if (nav.mode != Navigation.Mode.Explicit) continue;

                targets.Add(nav.selectOnUp);
                targets.Add(nav.selectOnDown);
                targets.Add(nav.selectOnLeft);
                targets.Add(nav.selectOnRight);
            }

            return targets;
        }

        private static Dictionary<Canvas, int> CountPerRootCanvas(List<Selectable> controls)
        {
            var counts = new Dictionary<Canvas, int>();
            foreach (var control in controls)
            {
                var root = HealthScope.RootCanvas(control.transform);
                if (root == null) continue;
                counts.TryGetValue(root, out var count);
                counts[root] = count + 1;
            }

            return counts;
        }

        private static void MakeNavigationAutomatic(Selectable control)
        {
            Undo.RecordObject(control, HealthContent.Undo.Fix);
            var nav  = control.navigation;
            nav.mode = Navigation.Mode.Automatic;
            control.navigation = nav;
        }

        // ── Text size ────────────────────────────────────────────────────────

        // Every text that would render under 9 px on a Steam Deck, hidden panels
        // included. One row for all of them: selecting the row selects them all.
        internal static void CheckTextSize(HealthScope scope, List<HealthIssue> issues)
        {
            var tooSmall = new List<Object>();
            Graphic smallest      = null;
            var     smallestPixels = float.MaxValue;

            foreach (var graphic in scope.Graphics)
            {
                var pixels = PixelsAtDeck(graphic);
                if (pixels >= MinTextPixels) continue;

                tooSmall.Add(graphic.gameObject);
                if (pixels < smallestPixels)
                {
                    smallest       = graphic;
                    smallestPixels = pixels;
                }
            }

            if (smallest == null) return;

            var label  = tooSmall.Count == 1
                ? smallest.name
                : string.Format(HealthContent.Messages.TextCountLabel, tooSmall.Count);
            var detail = tooSmall.Count == 1
                ? string.Format(HealthContent.Messages.TextTooSmallOne, smallestPixels)
                : string.Format(HealthContent.Messages.TextTooSmallMany, smallest.name, smallestPixels);

            issues.Add(new HealthIssue(HealthCheck.TextSize, HealthSeverity.Warning, label, detail, tooSmall.ToArray()));
        }

        // float.MaxValue when the graphic is not text, is empty or switched off,
        // or sits on a canvas whose size on a Deck cannot be known.
        private static float PixelsAtDeck(Graphic graphic)
        {
            if (!graphic.enabled || !TryReadText(graphic, out var text, out var fontSize)) return float.MaxValue;
            if (string.IsNullOrWhiteSpace(text)) return float.MaxValue;

            var root = HealthScope.RootCanvas(graphic.transform);
            if (root == null || root.renderMode == RenderMode.WorldSpace) return float.MaxValue;

            var canvasScale = CanvasScaleAtDeck(root);
            if (canvasScale <= 0f) return float.MaxValue;

            return fontSize * ScaleBelow(graphic.transform, root.transform) * canvasScale;
        }

        private static bool TryReadText(Graphic graphic, out string text, out float fontSize)
        {
            switch (graphic)
            {
                case TMP_Text tmp:
                    text = tmp.text; fontSize = tmp.fontSize; return true;
                case Text legacy:
                    text = legacy.text; fontSize = legacy.fontSize; return true;
                default:
                    text = null; fontSize = 0f; return false;
            }
        }

        // The scale between the canvas and the text, from the objects in between.
        private static float ScaleBelow(Transform transform, Transform root)
        {
            var scale = 1f;
            for (var t = transform; t != null && t != root; t = t.parent)
                scale *= Mathf.Abs(t.localScale.y);
            return scale;
        }

        // What CanvasScaler would set the canvas scale to on a 1280 x 800 screen.
        // 0 for Constant Physical Size, which depends on the screen's DPI.
        private static float CanvasScaleAtDeck(Canvas root)
        {
            var scaler = root.GetComponent<CanvasScaler>();
            if (scaler == null || !scaler.enabled) return root.scaleFactor;

            switch (scaler.uiScaleMode)
            {
                case CanvasScaler.ScaleMode.ConstantPixelSize:  return scaler.scaleFactor;
                case CanvasScaler.ScaleMode.ScaleWithScreenSize: return ScaleWithScreenSize(scaler);
                default:                                         return 0f;
            }
        }

        // The same sums as CanvasScaler.HandleScaleWithScreenSize.
        private static float ScaleWithScreenSize(CanvasScaler scaler)
        {
            var reference = scaler.referenceResolution;
            var width     = DeckWidth / reference.x;
            var height    = DeckHeight / reference.y;

            switch (scaler.screenMatchMode)
            {
                case CanvasScaler.ScreenMatchMode.Expand: return Mathf.Min(width, height);
                case CanvasScaler.ScreenMatchMode.Shrink: return Mathf.Max(width, height);
                default:
                    var log = Mathf.Lerp(Mathf.Log(width, 2f), Mathf.Log(height, 2f), scaler.matchWidthOrHeight);
                    return Mathf.Pow(2f, log);
            }
        }
    }
}
