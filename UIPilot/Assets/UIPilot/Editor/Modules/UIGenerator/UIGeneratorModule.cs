using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIPilot.Editor.Modules.UIGenerator
{
    internal static class UIGeneratorModule
    {
        // Buttons use Unity's default white Image background, while menu
        // panels use a dark overlay. Keep the two text treatments separate so
        // regenerating the UI preserves contrast in both places.
        private static readonly Color TitleTextColor  = Color.white;
        private static readonly Color ButtonTextColor = new(26f / 255f, 26f / 255f, 26f / 255f);

        // ── Public entry points ──────────────────────────────────────────────

        internal static void Generate(MenuType menuType)
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
            // FindIncludingInactive because GameManager.ShowPanel() can leave a panel
            // inactive; GameObject.Find alone would miss it and leave a duplicate.
            var brokenPanel = FindIncludingInactive(panelName);
            if (brokenPanel != null)
                Undo.DestroyObjectImmediate(brokenPanel);

            var existingCanvas = FindIncludingInactive(UIGeneratorContent.GameObjects.Canvas);
            var canvasIsNew    = existingCanvas == null;
            var canvasGO       = canvasIsNew ? CreateCanvas() : existingCanvas;
            canvasGO.transform.localScale = Vector3.one;

            if (canvasIsNew)
                Undo.RegisterCreatedObjectUndo(canvasGO, UIGeneratorContent.Undo.Action);

            var panelGO = BuildMenu(canvasGO, menuType);
            panelGO.transform.SetSiblingIndex(GetExpectedSiblingIndex(menuType));

            if (!canvasIsNew)
                Undo.RegisterCreatedObjectUndo(panelGO, UIGeneratorContent.Undo.Action);

            Selection.activeGameObject = canvasGO;
        }

        internal static void ClearPanel(MenuType menuType)
        {
            // Search scene-wide: panel may live inside an old canvas, not just UIPilot_Canvas.
            var panelGO = FindIncludingInactive(GetPanelName(menuType));
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

            var esGO = new GameObject(UIGeneratorContent.GameObjects.EventSystem);
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(esGO, UIGeneratorContent.Undo.Action);
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

        // ── Menu ─────────────────────────────────────────────────────────────

        private static GameObject BuildMenu(GameObject canvas, MenuType menuType)
        {
            var (title, buttons, prefix) = GetMenuConfig(menuType);

            var panelGO   = CreateUIObject(prefix + UIGeneratorContent.GameObjects.PanelSuffix, canvas);
            var panelRect = (RectTransform)panelGO.transform;
            SetStretch(panelRect);

            var bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f);

            var vlg = panelGO.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment         = TextAnchor.MiddleCenter;
            vlg.spacing                = 20f;
            vlg.padding                = new RectOffset(0, 0, 80, 80);
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = true;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            CreateTitle(panelGO, prefix + UIGeneratorContent.GameObjects.TitleSuffix, title);

            foreach (var label in buttons)
                CreateButton(panelGO, label);

            return panelGO;
        }

        // ── Title ────────────────────────────────────────────────────────────

        private static void CreateTitle(GameObject parent, string objectName, string displayText)
        {
            var go  = CreateUIObject(objectName, parent);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = displayText;
            tmp.fontSize  = 64f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color     = TitleTextColor;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 100f;
        }

        // ── Button ───────────────────────────────────────────────────────────

        private static void CreateButton(GameObject parent, string label)
        {
            var btnGO = CreateUIObject(UIGeneratorContent.GameObjects.ButtonPrefix + label, parent);
            btnGO.AddComponent<Image>();
            btnGO.AddComponent<Button>();

            var le = btnGO.AddComponent<LayoutElement>();
            le.preferredHeight = 70f;

            var textGO = CreateUIObject(UIGeneratorContent.GameObjects.ButtonTextChild, btnGO);
            SetStretch((RectTransform)textGO.transform);

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 28f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = ButtonTextColor;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static bool IsPanelIntact(string panelName, string[] expectedButtons)
        {
            var panel = FindIncludingInactive(panelName);
            if (panel == null) return false;

            foreach (var label in expectedButtons)
            {
                var expectedName = UIGeneratorContent.GameObjects.ButtonPrefix + label;
                var found        = false;

                foreach (Transform child in panel.transform)
                {
                    if (child.name == expectedName) { found = true; break; }
                }

                if (!found) return false;
            }

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

        // GameObject.Find only searches active objects. GameManager.ShowPanel()
        // can leave a panel inactive, so lifecycle lookups (intact-check, destroy-
        // before-regenerate, clear) need to also catch inactive instances —
        // otherwise an inactive panel goes unfound and a duplicate gets built
        // alongside it.
        private static GameObject FindIncludingInactive(string name)
        {
            var active = GameObject.Find(name);
            if (active != null) return active;

            foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
                if (candidate.name == name && candidate.scene.IsValid())
                    return candidate;

            return null;
        }

        private static string GetPanelName(MenuType menuType)
        {
            var (_, _, prefix) = GetMenuConfig(menuType);
            return prefix + UIGeneratorContent.GameObjects.PanelSuffix;
        }

        private static GameObject CreateUIObject(string name, GameObject parent)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();

            if (parent != null)
            {
                go.transform.SetParent(parent.transform, false);
                go.layer = parent.layer;
            }

            return go;
        }

        private static void SetStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
                MenuType.MainMenu     => (UIGeneratorContent.Menus.MainMenu,
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
