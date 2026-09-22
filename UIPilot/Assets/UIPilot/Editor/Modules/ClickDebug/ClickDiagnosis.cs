using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UIPilot.Editor.Modules.ClickDebug
{
    // When a click hits nothing that responds: is there a control under the pointer
    // that should have taken it, and what stops it? Only a control's own graphic is
    // looked at. Its label's Raycast Target is usually off on purpose.
    internal static class ClickDiagnosis
    {
        internal static ClickReport FindUnreachable(Vector2 position)
        {
            foreach (var control in Selectable.allSelectablesArray)
            {
                var graphic = control.targetGraphic != null ? control.targetGraphic : control.GetComponent<Graphic>();
                if (graphic == null || !graphic.isActiveAndEnabled || !IsUnder(graphic, position)) continue;

                var reason = WhyNotHit(graphic);
                if (reason == null) continue;

                return new ClickReport(ClickVerdict.Unreachable, control.name, new[] { reason },
                    new Object[] { control.gameObject, graphic.gameObject }, null, position);
            }

            return null;
        }

        internal static ClickReport Nothing(Vector2 position)
        {
            return new ClickReport(ClickVerdict.Missed, ClickDebugContent.Messages.NothingHere,
                new[] { ClickDebugContent.Messages.NothingDetail }, null, null, position);
        }

        // The CanvasGroup that switches this off, respecting Ignore Parent Groups.
        internal static CanvasGroup FindGroup(Transform from, Predicate<CanvasGroup> isOff)
        {
            for (var t = from; t != null; t = t.parent)
            {
                foreach (var group in t.GetComponents<CanvasGroup>())
                {
                    if (!group.enabled) continue;
                    if (isOff(group)) return group;
                    if (group.ignoreParentGroups) return null;
                }
            }

            return null;
        }

        // The reasons Unity's GraphicRaycaster skips a graphic, in the order it checks them.
        private static string WhyNotHit(Graphic graphic)
        {
            if (!graphic.raycastTarget)
                return string.Format(ClickDebugContent.Messages.RaycastOff, graphic.GetType().Name, graphic.name);

            var canvas = graphic.canvas;
            if (!HasLiveRaycaster(canvas))
                return string.Format(canvas.isRootCanvas
                    ? ClickDebugContent.Messages.NoRaycaster
                    : ClickDebugContent.Messages.NestedNoRaycaster, canvas.name);

            var group = FindGroup(graphic.transform, g => !g.blocksRaycasts);
            if (group != null)
                return string.Format(ClickDebugContent.Messages.GroupBlocks, group.name);

            if (graphic.canvasRenderer.cull)
                return ClickDebugContent.Messages.Masked;

            if (graphic is Image image && image.alphaHitTestMinimumThreshold > 0f)
                return string.Format(ClickDebugContent.Messages.SeeThrough, image.alphaHitTestMinimumThreshold);

            return null;
        }

        private static bool HasLiveRaycaster(Canvas canvas)
        {
            foreach (var raycaster in canvas.GetComponents<BaseRaycaster>())
                if (raycaster.isActiveAndEnabled) return true;
            return false;
        }

        private static bool IsUnder(Graphic graphic, Vector2 position)
        {
            var canvas = graphic.canvas;
            if (canvas == null) return false;

            // The camera a Screen Space - Camera or World Space canvas is seen through;
            // none for an overlay (or a camera canvas with no camera set).
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null
                       : canvas.worldCamera != null ? canvas.worldCamera
                       : canvas.renderMode == RenderMode.WorldSpace ? Camera.main : null;

            return RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, position, camera);
        }
    }
}
