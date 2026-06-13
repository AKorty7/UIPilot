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
        // ── Public entry point ───────────────────────────────────────────────

        internal static void Generate(MenuType menuType)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(UIGeneratorContent.Undo.Action);

            EnsureEventSystem();

            var canvasGO = BuildCanvas(menuType);
            Undo.RegisterCreatedObjectUndo(canvasGO, UIGeneratorContent.Undo.Action);

            Selection.activeGameObject = canvasGO;
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

        private static GameObject BuildCanvas(MenuType menuType)
        {
            var canvasGO = new GameObject(UIGeneratorContent.GameObjects.Canvas);
            canvasGO.layer = LayerMask.NameToLayer(UIGeneratorContent.Layers.UI);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            BuildMenu(canvasGO, menuType);

            return canvasGO;
        }

        // ── Menu ─────────────────────────────────────────────────────────────

        private static void BuildMenu(GameObject canvas, MenuType menuType)
        {
            var (title, buttons, prefix) = GetMenuConfig(menuType);

            var panelGO  = CreateUIObject(prefix + UIGeneratorContent.GameObjects.PanelSuffix, canvas);
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

            CreateTitle(panelGO, title);

            foreach (var label in buttons)
                CreateButton(panelGO, label);
        }

        // ── Title ────────────────────────────────────────────────────────────

        private static void CreateTitle(GameObject parent, string text)
        {
            var go  = CreateUIObject(UIGeneratorContent.GameObjects.TitleText, parent);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = 64f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

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
        }

        // ── Helpers ──────────────────────────────────────────────────────────

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
