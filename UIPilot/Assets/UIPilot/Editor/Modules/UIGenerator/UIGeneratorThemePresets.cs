using System;
using UnityEngine;
using UIPilot.Editor.Core;

namespace UIPilot.Editor.Modules.UIGenerator
{
    // The looks UIPilot ships. Each is also saved as an asset in Assets/UIPilot/Themes
    // for the developer to pick, duplicate and edit; these factories are what those
    // assets were made from, and SoftClub() is the fallback when no theme is selected.
    // Specified in design-system/uipilot/pages/generated-menus.md.
    internal static class UIGeneratorThemePresets
    {
        // Every shipped preset by asset name. Restore Default Themes recreates any
        // that are missing from this list, so a new preset must be added here.
        internal static readonly (string Name, Func<UIPilotTheme> Create)[] All =
        {
            (UIPilotLabels.Theme.DefaultAsset, SoftClub),
            (UIPilotLabels.Theme.NightAsset,   SoftClubNight),
            (UIPilotLabels.Theme.InkAsset,     Ink),
            (UIPilotLabels.Theme.FantasyAsset, FantasyRpg),
            (UIPilotLabels.Theme.JrpgAsset,    JrpgWindow),
            (UIPilotLabels.Theme.PixelAsset,   PixelRetro),
            (UIPilotLabels.Theme.SciFiAsset,   SciFiHud),
            (UIPilotLabels.Theme.ShooterAsset, MilitaryShooter),
        };

        // Gen X soft club / PS3-era adverts: airy blue wash, cobalt glass, hairlines,
        // a ribbon of light, wide lowercase lettering. The theme's own field defaults.
        internal static UIPilotTheme SoftClub()
        {
            var theme = ScriptableObject.CreateInstance<UIPilotTheme>();
            theme.font          = UIGeneratorArt.Font();
            theme.titleMaterial = UIGeneratorArt.GlowMaterial();
            return theme;
        }

        // The same artwork in a dark key: deep navy night, violet and cyan blooms.
        internal static UIPilotTheme SoftClubNight()
        {
            var theme = SoftClub();

            // The wash is far more opaque than the day theme's: in a Linear-colour
            // project a bright scene bleeds through a dark veil much more than the
            // alpha suggests, and at 77% this read as grey, not as night.
            theme.wash             = new Color32(0x06, 0x0A, 0x24, 0xEE);
            theme.bloomTopLeft     = new Color32(0x6A, 0x45, 0xF0, 0xB3);
            theme.bloomBottomRight = new Color32(0x22, 0xC3, 0xEE, 0x99);
            theme.bloomTopRight    = new Color32(0xA2, 0x4D, 0xF0, 0x73);
            theme.bloomCentre      = new Color32(0xA6, 0xDC, 0xFF, 0x30);
            theme.ribbonColor      = new Color32(0xBF, 0xE6, 0xFF, 0xA6);
            theme.sceneFrameColor  = new Color32(0xBF, 0xE6, 0xFF, 0x73);
            theme.sceneScanlineColor = new Color32(0xBF, 0xE6, 0xFF, 0x1A);

            theme.panel               = new Color32(0x09, 0x10, 0x36, 0xFA);
            theme.panelFrameColor     = new Color32(0xD6, 0xEC, 0xFF, 0xCC);
            theme.panelFrameEchoColor = new Color32(0xD6, 0xEC, 0xFF, 0x59);

            theme.text       = new Color32(0xEA, 0xF6, 0xFF, 0xFF);
            theme.textSoft   = new Color32(0xA9, 0xC4, 0xF5, 0xFF);
            theme.rule       = new Color32(0xD6, 0xEC, 0xFF, 0x80);
            theme.meterTrack = new Color32(0xD6, 0xEC, 0xFF, 0x33);
            theme.meterFill  = new Color32(0xEA, 0xF6, 0xFF, 0xFF);
            return theme;
        }

