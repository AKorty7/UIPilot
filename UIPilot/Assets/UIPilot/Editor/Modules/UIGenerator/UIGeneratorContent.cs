namespace UIPilot.Editor.Modules.UIGenerator
{
    internal static class UIGeneratorContent
    {
        internal static class Layers
        {
            internal const string UI = "UI";
        }

        internal static class GameObjects
        {
            internal const string NamePrefix         = "UIPilot_";
            internal const string Canvas             = "UIPilot_Canvas";
            internal const string PanelSuffix        = "_Panel";
            internal const string BandSuffix         = "_Band";
            internal const string BackdropSuffix     = "_Backdrop";
            internal const string BloomChild         = "Bloom";
            internal const string WaveChild          = "Wave";
            internal const string ScanlinesChild     = "Scanlines";
            internal const string FrameChild         = "Frame";
            internal const string FrameEchoChild     = "Frame Echo";
            internal const string CrossChild         = "Cross";
            internal const string IconChild          = "Icon";
            internal const string SpacerSuffix       = "_Spacer";
            internal const string FootnoteSuffix     = "_Footnote";
            internal const string ButtonBarChild     = "Bar";
            internal const string TitleSuffix        = "_Title";
            internal const string RuleSuffix         = "_Rule";
            internal const string RuleBarChild       = "Bar";
            internal const string ButtonPrefix       = "UIPilot_Btn_";
            internal const string ButtonTextChild    = "Text";
            internal const string MainMenuPrefix     = "UIPilot_MainMenu";
            internal const string PauseMenuPrefix    = "UIPilot_PauseMenu";
            internal const string SettingsMenuPrefix = "UIPilot_Settings";

            // Settings rows. The generated GameManager finds the value objects by
            // these names at runtime, so ScriptSetup reads them from here too.
            internal const string RowLabelChild      = "Label";
            internal const string RowValueChild      = "Value";
            internal const string VolumeRow          = "UIPilot_Settings_Row_Volume";
            internal const string FullscreenRow      = "UIPilot_Settings_Row_Fullscreen";
            internal const string QualityRow         = "UIPilot_Settings_Row_Quality";
            internal const string VolumeValue        = "UIPilot_Settings_VolumeValue";
            internal const string VolumeTrack        = "UIPilot_Settings_VolumeTrack";
            internal const string VolumeFill         = "UIPilot_Settings_VolumeFill";
            internal const string FullscreenValue    = "UIPilot_Settings_FullscreenValue";
            internal const string QualityValue       = "UIPilot_Settings_QualityValue";
            internal const string MainMenuFootnote   = MainMenuPrefix + FootnoteSuffix;
        }

        // The art kit in Assets/UIPilot/Art, by asset name (no extension, no path).
        // Vector sources and the build script live in tools/art, outside the project.
        internal static class Art
        {
            internal const string SpriteFilter   = " t:Sprite";
            internal const string TextureFilter  = " t:Texture2D";
            internal const string FontFilter     = " t:TMP_FontAsset";
            internal const string MaterialFilter = " t:Material";

            internal const string Glow      = "uipilot_glow";
            internal const string Focus     = "uipilot_focus";
            internal const string Frame     = "uipilot_frame";
            internal const string Cross     = "uipilot_cross";
            internal const string Scanlines = "uipilot_scanlines";
            internal const string Wave      = "uipilot_wave";

            internal const string IconPrefix       = "uipilot_icon_";
            internal const string IconPlay         = IconPrefix + "play";
            internal const string IconSettings     = IconPrefix + "settings";
            internal const string IconQuit         = IconPrefix + "quit";
            internal const string IconBack         = IconPrefix + "back";
            internal const string IconVolume       = IconPrefix + "volume";
            internal const string IconFullscreen   = IconPrefix + "fullscreen";
            internal const string IconQuality      = IconPrefix + "quality";
            internal const string IconChevronLeft  = IconPrefix + "chevron_left";
            internal const string IconChevronRight = IconPrefix + "chevron_right";

            internal const string FontAsset        = "Michroma SDF";
            internal const string FontGlowMaterial = "Michroma SDF Glow";

            // Genre themes: their artwork (in Art/Themes) and fonts (in Art/Fonts).
            internal const string FantasyPanel = "uipilot_fantasy_panel";
            internal const string FantasyFocus = "uipilot_fantasy_focus";
            internal const string FantasyRule  = "uipilot_fantasy_rule";
            internal const string JrpgPanel    = "uipilot_jrpg_panel";
            internal const string JrpgCursor   = "uipilot_jrpg_cursor";
            internal const string PixelPanel   = "uipilot_pixel_panel";
            internal const string PixelCursor  = "uipilot_pixel_cursor";
            internal const string SciFiPanel   = "uipilot_scifi_panel";
            internal const string SciFiFocus   = "uipilot_scifi_focus";
            internal const string SciFiRule    = "uipilot_scifi_rule";

            internal const string YoungSerif       = "Young Serif SDF";
            internal const string CrimsonPro       = "Crimson Pro Bold SDF";
            internal const string WorkSans         = "Work Sans Bold SDF";
            internal const string WorkSansShadow   = "Work Sans Bold SDF Shadow";
            internal const string Silkscreen       = "Silkscreen SDF";
            internal const string SilkscreenShadow = "Silkscreen SDF Shadow";
            internal const string Tektur           = "Tektur SDF";
            internal const string TekturGlow       = "Tektur SDF Glow";
        }

        // Title text on each panel. The main menu has no constant: its title is
        // the project's real Product Name, with this as the fallback if that is empty.
        internal static class Menus
        {
            internal const string MainMenuFallback = "Untitled Game";
            internal const string PauseMenu        = "Paused";
            internal const string SettingsMenu     = "Settings";
        }

        // Everything a player reads on the Settings panel and in the small print.
        // Values shown at build time are the project's real current values; the
        // generated GameManager keeps them live at runtime.
        internal static class Settings
        {
            internal const string VolumeLabel     = "Volume";
            internal const string FullscreenLabel = "Fullscreen";
            internal const string QualityLabel    = "Quality";
            internal const string StepDownGlyph   = "<";
            internal const string StepUpGlyph     = ">";
            internal const string On              = "On";
            internal const string Off             = "Off";
            internal const string FullVolume      = "100%";
            internal const string VersionFormat   = "Version {0}";
        }

        // A button's label is also its object-name suffix and the stem of the
        // method it calls: UIPilot_Btn_VolumeUp calls OnVolumeUpPressed().
        internal static class Buttons
        {
            internal const string Play        = "Play";
            internal const string Resume      = "Resume";
            internal const string Settings    = "Settings";
            internal const string Quit        = "Quit";
            internal const string Back        = "Back";
            internal const string VolumeDown  = "VolumeDown";
            internal const string VolumeUp    = "VolumeUp";
            internal const string Fullscreen  = "Fullscreen";
            internal const string QualityDown = "QualityDown";
            internal const string QualityUp   = "QualityUp";
        }

        internal static class ButtonSets
        {
            internal static readonly string[] MainMenu     = { Buttons.Play, Buttons.Settings, Buttons.Quit };
            internal static readonly string[] PauseMenu    = { Buttons.Resume, Buttons.Settings, Buttons.Quit };
            internal static readonly string[] SettingsMenu =
            {
                Buttons.VolumeDown, Buttons.VolumeUp, Buttons.Fullscreen,
                Buttons.QualityDown, Buttons.QualityUp, Buttons.Back
            };
        }

        internal static class Messages
        {
            internal const string PanelAlreadyExists = "UIPilot: Panel already exists, skipping: ";
            internal const string PanelIntact        = "UIPilot: Panel exists and is intact — skipping: ";
        }

        internal static class Undo
        {
            internal const string Action = "Generate UI";
        }
    }
}
