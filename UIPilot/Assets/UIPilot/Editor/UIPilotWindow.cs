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
using UIPilot.Editor.Modules.Resilience;
using UIPilot.Editor.Modules.SceneAudit;
using UIPilot.Editor.Modules.Validation;

namespace UIPilot.Editor
{
    public sealed class UIPilotWindow : EditorWindow
    {
        // ── Header state ─────────────────────────────────────────────────────
        private bool _showHelperText = true;

        // ── Collapse All state ───────────────────────────────────────────────
        private bool _allCollapsed   = false;
        private bool _savedBuildOpen;
        private bool _savedScanOpen;
        private bool _savedManualOpen;

        // ── Build section state ──────────────────────────────────────────────
        private bool _buildFoldout   = true;
        private bool _qbMainMenu     = true;
        private bool _qbPauseMenu    = true;
        private bool _qbSettingsMenu = false;

        // ── Scan & Repair state ──────────────────────────────────────────────
        private bool                                    _scanRepairFoldout = false;
        private List<SceneAuditResult>                  _auditResults      = null;
        private Vector2                                 _auditScrollPos;
        private List<ResilienceModule.ResilienceResult> _repairResults     = null;
        private Vector2                                 _repairScrollPos;

        // ── Validate state ───────────────────────────────────────────────────
        private List<ValidationResult> _validationResults;
        private Vector2                _validateScrollPos;

        // ── Manual section state ─────────────────────────────────────────────
        private bool _manualFoldout = false;

        // ── Generate state ───────────────────────────────────────────────────
        private MenuType _selectedMenuType;

        // ── Discover state ───────────────────────────────────────────────────
        private List<DiscoveredAction> _discoveredActions;
        private Vector2                _discoverScrollPos;

        // ── Window scroll state ──────────────────────────────────────────────
        private Vector2 _windowScrollPos;

        // ── Wire state ───────────────────────────────────────────────────────
        private List<Button>            _wiredButtons;
        private List<DiscoveredAction>  _availableActions;
        private Dictionary<string, int> _bindingSelections;
        private string[]                _actionDropdownOptions;
        private Vector2                 _wireScrollPos;

        // ── Cached GUIContent ────────────────────────────────────────────────
        private static readonly GUIContent ContentScanScene   = new GUIContent(SceneAuditContent.UI.ScanButton,        UIPilotLabels.SceneAudit.TooltipScanScene);
        private static readonly GUIContent ContentRepairScene = new GUIContent(UIPilotLabels.Resilience.RepairSceneButton, UIPilotLabels.Resilience.RepairSceneTooltip);
        private static readonly GUIContent ContentRunValidate = new GUIContent(UIPilotLabels.Validate.RunButton,       UIPilotLabels.Validate.TooltipValidate);
        private static readonly GUIContent ContentFix         = new GUIContent(UIPilotLabels.Validate.FixButton,       UIPilotLabels.Validate.TooltipFix);
        private static readonly GUIContent ContentGenerate    = new GUIContent(UIPilotLabels.Generate.ButtonLabel,     UIPilotLabels.Generate.TooltipGenerate);
        private static readonly GUIContent ContentClearMain   = new GUIContent(UIPilotLabels.Generate.ClearMainMenu,   UIPilotLabels.Generate.TooltipClearPanel);
        private static readonly GUIContent ContentClearPause  = new GUIContent(UIPilotLabels.Generate.ClearPauseMenu,  UIPilotLabels.Generate.TooltipClearPanel);
        private static readonly GUIContent ContentClearSet    = new GUIContent(UIPilotLabels.Generate.ClearSettings,   UIPilotLabels.Generate.TooltipClearPanel);
        private static readonly GUIContent ContentScan        = new GUIContent(UIPilotLabels.Discover.ScanButton,      UIPilotLabels.Discover.TooltipScan);
        private static readonly GUIContent ContentRefresh     = new GUIContent(UIPilotLabels.Wire.RefreshButton,       UIPilotLabels.Wire.TooltipRefresh);
        private static readonly GUIContent ContentApply       = new GUIContent(UIPilotLabels.Wire.ApplyButton,         UIPilotLabels.Wire.TooltipApply);
        private static readonly GUIContent ContentBuildUI     = new GUIContent(UIPilotLabels.QuickBuild.BuildButton,   UIPilotLabels.QuickBuild.TooltipBuild);
        private static readonly GUIContent ContentClearQuick  = new GUIContent(UIPilotLabels.QuickBuild.ClearButton,   UIPilotLabels.QuickBuild.TooltipClear);

