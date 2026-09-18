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
            var theme = ScriptableObject.CreateInstance<UIPilotTheme>();

            theme.wash           = new Color32(0x04, 0x06, 0x0A, 0xA6);
            theme.blooms         = false;
            theme.ribbon         = false;
            theme.sceneFrames    = false;
            theme.sceneScanlines = false;

            theme.panel             = new Color32(0x12, 0x18, 0x26, 0xFF);
            theme.panelScanlines    = false;
            theme.panelFrame        = true;
            theme.panelFrameColor   = new Color32(0x2B, 0x36, 0x50, 0xFF);
            theme.panelFrameEcho    = false;
            theme.registrationMarks = false;

            theme.font          = null; // TextMeshPro's default
            theme.titleMaterial = null;
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
            theme.focusFrame    = false;
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
    }
}
