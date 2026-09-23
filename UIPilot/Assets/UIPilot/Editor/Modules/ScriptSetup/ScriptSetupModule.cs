using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.UIGenerator;

namespace UIPilot.Editor.Modules.ScriptSetup
{
    internal static class ScriptSetupModule
    {
        // ── Public entry point ───────────────────────────────────────────────

        // Returns true when it wrote the script. Unity compiles it first, and new
        // methods exist only after that, so the caller wires the buttons once it has.
        internal static bool GenerateGameManager(MenuType[] selectedMenus)
        {
            var labels = CollectUniqueLabels(selectedMenus);

            if (IsGameManagerIntact())
                return AddMissingMethods(labels);

            if (HasCustomizedGameManagerScript() && !ConfirmOverwriteCustomizedGameManager())
            {
                Debug.Log(ScriptSetupContent.Messages.RegenerationCancelled);
                return false;
            }

            var written = WriteScript(BuildScript(labels));
            CreateGameObject();
            return written;
        }

        // A menu built after the others needs its buttons' methods in the script that
        // is already there. They are added before the class's closing brace, and
        // nothing already in the file changes, edited or not.
        private static bool AddMissingMethods(List<string> labels)
        {
            var content = File.ReadAllText(GetFullOutputPath());
            var missing = MissingLabels(content, labels);
            if (missing.Count == 0)
            {
                Debug.Log(ScriptSetupContent.Messages.GameManagerIntact);
                return false;
            }

            var updated = AppendMethods(content, missing);
            if (updated == null)
            {
                Debug.LogWarning(string.Format(ScriptSetupContent.Messages.MethodsNotAdded, MethodList(missing)));
                return false;
            }

            Debug.Log(string.Format(ScriptSetupContent.Messages.MethodsAdded, MethodList(missing)));
            return WriteScript(updated);
        }

        private static List<string> MissingLabels(string content, List<string> labels)
        {
            var declared = DeclaredLabels(content);
            var missing  = new List<string>();
            foreach (var label in labels)
                if (!declared.Contains(label))
                    missing.Add(label);

            return missing;
        }

        // The methods go before the file's last closing brace, which is the class's
        // only while the file declares one type. Otherwise nothing is added (null).
        private static string AppendMethods(string content, List<string> labels)
        {
            var close = content.LastIndexOf('}');
            if (close < 0 || TypeDeclaration.Matches(content).Count != 1) return null;

            var script = new StringBuilder(content, 0, close, content.Length + 1024);
            foreach (var label in labels)
            {
                script.Append('\n');
                script.Append(BuildMethod(label));
            }

            script.Append(content, close, content.Length - close);
            return script.ToString();
        }

        private static string MethodList(List<string> labels)
        {
            var methods = new List<string>();
            foreach (var label in labels)
                methods.Add(string.Format(ScriptSetupContent.Messages.MethodFormat, label));

            return string.Join(ScriptSetupContent.Messages.ListSeparator, methods);
        }

        // ── Play scene ───────────────────────────────────────────────────────
        // The main menu's Play button loads the GameManager's Game Scene. The
        // component is the developer's own type, so the field is reached by its
        // serialized name, like the Time Of Day field.

        // False when there is nothing to set here: no main menu (so no Play button),
        // or no GameManager with the field (not built yet, or an older script).
        internal static bool TryGetGameScene(out string scene)
        {
            scene = null;
            var mainMenu = UIGeneratorContent.GameObjects.MainMenuPrefix + UIGeneratorContent.GameObjects.PanelSuffix;
            if (UIPilotSceneQuery.FindInCanvas(UIGeneratorContent.GameObjects.Canvas, mainMenu) == null) return false;

            var property = FindGameSceneProperty();
            if (property == null) return false;

            scene = property.stringValue;
            return true;
        }

        // Undoable, and marks the scene dirty, like any Inspector edit.
        internal static void SetGameScene(string scenePath)
        {
            var property = FindGameSceneProperty();
            if (property == null || property.stringValue == scenePath) return;

            property.stringValue = scenePath;
            property.serializedObject.ApplyModifiedProperties();
        }

