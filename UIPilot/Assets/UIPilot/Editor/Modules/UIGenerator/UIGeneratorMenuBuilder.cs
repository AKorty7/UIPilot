using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UIPilot.Editor.Core;

namespace UIPilot.Editor.Modules.UIGenerator
{
    // Builds the STRUCTURE of one menu panel: which objects exist, how they are laid
    // out, which sprite each one shows. How it all LOOKS — colours, lettering, which
    // decorations are switched on — is a theme, applied by UIGeneratorStyler as the
    // last step. Keeping the two apart is what lets a theme be applied to a menu that
    // already exists: every theme shares this structure.
    //
    // Every decoration is always created, even if the theme hides it, so that a later
    // theme can switch it on. Hidden objects are inactive and cost nothing at runtime.
    //
    // The output is plain native UGUI. Art comes from the white, tintable sprite kit in
    // Assets/UIPilot/Art. If the kit is missing, the menu still builds, without it.
    internal static class UIGeneratorMenuBuilder
    {
        // ── Metrics (reference 1920×1080, 8-point grid) ──────────────────────
        private const float PanelMargin    = 64f;   // panel to the screen edges
        private const float PanelWidth     = 640f;
        private const float PanelPadding   = 80f;   // panel edge to the column
        private const float ColumnWidth    = 480f;
        private const float ColumnSpacing  = 8f;
        private const float EchoOffset     = 16f;
        private const float ScanlineRows   = 119f;  // one tile is 8 units tall

        private const float TitleHeight    = 96f;
        private const float RuleGap        = 40f;
        private const float CrossSize      = 24f;

        private const float RowHeight      = 56f;
        private const float IconSize       = 24f;
        internal const float LabelInset    = 40f;   // icon + gap; the styler places cursors by it
        private const float ValueInset     = 16f;

        private const float StepSize       = 40f;
        private const float StepIconSize   = 20f;
        private const float StepValueWidth = 152f;
        private const float MeterInset     = 16f;
        private const float MeterHeight    = 2f;
        private const float MeterDrop      = 18f;
        private const float FootnoteRise   = 40f;

        private static Kit _kit;

        // ── Entry point ──────────────────────────────────────────────────────

        internal static GameObject Build(GameObject canvas, MenuType menuType,
            string title, string[] buttons, string prefix, UIPilotTheme theme)
        {
            _kit = Kit.Load();

            var panelGO = CreateUIObject(prefix + UIGeneratorContent.GameObjects.PanelSuffix, canvas);
            SetStretch((RectTransform)panelGO.transform);

            // Invisible, but it stops clicks from reaching the game behind the menu.
            // (The wash is a child, so the scene can be drawn under it.)
            panelGO.AddComponent<Image>();

            // A left-aligned column. Children keep their preferred width instead of
            // stretching, so the menu holds its shape on any aspect ratio.
            var inset = (int)(PanelMargin + PanelPadding);
            var vlg   = panelGO.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment         = TextAnchor.UpperLeft;
            vlg.spacing                = ColumnSpacing;
            vlg.padding                = new RectOffset(inset, 0, inset, inset);
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = true;
            vlg.childForceExpandWidth  = false;
            vlg.childForceExpandHeight = false;

            // Drawn in this order: the scene, the wash over it, then the decorations.
            CreateScene(panelGO, prefix + UIGeneratorContent.GameObjects.SceneSuffix);
            CreateWash(panelGO, prefix + UIGeneratorContent.GameObjects.WashSuffix);
            CreateBackdrop(panelGO, prefix + UIGeneratorContent.GameObjects.BackdropSuffix);
            CreatePanel(panelGO, prefix + UIGeneratorContent.GameObjects.BandSuffix);

            // Top of the panel: who you are. Bottom of the panel: what you can do.
            CreateTitle(panelGO, prefix + UIGeneratorContent.GameObjects.TitleSuffix, title);
            CreateRule(panelGO, prefix + UIGeneratorContent.GameObjects.RuleSuffix);

            CreateContent(panelGO, menuType, buttons, prefix);

            UIGeneratorStyler.Apply(panelGO, theme);
            return panelGO;
        }

