using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.UIGenerator;

namespace UIPilot.Editor.Modules.ScriptSetup
{
    internal static class ScriptSetupModule
    {
        // ── Public entry point ───────────────────────────────────────────────

        internal static void GenerateGameManager(MenuType[] selectedMenus)
        {
            if (IsGameManagerIntact())
            {
                Debug.Log(ScriptSetupContent.Messages.GameManagerIntact);
                return;
            }

            if (HasCustomizedGameManagerScript() && !ConfirmOverwriteCustomizedGameManager())
            {
                Debug.Log(ScriptSetupContent.Messages.RegenerationCancelled);
                return;
            }

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

        private static bool IsGameManagerIntact()
        {
            return File.Exists(GetFullOutputPath())
                && GameObject.Find(ScriptSetupContent.GameObjects.ManagerName) != null;
        }

        // ── Customization protection ─────────────────────────────────────────
        // Quick Clear removes the scene GameObject but leaves the script file
        // on disk. That makes IsGameManagerIntact() report "not intact" even
        // when the file still holds hand-written logic, so regeneration must
        // check the file's actual contents before it can be overwritten.

        private static bool HasCustomizedGameManagerScript()
        {
            var fullPath = GetFullOutputPath();
            if (!File.Exists(fullPath)) return false;

            var content = File.ReadAllText(fullPath);
            return IsGameManagerCustomized(content);
        }

        private static bool ConfirmOverwriteCustomizedGameManager()
        {
            return EditorUtility.DisplayDialog(
                ScriptSetupContent.Dialogs.CustomGameManagerTitle,
                ScriptSetupContent.Dialogs.CustomGameManagerMessage,
                ScriptSetupContent.Dialogs.CustomGameManagerConfirm,
                ScriptSetupContent.Dialogs.CustomGameManagerCancel);
        }

        // Compares every On{Label}Pressed method body against the exact body
        // BuildMethod() would generate for that label. Any mismatch — extra
        // statements, edited logic, even a method that no longer parses as
        // expected — means the file has been customized beyond the stub.
        private static bool IsGameManagerCustomized(string fileContent)
        {
            var content = NormalizeLineEndings(fileContent);
            var declarations = Regex.Matches(content, @"public\s+void\s+On(\w+)Pressed\s*\(\s*\)");

            foreach (Match declaration in declarations)
            {
                var label = declaration.Groups[1].Value;

                var actualBody = ExtractMethodBody(content, declaration.Index + declaration.Length);
                if (actualBody == null)
                    return true; // Malformed/unexpected structure — protect rather than guess.

                var expectedMethod = NormalizeLineEndings(BuildMethod(label));
                var expectedBody   = ExtractMethodBody(expectedMethod, 0);

                if (actualBody.Trim() != expectedBody.Trim())
                    return true;
            }

            return false;
        }

        // Given text and a search start index, finds the next method body —
        // the text between the first '{' at/after startIndex and the closing
        // '}' that lines up at the method's own 4-space indent level.
        private static string ExtractMethodBody(string content, int searchFromIndex)
        {
            var openBraceIndex = content.IndexOf('{', searchFromIndex);
            if (openBraceIndex < 0) return null;

            var bodyStart  = openBraceIndex + 1;
            var closeIndex = content.IndexOf("\n    }", bodyStart);
            if (closeIndex < 0) return null;

            return content.Substring(bodyStart, closeIndex - bodyStart);
        }

        private static string NormalizeLineEndings(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }

        private static string BuildScript(List<string> labels)
        {
            var sb = new StringBuilder();
            sb.Append(ScriptSetupContent.Script.GeneratedHeader);
            sb.Append(BuildHeader());

            for (var i = 0; i < labels.Count; i++)
            {
                if (i > 0) sb.AppendLine();
                sb.Append(BuildMethod(labels[i]));
            }

            sb.Append(ScriptSetupContent.Script.Footer);
            return sb.ToString();
        }

        // Panel-name literals are derived from UIGeneratorContent — the same
        // prefix + PanelSuffix composition UIGeneratorModule uses to name the
        // panels it creates — so this stays the single source of truth.
        private static string BuildHeader()
        {
            return string.Format(
                ScriptSetupContent.Script.Header,
                GetPanelName(UIGeneratorContent.GameObjects.MainMenuPrefix),
                GetPanelName(UIGeneratorContent.GameObjects.PauseMenuPrefix),
                GetPanelName(UIGeneratorContent.GameObjects.SettingsMenuPrefix),
                UIGeneratorContent.GameObjects.VolumeValue,
                UIGeneratorContent.GameObjects.VolumeFill,
                UIGeneratorContent.GameObjects.FullscreenValue,
                UIGeneratorContent.GameObjects.QualityValue,
                UIGeneratorContent.GameObjects.MainMenuFootnote,
                UIGeneratorContent.Settings.VersionFormat,
                UIGeneratorContent.Settings.On,
                UIGeneratorContent.Settings.Off,
                UIPilotLabels.Scene.TimeOfDayProperty,
                ScriptSetupContent.GameObjects.TimeOfDayField);
        }

        private static string GetPanelName(string prefix)
        {
            return prefix + UIGeneratorContent.GameObjects.PanelSuffix;
        }

        // Button label → the body of its On{Label}Pressed() method. The labels are
        // UIGeneratorContent's, the same ones that name the buttons in the scene.
        private static readonly Dictionary<string, string> MethodBodies = new Dictionary<string, string>
        {
            { UIGeneratorContent.Buttons.Play,        ScriptSetupContent.Script.PlayMethodBody },
            { UIGeneratorContent.Buttons.Settings,    ScriptSetupContent.Script.SettingsMethodBody },
            { UIGeneratorContent.Buttons.Back,        ScriptSetupContent.Script.BackMethodBody },
            { UIGeneratorContent.Buttons.Resume,      ScriptSetupContent.Script.ResumeMethodBody },
            { UIGeneratorContent.Buttons.Quit,        ScriptSetupContent.Script.QuitMethodBody },
            { UIGeneratorContent.Buttons.VolumeDown,  ScriptSetupContent.Script.VolumeDownMethodBody },
            { UIGeneratorContent.Buttons.VolumeUp,    ScriptSetupContent.Script.VolumeUpMethodBody },
            { UIGeneratorContent.Buttons.Fullscreen,  ScriptSetupContent.Script.FullscreenMethodBody },
            { UIGeneratorContent.Buttons.QualityDown, ScriptSetupContent.Script.QualityDownMethodBody },
            { UIGeneratorContent.Buttons.QualityUp,   ScriptSetupContent.Script.QualityUpMethodBody },
        };

        // Known labels get their real body; any other (a button the developer
        // adds later) falls back to the generic stub template.
        private static string BuildMethod(string label)
        {
            return MethodBodies.TryGetValue(label, out var body)
                ? BuildMethodWithBody(label, body)
                : string.Format(ScriptSetupContent.Script.MethodTemplate, label);
        }

        private static string BuildMethodWithBody(string label, string body)
        {
            return
                "    public void On" + label + "Pressed()\n" +
                "    {\n" +
                body + "\n" +
                "    }\n";
        }

        // ── File I/O ─────────────────────────────────────────────────────────

        private static string GetFullOutputPath()
        {
            return Path.Combine(
                Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length),
                ScriptSetupContent.Paths.OutputAssetPath);
        }

        private static void WriteScript(string contents)
        {
            File.WriteAllText(GetFullOutputPath(), contents, Encoding.UTF8);
            Debug.Log(ScriptSetupContent.Messages.FileWritten);

            AssetDatabase.Refresh();
        }

        // ── Scene object ─────────────────────────────────────────────────────

        private static void CreateGameObject()
        {
            // Only the script may have been missing — never create a second GameObject.
            if (GameObject.Find(ScriptSetupContent.GameObjects.ManagerName) != null) return;

            var go = new GameObject(ScriptSetupContent.GameObjects.ManagerName);
            Undo.RegisterCreatedObjectUndo(go, ScriptSetupContent.Undo.Action);

            // Script component cannot be added in the same frame as AssetDatabase.Refresh();
            // UIPilotWindow attaches it once the type resolves.
            Debug.Log(ScriptSetupContent.Messages.GameObjectCreated);
        }
    }
}
