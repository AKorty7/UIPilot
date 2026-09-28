using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.ScriptSetup;

namespace UIPilot.Editor.Modules.UIGenerator
{
    internal static class UIGeneratorModule
    {
        // ── Public entry points ──────────────────────────────────────────────

        // theme may be null: the built-in Soft Club look is used.
        internal static void Generate(MenuType menuType, UIPilotTheme theme)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(UIGeneratorContent.Undo.Action);

            EnsureEventSystem();

            // Remove any pre-rename Canvas objects that contain UIPilot panels.
            CleanUpLegacyCanvases();

            var panelName = GetPanelName(menuType);

            var (_, buttons, _) = GetMenuConfig(menuType);
            if (IsPanelIntact(panelName, buttons))
            {
                Debug.Log(UIGeneratorContent.Messages.PanelIntact + panelName);
                return;
            }

            // Panel exists but is broken (no buttons) — destroy it before regenerating.
            var brokenPanel = FindPanel(panelName);
            if (brokenPanel != null)
                Undo.DestroyObjectImmediate(brokenPanel);

            var existingCanvas = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);
            var canvasIsNew    = existingCanvas == null;
            var canvasGO       = canvasIsNew ? CreateCanvas() : existingCanvas;
            canvasGO.transform.localScale = Vector3.one;

            if (canvasIsNew)
                Undo.RegisterCreatedObjectUndo(canvasGO, UIGeneratorContent.Undo.Action);

            var (title, menuButtons, prefix) = GetMenuConfig(menuType);
            var panelGO = UIGeneratorMenuBuilder.Build(
                canvasGO, menuType, title, menuButtons, prefix, ResolveTheme(theme));
            panelGO.transform.SetSiblingIndex(GetExpectedSiblingIndex(menuType));

            if (!canvasIsNew)
                Undo.RegisterCreatedObjectUndo(panelGO, UIGeneratorContent.Undo.Action);

