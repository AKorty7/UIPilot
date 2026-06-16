namespace UIPilot.Editor.Modules.ScriptSetup
{
    internal static class ScriptSetupContent
    {
        internal static class Paths
        {
            internal const string OutputAssetPath = "Assets/UIPilot_GameManager.cs";
        }

        internal static class Script
        {
            internal const string Header =
                "using UnityEngine;\n" +
                "\n" +
                "public class UIPilot_GameManager : MonoBehaviour\n" +
                "{\n";

            internal const string Footer = "}\n";

            internal const string MethodTemplate =
                "    public void On{0}Pressed()\n" +
                "    {{\n" +
                "        // TODO: implement {0} logic\n" +
                "        Debug.Log(\"{0} pressed — add your logic\");\n" +
                "    }}\n";

            internal const string PlayMethodBody =
                "        // TODO: Load your game scene here\n" +
                "        // Example: SceneManager.LoadScene(\"GameScene\");\n" +
                "        Debug.Log(\"Play pressed — add your scene load logic\");";
        }

        internal static class GameObjects
        {
            internal const string ManagerName = "UIPilot_GameManager";
        }

        internal static class Messages
        {
            internal const string AddComponentManually =
                "UIPilot: UIPilot_GameManager.cs written and compiled. " +
                "Add the UIPilot_GameManager component to the UIPilot_GameManager GameObject manually.";
            internal const string FileWritten =
                "UIPilot: Script written to Assets/UIPilot_GameManager.cs";
        }

        internal static class Undo
        {
            internal const string Action = "Generate GameManager";
        }

        // Button label with a bespoke Play body — keyed by label string.
        internal const string PlayLabel = "Play";
    }
}
