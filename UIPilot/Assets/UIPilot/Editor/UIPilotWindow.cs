using UnityEditor;
using UnityEngine;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.UIGenerator;

namespace UIPilot.Editor
{
    public sealed class UIPilotWindow : EditorWindow
    {
        private MenuType _selectedMenuType;

        // ── Lifecycle ────────────────────────────────────────────────────────

        [MenuItem(UIPilotLabels.Menu.WindowPath)]
        public static void Open()
        {
            var window = GetWindow<UIPilotWindow>();
            window.titleContent = new GUIContent(UIPilotLabels.Window.Title);
            window.minSize      = new Vector2(380f, 480f);
            window.Show();
        }

        // ── GUI ──────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            DrawHeader();

            EditorGUILayout.Space(8f);

            DrawGenerateSection();
            DrawSection(UIPilotLabels.Sections.Discover);
            DrawSection(UIPilotLabels.Sections.Wire);
            DrawSection(UIPilotLabels.Sections.Validate);
        }

        // ── Section: Generate ────────────────────────────────────────────────

        private void DrawGenerateSection()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(UIPilotLabels.Sections.Generate, EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _selectedMenuType = (MenuType)EditorGUILayout.EnumPopup(
                    UIPilotLabels.Generate.MenuTypeLabel, _selectedMenuType);

                EditorGUILayout.Space(4f);

                if (GUILayout.Button(UIPilotLabels.Generate.ButtonLabel))
                    UIGeneratorModule.Generate(_selectedMenuType);
            }
        }

        // ── Placeholder sections ─────────────────────────────────────────────

        private static void DrawHeader()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                UIPilotLabels.Window.Title,
                new GUIStyle(EditorStyles.largeLabel) { fontSize = 16, fontStyle = FontStyle.Bold }
            );
        }

        private static void DrawSection(string title)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(UIPilotLabels.Sections.Placeholder, EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space(2f);
            }
        }
    }
}
