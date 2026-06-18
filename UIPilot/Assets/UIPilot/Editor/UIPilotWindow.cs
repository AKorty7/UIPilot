using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.ActionDiscovery;
using UIPilot.Editor.Modules.Binding;
using UIPilot.Editor.Modules.ScriptSetup;
using UIPilot.Editor.Modules.UIGenerator;
using UIPilot.Editor.Modules.SceneAudit;
using UIPilot.Editor.Modules.Validation;

namespace UIPilot.Editor
{
    public sealed class UIPilotWindow : EditorWindow
    {
        // ── Scene Audit state ────────────────────────────────────────────────
        private bool                    _sceneAuditFoldout = false;
        private List<SceneAuditResult>  _auditResults      = null;
        private Vector2                 _auditScrollPos;

        // ── Quick Build state ────────────────────────────────────────────────
        private bool _quickBuildFoldout = true;
        private bool _manualFoldout     = false;
        private bool _qbMainMenu        = true;
        private bool _qbPauseMenu       = true;
        private bool _qbSettingsMenu    = false;

        // ── Generate state ───────────────────────────────────────────────────
        private MenuType _selectedMenuType;

        // ── Discover state ───────────────────────────────────────────────────
        private List<DiscoveredAction> _discoveredActions;
        private Vector2                _discoverScrollPos;

        // ── Validate state ───────────────────────────────────────────────────
        private List<ValidationResult> _validationResults;
        private Vector2                _validateScrollPos;

        // ── Wire state ───────────────────────────────────────────────────────
        private List<Button>            _wiredButtons;
        private List<DiscoveredAction>  _availableActions;
        private Dictionary<string, int> _bindingSelections;
        private string[]                _actionDropdownOptions;
        private Vector2                 _wireScrollPos;

        // ── Cached GUIContent ────────────────────────────────────────────────
        private static readonly GUIContent ContentGenerate      = new GUIContent(UIPilotLabels.Generate.ButtonLabel,       UIPilotLabels.Generate.TooltipGenerate);
        private static readonly GUIContent ContentClearMain     = new GUIContent(UIPilotLabels.Generate.ClearMainMenu,     UIPilotLabels.Generate.TooltipClearPanel);
        private static readonly GUIContent ContentClearPause    = new GUIContent(UIPilotLabels.Generate.ClearPauseMenu,    UIPilotLabels.Generate.TooltipClearPanel);
        private static readonly GUIContent ContentClearSet      = new GUIContent(UIPilotLabels.Generate.ClearSettings,     UIPilotLabels.Generate.TooltipClearPanel);
        private static readonly GUIContent ContentScan          = new GUIContent(UIPilotLabels.Discover.ScanButton,        UIPilotLabels.Discover.TooltipScan);
        private static readonly GUIContent ContentRefresh       = new GUIContent(UIPilotLabels.Wire.RefreshButton,         UIPilotLabels.Wire.TooltipRefresh);
        private static readonly GUIContent ContentApply         = new GUIContent(UIPilotLabels.Wire.ApplyButton,           UIPilotLabels.Wire.TooltipApply);
        private static readonly GUIContent ContentRunValidate   = new GUIContent(UIPilotLabels.Validate.RunButton,         UIPilotLabels.Validate.TooltipValidate);
        private static readonly GUIContent ContentFix           = new GUIContent(UIPilotLabels.Validate.FixButton,         UIPilotLabels.Validate.TooltipFix);
        private static readonly GUIContent ContentBuildUI        = new GUIContent(UIPilotLabels.QuickBuild.BuildButton,  UIPilotLabels.QuickBuild.TooltipBuild);
        private static readonly GUIContent ContentClearQuick    = new GUIContent(UIPilotLabels.QuickBuild.ClearButton,  UIPilotLabels.QuickBuild.TooltipClear);

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
            DrawQuickBuildSection();
            DrawSceneAuditSection();
            DrawManualSection();
        }

        // ── Section: Quick Build ─────────────────────────────────────────────

