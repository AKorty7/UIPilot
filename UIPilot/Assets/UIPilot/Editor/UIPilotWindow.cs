using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.ActionDiscovery;
using UIPilot.Editor.Modules.UIGenerator;

namespace UIPilot.Editor
{
    public sealed class UIPilotWindow : EditorWindow
    {
        private MenuType                _selectedMenuType;
        private List<DiscoveredAction>  _discoveredActions;
        private Vector2                 _discoverScrollPos;

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
            DrawDiscoverSection();
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

                EditorGUILayout.Space(4f);

                if (GUILayout.Button(UIPilotLabels.Generate.ClearMainMenu))
                    UIGeneratorModule.ClearPanel(MenuType.MainMenu);
                if (GUILayout.Button(UIPilotLabels.Generate.ClearPauseMenu))
                    UIGeneratorModule.ClearPanel(MenuType.PauseMenu);
                if (GUILayout.Button(UIPilotLabels.Generate.ClearSettings))
                    UIGeneratorModule.ClearPanel(MenuType.SettingsMenu);
            }
        }

        // ── Section: Discover ────────────────────────────────────────────────

        private void DrawDiscoverSection()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(UIPilotLabels.Sections.Discover, EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button(UIPilotLabels.Discover.ScanButton))
                    _discoveredActions = ActionDiscoveryModule.Scan();

                if (_discoveredActions == null) return;

                EditorGUILayout.Space(4f);

                if (_discoveredActions.Count == 0)
                {
                    EditorGUILayout.LabelField(UIPilotLabels.Discover.EmptyList,
                        EditorStyles.centeredGreyMiniLabel);
                    return;
                }

                _discoverScrollPos = EditorGUILayout.BeginScrollView(
                    _discoverScrollPos, GUILayout.Height(120f));

                foreach (var action in _discoveredActions)
                    EditorGUILayout.LabelField(action.FullLabel);

                EditorGUILayout.EndScrollView();
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
                EditorGUILayout.LabelField(UIPilotLabels.Sections.Placeholder,
                    EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space(2f);
            }
        }
    }
}
