using TMPro;
using UnityEngine;

namespace UIPilot.Editor.Core
{
    public enum UIPilotTextCase
    {
        AsTyped,
        Lowercase,
        Uppercase
    }

    // Everything that decides how a generated menu LOOKS, in one asset the developer
    // owns. Build UI reads it; Apply Theme pushes it onto menus that already exist.
    // Duplicate a preset (Assets/UIPilot/Themes) to make your own.
    //
    // A theme never changes structure: every menu has the same objects, and a theme
    // only recolours them, swaps the lettering, and switches decorations on or off.
    // That is what lets one theme be applied over another without rebuilding.
    //
    // Editor-only. The asset is never referenced by a scene, so it is not in builds.
    // Field defaults are the Soft Club look minus the font, which a field default
    // cannot reference. That is why Create > UIPilot > Theme is a menu item in
    // UIGeneratorModule, which starts from the full preset, not a [CreateAssetMenu].
    public sealed class UIPilotTheme : ScriptableObject
    {
        [Header("Atmosphere — drawn over your scene, behind the menu")]
        [Tooltip("A veil over the whole screen. Its alpha decides how much of the game shows through.")]
        public Color wash = new Color32(0x9D, 0xB6, 0xF2, 0xB8);

        [Tooltip("Four huge soft blooms of colour that drift across the screen.")]
        public bool blooms = true;
        public Color bloomTopLeft     = new Color32(0xB3, 0xA6, 0xF7, 0x8C);
        public Color bloomBottomRight = new Color32(0x7A, 0xD7, 0xEC, 0x99);
        public Color bloomTopRight    = new Color32(0xD2, 0xF7, 0xEC, 0x80);
        public Color bloomCentre      = new Color32(0xFF, 0xFF, 0xFF, 0x59);

        [Tooltip("The flowing ribbon of light across the scene side.")]
        public bool ribbon = true;
        public Color ribbonColor = new Color32(0xFF, 0xFF, 0xFF, 0x99);

        [Tooltip("Two thin overlapping frames drifting over the scene side.")]
        public bool sceneFrames = true;
        public Color sceneFrameColor = new Color32(0xFF, 0xFF, 0xFF, 0x80);

        [Tooltip("A horizontal band of scanlines across the scene side.")]
        public bool sceneScanlines = true;
        public Color sceneScanlineColor = new Color32(0xFF, 0xFF, 0xFF, 0x1F);

        [Header("Panel — the surface the menu text is read on")]
        [Tooltip("Keep this nearly opaque. If the scene shows through it, the scene decides your text contrast.")]
        public Color panel = new Color32(0x23, 0x43, 0xAE, 0xF2);

        public bool panelScanlines = true;
        public Color panelScanlineColor = new Color32(0xFF, 0xFF, 0xFF, 0x0D);

        [Tooltip("A hairline frame around the panel.")]
        public bool panelFrame = true;
        public Color panelFrameColor = new Color32(0xFF, 0xFF, 0xFF, 0xD9);

        [Tooltip("A second, fainter frame offset from the first.")]
        public bool panelFrameEcho = true;
        public Color panelFrameEchoColor = new Color32(0xFF, 0xFF, 0xFF, 0x66);

        [Tooltip("Small crosshair marks at frame corners and at the end of the title rule.")]
        public bool registrationMarks = true;

        [Header("Lettering")]
        [Tooltip("Leave empty to use TextMeshPro's default font.")]
        public TMP_FontAsset font;

        [Tooltip("Optional material preset of the font above, used for panel titles (for example a glow). Leave empty for the font's own material.")]
        public Material titleMaterial;

        [Tooltip("All primary text and icons. Check it against the Panel colour: 4.5:1 or better.")]
        public Color text = new Color32(0xF4, 0xFB, 0xFF, 0xFF);

        [Tooltip("Values at rest and small print.")]
        public Color textSoft = new Color32(0xD3, 0xE1, 0xFF, 0xFF);

        public UIPilotTextCase titleCase = UIPilotTextCase.Lowercase;
        public bool  titleBold     = false;
        public float titleTracking = 4f;
        [Tooltip("Largest title size. A long title shrinks from here to fit the column.")]
        public float titleSize     = 58f;

        public UIPilotTextCase itemCase = UIPilotTextCase.Lowercase;
        public float itemTracking = 6f;
        public float itemSize     = 24f;
        public float valueSize    = 20f;
        public float smallSize    = 15f;

        [Header("Focus — how the selected button is shown")]
        [Tooltip("On: a glowing hairline frame appears around the button. Off: a flat solid bar.")]
        public bool focusFrame = true;

        [Tooltip("At rest. Keep the alpha at 0 and the colour equal to Hover, so the fade never passes through an off-colour.")]
        public Color focusIdle     = new Color(1f, 1f, 1f, 0f);
        [Tooltip("Under the mouse pointer.")]
        public Color focusHover    = new Color(1f, 1f, 1f, 0.45f);
        [Tooltip("Selected by keyboard or gamepad, or last clicked.")]
        public Color focusSelected = new Color(1f, 1f, 1f, 1f);
        public Color focusPressed  = new Color32(0xCF, 0xE0, 0xFF, 0xFF);
        public Color focusDisabled = new Color(1f, 1f, 1f, 0.12f);

        [Header("Details")]
        [Tooltip("The hairline under each panel title.")]
        public Color rule       = new Color32(0xFF, 0xFF, 0xFF, 0x99);
        public Color meterTrack = new Color32(0xFF, 0xFF, 0xFF, 0x40);
        public Color meterFill  = new Color32(0xF4, 0xFB, 0xFF, 0xFF);
    }
}