        private void DrawQuickBuildSection()
        {
            _quickBuildFoldout = EditorGUILayout.Foldout(
                _quickBuildFoldout, UIPilotLabels.QuickBuild.SectionLabel, true, EditorStyles.foldoutHeader);

            if (!_quickBuildFoldout) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _qbMainMenu     = EditorGUILayout.Toggle(UIPilotLabels.QuickBuild.MainMenuToggle,     _qbMainMenu);
                _qbPauseMenu    = EditorGUILayout.Toggle(UIPilotLabels.QuickBuild.PauseMenuToggle,    _qbPauseMenu);
                _qbSettingsMenu = EditorGUILayout.Toggle(UIPilotLabels.QuickBuild.SettingsMenuToggle, _qbSettingsMenu);

                EditorGUILayout.Space(4f);

                var anySelected = _qbMainMenu || _qbPauseMenu || _qbSettingsMenu;
                using (new EditorGUI.DisabledScope(!anySelected))
                {
                    if (GUILayout.Button(ContentBuildUI))
                        ExecuteQuickBuild();
                }

                if (GUILayout.Button(ContentClearQuick))
                {
                    ExecuteQuickClear();
                    _auditResults = null;
                }
            }
        }

        private static void ExecuteQuickClear()
        {
            ClearConsole();

            var confirmed = EditorUtility.DisplayDialog(
                UIPilotLabels.QuickBuild.DialogTitle,
                UIPilotLabels.QuickBuild.DialogMessage,
                UIPilotLabels.QuickBuild.DialogConfirm,
                UIPilotLabels.QuickBuild.DialogCancel);

            if (!confirmed) return;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(UIPilotLabels.QuickBuild.ClearButton);

            var canvasGO  = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);
            var managerGO = GameObject.Find(ScriptSetupContent.GameObjects.ManagerName);

            if (canvasGO != null)
                Undo.DestroyObjectImmediate(canvasGO);

            if (managerGO != null)
                Undo.DestroyObjectImmediate(managerGO);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(UIPilotLabels.QuickBuild.ConsoleCleared);
        }

        private void ExecuteQuickBuild()
        {
            _auditResults = null;
            ClearConsole();
            Debug.Log(UIPilotLabels.QuickBuild.ConsoleStart);

            var selectedMenus = BuildSelectedMenuArray();

            foreach (var menu in selectedMenus)
                UIGeneratorModule.Generate(menu);

            ScriptSetupModule.GenerateGameManager(selectedMenus);

            EditorApplication.delayCall += QuickBuildDelayedWire;
        }

        private MenuType[] BuildSelectedMenuArray()
        {
            var list = new List<MenuType>();
            if (_qbMainMenu)     list.Add(MenuType.MainMenu);
            if (_qbPauseMenu)    list.Add(MenuType.PauseMenu);
            if (_qbSettingsMenu) list.Add(MenuType.SettingsMenu);
            return list.ToArray();
        }

        private void QuickBuildDelayedWire()
        {
            EditorApplication.delayCall -= QuickBuildDelayedWire;

            var go = GameObject.Find(ScriptSetupContent.GameObjects.ManagerName);
            if (go == null)
            {
                Debug.LogWarning(UIPilotLabels.QuickBuild.WarnGameObjectNotFound);
                return;
            }

            var managerType = Type.GetType("UIPilot_GameManager, Assembly-CSharp");
            if (managerType == null)
            {
                Debug.LogWarning(UIPilotLabels.QuickBuild.WarnTypeNotResolved);
                return;
            }

            go.AddComponent(managerType);
            EditorUtility.SetDirty(go);

            var buttons  = BindingModule.FindUIPilotButtons();
            var actions  = ActionDiscoveryModule.Scan();
            var selections = BuildAutoSelections(buttons, actions);

            BindingModule.ApplyBindings(selections, actions);
            ValidationModule.Validate();

            Debug.Log(UIPilotLabels.QuickBuild.ConsoleComplete);

            EditorUtility.SetDirty(go);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static Dictionary<string, int> BuildAutoSelections(
            List<Button> buttons, List<DiscoveredAction> actions)
        {
            const string btnPrefix = "UIPilot_Btn_";
            var selections = new Dictionary<string, int>();

            foreach (var btn in buttons)
            {
                if (btn == null) continue;

                var label = btn.name.StartsWith(btnPrefix, StringComparison.Ordinal)
                    ? btn.name.Substring(btnPrefix.Length)
                    : btn.name;

                var expectedMethod = "On" + label + "Pressed";

                for (var i = 0; i < actions.Count; i++)
                {
                    if (actions[i].MethodName == expectedMethod)
                    {
                        selections[btn.name] = i + 1; // +1 because index 0 = "None"
                        break;
                    }
                }

                if (!selections.ContainsKey(btn.name))
                    selections[btn.name] = 0;
            }

            return selections;
        }

        // ── Section: Scene Audit ─────────────────────────────────────────────

        private void DrawSceneAuditSection()
        {
            EditorGUILayout.Space(6f);
            _sceneAuditFoldout = EditorGUILayout.Foldout(
                _sceneAuditFoldout, SceneAuditContent.UI.SectionHeader, true, EditorStyles.foldoutHeader);

            if (!_sceneAuditFoldout) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button(SceneAuditContent.UI.ScanButton))
                    RunSceneAudit();

                if (_auditResults == null) return;

                EditorGUILayout.Space(4f);

                _auditScrollPos = EditorGUILayout.BeginScrollView(
                    _auditScrollPos, GUILayout.Height(180f));

                foreach (var result in _auditResults)
                    DrawAuditRow(result);

                EditorGUILayout.EndScrollView();
            }
        }

        private void RunSceneAudit()
        {
            ClearConsole();
            _auditResults = SceneAuditModule.Scan();

            var issues = 0;
            foreach (var r in _auditResults)
                if (r.Severity != SceneAuditSeverity.OK) issues++;

            Debug.Log(string.Format(
                UIPilotLabels.SceneAudit.ConsoleSummary, _auditResults.Count, issues));
        }

        private static void DrawAuditRow(SceneAuditResult result)
        {
            Color rowColor;
            switch (result.Severity)
            {
                case SceneAuditSeverity.OK:
                    rowColor = new Color(0.2f, 0.8f, 0.2f);
                    break;
                case SceneAuditSeverity.Warning:
                    rowColor = new Color(0.9f, 0.8f, 0.1f);
                    break;
                default: // Missing, Broken
                    rowColor = new Color(0.9f, 0.2f, 0.2f);
                    break;
            }

            var prev = GUI.color;
            GUI.color = rowColor;

            var text = string.IsNullOrEmpty(result.Detail)
                ? result.Label
                : result.Label + "  —  " + result.Detail;

            EditorGUILayout.LabelField(text);
            GUI.color = prev;
        }

        // ── Section: Manual (foldout wrapper) ────────────────────────────────

        private void DrawManualSection()
        {
            EditorGUILayout.Space(6f);
            _manualFoldout = EditorGUILayout.Foldout(
                _manualFoldout, UIPilotLabels.QuickBuild.ManualSectionLabel, true, EditorStyles.foldoutHeader);

            if (!_manualFoldout) return;

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

        // ── Section: Validate ────────────────────────────────────────────────

        private void DrawValidateSection()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(UIPilotLabels.Sections.Validate, EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button(ContentRunValidate))
                    _validationResults = ValidationModule.Validate();

                if (_validationResults == null) return;

                EditorGUILayout.Space(4f);

                var hasIssues = false;
                foreach (var r in _validationResults)
                    if (r.Severity == ValidationSeverity.Error || r.Severity == ValidationSeverity.Warning)
                    { hasIssues = true; break; }

                if (!hasIssues)
                {
                    EditorGUILayout.LabelField(UIPilotLabels.Validate.AllClear,
                        EditorStyles.centeredGreyMiniLabel);
                    return;
                }

                _validateScrollPos = EditorGUILayout.BeginScrollView(
                    _validateScrollPos, GUILayout.Height(150f));

                foreach (var result in _validationResults)
                    DrawValidationRow(result);

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawValidationRow(ValidationResult result)
        {
            Color rowColor;
            string icon;

            switch (result.Severity)
            {
                case ValidationSeverity.Error:
                    rowColor = new Color(1f, 0.35f, 0.35f);
                    icon     = "✖ ";
                    break;
                case ValidationSeverity.Warning:
                    rowColor = new Color(1f, 0.75f, 0.2f);
                    icon     = "⚠ ";
                    break;
                default:
                    rowColor = new Color(0.6f, 0.6f, 0.6f);
                    icon     = "● ";
                    break;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                var prev = GUI.color;
                GUI.color = rowColor;
                EditorGUILayout.LabelField(icon + result.Message);
                GUI.color = prev;

                if (result.AutoFix != null)
                {
                    if (GUILayout.Button(ContentFix, GUILayout.Width(38f)))
                    {
                        ValidationModule.RunAutoFix(result);
                        _validationResults = ValidationModule.Validate();
                    }
                }
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

        // ── Utilities ────────────────────────────────────────────────────────

        private static void ClearConsole()
        {
            var assembly = System.Reflection.Assembly
                .GetAssembly(typeof(UnityEditor.Editor));
            var type   = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(null, null);
        }
    }
}
