using UnityEditor;
using UnityEngine;

namespace UIPilot.Editor
{
    // Cached IMGUI styles, colours and layout options for UIPilotWindow.
    // EditorStyles only exists inside OnGUI, so styles are built on first use
    // and rebuilt if the editor skin changes — drawing itself allocates nothing.
    internal static class UIPilotStyles
    {
        // ── Colours ──────────────────────────────────────────────────────────
        // Status reads from the lamp, never from text colour, so rows stay
        // legible on both the dark and the light editor skin.
        internal static readonly Color Accent      = new Color32(0x0F, 0x8B, 0x8D, 0xFF);
        internal static readonly Color LampOk      = new Color32(0x3F, 0xB9, 0x50, 0xFF);
        internal static readonly Color LampCaution = new Color32(0xE3, 0xA0, 0x08, 0xFF);
        internal static readonly Color LampFault   = new Color32(0xE5, 0x48, 0x4D, 0xFF);
        internal static readonly Color LampIdle    = new Color32(0x7A, 0x7F, 0x87, 0xFF);
        internal static readonly Color Separator   = new Color(0.5f, 0.5f, 0.5f, 0.35f);

        // GUI.backgroundColor multiplies the skin's button texture, so the tint
        // that lands on the accent differs per skin.
        internal static Color PrimaryTint => EditorGUIUtility.isProSkin
            ? new Color(0.25f, 1.40f, 1.45f)
            : new Color(0.55f, 0.90f, 0.90f);

        // Section header strip: a faint lift off the window background with a
        // hairline on top — the same device the Inspector uses for components.
        internal static Color SectionStrip => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.055f)
            : new Color(0f, 0f, 0f, 0.060f);

        internal static Color SectionStripLine => EditorGUIUtility.isProSkin
            ? new Color(0f, 0f, 0f, 0.35f)
            : new Color(0f, 0f, 0f, 0.18f);

        // ── Layout options (immutable, safe to share) ────────────────────────
        internal static readonly GUILayoutOption[] ContentMaxWidth   = { GUILayout.MaxWidth(480f) };
        internal static readonly GUILayoutOption[] SectionHeaderSize = { GUILayout.Height(24f), GUILayout.ExpandWidth(true) };
        internal static readonly GUILayoutOption[] IconButtonSize = { GUILayout.Width(22f), GUILayout.Height(18f) };
        internal static readonly GUILayoutOption[] AccentBarSize  = { GUILayout.Width(3f),  GUILayout.Height(20f) };
        internal static readonly GUILayoutOption[] LampSlotSize   = { GUILayout.Width(14f), GUILayout.Height(18f) };
        internal static readonly GUILayoutOption[] FixButtonSize  = { GUILayout.Width(38f) };
        internal static readonly GUILayoutOption[] FitWidth       = { GUILayout.ExpandWidth(false) };
        internal static readonly GUILayoutOption[] StatusWordSize = { GUILayout.Width(52f) };
        internal static readonly GUILayoutOption[] WireNameWidth  = { GUILayout.Width(180f) };
        internal static readonly GUILayoutOption[] ActionListSize = { GUILayout.Height(120f) };
        internal static readonly GUILayoutOption[] WireListSize   = { GUILayout.Height(140f) };

        internal const float LampSize = 8f;

        // ── Styles ───────────────────────────────────────────────────────────
        internal static GUIStyle Title          { get; private set; }
        internal static GUIStyle SectionFoldout { get; private set; }
        internal static GUIStyle Helper        { get; private set; }
        internal static GUIStyle Card          { get; private set; }
        internal static GUIStyle PrimaryButton { get; private set; }
        internal static GUIStyle RowLabel      { get; private set; }
        internal static GUIStyle RowDetail     { get; private set; }
        internal static GUIStyle StatusWord    { get; private set; }
        internal static GUIStyle Description   { get; private set; }
        internal static GUIStyle Guidance      { get; private set; }

        private static bool _built;
        private static bool _builtForProSkin;

        internal static void EnsureBuilt()
        {
            var proSkin = EditorGUIUtility.isProSkin;
            if (_built && _builtForProSkin == proSkin) return;

            var muted   = proSkin ? new Color(0.70f, 0.72f, 0.75f) : new Color(0.32f, 0.34f, 0.37f);
            var caution = proSkin ? new Color(1.00f, 0.80f, 0.35f) : new Color(0.52f, 0.33f, 0.00f);

            Title = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 };

            SectionFoldout = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };

            Helper = new GUIStyle(EditorStyles.label) { wordWrap = true };
            Helper.normal.textColor = muted;

            Card = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(10, 10, 8, 10) };

            PrimaryButton = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 30f,
                fontSize    = 12,
                fontStyle   = FontStyle.Bold
            };

            RowLabel = new GUIStyle(EditorStyles.label) { wordWrap = true };

            RowDetail = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
            RowDetail.normal.textColor = muted;

            StatusWord = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperRight };
            StatusWord.normal.textColor = muted;
            StatusWord.padding.top      = 3; // sits on the row label's baseline

            Description = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
            Description.normal.textColor = muted;

            Guidance = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
            Guidance.normal.textColor = caution;

            _built           = true;
            _builtForProSkin = proSkin;
        }
    }
}