        // Dark, flat and quiet: an opaque ink panel, a teal focus bar, the default
        // font in capitals, no artwork at all. Genre-neutral and high contrast.
        internal static UIPilotTheme Ink()
        {
            var theme = Bare();

            theme.wash              = new Color32(0x04, 0x06, 0x0A, 0xA6);
            theme.panel             = new Color32(0x12, 0x18, 0x26, 0xFF);
            theme.panelFrame        = true;
            theme.panelFrameColor   = new Color32(0x2B, 0x36, 0x50, 0xFF);

            theme.text          = new Color32(0xF3, 0xF5, 0xF8, 0xFF);
            theme.textSoft      = new Color32(0x97, 0xA1, 0xB3, 0xFF);
            theme.titleCase     = UIPilotTextCase.Uppercase;
            theme.titleBold     = true;
            theme.titleTracking = 8f;
            theme.titleSize     = 60f;
            theme.itemCase      = UIPilotTextCase.AsTyped;
            theme.itemTracking  = 0f;
            theme.itemSize      = 30f;
            theme.valueSize     = 24f;
            theme.smallSize     = 18f;

            // The accent means one thing — focus is here — so the mouse pointer
            // gets a neutral bar instead.
            theme.focusStyle    = UIPilotFocusStyle.Bar;
            theme.focusIdle     = new Color32(0x2A, 0x35, 0x50, 0x00);
            theme.focusHover    = new Color32(0x2A, 0x35, 0x50, 0xFF);
            theme.focusSelected = new Color32(0x0C, 0x7C, 0x7E, 0xFF);
            theme.focusPressed  = new Color32(0x09, 0x5F, 0x61, 0xFF);
            theme.focusDisabled = new Color32(0x0C, 0x10, 0x1A, 0xFF);

            theme.rule       = new Color32(0x0C, 0x7C, 0x7E, 0xFF);
            theme.meterTrack = new Color32(0x2A, 0x35, 0x50, 0xFF);
            theme.meterFill  = new Color32(0x0C, 0x7C, 0x7E, 0xFF);
            return theme;
        }

        // A storybook RPG: an illuminated parchment page in a gilded frame, lit by
        // candlelight. Ink-brown serif lettering, a gold-leaf bar with a diamond on
        // the chosen line, a carved divider under the title.
        internal static UIPilotTheme FantasyRpg()
        {
            var theme = Bare();

            // The valley at dusk: castle on its crag, windows just lit. The layers
            // carry the light, so the wash is a light veil, not the mood.
            theme.sceneLayers = SceneLayers(UIGeneratorContent.Art.FantasyScene);
            theme.timeOfDay   = 0.75f;

            theme.wash             = new Color32(0x14, 0x0C, 0x06, 0x4D);
            theme.blooms           = true;
            theme.bloomTopLeft     = new Color32(0xFF, 0xB2, 0x5B, 0x26);   // faint: the scene lights itself
            theme.bloomBottomRight = new Color32(0xFF, 0x7A, 0x2E, 0x1A);
            theme.bloomTopRight    = new Color32(0xFF, 0xDD, 0x9E, 0x20);
            theme.bloomCentre      = new Color32(0xFF, 0xE8, 0xC0, 0x00);

            theme.panel     = Color.white;                      // the parchment carries its own colour
            theme.panelArt  = UIGeneratorArt.Sprite(UIGeneratorContent.Art.FantasyPanel);

            theme.font          = UIGeneratorArt.Font(UIGeneratorContent.Art.CrimsonPro);
            theme.titleFont     = UIGeneratorArt.Font(UIGeneratorContent.Art.YoungSerif);
            theme.text          = new Color32(0x3A, 0x25, 0x14, 0xFF);   // iron-gall ink, 10:1 on the page
            theme.textSoft      = new Color32(0x55, 0x3A, 0x20, 0xFF);   // 4.5:1 even on the scorched edge
            theme.titleCase     = UIPilotTextCase.Uppercase;
            theme.titleTracking = 6f;
            theme.titleSize     = 54f;
            theme.icons         = false;
            theme.itemCase      = UIPilotTextCase.AsTyped;
            theme.itemTracking  = 1f;
            theme.itemSize      = 32f;
            theme.valueSize     = 28f;
            theme.smallSize     = 17f;

            theme.focusStyle    = UIPilotFocusStyle.Bar;
            theme.focusArt      = UIGeneratorArt.Sprite(UIGeneratorContent.Art.FantasyFocus);
            SetFocusTint(theme, 0.5f);

            theme.rule       = Color.white;
            theme.ruleArt    = UIGeneratorArt.Sprite(UIGeneratorContent.Art.FantasyRule);
            theme.ruleHeight = 28f;
            theme.meterTrack = new Color32(0x3A, 0x25, 0x14, 0x40);
            theme.meterFill  = new Color32(0x8E, 0x2A, 0x1E, 0xFF);       // sealing-wax red
            return theme;
        }

