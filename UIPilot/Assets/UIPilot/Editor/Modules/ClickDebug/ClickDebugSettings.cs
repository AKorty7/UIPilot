using UnityEditor;

namespace UIPilot.Editor.Modules.ClickDebug
{
    // Stored per project and per user (UserSettings/, which is not in version
    // control), like UI Health's. Watching is on until someone turns it off;
    // the two extras that make noise are off until someone turns them on.
    internal static class ClickDebugSettings
    {
        internal static bool Watch
        {
            get => Get(ClickDebugContent.Settings.WatchKey, true);
            set => Set(ClickDebugContent.Settings.WatchKey, value);
        }

        internal static bool LogToConsole
        {
            get => Get(ClickDebugContent.Settings.LogKey, false);
            set => Set(ClickDebugContent.Settings.LogKey, value);
        }

        internal static bool SelectTarget
        {
            get => Get(ClickDebugContent.Settings.SelectKey, false);
            set => Set(ClickDebugContent.Settings.SelectKey, value);
        }

        private static bool Get(string key, bool unset)
        {
            var value = EditorUserSettings.GetConfigValue(key);
            return string.IsNullOrEmpty(value) ? unset : value == ClickDebugContent.Settings.On;
        }

        private static void Set(string key, bool value)
        {
            EditorUserSettings.SetConfigValue(key,
                value ? ClickDebugContent.Settings.On : ClickDebugContent.Settings.Off);
        }
    }
}
