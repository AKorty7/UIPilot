using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.ActionDiscovery;
using UIPilot.Editor.Modules.Binding;
using UIPilot.Editor.Modules.UIGenerator;

namespace UIPilot.Editor
{
    public sealed class UIPilotWindow : EditorWindow
    {
        // ── Generate state ───────────────────────────────────────────────────
        private MenuType _selectedMenuType;

        // ── Discover state ───────────────────────────────────────────────────
        private List<DiscoveredAction> _discoveredActions;
        private Vector2                _discoverScrollPos;

        // ── Wire state ───────────────────────────────────────────────────────
        private List<Button>            _wiredButtons;
        private List<DiscoveredAction>  _availableActions;
        private Dictionary<string, int> _bindingSelections;
        private string[]                _actionDropdownOptions;
        private Vector2                 _wireScrollPos;

        // ── Cached GUIContent (tooltips) ─────────────────────────────────────
        private static readonly GUIContent ContentGenerate    = new GUIContent(UIPilotLabels.Generate.ButtonLabel,    UIPilotLabels.Generate.TooltipGenerate);
        private static readonly GUIContent ContentClearMain   = new GUIContent(UIPilotLabels.Generate.ClearMainMenu,  UIPilotLabels.Generate.TooltipClearPanel);
        private static readonly GUIContent ContentClearPause  = new GUIContent(UIPilotLabels.Generate.ClearPauseMenu, UIPilotLabels.Generate.TooltipClearPanel);
        private static readonly GUIContent ContentClearSet    = new GUIContent(UIPilotLabels.Generate.ClearSettings,  UIPilotLabels.Generate.TooltipClearPanel);
        private static readonly GUIContent ContentScan        = new GUIContent(UIPilotLabels.Discover.ScanButton,     UIPilotLabels.Discover.TooltipScan);
        private static readonly GUIContent ContentRefresh     = new GUIContent(UIPilotLabels.Wire.RefreshButton,      UIPilotLabels.Wire.TooltipRefresh);
        private static readonly GUIContent ContentApply       = new GUIContent(UIPilotLabels.Wire.ApplyButton,        UIPilotLabels.Wire.TooltipApply);
        // ContentValidate will be wired in Module 4 (ValidationModule).

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
            DrawWireSection();
            DrawValidateSection();
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

                if (GUILayout.Button(ContentGenerate))
                    UIGeneratorModule.Generate(_selectedMenuType);

                EditorGUILayout.Space(4f);

                if (GUILayout.Button(ContentClearMain))
                    UIGeneratorModule.ClearPanel(MenuType.MainMenu);
                if (GUILayout.Button(ContentClearPause))
                    UIGeneratorModule.ClearPanel(MenuType.PauseMenu);
                if (GUILayout.Button(ContentClearSet))
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
                if (GUILayout.Button(ContentScan))
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

        // ── Section: Wire ────────────────────────────────────────────────────

        private void DrawWireSection()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(UIPilotLabels.Sections.Wire, EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button(ContentRefresh))
                    RefreshWireState();

                if (_wiredButtons == null
                    || _availableActions == null
                    || _bindingSelections == null
                    || _actionDropdownOptions == null)
                {
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.HelpBox(UIPilotLabels.Wire.HelpRefreshNeeded, MessageType.Info);
                    return;
                }

                EditorGUILayout.Space(4f);

                if (_wiredButtons.Count == 0)
                {
                    EditorGUILayout.LabelField(UIPilotLabels.Wire.NoButtons,
                        EditorStyles.centeredGreyMiniLabel);
                    return;
                }

                if (_availableActions.Count == 0)
                {
                    EditorGUILayout.HelpBox(UIPilotLabels.Wire.HelpNoActions, MessageType.Warning);
                    return;
                }

                _wireScrollPos = EditorGUILayout.BeginScrollView(
                    _wireScrollPos, GUILayout.Height(140f));

                foreach (var btn in _wiredButtons)
                {
                    if (btn == null) continue;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(btn.name, GUILayout.Width(180f));

                        if (!_bindingSelections.ContainsKey(btn.name))
                            _bindingSelections[btn.name] = 0;

                        _bindingSelections[btn.name] = EditorGUILayout.Popup(
                            _bindingSelections[btn.name],
                            _actionDropdownOptions);
                    }
                }

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField(UIPilotLabels.Wire.AssignHint,
                    EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space(2f);

                if (GUILayout.Button(ContentApply))
                    BindingModule.ApplyBindings(_bindingSelections, _availableActions);
            }
        }

        // ── Section: Validate (placeholder) ─────────────────────────────────

        private static void DrawValidateSection()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(UIPilotLabels.Sections.Validate, EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(UIPilotLabels.Sections.Placeholder,
                    EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space(2f);
            }
        }

        // ── Wire helpers ─────────────────────────────────────────────────────

        private void RefreshWireState()
        {
            _wiredButtons      = BindingModule.FindUIPilotButtons();
            _availableActions  = ActionDiscoveryModule.Scan();
            _bindingSelections = new Dictionary<string, int>();

            var options = new string[_availableActions.Count + 1];
            options[0] = UIPilotLabels.Wire.NoneOption;
            for (var i = 0; i < _availableActions.Count; i++)
                options[i + 1] = _availableActions[i].FullLabel;

            _actionDropdownOptions = options;
        }

        // ── Header ───────────────────────────────────────────────────────────

        private static void DrawHeader()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                UIPilotLabels.Window.Title,
                new GUIStyle(EditorStyles.largeLabel) { fontSize = 16, fontStyle = FontStyle.Bold }
            );
            EditorGUILayout.Space(4f);
            EditorGUILayout.HelpBox(UIPilotLabels.Window.WorkflowGuide, MessageType.Info);
        }
    }
}
