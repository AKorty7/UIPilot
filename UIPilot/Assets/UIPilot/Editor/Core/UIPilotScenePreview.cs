using UnityEditor;
using UnityEngine;

namespace UIPilot.Editor.Core
{
    // Shows a theme's scene at its Time Of Day in the Editor. The scene layers read
    // one global shader value, which a script reload clears, so the last hour is
    // kept for the session and put back after every reload. In a build the
    // generated UIPilot_GameManager sets the same value.
    [InitializeOnLoad]
    internal static class UIPilotScenePreview
    {
        static UIPilotScenePreview()
        {
            Shader.SetGlobalFloat(UIPilotLabels.Scene.TimeOfDayProperty,
                SessionState.GetFloat(UIPilotLabels.Scene.SessionHourKey, UIPilotLabels.Scene.DefaultHour));
        }

        internal static void SetHour(float hour)
        {
            hour = Mathf.Repeat(hour, 1f);
            SessionState.SetFloat(UIPilotLabels.Scene.SessionHourKey, hour);
            Shader.SetGlobalFloat(UIPilotLabels.Scene.TimeOfDayProperty, hour);
            SceneView.RepaintAll();
        }
    }
}
