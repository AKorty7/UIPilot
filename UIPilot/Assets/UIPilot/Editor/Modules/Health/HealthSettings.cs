using UnityEditor;

namespace UIPilot.Editor.Modules.Health
{
    // Stored per project and per user (UserSettings/, which is not in version
    // control), so one developer switching a check off does not switch it off
    // for the whole team. Everything is on until someone turns it off.
    internal static class HealthSettings
    {
        internal static bool IsEnabled(HealthCheck check)
        {
            return Get(HealthContent.Settings.CheckKeyPrefix + check);
        }

        internal static void SetEnabled(HealthCheck check, bool enabled)
        {
            Set(HealthContent.Settings.CheckKeyPrefix + check, enabled);
        }

        internal static bool WarnOnPlay
        {
            get => Get(HealthContent.Settings.WarnOnPlayKey);
            set => Set(HealthContent.Settings.WarnOnPlayKey, value);
        }

        internal static bool WarnOnBuild
        {
            get => Get(HealthContent.Settings.WarnOnBuildKey);
            set => Set(HealthContent.Settings.WarnOnBuildKey, value);
        }

        private static bool Get(string key)
        {
            return EditorUserSettings.GetConfigValue(key) != HealthContent.Settings.Off;
        }

        private static void Set(string key, bool value)
        {
            EditorUserSettings.SetConfigValue(key,
                value ? HealthContent.Settings.On : HealthContent.Settings.Off);
        }
    }
}