        // The scenes Play can load. selected is the one the field names, -1 for
        // none (Play stays in this scene); missing is true when the field names a
        // scene not in the list, which is then added at the end so the row can
        // still show it.
        internal static List<string> GameSceneChoices(string current, out int selected, out bool missing)
        {
            var choices = OtherBuildScenes();
            selected = IndexOfScene(choices, current);
            missing  = !string.IsNullOrEmpty(current) && selected < 0;

            if (missing)
            {
                choices.Add(current);
                selected = choices.Count - 1;
            }

            return choices;
        }

        // Every enabled scene in the build's scene list (the active Build
        // Profile's, when it has its own) except this one.
        private static List<string> OtherBuildScenes()
        {
            var active = SceneManager.GetActiveScene().path;
            var scenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.enabled && !string.IsNullOrEmpty(scene.path) && scene.path != active)
                    scenes.Add(scene.path);

            return scenes;
        }

        // The field may hold a path (the window writes one) or a name typed by hand.
        // -1 when it is empty or names no scene in the list.
        private static int IndexOfScene(List<string> scenes, string scene)
        {
            if (string.IsNullOrEmpty(scene)) return -1;

            for (var i = 0; i < scenes.Count; i++)
                if (scenes[i] == scene || Path.GetFileNameWithoutExtension(scenes[i]) == scene)
                    return i;

            return -1;
        }

        private static SerializedProperty FindGameSceneProperty()
        {
            var manager = GameObject.Find(ScriptSetupContent.GameObjects.ManagerName);
            if (manager == null) return null;

            foreach (var component in manager.GetComponents<MonoBehaviour>())
            {
                if (component == null) continue;   // a missing-script slot

                var property = new SerializedObject(component).FindProperty(ScriptSetupContent.GameObjects.GameSceneField);
                if (property != null && property.propertyType == SerializedPropertyType.String) return property;
            }

            return null;
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

        // The file is exactly what UIPilot would write for the buttons it has methods
        // for, or it is the developer's: any edit counts, to a method, a field or a
        // comment. Line endings and blank space around the file do not.
        private static bool IsGameManagerCustomized(string fileContent)
        {
            var expected = BuildScript(DeclaredLabels(fileContent));
            return Canonical(fileContent) != Canonical(expected);
        }

        private static readonly Regex MethodDeclaration =
            new Regex(@"public\s+void\s+On(\w+)Pressed\s*\(\s*\)");

        // A type declared at the start of a line, so a comment mentioning one is not.
        private static readonly Regex TypeDeclaration = new Regex(
            @"^\s*(?:(?:public|internal|private|protected|sealed|static|abstract|partial)\s+)*(?:class|struct|interface|enum|record)\s+\w",
            RegexOptions.Multiline);

        // The labels whose On{Label}Pressed methods the file declares, in file order.
        private static List<string> DeclaredLabels(string content)
        {
            var labels = new List<string>();
            foreach (Match declaration in MethodDeclaration.Matches(content))
                labels.Add(declaration.Groups[1].Value);

            return labels;
        }

        private static string Canonical(string text)
        {
            return NormalizeLineEndings(text).Trim();
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

            // '\n', not AppendLine: the template's own line endings are '\n', and one
            // file should not mix them.
            for (var i = 0; i < labels.Count; i++)
            {
                if (i > 0) sb.Append('\n');
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
                ScriptSetupContent.GameObjects.TimeOfDayField,
                ScriptSetupContent.GameObjects.GameSceneField,
                UIGeneratorContent.GameObjects.ButtonPrefix + UIGeneratorContent.Buttons.Quit,
                UIGeneratorContent.Confirm.Quit);
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

        // False when the file already says exactly this: Unity would not recompile
        // it, and a build waiting for that compile would wait for nothing.
        private static bool WriteScript(string contents)
        {
            var path = GetFullOutputPath();
            if (File.Exists(path) && NormalizeLineEndings(File.ReadAllText(path)) == NormalizeLineEndings(contents))
                return false;

            File.WriteAllText(path, contents, Encoding.UTF8);
            Debug.Log(ScriptSetupContent.Messages.FileWritten);

            AssetDatabase.Refresh();
            return true;
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