        private static void CreateContent(GameObject panelGO, MenuType menuType, string[] buttons, string prefix)
        {
            if (menuType == MenuType.SettingsMenu)
            {
                // Settings come straight under the title; Back goes to the foot of
                // the panel, where every other menu keeps its way out.
                CreateSettingsRows(panelGO);
                CreateSpacer(panelGO, prefix + UIGeneratorContent.GameObjects.SpacerSuffix);
                CreateButton(panelGO, UIGeneratorContent.Buttons.Back);
                return;
            }

            CreateSpacer(panelGO, prefix + UIGeneratorContent.GameObjects.SpacerSuffix);

            foreach (var label in buttons)
                CreateButton(panelGO, label);

            if (menuType == MenuType.MainMenu)
                CreateFootnote(panelGO, UIGeneratorContent.GameObjects.MainMenuFootnote,
                    string.Format(UIGeneratorContent.Settings.VersionFormat, PlayerSettings.bundleVersion));
        }

        // ── Scene: what the menu is seen against ────────────────────────────
        // The developer's own picture, then the theme's layers, back to front, all
        // under everything else. Each covers the screen and keeps its shape (an
        // AspectRatioFitter crops it), so it looks right on any aspect ratio without
        // a script. The mask hides what the crop pushes out. Every menu gets the full
        // set of layer slots; a theme fills the ones it needs.

        private static void CreateScene(GameObject parent, string objectName)
        {
            var go = CreateDecor(parent, objectName);
            SetStretch((RectTransform)go.transform);
            go.AddComponent<RectMask2D>();

            CreateCover(go, UIGeneratorContent.GameObjects.PictureChild);
            for (var i = 0; i < UIGeneratorContent.GameObjects.SceneLayerSlots; i++)
                CreateCover(go, UIGeneratorContent.GameObjects.LayerChild);
        }

        private static Image CreateCover(GameObject parent, string objectName)
        {
            var go = CreateUIObject(objectName, parent);
            SetStretch((RectTransform)go.transform);

            var image = go.AddComponent<Image>();
            image.raycastTarget = false;

            var fitter = go.AddComponent<AspectRatioFitter>();
            fitter.aspectMode  = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 16f / 9f;
            return image;
        }

        // The veil between the scene and the menu.
        private static void CreateWash(GameObject parent, string objectName)
        {
            var go = CreateDecor(parent, objectName);
            SetStretch((RectTransform)go.transform);
            go.AddComponent<Image>().raycastTarget = false;
        }

        // ── Backdrop: blooms, frames, scanline band, ribbon ──────────────────
        // Placed by fractions of the screen, so the composition holds its
        // proportions on any aspect ratio. Draw order = creation order.

        private static void CreateBackdrop(GameObject parent, string objectName)
        {
            var go = CreateDecor(parent, objectName);
            SetStretch((RectTransform)go.transform);

            // Order matters to the styler: top-left, bottom-right, top-right, centre.
            CreateFractionSprite(go, UIGeneratorContent.GameObjects.BloomChild, _kit.Glow, false, new Rect(-0.15f, 0.40f, 0.75f, 0.85f));
            CreateFractionSprite(go, UIGeneratorContent.GameObjects.BloomChild, _kit.Glow, false, new Rect( 0.35f, -0.30f, 0.80f, 1.05f));
            CreateFractionSprite(go, UIGeneratorContent.GameObjects.BloomChild, _kit.Glow, false, new Rect( 0.55f, 0.45f, 0.65f, 0.75f));
            CreateFractionSprite(go, UIGeneratorContent.GameObjects.BloomChild, _kit.Glow, false, new Rect( 0.30f, 0.20f, 0.55f, 0.60f));

            var bandFraction = new Rect(0.36f, 0.56f, 0.64f, 0.13f);
            CreateScanlines(go, ScanlineRows * bandFraction.height, bandFraction);

            // Two overlapping hairline frames: the first is the stronger of the pair.
            CreateFractionSprite(go, UIGeneratorContent.GameObjects.FrameChild, _kit.Frame, true, new Rect(0.60f, 0.24f, 0.31f, 0.56f));
            CreateFractionSprite(go, UIGeneratorContent.GameObjects.FrameChild, _kit.Frame, true, new Rect(0.70f, 0.14f, 0.17f, 0.36f));

            CreateFractionSprite(go, UIGeneratorContent.GameObjects.WaveChild, _kit.Wave, false, new Rect(0.28f, 0.10f, 0.76f, 0.42f));
        }