        // The console RPG window: a deep blue gradient box with a white double
        // border, clean capitals with a drop shadow, a pointing glove as the cursor.
        internal static UIPilotTheme JrpgWindow()
        {
            var theme = Bare();

            theme.wash      = new Color32(0x00, 0x02, 0x12, 0x73);
            theme.panel     = Color.white;
            theme.panelArt  = UIGeneratorArt.Sprite(UIGeneratorContent.Art.JrpgPanel);

            theme.font          = UIGeneratorArt.Font(UIGeneratorContent.Art.WorkSans);
            theme.titleMaterial = UIGeneratorArt.Material(UIGeneratorContent.Art.WorkSansShadow);
            theme.textMaterial  = UIGeneratorArt.Material(UIGeneratorContent.Art.WorkSansShadow);
            theme.text          = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
            theme.textSoft      = new Color32(0xC4, 0xCF, 0xFF, 0xFF);
            theme.titleCase     = UIPilotTextCase.Uppercase;
            theme.titleTracking = 8f;
            theme.titleSize     = 46f;
            theme.icons         = false;
            theme.itemCase      = UIPilotTextCase.Uppercase;
            theme.itemTracking  = 3f;
            theme.itemSize      = 27f;
            theme.valueSize     = 23f;
            theme.smallSize     = 16f;

            theme.focusStyle = UIPilotFocusStyle.Marker;
            theme.focusArt   = UIGeneratorArt.Sprite(UIGeneratorContent.Art.JrpgCursor);
            theme.markerSize = new Vector2(38f, 30f);
            SetFocusTint(theme, 0.45f);

            theme.rule       = new Color32(0xFF, 0xFF, 0xFF, 0x8C);
            theme.ruleHeight = 2f;
            theme.meterTrack = new Color32(0xFF, 0xFF, 0xFF, 0x40);
            theme.meterFill  = Color.white;
            return theme;
        }

