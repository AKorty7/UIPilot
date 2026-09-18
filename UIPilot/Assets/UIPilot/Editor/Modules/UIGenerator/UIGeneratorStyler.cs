using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIPilot.Editor.Core;

namespace UIPilot.Editor.Modules.UIGenerator
{
    // Pushes a theme onto one menu panel. Building a menu and restyling an existing
    // one both end here, so an applied theme is identical to a freshly built one.
    //
    // It only ever touches appearance — colours, sprites, fonts, which decorations
    // are switched on. It never creates, deletes, renames, moves or rewires anything,
    // which is why a theme can be applied over a menu the developer has rearranged.
    // Objects are found by the names UIGeneratorMenuBuilder gave them; anything the
    // developer deleted is simply skipped.
    internal static class UIGeneratorStyler
    {
        // The focus frame sprite carries a transparent halo margin around its
        // visible frame, so the plate is larger than the button by this much.
        internal const float HaloMargin = 24f;
        // On a menu item the frame starts a little left of the icon.
        internal const float FramePad   = 16f;

        internal static void Apply(GameObject panelGO, UIPilotTheme theme)
        {
            var panel = panelGO.transform;

            SetImageColor(panelGO, theme.wash);

            StyleBackdrop(FindBySuffix(panel, UIGeneratorContent.GameObjects.BackdropSuffix), theme);
            StylePanel(FindBySuffix(panel, UIGeneratorContent.GameObjects.BandSuffix), theme);
            StyleRule(FindBySuffix(panel, UIGeneratorContent.GameObjects.RuleSuffix), theme);
            StyleMarks(panel, theme);
            StyleLettering(panel, theme);
            StyleButtons(panel, theme);
            StyleDetails(panel, theme);
        }

        // ── Atmosphere ───────────────────────────────────────────────────────

        private static void StyleBackdrop(Transform backdrop, UIPilotTheme theme)
        {
            if (backdrop == null) return;

            var bloomColors = new[]
            {
                theme.bloomTopLeft, theme.bloomBottomRight, theme.bloomTopRight, theme.bloomCentre
            };

            int bloom = 0, frame = 0;
            foreach (Transform child in backdrop)
            {
                if (child.name == UIGeneratorContent.GameObjects.BloomChild)
                {
                    Show(child, theme.blooms);
                    SetImageColor(child.gameObject, bloomColors[Math.Min(bloom++, bloomColors.Length - 1)]);
                }
                else if (child.name == UIGeneratorContent.GameObjects.WaveChild)
                {
                    Show(child, theme.ribbon);
                    SetImageColor(child.gameObject, theme.ribbonColor);
                }
                else if (child.name == UIGeneratorContent.GameObjects.FrameChild)
                {
                    // The second frame is the fainter of the pair.
                    var tint = theme.sceneFrameColor;
                    if (frame++ > 0) tint.a *= 0.55f;

                    Show(child, theme.sceneFrames);
                    SetImageColor(child.gameObject, tint);
                }
                else if (child.name == UIGeneratorContent.GameObjects.ScanlinesChild)
                {
                    Show(child, theme.sceneScanlines);
                    SetRawImageColor(child.gameObject, theme.sceneScanlineColor);
                }
            }
        }

        private static void StylePanel(Transform band, UIPilotTheme theme)
        {
            if (band == null) return;

            SetImageColor(band.gameObject, theme.panel);

            foreach (Transform child in band)
            {
                if (child.name == UIGeneratorContent.GameObjects.ScanlinesChild)
                {
                    Show(child, theme.panelScanlines);
                    SetRawImageColor(child.gameObject, theme.panelScanlineColor);
                }
                else if (child.name == UIGeneratorContent.GameObjects.FrameChild)
                {
                    Show(child, theme.panelFrame);
                    SetImageColor(child.gameObject, theme.panelFrameColor);
                }
                else if (child.name == UIGeneratorContent.GameObjects.FrameEchoChild)
                {
                    Show(child, theme.panelFrameEcho);
                    SetImageColor(child.gameObject, theme.panelFrameEchoColor);
                }
            }
        }

        private static void StyleRule(Transform rule, UIPilotTheme theme)
        {
            if (rule == null) return;

            var line = rule.Find(UIGeneratorContent.GameObjects.RuleBarChild);
            if (line != null) SetImageColor(line.gameObject, theme.rule);
        }

        // Registration marks live in two places (the panel's corners, the end of
        // the rule); one switch covers them all.
        private static void StyleMarks(Transform panel, UIPilotTheme theme)
        {
            foreach (var image in panel.GetComponentsInChildren<Image>(true))
            {
                if (image.name != UIGeneratorContent.GameObjects.CrossChild) continue;

                Show(image.transform, theme.registrationMarks);
                image.color = theme.panelFrameColor;
            }
        }

        // ── Lettering ────────────────────────────────────────────────────────