            Selection.activeGameObject = canvasGO;
        }

        // Restyles every generated menu already in the scene. Appearance only:
        // layout, names and button wiring are untouched, so it is safe on menus the
        // developer has rearranged. Returns how many menus were restyled.
        internal static int ApplyTheme(UIPilotTheme theme)
        {
            var resolved = ResolveTheme(theme);
            var restyled = 0;

            foreach (MenuType menuType in Enum.GetValues(typeof(MenuType)))
            {
                var panelGO = FindPanel(GetPanelName(menuType));
                if (panelGO == null) continue;

                Undo.RegisterFullObjectHierarchyUndo(panelGO, UIPilotLabels.Theme.UndoApply);
                UIGeneratorStyler.Apply(panelGO, resolved);
                restyled++;
            }

            if (restyled > 0) SetSceneHour(resolved);
            return restyled;
        }

        // The game starts its scene at the theme's hour: the generated GameManager's
        // Time Of Day field takes the theme's value. The component is the developer's
        // own type, so the field is reached by its serialized name (its name and the
        // object's are ScriptSetup's constants, read here, not a call into it). Build
        // UI calls this once the GameManager exists; Apply Theme calls it too.
        internal static void SetSceneHour(UIPilotTheme theme)
        {
            var resolved = ResolveTheme(theme);
            var manager  = GameObject.Find(ScriptSetupContent.GameObjects.ManagerName);
            if (manager == null) return;

            foreach (var component in manager.GetComponents<MonoBehaviour>())
            {
                if (component == null) continue;   // a missing-script slot

                var serialized = new SerializedObject(component);
                var hour       = serialized.FindProperty(ScriptSetupContent.GameObjects.TimeOfDayField);
                if (hour == null || hour.propertyType != SerializedPropertyType.Float) continue;

                if (!Mathf.Approximately(hour.floatValue, resolved.timeOfDay))
                {
                    hour.floatValue = resolved.timeOfDay;
                    serialized.ApplyModifiedProperties();   // undoable with the apply
                }
                return;
            }
        }

        // The preset the window selects when the developer has not chosen a theme.
        internal static UIPilotTheme FindDefaultThemeAsset()
        {
            foreach (var guid in AssetDatabase.FindAssets(
                         UIPilotLabels.Theme.DefaultAsset + UIPilotLabels.Theme.AssetFilter))
            {
                var theme = AssetDatabase.LoadAssetAtPath<UIPilotTheme>(AssetDatabase.GUIDToAssetPath(guid));
                if (theme != null && theme.name == UIPilotLabels.Theme.DefaultAsset) return theme;
            }

            return null;
        }

        // No theme selected, or its asset was deleted: fall back to the built-in look.
        private static UIPilotTheme ResolveTheme(UIPilotTheme theme)
        {
            return theme != null ? theme : UIGeneratorThemePresets.SoftClub();
        }

        // ── Theme assets ─────────────────────────────────────────────────────

        // Assets > Create > UIPilot > Theme. A new theme starts as a full copy of
        // Soft Club, font and title glow included, then enters rename like any asset.
        [MenuItem(UIPilotLabels.Theme.CreateMenuPath)]
        private static void CreateThemeAsset()
        {
            ProjectWindowUtil.CreateAsset(UIGeneratorThemePresets.SoftClub(), UIPilotLabels.Theme.NewAssetFile);
        }

        // The shipped presets that no longer exist anywhere in the project, by name.
        // A preset the developer moved or edited still counts as present.
        internal static List<string> FindMissingDefaultThemes()
        {
            var present = new HashSet<string>();
            foreach (var guid in AssetDatabase.FindAssets(UIPilotLabels.Theme.AssetFilter))
                present.Add(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)));

            var missing = new List<string>();
            foreach (var preset in UIGeneratorThemePresets.All)
                if (!present.Contains(preset.Name))
                    missing.Add(preset.Name);

            return missing;
        }

        // Recreates the missing presets from their factories and returns the new
        // asset paths. Never overwrites: a theme that exists is left alone, and a
        // stray file at the target path gets a numbered name instead.
        internal static List<string> RestoreDefaultThemes()
        {
            var missing  = FindMissingDefaultThemes();
            var restored = new List<string>();

            foreach (var preset in UIGeneratorThemePresets.All)
            {
                if (!missing.Contains(preset.Name)) continue;

                EnsureFolder(UIPilotLabels.Theme.PresetFolder);
                var path = AssetDatabase.GenerateUniqueAssetPath(
                    UIPilotLabels.Theme.PresetFolder + "/" + preset.Name + UIPilotLabels.Theme.AssetExtension);

                AssetDatabase.CreateAsset(preset.Create(), path);
                restored.Add(path);
            }

            if (restored.Count > 0)
                AssetDatabase.SaveAssets();

            return restored;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;

            var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        internal static void ClearPanel(MenuType menuType)
        {
            // Search scene-wide: panel may live inside an old canvas, not just UIPilot_Canvas.
            var panelGO = FindPanel(GetPanelName(menuType));
            if (panelGO == null) return;

            var parentCanvas = panelGO.transform.parent != null
                ? panelGO.transform.parent.gameObject
                : null;

            Undo.DestroyObjectImmediate(panelGO);

            // If the parent canvas is now empty of UIPilot children, destroy it too.
            if (parentCanvas != null && !HasUIPilotChildren(parentCanvas))
                Undo.DestroyObjectImmediate(parentCanvas);
        }

        // ── EventSystem ──────────────────────────────────────────────────────

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;

            UIPilotEventSystem.Create(UIGeneratorContent.Undo.Action);
        }

        // ── Canvas ───────────────────────────────────────────────────────────

        private static GameObject CreateCanvas()
        {
            var canvasGO = new GameObject(UIGeneratorContent.GameObjects.Canvas);
            canvasGO.layer = LayerMask.NameToLayer(UIGeneratorContent.Layers.UI);
            canvasGO.transform.localScale = Vector3.one;

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            return canvasGO;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        // Finds a panel even when the developer has unticked it in the Inspector.
        private static GameObject FindPanel(string panelName)
        {
            return UIPilotSceneQuery.FindInCanvas(UIGeneratorContent.GameObjects.Canvas, panelName);
        }

        private static bool IsPanelIntact(string panelName, string[] expectedButtons)
        {
            var panel = FindPanel(panelName);
            if (panel == null) return false;

            // Anywhere under the panel: Settings buttons sit inside their rows.
            var present = new System.Collections.Generic.HashSet<string>();
            foreach (var button in panel.GetComponentsInChildren<Button>(true))
                present.Add(button.name);

            foreach (var label in expectedButtons)
                if (!present.Contains(UIGeneratorContent.GameObjects.ButtonPrefix + label))
                    return false;

            return true;
        }

        private static void CleanUpLegacyCanvases()
        {
            var allCanvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in allCanvases)
            {
                if (canvas.name == UIGeneratorContent.GameObjects.Canvas) continue;
                if (HasUIPilotChildren(canvas.gameObject))
                    Undo.DestroyObjectImmediate(canvas.gameObject);
            }
        }

        private static bool HasUIPilotChildren(GameObject go)
        {
            foreach (Transform child in go.transform)
                if (child.name.StartsWith(UIGeneratorContent.GameObjects.NamePrefix,
                        System.StringComparison.Ordinal))
                    return true;
            return false;
        }

        private static string GetPanelName(MenuType menuType)
        {
            var (_, _, prefix) = GetMenuConfig(menuType);
            return prefix + UIGeneratorContent.GameObjects.PanelSuffix;
        }

        private static int GetExpectedSiblingIndex(MenuType menuType)
        {
            return menuType switch
            {
                MenuType.MainMenu     => 0,
                MenuType.PauseMenu    => 1,
                MenuType.SettingsMenu => 2,
                _                     => throw new ArgumentOutOfRangeException(nameof(menuType), menuType, null)
            };
        }

        private static (string title, string[] buttons, string prefix) GetMenuConfig(MenuType menuType)
        {
            return menuType switch
            {
                // The project's real name, from Player Settings > Product Name.
                MenuType.MainMenu     => (string.IsNullOrWhiteSpace(PlayerSettings.productName)
                                              ? UIGeneratorContent.Menus.MainMenuFallback
                                              : PlayerSettings.productName,
                                          UIGeneratorContent.ButtonSets.MainMenu,
                                          UIGeneratorContent.GameObjects.MainMenuPrefix),

                MenuType.PauseMenu    => (UIGeneratorContent.Menus.PauseMenu,
                                          UIGeneratorContent.ButtonSets.PauseMenu,
                                          UIGeneratorContent.GameObjects.PauseMenuPrefix),

                MenuType.SettingsMenu => (UIGeneratorContent.Menus.SettingsMenu,
                                          UIGeneratorContent.ButtonSets.SettingsMenu,
                                          UIGeneratorContent.GameObjects.SettingsMenuPrefix),

                _                     => throw new ArgumentOutOfRangeException(nameof(menuType), menuType, null)
            };
        }
    }
}