        // ── Lifecycle ────────────────────────────────────────────────────────

        [MenuItem(UIPilotLabels.Menu.WindowPath)]
        public static void Open()
        {
            var window = GetWindow<UIPilotWindow>();
            window.titleContent = new GUIContent(UIPilotLabels.Window.Title);
            window.minSize      = new Vector2(380f, 480f);
            window.Show();
        }

        private void OnEnable()
        {
            minSize         = new Vector2(300f, 500f);
            _showHelperText = EditorPrefs.GetBool(UIPilotLabels.Window.EditorPrefsHelperText, true);
            _buildFoldout   = EditorPrefs.GetBool(UIPilotLabels.Window.EditorPrefsBuildOpen,  true);
            _manualFoldout  = EditorPrefs.GetBool(UIPilotLabels.Window.EditorPrefsManualOpen, false);
        }

        // ── GUI ──────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            _windowScrollPos = EditorGUILayout.BeginScrollView(_windowScrollPos);
            DrawHeader();
            EditorGUILayout.Space(6f);
            DrawBuildSection();
            DrawScanRepairSection();
            DrawManualSection();
            EditorGUILayout.EndScrollView();
        }

        // ── Header ───────────────────────────────────────────────────────────

        private void DrawHeader()
        {
            EditorGUILayout.Space(6f);

            // Title row with Collapse All toggle
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    UIPilotLabels.Window.Title,
                    new GUIStyle(EditorStyles.largeLabel) { fontSize = 16, fontStyle = FontStyle.Bold });

                var collapseLabel = _allCollapsed
                    ? UIPilotLabels.Window.ExpandAllButton
                    : UIPilotLabels.Window.CollapseAllButton;