        // 8-bit: a black window with a white stepped border, pixel capitals, a
        // yellow arrow for the cursor, faint CRT lines across the window.
        internal static UIPilotTheme PixelRetro()
        {
            var theme = Bare();

            theme.wash               = new Color32(0x04, 0x04, 0x0C, 0x99);

            theme.panel              = Color.white;
            theme.panelArt           = UIGeneratorArt.Sprite(UIGeneratorContent.Art.PixelPanel);
            theme.panelArtScale      = 0.25f;                   // one art pixel = four units
            theme.panelScanlines     = true;
            theme.panelScanlineColor = new Color32(0xFF, 0xFF, 0xFF, 0x0A);

            theme.font          = UIGeneratorArt.Font(UIGeneratorContent.Art.Silkscreen);
            theme.titleMaterial = UIGeneratorArt.Material(UIGeneratorContent.Art.SilkscreenShadow);
            theme.text          = new Color32(0xF4, 0xF1, 0xE8, 0xFF);
            theme.textSoft      = new Color32(0xA8, 0xA8, 0xC0, 0xFF);
            theme.titleCase     = UIPilotTextCase.Uppercase;
            theme.titleTracking = 4f;
            theme.titleSize     = 48f;
            theme.icons         = false;
            theme.itemCase      = UIPilotTextCase.Uppercase;
            theme.itemTracking  = 2f;
            theme.itemSize      = 26f;
            theme.valueSize     = 22f;
            theme.smallSize     = 16f;

            theme.focusStyle    = UIPilotFocusStyle.Marker;
            theme.focusArt      = UIGeneratorArt.Sprite(UIGeneratorContent.Art.PixelCursor);
            theme.markerSize    = new Vector2(20f, 32f);           // 5x8 art pixels, 4x
            theme.focusIdle     = new Color32(0xF8, 0xD0, 0x30, 0x00);
            theme.focusHover    = new Color32(0xF4, 0xF1, 0xE8, 0x73);
            theme.focusSelected = new Color32(0xF8, 0xD0, 0x30, 0xFF);   // the arrow is yellow
            theme.focusPressed  = new Color32(0xFF, 0xF0, 0xA0, 0xFF);
            theme.focusDisabled = new Color32(0xF4, 0xF1, 0xE8, 0x1F);

            theme.rule       = new Color32(0xF4, 0xF1, 0xE8, 0xFF);
            theme.ruleHeight = 4f;
            theme.meterTrack = new Color32(0xF4, 0xF1, 0xE8, 0x40);
            theme.meterFill  = new Color32(0xF8, 0xD0, 0x30, 0xFF);
            return theme;
        }

        // A starship console: a clipped-corner glass slab edged in cyan, a scanning
        // focus bar, technical capitals with a faint glow, brackets and scanlines.
        internal static UIPilotTheme SciFiHud()
        {
            var theme = Bare();

            var cyan = new Color32(0x37, 0xE0, 0xFF, 0xFF);

            theme.wash               = new Color32(0x01, 0x06, 0x0D, 0xC4);
            theme.blooms             = true;
            theme.bloomTopLeft       = new Color32(0x1F, 0xC8, 0xE8, 0x38);
            theme.bloomBottomRight   = new Color32(0x0F, 0xA3, 0xB1, 0x40);
            theme.bloomTopRight      = new Color32(0x37, 0xE0, 0xFF, 0x1F);
            theme.bloomCentre        = new Color32(0x37, 0xE0, 0xFF, 0x00);
            theme.sceneFrames        = true;
            theme.sceneFrameColor    = new Color32(0x37, 0xE0, 0xFF, 0x59);
            theme.sceneScanlines     = true;
            theme.sceneScanlineColor = new Color32(0x37, 0xE0, 0xFF, 0x1A);

            theme.panel              = Color.white;
            theme.panelArt           = UIGeneratorArt.Sprite(UIGeneratorContent.Art.SciFiPanel);
            theme.panelScanlines     = true;
            theme.panelScanlineColor = new Color32(0x37, 0xE0, 0xFF, 0x0D);
            theme.panelFrameColor    = cyan;                    // colours the registration marks
            theme.registrationMarks  = true;

            theme.font          = UIGeneratorArt.Font(UIGeneratorContent.Art.Tektur);
            theme.titleMaterial = UIGeneratorArt.Material(UIGeneratorContent.Art.TekturGlow);
            theme.text          = new Color32(0xE4, 0xFB, 0xFF, 0xFF);
            theme.textSoft      = new Color32(0x86, 0xD9, 0xE6, 0xFF);
            theme.titleCase     = UIPilotTextCase.Uppercase;
            theme.titleTracking = 8f;
            theme.titleSize     = 50f;
            theme.itemCase      = UIPilotTextCase.Uppercase;
            theme.itemTracking  = 4f;
            theme.itemSize      = 25f;
            theme.valueSize     = 21f;
            theme.smallSize     = 15f;

            theme.focusStyle = UIPilotFocusStyle.Bar;
            theme.focusArt   = UIGeneratorArt.Sprite(UIGeneratorContent.Art.SciFiFocus);
            SetFocusTint(theme, 0.4f);

            theme.rule       = Color.white;
            theme.ruleArt    = UIGeneratorArt.Sprite(UIGeneratorContent.Art.SciFiRule);
            theme.ruleHeight = 16f;
            theme.meterTrack = new Color32(0x37, 0xE0, 0xFF, 0x33);
            theme.meterFill  = cyan;
            return theme;
        }