        private static void CreateFractionSprite(GameObject parent, string objectName, Sprite sprite, bool sliced, Rect fraction)
        {
            if (sprite == null) return;

            var image = CreateSprite(parent, objectName, sprite, sliced);
            SetFraction((RectTransform)image.transform, fraction);
        }

        // ── Panel: the surface the menu is read on ───────────────────────────
        // Floats 64 in from the left, top and bottom. Inside it: scanlines; around
        // it: a hairline frame, an offset echo of that frame, and registration marks.

        private static void CreatePanel(GameObject parent, string objectName)
        {
            var go   = CreateDecor(parent, objectName);
            var rect = (RectTransform)go.transform;
            rect.anchorMin        = Vector2.zero;
            rect.anchorMax        = new Vector2(0f, 1f);
            rect.pivot            = new Vector2(0f, 0.5f);
            rect.sizeDelta        = new Vector2(PanelWidth, -PanelMargin * 2f);
            rect.anchoredPosition = new Vector2(PanelMargin, 0f);

            go.AddComponent<Image>().raycastTarget = false;

            CreateScanlines(go, ScanlineRows, new Rect(0f, 0f, 1f, 1f));

            if (_kit.Frame == null) return;

            var frame = CreateSprite(go, UIGeneratorContent.GameObjects.FrameChild, _kit.Frame, true);
            SetStretch((RectTransform)frame.transform);

            var echo     = CreateSprite(go, UIGeneratorContent.GameObjects.FrameEchoChild, _kit.Frame, true);
            var echoRect = (RectTransform)echo.transform;
            SetStretch(echoRect);
            echoRect.offsetMin = new Vector2(EchoOffset, -EchoOffset);
            echoRect.offsetMax = new Vector2(EchoOffset, -EchoOffset);

            // Registration marks where the echo frame's corners fall.
            CreateCross(go, Vector2.one,  new Vector2(EchoOffset, -EchoOffset));
            CreateCross(go, Vector2.zero, new Vector2(EchoOffset, -EchoOffset));
        }

        // A RawImage tiles a repeating texture in one quad; an Image would build a
        // quad per tile.
        private static void CreateScanlines(GameObject parent, float rows, Rect fraction)
        {
            if (_kit.Scanlines == null) return;

            var go = CreateUIObject(UIGeneratorContent.GameObjects.ScanlinesChild, parent);
            SetFraction((RectTransform)go.transform, fraction);

            var raw = go.AddComponent<RawImage>();
            raw.texture       = _kit.Scanlines;
            raw.uvRect        = new Rect(0f, 0f, 1f, rows);
            raw.raycastTarget = false;
        }

        private static void CreateCross(GameObject parent, Vector2 anchor, Vector2 offset)
        {
            if (_kit.Cross == null) return;

            var cross = CreateSprite(parent, UIGeneratorContent.GameObjects.CrossChild, _kit.Cross, false);
            var rect  = (RectTransform)cross.transform;
            rect.anchorMin        = anchor;
            rect.anchorMax        = anchor;
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.sizeDelta        = new Vector2(CrossSize, CrossSize);
            rect.anchoredPosition = offset;
        }

        // ── Title, rule, small print ─────────────────────────────────────────

        private static void CreateTitle(GameObject parent, string objectName, string displayText)
        {
            // Sized to the column: TMP picks the largest size at which the name fits
            // the title box, on one line or — for long names — two, so it never
            // leaves the panel. Static text, so auto-sizing costs one layout pass.
            // Letter case is a style the theme sets, never the text itself: the
            // developer types the name normally.
            var tmp = CreateText(parent, objectName, displayText, TextAlignmentOptions.BottomLeft);
            tmp.enableAutoSizing = true;
            tmp.lineSpacing      = -10f;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode     = TextOverflowModes.Overflow;

            var le = tmp.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth  = ColumnWidth;
            le.preferredHeight = TitleHeight;
        }

