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
    // are switched on, and the size of decorations that sit outside the layout (the
    // focus plate, the rule's line). It never creates, deletes, renames, moves or
    // rewires anything, which is why a theme can be applied over a menu the developer
    // has rearranged. Objects are found by the names UIGeneratorMenuBuilder gave them;
    // anything the developer deleted is simply skipped.
    //
    // Every property a theme can set is set on every apply, back to its default when
    // the theme leaves it empty, so switching themes never leaves the last one behind.
    internal static class UIGeneratorStyler
    {
        // On a menu item the frame starts a little left of the icon.
        internal const float FramePad   = 16f;
        // Where the builder draws the rule's line: its centre, below the holder's top.
        private const float RuleCentre  = 12.5f;
        // A cursor ends this far before the item's words, whatever its size.
        private const float MarkerGap   = 8f;
        // What an empty cover slot is shaped for: the reference screen.
        private const float DefaultCoverAspect = 16f / 9f;

        internal static void Apply(GameObject panelGO, UIPilotTheme theme)
        {
            var panel = panelGO.transform;

            StyleScene(FindBySuffix(panel, UIGeneratorContent.GameObjects.SceneSuffix), theme);
            StyleWash(panelGO, FindBySuffix(panel, UIGeneratorContent.GameObjects.WashSuffix), theme);
            StyleBackdrop(FindBySuffix(panel, UIGeneratorContent.GameObjects.BackdropSuffix), theme);
            StylePanel(FindBySuffix(panel, UIGeneratorContent.GameObjects.BandSuffix), theme);
            StyleRule(FindBySuffix(panel, UIGeneratorContent.GameObjects.RuleSuffix), theme);
            StyleMarks(panel, theme);
            StyleLettering(panel, theme);
            StyleButtons(panel, theme);
            StyleDetails(panel, theme);
        }

        // ── Scene ────────────────────────────────────────────────────────────

        // The picture, if any, replaces the layers. The hour is shown at once and
        // kept for the session, so the Editor and the game agree on what is seen.
        private static void StyleScene(Transform scene, UIPilotTheme theme)
        {
            if (scene == null) return;

            var layer = 0;
            foreach (Transform child in scene)
            {
                if (child.name == UIGeneratorContent.GameObjects.PictureChild)
                    StyleCover(child, theme.picture, null);
                else if (child.name == UIGeneratorContent.GameObjects.LayerChild)
                    StyleLayer(child, theme, layer++);
            }

            UIPilotScenePreview.SetHour(theme.timeOfDay);
        }

        // Layers share one 16:9 frame whatever their texture's size, so a flat sky
        // can be a 4 x 4 texture and a gradient a 4 x 720 one.
        private static void StyleLayer(Transform cover, UIPilotTheme theme, int index)
        {
            var hasLayer = theme.picture == null && theme.sceneLayers != null && index < theme.sceneLayers.Length;
            var sprite   = hasLayer ? theme.sceneLayers[index].sprite   : null;
            var material = hasLayer ? theme.sceneLayers[index].material : null;
            StyleCover(cover, sprite, material, false);
        }

        // A full-screen image that keeps its proportions: the sprite's own for a
        // picture, the reference screen's for a layer. Hidden when empty, so the
        // game scene shows through. The fitter only sizes an active object, so a
        // hidden one is put back to its built size by hand.
        private static void StyleCover(Transform cover, Sprite art, Material material, bool ownAspect = true)
        {
            var image = cover.GetComponent<Image>();
            if (image == null) return;

            SetArt(image, art, Image.Type.Simple, 1f);
            image.color    = Color.white;
            image.material = art != null ? material : null;
            Show(cover, art != null);

            var fitter = cover.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = ownAspect && art != null && art.rect.height > 0f ? art.rect.width / art.rect.height : DefaultCoverAspect;
            if (art == null)
                ((RectTransform)cover).sizeDelta = Vector2.zero;
        }

        // Menus built before the wash had its own object carry it on the panel.
        private static void StyleWash(GameObject panelGO, Transform wash, UIPilotTheme theme)
        {
            if (wash == null)
            {
                SetImageColor(panelGO, theme.wash);
                return;
            }

            SetImageColor(panelGO, Color.clear);
            SetImageColor(wash.gameObject, theme.wash);
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
            var surface = band.GetComponent<Image>();
            if (surface != null) SetArt(surface, theme.panelArt, Image.Type.Sliced, theme.panelArtScale);

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
            if (line == null) return;

            var image = line.GetComponent<Image>();
            if (image == null) return;

            // Plain line or ornament, it stays centred where the builder drew the line.
            image.color = theme.rule;
            SetArt(image, theme.ruleArt, Image.Type.Simple, 1f);
            image.preserveAspect = theme.ruleArt != null;

            var height = Mathf.Max(1f, theme.ruleHeight);
            var rect   = (RectTransform)line;
            rect.sizeDelta        = new Vector2(0f, height);
            rect.anchoredPosition = new Vector2(0f, height * 0.5f - RuleCentre);
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
            var font      = theme.font != null ? theme.font : TMP_Settings.defaultFontAsset;
            var titleFont = theme.titleFont != null ? theme.titleFont : font;

            foreach (var tmp in panel.GetComponentsInChildren<TMP_Text>(true))
            {
                if (tmp.name.EndsWith(UIGeneratorContent.GameObjects.TitleSuffix, StringComparison.Ordinal))
                {
                    StyleTitle(tmp, titleFont, theme);
                    continue;
                }

                // Assigning the font also resets the material to that font's own,
                // which is what clears a previous theme's material preset.
                tmp.font = font;
                if (theme.font != null && theme.textMaterial != null)
                    tmp.fontSharedMaterial = theme.textMaterial;

                if (tmp.name.EndsWith(UIGeneratorContent.GameObjects.FootnoteSuffix, StringComparison.Ordinal))
                    StyleText(tmp, theme.smallSize, theme.textSoft, UIPilotTextCase.Uppercase, false, theme.itemTracking * 3f);
                else if (tmp.name.EndsWith(UIGeneratorContent.GameObjects.RowValueChild, StringComparison.Ordinal))
                    StyleText(tmp, theme.valueSize, theme.textSoft, theme.itemCase, false, theme.itemTracking * 0.5f);
                else
                    StyleText(tmp, theme.itemSize, theme.text, theme.itemCase, false, theme.itemTracking);
            }
        }

        private static void StyleTitle(TMP_Text tmp, TMP_FontAsset font, UIPilotTheme theme)
        {
            tmp.font = font;
            StyleText(tmp, theme.titleSize, theme.text, theme.titleCase, theme.titleBold, theme.titleTracking);

            // The title auto-sizes: a long name shrinks from the theme's size.
            tmp.fontSizeMax = theme.titleSize;
            tmp.fontSizeMin = theme.titleSize * 0.5f;

            // A material preset only fits the font it was made from, and a theme
            // without its own font is on TextMeshPro's default.
            if (font != TMP_Settings.defaultFontAsset && theme.titleMaterial != null)
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
                if (button.GetComponent<LayoutElement>() != null)
                    StyleItemPlate(plate, theme);
                else
                    StyleStepPlate(plate, theme);
            }
        }

        private static void StyleItemPlate(Image plate, UIPilotTheme theme)
        {
            switch (theme.focusStyle)
            {
                case UIPilotFocusStyle.Frame:
                    var frame = theme.focusArt != null ? theme.focusArt : UIGeneratorArt.Sprite(UIGeneratorContent.Art.Focus);
                    StretchPlate(plate, frame, FramePad + theme.focusReach, theme.focusReach);
                    break;
                case UIPilotFocusStyle.Marker when theme.focusArt != null:
                    PlaceMarker(plate, theme.focusArt, theme.markerSize);
                    break;
                default:
                    StretchPlate(plate, theme.focusArt, FramePad, 0f);
                    break;
            }
        }

        // The step arrows are too small for a marker beside them: a marker theme
        // frames them with UIPilot's hairline instead, other themes as their items.
        private static void StyleStepPlate(Image plate, UIPilotTheme theme)
        {
            switch (theme.focusStyle)
            {
                case UIPilotFocusStyle.Frame:
                    var frame = theme.focusArt != null ? theme.focusArt : UIGeneratorArt.Sprite(UIGeneratorContent.Art.Focus);
                    StretchPlate(plate, frame, theme.focusReach, theme.focusReach);
                    break;
                case UIPilotFocusStyle.Marker:
                    StretchPlate(plate, UIGeneratorArt.Sprite(UIGeneratorContent.Art.Frame), 0f, 0f);
                    plate.fillCenter = false;
                    break;
                default:
                    StretchPlate(plate, theme.focusArt, 0f, 0f);
                    break;
            }
        }

        // Covers the button, reaching past its left edge by padLeft and past every
        // other edge by reach. With no art: a flat bar.
        private static void StretchPlate(Image plate, Sprite art, float padLeft, float reach)
        {
            SetArt(plate, art, Image.Type.Sliced, 1f);

            var rect = (RectTransform)plate.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot     = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(-padLeft, -reach);
            rect.offsetMax = new Vector2(reach, reach);
        }

        // A cursor where the item's icon would be, drawn at its own proportions and
        // ending MarkerGap before the words: a wide one reaches back past the item's
        // left edge rather than crowding the text.
        private static void PlaceMarker(Image plate, Sprite art, Vector2 size)
        {
            SetArt(plate, art, Image.Type.Simple, 1f);
            plate.preserveAspect = true;

            var rect = (RectTransform)plate.transform;
            rect.anchorMin        = new Vector2(0f, 0.5f);
            rect.anchorMax        = new Vector2(0f, 0.5f);
            rect.pivot            = new Vector2(0f, 0.5f);
            rect.sizeDelta        = size;
            rect.anchoredPosition = new Vector2(UIGeneratorMenuBuilder.LabelInset - MarkerGap - size.x, 0f);
        }

        // ── Details ──────────────────────────────────────────────────────────

        private static void StyleDetails(Transform panel, UIPilotTheme theme)
        {
            foreach (var image in panel.GetComponentsInChildren<Image>(true))
            {
                if (image.name == UIGeneratorContent.GameObjects.IconChild)        StyleIcon(image, theme);
                else if (image.name == UIGeneratorContent.GameObjects.VolumeTrack) image.color = theme.meterTrack;
                else if (image.name == UIGeneratorContent.GameObjects.VolumeFill)  image.color = theme.meterFill;
            }
        }

        // The Icons switch hides the icons that label items and rows. The arrows of
        // a settings stepper are the buttons themselves, so they always stay.
        private static void StyleIcon(Image icon, UIPilotTheme theme)
        {
            icon.color = theme.text;

            var owner    = icon.transform.parent;
            var isArrow  = owner != null && owner.GetComponent<Button>() != null
                           && owner.GetComponent<LayoutElement>() == null;
            Show(icon.transform, theme.icons || isArrow);
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

        // Artwork on an image, or none. Every property the art changes is set back
        // when it goes, so a flat theme applied after a drawn one matches a fresh build.
        private static void SetArt(Image image, Sprite art, Image.Type drawnType, float borderScale)
        {
            image.sprite                  = art;
            image.type                    = art != null ? drawnType : Image.Type.Simple;
            image.fillCenter              = true;
            image.preserveAspect          = false;
            image.pixelsPerUnitMultiplier = art != null ? Mathf.Max(0.01f, borderScale) : 1f;
        }

        private static void SetRawImageColor(GameObject go, Color color)
        {
            var raw = go.GetComponent<RawImage>();
            if (raw != null) raw.color = color;
        }
    }
}