                if (GUILayout.Button(collapseLabel, GUILayout.Width(22f), GUILayout.Height(18f)))
                    ToggleCollapseAll();
            }

            // Helper text row with ? toggle
            using (new EditorGUILayout.HorizontalScope())
            {
                if (_showHelperText)
                {
                    var wrapStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };
                    EditorGUILayout.LabelField(UIPilotLabels.Window.HelperText, wrapStyle);
                }
                else
                {
                    GUILayout.FlexibleSpace();
                }

                if (GUILayout.Button(UIPilotLabels.Window.HelpToggle, GUILayout.Width(22f), GUILayout.Height(18f)))
                {
                    _showHelperText = !_showHelperText;
                    EditorPrefs.SetBool(UIPilotLabels.Window.EditorPrefsHelperText, _showHelperText);
                }
            }
        }

        private void ToggleCollapseAll()
        {
            if (!_allCollapsed)
            {
                _savedBuildOpen    = _buildFoldout;
                _savedScanOpen     = _scanRepairFoldout;
                _savedManualOpen   = _manualFoldout;
                _buildFoldout      = false;
                _scanRepairFoldout = false;
                _manualFoldout     = false;
            }
            else
            {
                _buildFoldout      = _savedBuildOpen;
                _scanRepairFoldout = _savedScanOpen;
                _manualFoldout     = _savedManualOpen;
            }

            _allCollapsed = !_allCollapsed;
        }

        // ── Section: Build ───────────────────────────────────────────────────

        private void DrawBuildSection()
        {
            var newOpen = EditorGUILayout.Foldout(
                _buildFoldout, UIPilotLabels.Sections.Build, true, EditorStyles.foldoutHeader);

            if (newOpen != _buildFoldout)
            {
                _buildFoldout = newOpen;
                _allCollapsed = false;
                EditorPrefs.SetBool(UIPilotLabels.Window.EditorPrefsBuildOpen, _buildFoldout);
            }

            if (!_buildFoldout) return;

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
                    _auditResults  = null;
                    _repairResults = null;
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
            _auditResults  = null;
            _repairResults = null;
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

            var buttons    = BindingModule.FindUIPilotButtons();
            var actions    = ActionDiscoveryModule.Scan();
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

        // ── Section: Scan & Repair ───────────────────────────────────────────

        private void DrawScanRepairSection()
        {
            EditorGUILayout.Space(6f);

            var newOpen = EditorGUILayout.Foldout(
                _scanRepairFoldout, UIPilotLabels.Sections.ScanRepair, true, EditorStyles.foldoutHeader);

            if (newOpen != _scanRepairFoldout)
            {
                _scanRepairFoldout = newOpen;
                _allCollapsed      = false;
            }

            if (!_scanRepairFoldout) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // 1. Scan Scene button
                if (GUILayout.Button(ContentScanScene))
                {
                    RunSceneAudit();
                    _repairResults = null;
                }

                // 2. Audit results
                if (_auditResults != null)
                {
                    EditorGUILayout.Space(4f);

                    GUILayout.BeginVertical(GUILayout.Height(200f));

                    foreach (var result in _auditResults)
                        DrawAuditRow(result);

                    GUILayout.EndVertical();

                    // 3. Repair Scene button — conditional on issues
                    if (AuditHasIssues())
                    {
                        EditorGUILayout.Space(4f);
                        if (GUILayout.Button(ContentRepairScene))
                            RunRepair();
                    }

                    // 4. Repair results
                    if (_repairResults != null)
                    {
                        EditorGUILayout.Space(4f);
                        EditorGUILayout.LabelField(UIPilotLabels.Resilience.RepairResultsHeader,
                            EditorStyles.boldLabel);

                        GUILayout.BeginVertical(GUILayout.Height(120f));

                        foreach (var r in _repairResults)
                            DrawRepairRow(r);

                        GUILayout.EndVertical();
                    }
                }

                // 5. Divider
                EditorGUILayout.Space(8f);
                DrawSeparator();
                EditorGUILayout.Space(4f);

                // 6. Run Validation button
                if (GUILayout.Button(ContentRunValidate))
                    _validationResults = ValidationModule.Validate();

                // 7. Validation results
                if (_validationResults != null)
                {
                    EditorGUILayout.Space(4f);

                    var hasIssues = false;
                    foreach (var r in _validationResults)
                        if (r.Severity == ValidationSeverity.Error || r.Severity == ValidationSeverity.Warning)
                        { hasIssues = true; break; }

                    if (!hasIssues)
                    {
                        EditorGUILayout.LabelField(UIPilotLabels.Validate.AllClear,
                            EditorStyles.centeredGreyMiniLabel);
                    }
                    else
                    {
                        GUILayout.BeginVertical(GUILayout.Height(150f));

                        foreach (var result in _validationResults)
                            DrawValidationRow(result);

                        GUILayout.EndVertical();
                    }
                }
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

        private bool AuditHasIssues()
        {
            if (_auditResults == null) return false;
            foreach (var r in _auditResults)
                if (r.Severity != SceneAuditSeverity.OK) return true;
            return false;
        }

        private void RunRepair()
        {
            ClearConsole();
            Debug.Log(ResilienceContent.Console.RepairStart);

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(ResilienceContent.UndoLabel);

            var followUps = ResilienceModule.Repair(_auditResults);
            _repairResults = ExecuteRepairFollowUps(followUps);

            var validationResults = ValidationModule.Validate();
            var remainingIssues   = 0;
            foreach (var v in validationResults)
                if (v.Severity == ValidationSeverity.Error || v.Severity == ValidationSeverity.Warning)
                    remainingIssues++;

            Debug.Log(string.Format(ResilienceContent.Console.ValidationLog, remainingIssues));

            Undo.CollapseUndoOperations(undoGroup);

            var succeeded = 0;
            foreach (var r in _repairResults)
                if (r.Success) succeeded++;

            Debug.Log(string.Format(
                ResilienceContent.Console.RepairComplete, _repairResults.Count, succeeded));

            _auditResults = SceneAuditModule.Scan();
            Repaint();
        }

        // Runs the cross-module steps Resilience recorded, in audit order.
        private static List<ResilienceModule.ResilienceResult> ExecuteRepairFollowUps(
            List<ResilienceModule.RepairFollowUp> followUps)
        {
            var results = new List<ResilienceModule.ResilienceResult>();

            foreach (var followUp in followUps)
            {
                if (!followUp.NeedsWindowAction)
                {
                    results.Add(followUp.CompletedResult);
                    continue;
                }

                try
                {
                    switch (followUp.Kind)
                    {
                        case ResilienceModule.RepairFollowUpKind.GeneratePanel:
                        {
                            UIGeneratorModule.Generate(followUp.MenuType);
                            break;
                        }

                        case ResilienceModule.RepairFollowUpKind.GenerateGameManager:
                        {
                            ScriptSetupModule.GenerateGameManager(followUp.Menus);
                            break;
                        }

                        case ResilienceModule.RepairFollowUpKind.ReapplyBindings:
                        {
                            var buttons    = BindingModule.FindUIPilotButtons();
                            var actions    = ActionDiscoveryModule.Scan();
                            var selections = BuildAutoSelections(buttons, actions);
                            BindingModule.ApplyBindings(selections, actions);
                            break;
                        }
                    }

                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                    var success = true;
                    if (followUp.Kind == ResilienceModule.RepairFollowUpKind.GeneratePanel
                        && followUp.ResultLabel == ResilienceContent.Labels.Canvas)
                    {
                        success = GameObject.Find(UIGeneratorContent.GameObjects.Canvas) != null;
                    }

                    results.Add(ResilienceModule.MakeResult(
                        followUp.ResultLabel, followUp.SuccessDetail, success));
                }
                catch (Exception ex)
                {
                    results.Add(ResilienceModule.MakeResult(
                        followUp.ResultLabel, ex.Message, false));
                }
            }

            return results;
        }

        private static void DrawAuditRow(SceneAuditResult result)
        {
            Color rowColor;
            switch (result.Severity)
            {
                case SceneAuditSeverity.OK:
                    rowColor = new Color(0.5f, 0.5f, 0.5f);
                    break;
                case SceneAuditSeverity.Warning:
                    rowColor = new Color(0.9f, 0.8f, 0.1f);
                    break;
                default: // Missing, Broken
                    rowColor = new Color(0.9f, 0.2f, 0.2f);
                    break;
            }

            var style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = rowColor;

            var text = string.IsNullOrEmpty(result.Detail)
                ? result.Label
                : result.Label + "  —  " + result.Detail;

            EditorGUILayout.LabelField(text, style);
        }

        private static void DrawRepairRow(ResilienceModule.ResilienceResult result)
        {
            var style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = result.Success
                ? new Color(0.2f, 0.8f, 0.2f)
                : new Color(0.9f, 0.2f, 0.2f);

            var prefix = result.Success
                ? UIPilotLabels.Resilience.RepairSuccessPrefix
                : UIPilotLabels.Resilience.RepairFailPrefix;

            var body = string.IsNullOrEmpty(result.Detail)
                ? result.Label
                : result.Label + "  —  " + result.Detail;

            EditorGUILayout.LabelField(prefix + " " + body, style);
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

        private static void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        // ── Section: Manual ──────────────────────────────────────────────────

        private void DrawManualSection()
        {
            EditorGUILayout.Space(6f);

            var newOpen = EditorGUILayout.Foldout(
                _manualFoldout, UIPilotLabels.QuickBuild.ManualSectionLabel, true, EditorStyles.foldoutHeader);

            if (newOpen != _manualFoldout)
            {
                _manualFoldout = newOpen;
                _allCollapsed  = false;
                EditorPrefs.SetBool(UIPilotLabels.Window.EditorPrefsManualOpen, _manualFoldout);
            }

            if (!_manualFoldout) return;

            DrawGenerateSection();
            DrawDiscoverSection();
            DrawWireSection();
        }

        // ── Section: Generate ────────────────────────────────────────────────

        private void DrawGenerateSection()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(UIPilotLabels.Sections.Generate, EditorStyles.boldLabel);
            var descStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
            EditorGUILayout.LabelField(UIPilotLabels.Manual.GenerateDesc, descStyle);
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
            var descStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
            EditorGUILayout.LabelField(UIPilotLabels.Manual.DiscoverDesc, descStyle);
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
            var descStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
            EditorGUILayout.LabelField(UIPilotLabels.Manual.WireDesc, descStyle);
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

                var guidanceStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    wordWrap  = true,
                    normal    = { textColor = new Color(1f, 0.85f, 0.4f) }
                };
                EditorGUILayout.LabelField(UIPilotLabels.Manual.WireGuidance, guidanceStyle);
                EditorGUILayout.Space(2f);

                _wireScrollPos = EditorGUILayout.BeginScrollView(
                    _wireScrollPos, GUILayout.Height(140f));

                var seen = new HashSet<string>();
                foreach (var btn in _wiredButtons)
                {
                    if (btn == null) continue;
                    if (!seen.Add(btn.name)) continue;

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

                var noteStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
                EditorGUILayout.LabelField(UIPilotLabels.Manual.ApplyBindingsNote, noteStyle);
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