        private static void StyleLettering(Transform panel, UIPilotTheme theme)
        {
            var font = theme.font != null ? theme.font : TMP_Settings.defaultFontAsset;

            foreach (var tmp in panel.GetComponentsInChildren<TMP_Text>(true))
            {
                // Assigning the font also resets the material to that font's own,
                // which is what clears a previous theme's title material.
                tmp.font = font;

                if (tmp.name.EndsWith(UIGeneratorContent.GameObjects.TitleSuffix, StringComparison.Ordinal))
                    StyleTitle(tmp, theme);
                else if (tmp.name.EndsWith(UIGeneratorContent.GameObjects.FootnoteSuffix, StringComparison.Ordinal))
                    StyleText(tmp, theme.smallSize, theme.textSoft, UIPilotTextCase.Uppercase, false, theme.itemTracking * 3f);
                else if (tmp.name.EndsWith(UIGeneratorContent.GameObjects.RowValueChild, StringComparison.Ordinal))
                    StyleText(tmp, theme.valueSize, theme.textSoft, theme.itemCase, false, theme.itemTracking * 0.5f);
                else
                    StyleText(tmp, theme.itemSize, theme.text, theme.itemCase, false, theme.itemTracking);
            }
        }

        private static void StyleTitle(TMP_Text tmp, UIPilotTheme theme)
        {
            StyleText(tmp, theme.titleSize, theme.text, theme.titleCase, theme.titleBold, theme.titleTracking);

            // The title auto-sizes: a long name shrinks from the theme's size.
            tmp.fontSizeMax = theme.titleSize;
            tmp.fontSizeMin = theme.titleSize * 0.5f;

            // A material preset only fits the font it was made from.
            if (theme.font != null && theme.titleMaterial != null)
                tmp.fontSharedMaterial = theme.titleMaterial;
        }

        private static void StyleText(TMP_Text tmp, float size, Color color,
            UIPilotTextCase textCase, bool bold, float tracking)
        {
            var style = bold ? FontStyles.Bold : FontStyles.Normal;
            if (textCase == UIPilotTextCase.Lowercase) style |= FontStyles.LowerCase;
            if (textCase == UIPilotTextCase.Uppercase) style |= FontStyles.UpperCase;

            tmp.fontSize         = size;
            tmp.fontStyle        = style;
            tmp.characterSpacing = tracking;
            tmp.color            = color;
        }

        // ── Buttons ──────────────────────────────────────────────────────────

        private static void StyleButtons(Transform panel, UIPilotTheme theme)
        {
            var frameSprite = theme.focusFrame ? UIGeneratorArt.Sprite(UIGeneratorContent.Art.Focus) : null;

            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor      = theme.focusIdle;
            colors.highlightedColor = theme.focusHover;
            colors.pressedColor     = theme.focusPressed;
            colors.selectedColor    = theme.focusSelected;
            colors.disabledColor    = theme.focusDisabled;
            colors.colorMultiplier  = 1f;
            colors.fadeDuration     = 0.1f;

            foreach (var button in panel.GetComponentsInChildren<Button>(true))
            {
                button.colors = colors;

                var plate = button.targetGraphic as Image;
                if (plate == null || plate.gameObject == button.gameObject) continue;

                // Menu items are laid out by the column and carry a LayoutElement;
                // the little step buttons inside a settings row do not.
                var pad = button.GetComponent<LayoutElement>() != null ? FramePad : 0f;
                StylePlate(plate, frameSprite, pad);
            }
        }

        // With the frame sprite: a glowing hairline frame, larger than the button
        // because its halo spills past it. Without: a flat bar the button's size.
        private static void StylePlate(Image plate, Sprite frameSprite, float padLeft)
        {
            var rect = (RectTransform)plate.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            if (frameSprite != null)
            {
                plate.sprite   = frameSprite;
                plate.type     = Image.Type.Sliced;
                rect.offsetMin = new Vector2(-(padLeft + HaloMargin), -HaloMargin);
                rect.offsetMax = new Vector2(HaloMargin, HaloMargin);
            }
            else
            {
                plate.sprite   = null;
                plate.type     = Image.Type.Simple;
                rect.offsetMin = new Vector2(-padLeft, 0f);
                rect.offsetMax = Vector2.zero;
            }
        }

        // ── Details ──────────────────────────────────────────────────────────

        private static void StyleDetails(Transform panel, UIPilotTheme theme)
        {
            foreach (var image in panel.GetComponentsInChildren<Image>(true))
            {
                if (image.name == UIGeneratorContent.GameObjects.IconChild)        image.color = theme.text;
                else if (image.name == UIGeneratorContent.GameObjects.VolumeTrack) image.color = theme.meterTrack;
                else if (image.name == UIGeneratorContent.GameObjects.VolumeFill)  image.color = theme.meterFill;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static Transform FindBySuffix(Transform parent, string suffix)
        {
            foreach (Transform child in parent)
                if (child.name.EndsWith(suffix, StringComparison.Ordinal))
                    return child;
            return null;
        }

        private static void Show(Transform target, bool visible)
        {
            if (target.gameObject.activeSelf != visible)
                target.gameObject.SetActive(visible);
        }

        private static void SetImageColor(GameObject go, Color color)
        {
            var image = go.GetComponent<Image>();
            if (image != null) image.color = color;
        }

        private static void SetRawImageColor(GameObject go, Color color)
        {
            var raw = go.GetComponent<RawImage>();
            if (raw != null) raw.color = color;
        }
    }
}