        // A hairline the width of the column, closed by a registration mark. The
        // holder reserves the gap between the title and whatever comes next.
        private static void CreateRule(GameObject parent, string objectName)
        {
            var holder = CreateUIObject(objectName, parent);

            var le = holder.AddComponent<LayoutElement>();
            le.preferredWidth  = ColumnWidth;
            le.preferredHeight = RuleGap;

            var lineGO = CreateUIObject(UIGeneratorContent.GameObjects.RuleBarChild, holder);
            var rect   = (RectTransform)lineGO.transform;
            rect.anchorMin        = new Vector2(0f, 1f);
            rect.anchorMax        = Vector2.one;
            rect.pivot            = new Vector2(0f, 1f);
            rect.sizeDelta        = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(0f, -CrossSize * 0.5f);

            lineGO.AddComponent<Image>().raycastTarget = false;

            CreateCross(holder, Vector2.one, new Vector2(0f, -CrossSize * 0.5f));
        }

        // The small print every shipped main menu carries. Pinned to the foot of
        // the panel, outside the layout group. The generated script keeps it live.
        private static void CreateFootnote(GameObject parent, string objectName, string text)
        {
            var tmp  = CreateText(parent, objectName, text, TextAlignmentOptions.BottomLeft);
            var rect = (RectTransform)tmp.transform;
            rect.anchorMin        = Vector2.zero;
            rect.anchorMax        = Vector2.zero;
            rect.pivot            = Vector2.zero;
            rect.sizeDelta        = new Vector2(ColumnWidth, 24f);
            rect.anchoredPosition = new Vector2(PanelMargin + PanelPadding, PanelMargin + FootnoteRise);

            tmp.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        }

        // Takes all the height the column does not need, which is what pushes the
        // menu to the bottom of the panel on any screen height.
        private static void CreateSpacer(GameObject parent, string objectName)
        {
            var go = CreateUIObject(objectName, parent);

            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = ColumnWidth;
            le.flexibleHeight = 1f;
        }

        // ── Settings rows ────────────────────────────────────────────────────
        // Real controls: three settings every project has, each shown with the
        // project's actual current value. Every control is a Button that calls a
        // no-argument method, so the rows go through the same discover / wire /
        // audit pipeline as every other UIPilot button, and keyboard and gamepad
        // reach them through ordinary UI navigation.

        private static void CreateSettingsRows(GameObject panelGO)
        {
            var fill = CreateStepperRow(panelGO,
                UIGeneratorContent.GameObjects.VolumeRow,
                UIGeneratorContent.Art.IconVolume,
                UIGeneratorContent.Settings.VolumeLabel,
                UIGeneratorContent.Buttons.VolumeDown,
                UIGeneratorContent.Buttons.VolumeUp,
                UIGeneratorContent.GameObjects.VolumeValue,
                UIGeneratorContent.Settings.FullVolume,
                true);
            fill.anchorMax = Vector2.one; // AudioListener.volume starts at 1

            var fullscreen = CreateButton(panelGO,
                UIGeneratorContent.Buttons.Fullscreen,
                UIGeneratorContent.Settings.FullscreenLabel);

            var state = CreateText(fullscreen, UIGeneratorContent.GameObjects.FullscreenValue,
                PlayerSettings.fullScreenMode == FullScreenMode.Windowed
                    ? UIGeneratorContent.Settings.Off
                    : UIGeneratorContent.Settings.On,
                TextAlignmentOptions.Right);
            var stateRect = (RectTransform)state.transform;
            SetStretch(stateRect);
            stateRect.offsetMax = new Vector2(-ValueInset, 0f);

            CreateStepperRow(panelGO,
                UIGeneratorContent.GameObjects.QualityRow,
                UIGeneratorContent.Art.IconQuality,
                UIGeneratorContent.Settings.QualityLabel,
                UIGeneratorContent.Buttons.QualityDown,
                UIGeneratorContent.Buttons.QualityUp,
                UIGeneratorContent.GameObjects.QualityValue,
                QualitySettings.names[QualitySettings.GetQualityLevel()],
                false);
        }