        private static UIPilotSceneLayer[] SceneLayers((string Sprite, string Material)[] names)
        {
            var layers = new UIPilotSceneLayer[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                layers[i].sprite   = UIGeneratorArt.Sprite(names[i].Sprite);
                layers[i].material = UIGeneratorArt.Material(names[i].Material);
            }
            return layers;
        }

        // The modern military shooter: a near-black slab with one amber edge,
        // condensed capitals, hazard stripes, a tactical overlay over a grey
        // valley. Nothing glows; the slickness is in the restraint.
        internal static UIPilotTheme MilitaryShooter()
        {
            var theme = Bare();

            theme.sceneLayers = SceneLayers(UIGeneratorContent.Art.ShooterScene);
            theme.timeOfDay   = 0.55f;                          // an overcast afternoon

            theme.wash              = new Color32(0x05, 0x07, 0x0A, 0x73);
            theme.registrationMarks = true;
            theme.panelFrameColor   = new Color32(0xB8, 0xC0, 0xC8, 0xFF);   // colours the marks

            theme.panel     = Color.white;
            theme.panelArt  = UIGeneratorArt.Sprite(UIGeneratorContent.Art.ShooterPanel);

            theme.font          = UIGeneratorArt.Font(UIGeneratorContent.Art.BigShoulders);
            theme.text          = new Color32(0xE8, 0xEA, 0xED, 0xFF);
            theme.textSoft      = new Color32(0x9A, 0xA3, 0xAD, 0xFF);
            theme.titleCase     = UIPilotTextCase.Uppercase;
            theme.titleBold     = false;                        // the face is already bold
            theme.titleTracking = 1f;
            theme.titleSize     = 64f;
            theme.itemCase      = UIPilotTextCase.Uppercase;
            theme.itemTracking  = 2f;
            theme.itemSize      = 30f;
            theme.valueSize     = 25f;
            theme.smallSize     = 16f;

            theme.focusStyle = UIPilotFocusStyle.Bar;
            theme.focusArt   = UIGeneratorArt.Sprite(UIGeneratorContent.Art.ShooterFocus);
            SetFocusTint(theme, 0.5f);

            theme.rule       = Color.white;
            theme.ruleArt    = UIGeneratorArt.Sprite(UIGeneratorContent.Art.ShooterRule);
            theme.ruleHeight = 12f;
            theme.meterTrack = new Color32(0xFF, 0xFF, 0xFF, 0x33);
            theme.meterFill  = new Color32(0xF2, 0xA3, 0x3A, 0xFF);           // the amber of the edge bar
            return theme;
        }

        // Every decoration off: the starting point for a theme that brings its own.
        private static UIPilotTheme Bare()
        {
            var theme = ScriptableObject.CreateInstance<UIPilotTheme>();
            theme.blooms            = false;
            theme.ribbon            = false;
            theme.sceneFrames       = false;
            theme.sceneScanlines    = false;
            theme.panelScanlines    = false;
            theme.panelFrame        = false;
            theme.panelFrameEcho    = false;
            theme.registrationMarks = false;
            theme.font              = null;   // TextMeshPro's default
            theme.titleMaterial     = null;
            return theme;
        }

        // For focus artwork that carries its own colours: white shows it as drawn,
        // hover shows it fainter, rest hides it.
        private static void SetFocusTint(UIPilotTheme theme, float hoverAlpha)
        {
            theme.focusIdle     = new Color(1f, 1f, 1f, 0f);
            theme.focusHover    = new Color(1f, 1f, 1f, hoverAlpha);
            theme.focusSelected = Color.white;
            theme.focusPressed  = new Color(0.85f, 0.85f, 0.85f, 1f);
            theme.focusDisabled = new Color(1f, 1f, 1f, 0.12f);
        }
    }
}
