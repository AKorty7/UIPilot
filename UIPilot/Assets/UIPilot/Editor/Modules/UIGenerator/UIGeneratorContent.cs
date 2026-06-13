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
            internal const string EventSystem        = "EventSystem";
            internal const string PanelSuffix        = "_Panel";
            internal const string TitleSuffix        = "_Title";
            internal const string ButtonPrefix       = "UIPilot_Btn_";
            internal const string ButtonTextChild    = "Text";
            internal const string MainMenuPrefix     = "UIPilot_MainMenu";
            internal const string PauseMenuPrefix    = "UIPilot_PauseMenu";
            internal const string SettingsMenuPrefix = "UIPilot_Settings";
        }

        internal static class Menus
        {
            internal const string MainMenu     = "Main Menu";
            internal const string PauseMenu    = "Pause Menu";
            internal const string SettingsMenu = "Settings Menu";
        }

        internal static class Buttons
        {
            internal const string Play     = "Play";
            internal const string Resume   = "Resume";
            internal const string Settings = "Settings";
            internal const string Quit     = "Quit";
            internal const string Back     = "Back";
        }

        internal static class ButtonSets
        {
            internal static readonly string[] MainMenu     = { Buttons.Play, Buttons.Settings, Buttons.Quit };
            internal static readonly string[] PauseMenu    = { Buttons.Resume, Buttons.Settings, Buttons.Quit };
            internal static readonly string[] SettingsMenu = { Buttons.Back };
        }

        internal static class Messages
        {
            internal const string PanelAlreadyExists = "UIPilot: Panel already exists, skipping: ";
        }

        internal static class Undo
        {
            internal const string Action = "Generate UI";
        }
    }
}