        // Icon and label on the left; on the right a "‹ value ›" stepper. With a
        // meter, the value gets a thin track under it whose fill the script
        // resizes. Returns the fill (null without a meter).
        private static RectTransform CreateStepperRow(GameObject parent, string rowName, string iconName,
            string labelText, string downLabel, string upLabel, string valueName, string valueText, bool withMeter)
        {
            var rowGO = CreateUIObject(rowName, parent);
            var le    = rowGO.AddComponent<LayoutElement>();
            le.preferredWidth  = ColumnWidth;
            le.preferredHeight = RowHeight;

            CreateIcon(rowGO, iconName, IconSize, new Vector2(0f, 0.5f));

            var label     = CreateText(rowGO, UIGeneratorContent.GameObjects.RowLabelChild, labelText, TextAlignmentOptions.Left);
            var labelRect = (RectTransform)label.transform;
            SetStretch(labelRect);
            labelRect.offsetMin = new Vector2(LabelInset, 0f);

            // Created left to right, so hierarchy order — which is what first-focus
            // and the Inspector follow — matches what the player sees. Positions
            // are measured from the row's right edge.
            CreateStepButton(rowGO, downLabel, UIGeneratorContent.Art.IconChevronLeft, StepSize + StepValueWidth);

            var value = CreateText(rowGO, valueName, valueText, TextAlignmentOptions.Center);
            AnchorRight((RectTransform)value.transform, StepSize, new Vector2(StepValueWidth, StepSize));

            var fill = withMeter ? CreateMeter(rowGO) : null;

            CreateStepButton(rowGO, upLabel, UIGeneratorContent.Art.IconChevronRight, 0f);

            return fill;
        }

        private static RectTransform CreateMeter(GameObject rowGO)
        {
            var trackGO   = CreateUIObject(UIGeneratorContent.GameObjects.VolumeTrack, rowGO);
            var trackRect = (RectTransform)trackGO.transform;
            AnchorRight(trackRect, StepSize + MeterInset, new Vector2(StepValueWidth - MeterInset * 2f, MeterHeight));
            trackRect.anchoredPosition += new Vector2(0f, -MeterDrop);
            trackGO.AddComponent<Image>().raycastTarget = false;

            var fillGO   = CreateUIObject(UIGeneratorContent.GameObjects.VolumeFill, trackGO);
            var fillRect = (RectTransform)fillGO.transform;
            SetStretch(fillRect);
            fillGO.AddComponent<Image>().raycastTarget = false;

            return fillRect;
        }

        // A square chevron button pinned to the right of its row.
        private static void CreateStepButton(GameObject rowGO, string label, string iconName, float rightOffset)
        {
            var btnGO = CreateUIObject(UIGeneratorContent.GameObjects.ButtonPrefix + label, rowGO);
            AnchorRight((RectTransform)btnGO.transform, rightOffset, new Vector2(StepSize, StepSize));

            AddButton(btnGO);
            CreateIcon(btnGO, iconName, StepIconSize, new Vector2(0.5f, 0.5f));
        }

        // ── Menu item ────────────────────────────────────────────────────────
        // The label names the object and picks the method (see UIGeneratorContent
        // .Buttons); the display text is what the player reads, and defaults to it.

        private static GameObject CreateButton(GameObject parent, string label, string displayText = null)
        {
            var btnGO = CreateUIObject(UIGeneratorContent.GameObjects.ButtonPrefix + label, parent);

            var le = btnGO.AddComponent<LayoutElement>();
            le.preferredWidth  = ColumnWidth;
            le.preferredHeight = RowHeight;

            AddButton(btnGO);
            CreateIcon(btnGO, GetIconName(label), IconSize, new Vector2(0f, 0.5f));

            // Left = middle of the *line*, not of the glyphs: every label sits on
            // the same baseline whether or not the word has a descender.
            var text     = CreateText(btnGO, UIGeneratorContent.GameObjects.ButtonTextChild, displayText ?? label, TextAlignmentOptions.Left);
            var textRect = (RectTransform)text.transform;
            SetStretch(textRect);
            textRect.offsetMin = new Vector2(LabelInset, 0f);

            return btnGO;
        }

        private static string GetIconName(string label)
        {
            switch (label)
            {
                case UIGeneratorContent.Buttons.Play:
                case UIGeneratorContent.Buttons.Resume:     return UIGeneratorContent.Art.IconPlay;
                case UIGeneratorContent.Buttons.Settings:   return UIGeneratorContent.Art.IconSettings;
                case UIGeneratorContent.Buttons.Quit:       return UIGeneratorContent.Art.IconQuit;
                case UIGeneratorContent.Buttons.Back:       return UIGeneratorContent.Art.IconBack;
                case UIGeneratorContent.Buttons.Fullscreen: return UIGeneratorContent.Art.IconFullscreen;
                default:                                    return null;
            }
        }

