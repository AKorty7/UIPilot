using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UIPilot.Editor.Modules.Health
{
    // Checks that a click reaches the control under the pointer: a raycaster on
    // the right canvas, and nothing clickable drawn on top of the control.
    internal static class HealthRaycastChecks
    {
        // ── Graphic Raycasters ───────────────────────────────────────────────

        // A GraphicRaycaster only hits graphics registered with its own canvas,
        // and a graphic registers with its NEAREST enabled canvas. So a nested
        // canvas with buttons needs a raycaster of its own.
        internal static void CheckRaycasters(HealthScope scope, List<HealthIssue> issues)
        {
            var needing = new List<Canvas>();

            foreach (var selectable in scope.Selectables)
            {
                if (!HealthScope.IsLive(selectable)) continue;

                var canvas = HealthScope.NearestCanvas(selectable.transform);
                if (canvas != null && !HasLiveRaycaster(canvas) && !needing.Contains(canvas))
                    needing.Add(canvas);
            }

            foreach (var canvas in needing)
                issues.Add(new HealthIssue(HealthCheck.Raycasters, HealthSeverity.Broken, canvas.name,
                    canvas.isRootCanvas ? HealthContent.Messages.NoRaycaster : HealthContent.Messages.NestedNoRaycaster,
                    new Object[] { canvas.gameObject }, () => EnableRaycaster(canvas)));
        }

        // Any raycaster counts, so a custom one is never reported as missing.
        private static bool HasLiveRaycaster(Canvas canvas)
        {
            foreach (var raycaster in canvas.GetComponents<BaseRaycaster>())
                if (raycaster.enabled) return true;
            return false;
        }

        private static void EnableRaycaster(Canvas canvas)
        {
            var existing = canvas.GetComponent<GraphicRaycaster>();
            if (existing == null)
            {
                Undo.AddComponent<GraphicRaycaster>(canvas.gameObject);
                return;
            }

            Undo.RecordObject(existing, HealthContent.Undo.Fix);
            existing.enabled = true;
        }

        // ── Blocked buttons ──────────────────────────────────────────────────

        // A graphic that could take a click: a raycast target on a screen-space
        // canvas, with where it sits on screen and in draw order.
        private sealed class Hit
        {
            internal Graphic Graphic;
            internal Canvas  Root;
            internal Camera  Camera;
            internal bool    Overlay;
            internal int     Order;
            internal Rect    ScreenBounds;
        }

        // Tests each control at its centre, as a player would click it, and reports
        // what takes that click instead. Grouped by the blocker: one full-screen
        // image over five buttons is one problem, with one fix.
        internal static void CheckBlockedButtons(HealthScope scope, List<HealthIssue> issues)
        {
            var hits      = CollectHitsTopFirst(scope);
            var positions = new Dictionary<Graphic, int>();
            for (var i = 0; i < hits.Count; i++) positions[hits[i].Graphic] = i;

            var blocked = new Dictionary<Graphic, List<string>>();
            var found   = new List<Graphic>();
            var screens = new Dictionary<Transform, bool>();

            foreach (var selectable in scope.Selectables)
            {
                var blocker = FindBlocker(selectable, hits, positions, screens);
                if (blocker == null) continue;

                if (!blocked.TryGetValue(blocker, out var names))
                {
                    blocked[blocker] = names = new List<string>();
                    found.Add(blocker);
                }

                names.Add(selectable.name);
            }

            foreach (var blocker in found)
                issues.Add(new HealthIssue(HealthCheck.BlockedButtons, HealthSeverity.Warning, blocker.name,
                    string.Format(HealthContent.Messages.Blocking, blocker.GetType().Name,
                        HealthScope.NameList(blocked[blocker])),
                    new Object[] { blocker.gameObject }, () => TurnOffRaycastTarget(blocker)));
        }

        private static void TurnOffRaycastTarget(Graphic graphic)
        {
            Undo.RecordObject(graphic, HealthContent.Undo.Fix);
            graphic.raycastTarget = false;
        }

        // Hits are sorted top first, so everything before the control's own graphic
        // is drawn above it, and the first of those that takes the click is the one
        // the player would hit. Nothing below is ever looked at.
        private static Graphic FindBlocker(Selectable selectable, List<Hit> hits,
            Dictionary<Graphic, int> positions, Dictionary<Transform, bool> screens)
        {
            if (!HealthScope.IsLive(selectable)) return null;

            var ownIndex = OwnHitIndex(selectable, hits, positions, out var point);
            if (ownIndex < 0) return null; // nothing of the control itself is under its centre

            var own = hits[ownIndex];
            for (var i = 0; i < ownIndex; i++)
            {
                var hit = hits[i];
                if (!hit.ScreenBounds.Contains(point) || !IsComparable(hit, own)) continue;
                if (hit.Graphic.transform.IsChildOf(selectable.transform) || !TakesClick(hit, point)) continue;

                // A blocker from another screen (a panel with its own live controls)
                // is a menu the game shows instead, not a mistake.
                return IsAnotherScreen(hit.Graphic.transform, selectable.transform, screens) ? null : hit.Graphic;
            }

            return null;
        }

        // The top-most graphic of the control itself (or of a child: clicks on
        // children bubble up to it) that a click at its centre would hit.
        private static int OwnHitIndex(Selectable selectable, List<Hit> hits,
            Dictionary<Graphic, int> positions, out Vector2 point)
        {
            point = Vector2.zero;
            var root = HealthScope.RootCanvas(selectable.transform);
            if (root == null || root.renderMode == RenderMode.WorldSpace) return -1;
            if (!(selectable.transform is RectTransform rect)) return -1;

            point = RectTransformUtility.WorldToScreenPoint(
                HealthScope.EventCamera(root), rect.TransformPoint(rect.rect.center));

            var best = -1;
            foreach (var graphic in selectable.GetComponentsInChildren<Graphic>(false))
                if (positions.TryGetValue(graphic, out var index) && (best < 0 || index < best)
                    && TakesClick(hits[index], point))
                    best = index;

            return best;
        }

        // Whether draw order between two graphics is certain: within one canvas it
        // is hierarchy order; across root canvases, an overlay canvas is above a
        // camera canvas and above an overlay with a lower Sort Order. Anything else
        // (nested canvases, camera canvases against each other) is left unreported.
        private static bool IsComparable(Hit hit, Hit own)
        {
            if (hit.Root == own.Root) return hit.Graphic.canvas == own.Graphic.canvas;
            return hit.Overlay && (!own.Overlay || hit.Root.sortingOrder != own.Root.sortingOrder);
        }

        private static bool TakesClick(Hit hit, Vector2 point)
        {
            return hit.ScreenBounds.Contains(point)
                && RectTransformUtility.RectangleContainsScreenPoint(
                       hit.Graphic.rectTransform, point, hit.Camera, hit.Graphic.raycastPadding)
                && hit.Graphic.Raycast(point, hit.Camera); // CanvasGroup, Mask, RectMask2D, alpha hit test
        }

        // The branch the blocker sits in, below the lowest ancestor it shares with
        // the control. If that branch holds live controls, it is another screen.
        private static bool IsAnotherScreen(Transform blocker, Transform control, Dictionary<Transform, bool> cache)
        {
            var branch = blocker;
            for (var t = blocker.parent; t != null && !control.IsChildOf(t); t = t.parent)
                branch = t;

            if (cache.TryGetValue(branch, out var isScreen)) return isScreen;

            isScreen = false;
            foreach (var selectable in branch.GetComponentsInChildren<Selectable>(false))
                if (HealthScope.IsLive(selectable))
                    isScreen = true;

            return cache[branch] = isScreen;
        }

        // ── Hit collection ───────────────────────────────────────────────────

        // Top of the screen first: overlay canvases before camera canvases, higher
        // Sort Order first, and within a canvas, later in the hierarchy first.
        private static List<Hit> CollectHitsTopFirst(HealthScope scope)
        {
            var order = new Dictionary<Transform, int>();
            foreach (var canvas in scope.Canvases)
                if (canvas.isRootCanvas && canvas.isActiveAndEnabled)
                    NumberInDrawOrder(canvas.transform, order);

            var hits = new List<Hit>();
            foreach (var graphic in scope.Graphics)
            {
                var hit = ToHit(graphic, order);
                if (hit != null) hits.Add(hit);
            }

            hits.Sort(TopFirst);
            return hits;
        }

        private static int TopFirst(Hit a, Hit b)
        {
            if (a.Overlay != b.Overlay) return a.Overlay ? -1 : 1;
            if (a.Root != b.Root && a.Root.sortingOrder != b.Root.sortingOrder)
                return b.Root.sortingOrder.CompareTo(a.Root.sortingOrder);
            return b.Order.CompareTo(a.Order);
        }

        // Within one canvas, later in the hierarchy is drawn on top.
        private static void NumberInDrawOrder(Transform transform, Dictionary<Transform, int> order)
        {
            order[transform] = order.Count;
            foreach (Transform child in transform)
                if (child.gameObject.activeSelf)
                    NumberInDrawOrder(child, order);
        }

        private static Hit ToHit(Graphic graphic, Dictionary<Transform, int> order)
        {
            if (!graphic.raycastTarget || !graphic.isActiveAndEnabled || graphic.canvasRenderer.cull) return null;
            if (!order.TryGetValue(graphic.transform, out var drawOrder)) return null;

            var root = HealthScope.RootCanvas(graphic.transform);
            if (root == null || root.renderMode == RenderMode.WorldSpace) return null;

            var camera = HealthScope.EventCamera(root);
            return new Hit
            {
                Graphic      = graphic,
                Root         = root,
                Camera       = camera,
                Overlay      = HealthScope.IsOverlay(root),
                Order        = drawOrder,
                ScreenBounds = ScreenBounds(graphic.rectTransform, camera)
            };
        }

        private static readonly Vector3[] Corners = new Vector3[4];

        private static Rect ScreenBounds(RectTransform rect, Camera camera)
        {
            rect.GetWorldCorners(Corners);
            var min = Vector2.positiveInfinity;
            var max = Vector2.negativeInfinity;

            foreach (var corner in Corners)
            {
                var point = RectTransformUtility.WorldToScreenPoint(camera, corner);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x - 1f, min.y - 1f, max.x + 1f, max.y + 1f);
        }
    }
}
