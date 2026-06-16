using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UIPilot.Editor.Modules.UIGenerator;

namespace UIPilot.Editor.Modules.ScriptSetup
{
    internal static class ScriptSetupModule
    {
        // ── Public entry point ───────────────────────────────────────────────

        internal static void GenerateGameManager(MenuType[] selectedMenus)
        {
            var labels = CollectUniqueLabels(selectedMenus);
            var script = BuildScript(labels);

            WriteScript(script);
            CreateGameObject();
        }

        // ── Label collection ─────────────────────────────────────────────────

        private static List<string> CollectUniqueLabels(MenuType[] menus)
        {
            var seen   = new HashSet<string>();
            var result = new List<string>();

            foreach (var menu in menus)
            {
                var set = GetButtonSet(menu);
                foreach (var label in set)
                    if (seen.Add(label))
                        result.Add(label);
            }

            return result;
        }

        private static string[] GetButtonSet(MenuType menu)
        {
            switch (menu)
            {
                case MenuType.MainMenu:     return UIGeneratorContent.ButtonSets.MainMenu;
                case MenuType.PauseMenu:    return UIGeneratorContent.ButtonSets.PauseMenu;
                case MenuType.SettingsMenu: return UIGeneratorContent.ButtonSets.SettingsMenu;
                default:                   return new string[0];
            }
        }

        // ── Script generation ────────────────────────────────────────────────

        private static string BuildScript(List<string> labels)
        {
            var sb = new StringBuilder();
            sb.Append(ScriptSetupContent.Script.Header);

            for (var i = 0; i < labels.Count; i++)
            {
                if (i > 0) sb.AppendLine();
                sb.Append(BuildMethod(labels[i]));
            }

            sb.Append(ScriptSetupContent.Script.Footer);
            return sb.ToString();
        }

        private static string BuildMethod(string label)
        {
            // Play gets a bespoke body; all others use the generic template.
            if (label == ScriptSetupContent.PlayLabel)
            {
                return
                    "    public void OnPlayPressed()\n" +
                    "    {\n" +
                    ScriptSetupContent.Script.PlayMethodBody + "\n" +
                    "    }\n";
            }

            return string.Format(ScriptSetupContent.Script.MethodTemplate, label);
        }

        // ── File I/O ─────────────────────────────────────────────────────────

        private static void WriteScript(string contents)
        {
            var fullPath = Path.Combine(
                Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length),
                ScriptSetupContent.Paths.OutputAssetPath);

            File.WriteAllText(fullPath, contents, Encoding.UTF8);
            Debug.Log(ScriptSetupContent.Messages.FileWritten);

            AssetDatabase.Refresh();
        }

        // ── Scene object ─────────────────────────────────────────────────────

        private static void CreateGameObject()
        {
            var go = new GameObject(ScriptSetupContent.GameObjects.ManagerName);
            Undo.RegisterCreatedObjectUndo(go, ScriptSetupContent.Undo.Action);

            // Script component cannot be added in the same frame as AssetDatabase.Refresh().
            Debug.Log(ScriptSetupContent.Messages.AddComponentManually);
        }
    }
}