        // Makes the object a Button. Two graphics, two jobs. An invisible Image exactly
        // the size of the button takes the clicks. The focus plate is the Button's
        // target graphic, so Unity's own Color Tint drives the focus effect with no
        // script. The plate does not take clicks: under a theme with a glowing frame it
        // is larger than the button, and neighbouring hit areas would overlap. Its
        // sprite, size and colours belong to the theme (UIGeneratorStyler).
        private static void AddButton(GameObject btnGO)
        {
            btnGO.AddComponent<Image>().color = Color.clear;

            var plateGO = CreateUIObject(UIGeneratorContent.GameObjects.ButtonBarChild, btnGO);
            SetStretch((RectTransform)plateGO.transform);

            var plate = plateGO.AddComponent<Image>();
            plate.raycastTarget = false;

            btnGO.AddComponent<Button>().targetGraphic = plate;
        }

        // ── Primitives ───────────────────────────────────────────────────────

        private static void CreateIcon(GameObject parent, string iconName, float size, Vector2 anchor)
        {
            var sprite = iconName != null ? UIGeneratorArt.Sprite(iconName) : null;
            if (sprite == null) return;

            var icon = CreateSprite(parent, UIGeneratorContent.GameObjects.IconChild, sprite, false);
            var rect = (RectTransform)icon.transform;
            rect.anchorMin        = anchor;
            rect.anchorMax        = anchor;
            rect.pivot            = anchor;
            rect.sizeDelta        = new Vector2(size, size);
            rect.anchoredPosition = Vector2.zero;
        }

        private static Image CreateSprite(GameObject parent, string objectName, Sprite sprite, bool sliced)
        {
            var go    = CreateUIObject(objectName, parent);
            var image = go.AddComponent<Image>();
            image.sprite        = sprite;
            image.raycastTarget = false;

            if (sliced)
            {
                image.type       = Image.Type.Sliced;
                image.fillCenter = false;
            }

            return image;
        }

        // Size, colour, letter case, tracking and font all come from the theme.
        private static TextMeshProUGUI CreateText(GameObject parent, string objectName, string text,
            TextAlignmentOptions alignment)
        {
            var go  = CreateUIObject(objectName, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text             = text;
            tmp.alignment        = alignment;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget    = false;
            return tmp;
        }

        // A decorative child that sits outside the layout group.
        private static GameObject CreateDecor(GameObject parent, string objectName)
        {
            var go = CreateUIObject(objectName, parent);
            go.AddComponent<LayoutElement>().ignoreLayout = true;
            return go;
        }

        // Places a rect by fractions of its parent.
        private static void SetFraction(RectTransform rect, Rect fraction)
        {
            rect.anchorMin = fraction.min;
            rect.anchorMax = fraction.max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void AnchorRight(RectTransform rect, float rightOffset, Vector2 size)
        {
            rect.anchorMin        = new Vector2(1f, 0.5f);
            rect.anchorMax        = new Vector2(1f, 0.5f);
            rect.pivot            = new Vector2(1f, 0.5f);
            rect.sizeDelta        = size;
            rect.anchoredPosition = new Vector2(-rightOffset, 0f);
        }

        private static GameObject CreateUIObject(string name, GameObject parent)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();

            if (parent != null)
            {
                go.transform.SetParent(parent.transform, false);
                go.layer = parent.layer;
            }

            return go;
        }

        private static void SetStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // ── The art kit, loaded once per build ───────────────────────────────

        private sealed class Kit
        {
            internal Sprite    Glow, Frame, Cross, Wave;
            internal Texture2D Scanlines;

            internal static Kit Load()
            {
                return new Kit
                {
                    Glow      = UIGeneratorArt.Sprite(UIGeneratorContent.Art.Glow),
                    Frame     = UIGeneratorArt.Sprite(UIGeneratorContent.Art.Frame),
                    Cross     = UIGeneratorArt.Sprite(UIGeneratorContent.Art.Cross),
                    Wave      = UIGeneratorArt.Sprite(UIGeneratorContent.Art.Wave),
                    Scanlines = UIGeneratorArt.Texture(UIGeneratorContent.Art.Scanlines)
                };
            }
        }
    }
}
