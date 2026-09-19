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
    public sealed partial class UIPilotWindow : EditorWindow
    {
        // How long Unity gets to start compiling before a waiting build counts as stalled.
        private const double CompileGraceSeconds = 5.0;

        // ── Header state ─────────────────────────────────────────────────────
        private bool _showHelperText = true;

        // ── Collapse All state ───────────────────────────────────────────────
        private bool _allCollapsed   = false;
        private bool _savedHealthOpen;
        private bool _savedBuildOpen;
        private bool _savedScanOpen;
        private bool _savedManualOpen;

        // ── Build section state ──────────────────────────────────────────────
        private bool _buildFoldout   = true;
        private bool _qbMainMenu     = true;
        private bool _qbPauseMenu    = true;
        private bool _qbSettingsMenu = true;

        // The look Build UI generates. Null = the built-in Soft Club look.
        // Serialized so Unity counts it as a reference: opening another scene
        // unloads assets nothing refers to, and the choice would silently reset.
        [SerializeField] private UIPilotTheme _theme;

        // Shipped presets deleted from the project. Refreshed when the project
        // changes, not per repaint: finding them is an AssetDatabase search.
        private List<string> _missingPresets = new List<string>();

        // ── Scan & Repair state ──────────────────────────────────────────────
        private bool                                    _scanRepairFoldout = false;
        private List<SceneAuditResult>                  _auditResults      = null;
        private string                                  _auditSummary      = null;
        private int                                     _auditIssueCount   = 0;
        private List<ResilienceModule.ResilienceResult> _repairResults     = null;

        // ── Validate state ───────────────────────────────────────────────────
        private List<ValidationResult> _validationResults;

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
        private readonly HashSet<string> _wireSeenNames = new HashSet<string>();

        // ── Cached GUIContent ────────────────────────────────────────────────
        private static readonly GUIContent ContentHelp        = new GUIContent(UIPilotLabels.Window.HelpToggle,        UIPilotLabels.Window.TooltipHelp);
        private static readonly GUIContent ContentCollapse    = new GUIContent(UIPilotLabels.Window.CollapseAllButton, UIPilotLabels.Window.TooltipCollapse);
        private static readonly GUIContent ContentExpand      = new GUIContent(UIPilotLabels.Window.ExpandAllButton,   UIPilotLabels.Window.TooltipExpand);
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
        private static readonly GUIContent ContentTheme       = new GUIContent(UIPilotLabels.Theme.FieldLabel,         UIPilotLabels.Theme.FieldTooltip);
        private static readonly GUIContent ContentApplyTheme  = new GUIContent(UIPilotLabels.Theme.ApplyButton,        UIPilotLabels.Theme.ApplyTooltip);
        private static readonly GUIContent ContentRestoreThemes = new GUIContent(UIPilotLabels.Theme.RestoreButton,    UIPilotLabels.Theme.RestoreTooltip);
        private static readonly GUIContent ContentClearQuick  = new GUIContent(UIPilotLabels.QuickBuild.ClearButton,   UIPilotLabels.QuickBuild.TooltipClear);

        // ── Lifecycle ────────────────────────────────────────────────────────

        [MenuItem(UIPilotLabels.Menu.WindowPath)]
        public static void Open()
        {
            var window = GetWindow<UIPilotWindow>();
            window.titleContent = new GUIContent(UIPilotLabels.Window.Title);
            window.Show();
        }

        private void OnEnable()
        {
            minSize            = new Vector2(320f, 480f);
            _showHelperText    = EditorPrefs.GetBool(UIPilotLabels.Window.EditorPrefsHelperText, true);
            _buildFoldout      = EditorPrefs.GetBool(UIPilotLabels.Window.EditorPrefsBuildOpen,  true);
            _scanRepairFoldout = EditorPrefs.GetBool(UIPilotLabels.Window.EditorPrefsScanOpen,   false);
            _manualFoldout     = EditorPrefs.GetBool(UIPilotLabels.Window.EditorPrefsManualOpen, false);
            _healthFoldout       = EditorPrefs.GetBool(UIPilotLabels.Health.EditorPrefsOpen,       true);
            _healthChecksFoldout = EditorPrefs.GetBool(UIPilotLabels.Health.EditorPrefsChecksOpen, false);
            _theme             = LoadSavedTheme();
            RefreshMissingPresets();
            UIPilotHealthMonitor.Changed += Repaint;

            // A Quick Build that had to wait for script compilation resumes here:
            // OnEnable runs again after the domain reload, delayCall does not survive it.
            if (SessionState.GetBool(UIPilotLabels.QuickBuild.SessionPendingWire, false))
            {
                SessionState.EraseBool(UIPilotLabels.QuickBuild.SessionPendingWire);
                EditorApplication.delayCall += ResumeQuickBuildWire;
            }
        }

        private void OnDisable()
        {
            UIPilotHealthMonitor.Changed -= Repaint;
        }

        // A preset deleted or restored in the Project window shows up here at once.
        private void OnProjectChange()
        {
            RefreshMissingPresets();
            Repaint();
        }

        // Keeps the Build status row live while Unity compiles; idle otherwise.
        private void OnInspectorUpdate()
        {
            if (SessionState.GetBool(UIPilotLabels.QuickBuild.SessionPendingWire, false))
                Repaint();
        }

        // ── GUI ──────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            UIPilotStyles.EnsureBuilt();

            _windowScrollPos = EditorGUILayout.BeginScrollView(_windowScrollPos);

            // A tool window gets docked at any width. Past a comfortable measure
            // the content stops stretching, instead of every button turning into
            // a banner across the screen.
            using (new EditorGUILayout.VerticalScope(UIPilotStyles.ContentMaxWidth))
            {
                DrawHeader();
                EditorGUILayout.Space(4f);
                DrawHealthSection();
                DrawBuildSection();
                DrawScanRepairSection();
                DrawManualSection();
                EditorGUILayout.Space(8f);
            }

            EditorGUILayout.EndScrollView();
        }

        // ── Header ───────────────────────────────────────────────────────────

        private void DrawHeader()
        {
            EditorGUILayout.Space(8f);

            // Accent bar + title on the left, the two window controls on the right.
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(4f);
                var barRect = GUILayoutUtility.GetRect(
                    GUIContent.none, GUIStyle.none, UIPilotStyles.AccentBarSize);
                EditorGUI.DrawRect(barRect, UIPilotStyles.Accent);
                GUILayout.Space(8f);

                GUILayout.Label(UIPilotLabels.Window.Title, UIPilotStyles.Title);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(ContentHelp, UIPilotStyles.IconButtonSize))
                {
                    _showHelperText = !_showHelperText;
                    EditorPrefs.SetBool(UIPilotLabels.Window.EditorPrefsHelperText, _showHelperText);
                }

                if (GUILayout.Button(_allCollapsed ? ContentExpand : ContentCollapse,
                        UIPilotStyles.IconButtonSize))
                    ToggleCollapseAll();
            }

            if (_showHelperText)
            {
                EditorGUILayout.Space(4f);
                GUILayout.Label(UIPilotLabels.Window.HelperText, UIPilotStyles.Helper);
            }

            EditorGUILayout.Space(8f);
            DrawSeparator();
        }

        // Draws a section's foldout header and keeps its open state, the
        // Collapse All toggle and EditorPrefs in step. Returns the open state.
        private bool DrawSectionFoldout(ref bool isOpen, string label, string prefsKey)
        {
            EditorGUILayout.Space(8f);

            // A header strip, like the Inspector's component headers, so the three
            // sections read as the window's top level and what is inside as nested.
            var strip = GUILayoutUtility.GetRect(
                GUIContent.none, GUIStyle.none, UIPilotStyles.SectionHeaderSize);
            EditorGUI.DrawRect(strip, UIPilotStyles.SectionStrip);
            EditorGUI.DrawRect(new Rect(strip.x, strip.y, strip.width, 1f), UIPilotStyles.SectionStripLine);

            var foldRect = new Rect(strip.x + 6f, strip.y + 3f, strip.width - 6f, 18f);
            var newOpen  = EditorGUI.Foldout(foldRect, isOpen, label, true, UIPilotStyles.SectionFoldout);
            if (newOpen != isOpen)
            {
                isOpen        = newOpen;
                _allCollapsed = false;
                EditorPrefs.SetBool(prefsKey, isOpen);
            }

            return isOpen;
        }

        private void ToggleCollapseAll()
        {
            if (!_allCollapsed)
            {
                _savedHealthOpen   = _healthFoldout;
                _savedBuildOpen    = _buildFoldout;
                _savedScanOpen     = _scanRepairFoldout;
                _savedManualOpen   = _manualFoldout;
                _healthFoldout     = false;
                _buildFoldout      = false;
                _scanRepairFoldout = false;
                _manualFoldout     = false;
            }
            else
            {
                _healthFoldout     = _savedHealthOpen;
                _buildFoldout      = _savedBuildOpen;
                _scanRepairFoldout = _savedScanOpen;
                _manualFoldout     = _savedManualOpen;
            }

            _allCollapsed = !_allCollapsed;
        }

        // ── Section: Build ───────────────────────────────────────────────────

        private void DrawBuildSection()
        {
            if (!DrawSectionFoldout(ref _buildFoldout, UIPilotLabels.Sections.Build,
                    UIPilotLabels.Window.EditorPrefsBuildOpen))
                return;

            using (new EditorGUILayout.VerticalScope(UIPilotStyles.Card))
            {
                _qbMainMenu     = EditorGUILayout.ToggleLeft(UIPilotLabels.QuickBuild.MainMenuToggle,     _qbMainMenu);
                _qbPauseMenu    = EditorGUILayout.ToggleLeft(UIPilotLabels.QuickBuild.PauseMenuToggle,    _qbPauseMenu);
                _qbSettingsMenu = EditorGUILayout.ToggleLeft(UIPilotLabels.QuickBuild.SettingsMenuToggle, _qbSettingsMenu);

                EditorGUILayout.Space(8f);
                DrawThemeField();
                EditorGUILayout.Space(8f);

                // The one primary action in the window: taller, bold, accent-tinted.
                // Disabled while a build is waiting on Unity's compiler, so a second
                // click cannot start another one on top of it.
                var anySelected = _qbMainMenu || _qbPauseMenu || _qbSettingsMenu;
                using (new EditorGUI.DisabledScope(!anySelected || IsBuildCompiling()))
                {
                    var previousTint = GUI.backgroundColor;
                    GUI.backgroundColor = UIPilotStyles.PrimaryTint;
                    var buildClicked = GUILayout.Button(ContentBuildUI, UIPilotStyles.PrimaryButton);
                    GUI.backgroundColor = previousTint;

                    // Both actions can open a dialog mid-layout; ExitGUI stops IMGUI
                    // from finishing a layout pass the dialog has invalidated.
                    if (buildClicked)
                    {
                        ExecuteQuickBuild();
                        GUIUtility.ExitGUI();
                    }
                }

                EditorGUILayout.Space(4f);

                // Restyling sits with Build UI. Quick Clear, the one destructive
                // action, comes last and stands apart from both.
                if (GUILayout.Button(ContentApplyTheme))
                    ApplyTheme();

                EditorGUILayout.Space(8f);

                if (GUILayout.Button(ContentClearQuick))
                {
                    ExecuteQuickClear();
                    _auditResults  = null;
                    _repairResults = null;
                    GUIUtility.ExitGUI();
                }

                DrawBuildStatus();
            }
        }

        // ── Theme ────────────────────────────────────────────────────────────

        private void DrawThemeField()
        {
            // Unloaded or deleted since it was picked: restore it from the saved
            // GUID (which stays empty if the developer cleared the field on purpose).
            if (_theme == null
                && !string.IsNullOrEmpty(EditorPrefs.GetString(UIPilotLabels.Theme.EditorPrefsKey, string.Empty)))
                _theme = LoadSavedTheme();

            EditorGUI.BeginChangeCheck();
            var picked = (UIPilotTheme)EditorGUILayout.ObjectField(
                ContentTheme, _theme, typeof(UIPilotTheme), false);

            if (EditorGUI.EndChangeCheck())
            {
                _theme = picked;
                SaveTheme();
            }

            if (_theme == null)
                GUILayout.Label(UIPilotLabels.Theme.BuiltInHint, UIPilotStyles.Description);

            DrawMissingPresets();
        }

        // Only there while a shipped preset is gone, so the Build card stays
        // quiet the rest of the time. The fix sits on the same line as the problem.
        private void DrawMissingPresets()
        {
            if (_missingPresets.Count == 0) return;

            EditorGUILayout.Space(2f);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(string.Format(UIPilotLabels.Theme.MissingHint,
                        string.Join(UIPilotLabels.Theme.ListSeparator, _missingPresets)),
                    UIPilotStyles.Description);

                if (GUILayout.Button(ContentRestoreThemes, EditorStyles.miniButton, UIPilotStyles.FitWidth))
                {
                    RestoreDefaultThemes();
                    GUIUtility.ExitGUI();
                }
            }
        }

        private void RefreshMissingPresets()
        {
            _missingPresets = UIGeneratorModule.FindMissingDefaultThemes();
        }

        // Confirms in the Console and pings the first restored asset, so the
        // developer sees where the presets went.
        private void RestoreDefaultThemes()
        {
            var restored = UIGeneratorModule.RestoreDefaultThemes();
            RefreshMissingPresets();
            if (restored.Count == 0) return;

            Debug.Log(string.Format(UIPilotLabels.Theme.ConsoleRestored,
                string.Join(UIPilotLabels.Theme.ListSeparator, restored)));
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UIPilotTheme>(restored[0]));
        }

        // The choice is remembered by asset GUID, so it survives the theme being
        // renamed or moved.
        private void SaveTheme()
        {
            var path = _theme != null ? AssetDatabase.GetAssetPath(_theme) : string.Empty;
            EditorPrefs.SetString(UIPilotLabels.Theme.EditorPrefsKey,
                string.IsNullOrEmpty(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path));
        }

        private static UIPilotTheme LoadSavedTheme()
        {
            // Never chosen: start on the shipped default preset, if it is there.
            if (!EditorPrefs.HasKey(UIPilotLabels.Theme.EditorPrefsKey))
                return UIGeneratorModule.FindDefaultThemeAsset();

            var guid = EditorPrefs.GetString(UIPilotLabels.Theme.EditorPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(guid)) return null; // deliberately cleared

            return AssetDatabase.LoadAssetAtPath<UIPilotTheme>(AssetDatabase.GUIDToAssetPath(guid));
        }

        private void ApplyTheme()
        {
            var restyled = UIGeneratorModule.ApplyTheme(_theme);
            if (restyled == 0)
            {
                Debug.Log(UIPilotLabels.Theme.ConsoleNoMenus);
                return;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(string.Format(UIPilotLabels.Theme.ConsoleApplied,
                _theme != null ? _theme.name : UIPilotLabels.Theme.BuiltInName, restyled));
        }

        // Build UI finishes on a later editor tick — after a recompile, the first
        // time — so its outcome is reported here, not only in the Console. The
        // state lives in SessionState because the window's fields do not survive
        // the domain reload that sits in the middle of a first build.
        private static void DrawBuildStatus()
        {
            if (SessionState.GetBool(UIPilotLabels.QuickBuild.SessionPendingWire, false))
            {
                EditorGUILayout.Space(4f);

                if (IsBuildStalled())
                    DrawStatusRow(UIPilotStyles.LampFault, UIPilotLabels.Status.Stalled,
                        UIPilotLabels.QuickBuild.StatusStalled, null);
                else
                    DrawStatusRow(UIPilotStyles.LampCaution, UIPilotLabels.Status.Working,
                        UIPilotLabels.QuickBuild.StatusWaiting, null);

                return;
            }

            if (!SessionState.GetBool(UIPilotLabels.QuickBuild.SessionBuildDone, false)) return;

            EditorGUILayout.Space(4f);
            DrawStatusRow(UIPilotStyles.LampOk, UIPilotLabels.Status.Ready,
                UIPilotLabels.QuickBuild.StatusDone, null);
        }

        private static bool IsBuildCompiling()
        {
            return EditorApplication.isCompiling
                && SessionState.GetBool(UIPilotLabels.QuickBuild.SessionPendingWire, false);
        }

        // Unity takes a tick or two to start compiling after the script is written,
        // so "not compiling" only counts as stalled once the grace period has passed.
        private static bool IsBuildStalled()
        {
            if (EditorApplication.isCompiling) return false;

            var waitingSince = SessionState.GetFloat(UIPilotLabels.QuickBuild.SessionPendingSince, 0f);
            return EditorApplication.timeSinceStartup - waitingSince > CompileGraceSeconds;
        }

        private static void ExecuteQuickClear()
        {
            var confirmed = EditorUtility.DisplayDialog(
                UIPilotLabels.QuickBuild.DialogTitle,
                UIPilotLabels.QuickBuild.DialogMessage,
                UIPilotLabels.QuickBuild.DialogConfirm,
                UIPilotLabels.QuickBuild.DialogCancel);

            if (!confirmed) return;

            ClearConsole();
            SessionState.EraseBool(UIPilotLabels.QuickBuild.SessionPendingWire);
            SessionState.EraseBool(UIPilotLabels.QuickBuild.SessionBuildDone);

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
            SessionState.EraseBool(UIPilotLabels.QuickBuild.SessionBuildDone);
            ClearConsole();
            Debug.Log(UIPilotLabels.QuickBuild.ConsoleStart);

            var selectedMenus = BuildSelectedMenuArray();

            foreach (var menu in selectedMenus)
                UIGeneratorModule.Generate(menu, _theme);

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
            TryQuickBuildWire(true);
        }

        private void ResumeQuickBuildWire()
        {
            EditorApplication.delayCall -= ResumeQuickBuildWire;
            TryQuickBuildWire(false);
        }

        // Attaches the GameManager component and wires every button to it.
        // Returns false only on failure — waiting for a compile counts as success.
        private static bool TryQuickBuildWire(bool canWaitForCompile)
        {
            var go = GameObject.Find(ScriptSetupContent.GameObjects.ManagerName);
            if (go == null)
            {
                Debug.LogWarning(UIPilotLabels.QuickBuild.WarnGameObjectNotFound);
                return false;
            }

            var managerType = Type.GetType("UIPilot_GameManager, Assembly-CSharp");

            // A freshly written script has no type (or a stale one) until Unity
            // compiles it and reloads the domain — hand over to OnEnable.
            if (canWaitForCompile && (managerType == null || EditorApplication.isCompiling))
            {
                SessionState.SetBool(UIPilotLabels.QuickBuild.SessionPendingWire, true);
                SessionState.SetFloat(UIPilotLabels.QuickBuild.SessionPendingSince,
                    (float)EditorApplication.timeSinceStartup);
                Debug.Log(UIPilotLabels.QuickBuild.ConsoleWaitingForCompile);
                return true;
            }

            if (managerType == null)
            {
                Debug.LogWarning(UIPilotLabels.QuickBuild.WarnTypeNotResolved);
                return false;
            }

            // Build UI on an intact scene reaches here too — never add a second copy.
            if (go.GetComponent(managerType) == null)
            {
                // A deleted-then-regenerated script leaves a dead "Missing Script" slot behind.
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                Undo.AddComponent(go, managerType);
            }

            EditorUtility.SetDirty(go);

            var buttons    = BindingModule.FindUIPilotButtons();
            var actions    = ActionDiscoveryModule.Scan();
            var selections = BuildAutoSelections(buttons, actions);

            BindingModule.ApplyBindings(selections, actions);
            ValidationModule.Validate();

            Debug.Log(UIPilotLabels.QuickBuild.ConsoleComplete);
            SessionState.SetBool(UIPilotLabels.QuickBuild.SessionBuildDone, true);

            EditorUtility.SetDirty(go);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            return true;
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
            if (!DrawSectionFoldout(ref _scanRepairFoldout, UIPilotLabels.Sections.ScanRepair,
                    UIPilotLabels.Window.EditorPrefsScanOpen))
                return;

            using (new EditorGUILayout.VerticalScope(UIPilotStyles.Card))
            {
                DrawAuditBlock();

                EditorGUILayout.Space(8f);
                DrawSeparator();
                EditorGUILayout.Space(8f);

                DrawValidationBlock();
            }
        }

        // Scan Scene → result rows → Repair Scene (only when something is wrong)
        // → repair results. Rows flow at their natural height; the window scrolls.
        private void DrawAuditBlock()
        {
            if (GUILayout.Button(ContentScanScene))
            {
                RunSceneAudit();
                _repairResults = null;
            }

            if (_auditResults == null)
            {
                GUILayout.Label(UIPilotLabels.SceneAudit.EmptyHint, UIPilotStyles.Description);
                return;
            }

            EditorGUILayout.Space(4f);
            GUILayout.Label(_auditSummary, EditorStyles.boldLabel);

            foreach (var result in _auditResults)
            {
                GetAuditStatus(result.Severity, out var lamp, out var word);
                DrawStatusRow(lamp, word, result.Label, result.Detail);
            }

            if (_auditIssueCount > 0)
            {
                EditorGUILayout.Space(4f);
                if (GUILayout.Button(ContentRepairScene))
                {
                    RunRepair();
                    GUIUtility.ExitGUI(); // repair can open a dialog mid-layout
                }
            }

            DrawRepairResults();
        }

        private void DrawRepairResults()
        {
            if (_repairResults == null) return;

            EditorGUILayout.Space(8f);
            GUILayout.Label(UIPilotLabels.Resilience.RepairResultsHeader, EditorStyles.boldLabel);

            foreach (var r in _repairResults)
                DrawStatusRow(
                    r.Success ? UIPilotStyles.LampOk       : UIPilotStyles.LampFault,
                    r.Success ? UIPilotLabels.Status.Fixed : UIPilotLabels.Status.Failed,
                    r.Label, r.Detail);
        }

        private void DrawValidationBlock()
        {
            if (GUILayout.Button(ContentRunValidate))
                _validationResults = ValidationModule.Validate();

            if (_validationResults == null)
            {
                GUILayout.Label(UIPilotLabels.Validate.EmptyHint, UIPilotStyles.Description);
                return;
            }

            EditorGUILayout.Space(4f);

            if (!ValidationHasIssues())
            {
                DrawStatusRow(UIPilotStyles.LampOk, UIPilotLabels.Status.Ok,
                    UIPilotLabels.Validate.AllClear, null);
                return;
            }

            foreach (var result in _validationResults)
                DrawValidationRow(result);
        }

        private bool ValidationHasIssues()
        {
            foreach (var r in _validationResults)
                if (r.Severity == ValidationSeverity.Error || r.Severity == ValidationSeverity.Warning)
                    return true;
            return false;
        }

        private void RunSceneAudit()
        {
            ClearConsole();
            SetAuditResults(SceneAuditModule.Scan());

            Debug.Log(string.Format(
                UIPilotLabels.SceneAudit.ConsoleSummary, _auditResults.Count, _auditIssueCount));
        }

        // The summary line is formatted here, once per scan, not on every repaint.
        private void SetAuditResults(List<SceneAuditResult> results)
        {
            _auditResults    = results;
            _auditIssueCount = 0;

            foreach (var r in results)
                if (r.Severity != SceneAuditSeverity.OK) _auditIssueCount++;

            _auditSummary = _auditIssueCount == 0
                ? string.Format(UIPilotLabels.SceneAudit.SummaryAllClear, results.Count)
                : string.Format(UIPilotLabels.SceneAudit.SummaryIssues, results.Count, _auditIssueCount);
        }

        private void RunRepair()
        {
            ClearConsole();
            Debug.Log(ResilienceContent.Console.RepairStart);

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(ResilienceContent.UndoLabel);

            var followUps = ResilienceModule.Repair(_auditResults);
            _repairResults = ExecuteRepairFollowUps(followUps, _theme);

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

            SetAuditResults(SceneAuditModule.Scan());
            Repaint();
        }

        // Runs the cross-module steps Resilience recorded, in audit order.
        private static List<ResilienceModule.ResilienceResult> ExecuteRepairFollowUps(
            List<ResilienceModule.RepairFollowUp> followUps, UIPilotTheme theme)
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
                    var success = true;

                    switch (followUp.Kind)
                    {
                        case ResilienceModule.RepairFollowUpKind.GeneratePanel:
                        {
                            UIGeneratorModule.Generate(followUp.MenuType, theme);
                            break;
                        }

                        case ResilienceModule.RepairFollowUpKind.GenerateGameManager:
                        {
                            // ScriptSetup only restores the script and a bare GameObject.
                            // The old listeners pointed at the destroyed component, so
                            // attach a new one and rebind every button to it.
                            ScriptSetupModule.GenerateGameManager(followUp.Menus);
                            success = TryQuickBuildWire(true);
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

        // ── Status rows ──────────────────────────────────────────────────────
        // One result line: a status lamp, the item name with — only when there is
        // something to say — a wrapped detail line beneath it, and the status as a
        // word. The word matters: without it Fixed/Failed and Warning/Broken would
        // differ by lamp colour alone. Text keeps the skin's own colour so it reads
        // on both the dark and the light editor.

        private static void DrawStatusRow(Color lamp, string statusWord, string label, string detail)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLamp(lamp);

                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Label(label, UIPilotStyles.RowLabel);

                    if (!string.IsNullOrEmpty(detail))
                        GUILayout.Label(detail, UIPilotStyles.RowDetail);
                }

                GUILayout.Label(statusWord, UIPilotStyles.StatusWord, UIPilotStyles.StatusWordSize);
            }
        }

        private static void DrawLamp(Color color)
        {
            var slot = GUILayoutUtility.GetRect(
                GUIContent.none, GUIStyle.none, UIPilotStyles.LampSlotSize);

            EditorGUI.DrawRect(
                new Rect(slot.x + 2f, slot.y + 5f, UIPilotStyles.LampSize, UIPilotStyles.LampSize),
                color);
        }

        // Each severity maps to a lamp and a word together, so the two never disagree.
        private static void GetAuditStatus(SceneAuditSeverity severity, out Color lamp, out string word)
        {
            switch (severity)
            {
                case SceneAuditSeverity.OK:
                    lamp = UIPilotStyles.LampOk;      word = UIPilotLabels.Status.Ok;      break;
                case SceneAuditSeverity.Warning:
                    lamp = UIPilotStyles.LampCaution; word = UIPilotLabels.Status.Warning; break;
                case SceneAuditSeverity.Missing:
                    lamp = UIPilotStyles.LampFault;   word = UIPilotLabels.Status.Missing; break;
                default:
                    lamp = UIPilotStyles.LampFault;   word = UIPilotLabels.Status.Broken;  break;
            }
        }

        // ValidationSeverity.Info marks a check that passed.
        private static void GetValidationStatus(ValidationSeverity severity, out Color lamp, out string word)
        {
            switch (severity)
            {
                case ValidationSeverity.Error:
                    lamp = UIPilotStyles.LampFault;   word = UIPilotLabels.Status.Error;   break;
                case ValidationSeverity.Warning:
                    lamp = UIPilotStyles.LampCaution; word = UIPilotLabels.Status.Warning; break;
                default:
                    lamp = UIPilotStyles.LampOk;      word = UIPilotLabels.Status.Ok;      break;
            }
        }

        private void DrawValidationRow(ValidationResult result)
        {
            GetValidationStatus(result.Severity, out var lamp, out var word);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLamp(lamp);
                GUILayout.Label(result.Message, UIPilotStyles.RowLabel);

                if (result.AutoFix != null
                    && GUILayout.Button(ContentFix, UIPilotStyles.FixButtonSize))
                {
                    ValidationModule.RunAutoFix(result);
                    _validationResults = ValidationModule.Validate();
                }

                GUILayout.Label(word, UIPilotStyles.StatusWord, UIPilotStyles.StatusWordSize);
            }
        }

        private static void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, UIPilotStyles.Separator);
        }

        // ── Section: Manual ──────────────────────────────────────────────────

        private void DrawManualSection()
        {
            if (!DrawSectionFoldout(ref _manualFoldout, UIPilotLabels.QuickBuild.ManualSectionLabel,
                    UIPilotLabels.Window.EditorPrefsManualOpen))
                return;

            // Indented: these three are steps inside Manual, not sections of their own.
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(12f);

                using (new EditorGUILayout.VerticalScope())
                {
                    DrawGenerateSection();
                    DrawDiscoverSection();
                    DrawWireSection();
                }
            }
        }

        private static void DrawSubsectionHeader(string title, string description)
        {
            EditorGUILayout.Space(8f);
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.Label(description, UIPilotStyles.Description);
        }

        // ── Section: Generate ────────────────────────────────────────────────

        private void DrawGenerateSection()
        {
            DrawSubsectionHeader(UIPilotLabels.Sections.Generate, UIPilotLabels.Manual.GenerateDesc);

            using (new EditorGUILayout.VerticalScope(UIPilotStyles.Card))
            {
                _selectedMenuType = (MenuType)EditorGUILayout.EnumPopup(
                    UIPilotLabels.Generate.MenuTypeLabel, _selectedMenuType);

                EditorGUILayout.Space(4f);

                if (GUILayout.Button(ContentGenerate))
                    UIGeneratorModule.Generate(_selectedMenuType, _theme);

                EditorGUILayout.Space(8f);

                // One segmented row instead of three stacked full-width buttons.
                GUILayout.Label(UIPilotLabels.Generate.ClearRowLabel, UIPilotStyles.Description);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(ContentClearMain, EditorStyles.miniButtonLeft))
                        UIGeneratorModule.ClearPanel(MenuType.MainMenu);
                    if (GUILayout.Button(ContentClearPause, EditorStyles.miniButtonMid))
                        UIGeneratorModule.ClearPanel(MenuType.PauseMenu);
                    if (GUILayout.Button(ContentClearSet, EditorStyles.miniButtonRight))
                        UIGeneratorModule.ClearPanel(MenuType.SettingsMenu);
                }
            }
        }

        // ── Section: Discover ────────────────────────────────────────────────

        private void DrawDiscoverSection()
        {
            DrawSubsectionHeader(UIPilotLabels.Sections.Discover, UIPilotLabels.Manual.DiscoverDesc);

            using (new EditorGUILayout.VerticalScope(UIPilotStyles.Card))
            {
                if (GUILayout.Button(ContentScan))
                    _discoveredActions = ActionDiscoveryModule.Scan();

                if (_discoveredActions == null) return;

                EditorGUILayout.Space(4f);

                if (_discoveredActions.Count == 0)
                {
                    GUILayout.Label(UIPilotLabels.Discover.EmptyList, UIPilotStyles.Description);
                    return;
                }

                _discoverScrollPos = EditorGUILayout.BeginScrollView(
                    _discoverScrollPos, UIPilotStyles.ActionListSize);

                foreach (var action in _discoveredActions)
                    EditorGUILayout.LabelField(action.FullLabel);

                EditorGUILayout.EndScrollView();
            }
        }

        // ── Section: Wire ────────────────────────────────────────────────────

        private void DrawWireSection()
        {
            DrawSubsectionHeader(UIPilotLabels.Sections.Wire, UIPilotLabels.Manual.WireDesc);

            using (new EditorGUILayout.VerticalScope(UIPilotStyles.Card))
            {
                if (GUILayout.Button(ContentRefresh))
                    RefreshWireState();

                EditorGUILayout.Space(4f);

                if (DrawWireEmptyState()) return;

                GUILayout.Label(UIPilotLabels.Manual.WireGuidance, UIPilotStyles.Guidance);
                EditorGUILayout.Space(4f);

                DrawWireRows();

                EditorGUILayout.Space(4f);
                GUILayout.Label(UIPilotLabels.Wire.AssignHint, UIPilotStyles.Description);
                EditorGUILayout.Space(4f);

                if (GUILayout.Button(ContentApply))
                    BindingModule.ApplyBindings(_bindingSelections, _availableActions);

                GUILayout.Label(UIPilotLabels.Manual.ApplyBindingsNote, UIPilotStyles.Description);
            }
        }

        // Says what to do next when there is nothing to wire yet.
        // Returns true when it drew a message in place of the wiring rows.
        private bool DrawWireEmptyState()
        {
            if (!IsWireStateLoaded())
            {
                EditorGUILayout.HelpBox(UIPilotLabels.Wire.HelpRefreshNeeded, MessageType.Info);
                return true;
            }

            if (_wiredButtons.Count == 0)
            {
                GUILayout.Label(UIPilotLabels.Wire.NoButtons, UIPilotStyles.Description);
                return true;
            }

            if (_availableActions.Count == 0)
            {
                EditorGUILayout.HelpBox(UIPilotLabels.Wire.HelpNoActions, MessageType.Warning);
                return true;
            }

            return false;
        }

        private bool IsWireStateLoaded()
        {
            return _wiredButtons != null
                && _availableActions != null
                && _bindingSelections != null
                && _actionDropdownOptions != null;
        }

        // One row per button name — menus share names (Settings, Quit), and a
        // selection is applied to every button that carries the name.
        private void DrawWireRows()
        {
            _wireScrollPos = EditorGUILayout.BeginScrollView(
                _wireScrollPos, UIPilotStyles.WireListSize);

            _wireSeenNames.Clear();
            foreach (var btn in _wiredButtons)
            {
                if (btn == null || !_wireSeenNames.Add(btn.name)) continue;

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(btn.name, UIPilotStyles.WireNameWidth);

                    _bindingSelections.TryGetValue(btn.name, out var selected);
                    _bindingSelections[btn.name] = EditorGUILayout.Popup(
                        selected, _actionDropdownOptions);
                }
            }

            EditorGUILayout.EndScrollView();
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
            // Internal Unity API — skip silently if a future version moves it.
            var type   = assembly.GetType("UnityEditor.LogEntries");
            var method = type?.GetMethod("Clear");
            method?.Invoke(null, null);
        }
    }
}
