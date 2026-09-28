// Renders the source images for UIPilot's Asset Store pages. NOT part of the package:
// tools/store/render.ps1 copies it into Assets/_UIPilotStore/Editor for one run and
// deletes it afterwards.
//
//   RenderGame     (batch) menus over the Soft Signal backdrops, 3:2 and social size
//   SetEditorScale (batch) sets Unity's UI scaling to 200% for the next launch
//   CaptureEditor  (GUI)   UIPilot window, Hierarchy and toolbar at 2x, then puts the
//                          UI scaling and the toolbar lamp back as they were
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UIPilot.Editor;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.ScriptSetup;
using UIPilot.Editor.Modules.UIGenerator;
using Object = UnityEngine.Object;

public static class UIPilotStoreRender
{
    private const string DemoGameName = "Soft Signal";   // a made-up game, so the menu shows "your" title
    private const string ScaleKey     = "CustomEditorUIScale";
    private const BindingFlags Priv   = BindingFlags.NonPublic | BindingFlags.Instance;

    private static string Out       => Environment.GetEnvironmentVariable("UIPILOT_STORE_OUT");
    private static string Backdrops => Environment.GetEnvironmentVariable("UIPILOT_STORE_BACKDROPS");
    private static readonly StringBuilder Log = new StringBuilder();

    private enum Panel { Main, Pause, Settings }

    private sealed class Shot
    {
        public string Name, Theme, Mood;
        public int Width = 3900, Height = 2600;
        public Panel Panel = Panel.Main;
        public int Focus = 0, Hover = -1;
        public bool Words = true;
    }

    // ── Game renders (batch) ────────────────────────────────────────────────

    public static void RenderGame()
    {
        var realName = PlayerSettings.productName;
        try
        {
            PlayerSettings.productName = DemoGameName;
            foreach (var shot in GameShots())
                Render(shot);
        }
        catch (Exception e) { Log.AppendLine("EXCEPTION: " + e); }
        finally { PlayerSettings.productName = realName; }

        Log.AppendLine("product name restored: " + PlayerSettings.productName);
        File.WriteAllText(Path.Combine(Out, "game.txt"), Log.ToString());
        EditorApplication.Exit(0);
    }

    private static IEnumerable<Shot> GameShots()
    {
        yield return new Shot { Name = "main_day",       Theme = "Soft Club",       Mood = "day",   Focus = 0, Hover = 1 };
        yield return new Shot { Name = "main_night",     Theme = "Soft Club Night", Mood = "night", Focus = 0, Hover = 1 };
        yield return new Shot { Name = "main_ink",       Theme = "Ink",             Mood = "ink",   Focus = 0, Hover = 1 };
        yield return new Shot { Name = "main_day_focus2", Theme = "Soft Club",      Mood = "day",   Focus = 1 };
        yield return new Shot { Name = "pause_day",      Theme = "Soft Club",       Mood = "day",   Panel = Panel.Pause, Focus = 0 };
        yield return new Shot { Name = "settings_day",   Theme = "Soft Club",       Mood = "day",   Panel = Panel.Settings, Focus = 2 };
        yield return new Shot { Name = "settings_night", Theme = "Soft Club Night", Mood = "night", Panel = Panel.Settings, Focus = 2 };
        yield return new Shot { Name = "main_day_wordless", Theme = "Soft Club",    Mood = "day",   Focus = 0, Words = false };
        yield return new Shot { Name = "social_day_wordless", Theme = "Soft Club",  Mood = "day",   Focus = 0, Words = false, Width = 2400, Height = 1260 };
    }

    private static void Render(Shot shot)
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var theme = AssetDatabase.LoadAssetAtPath<UIPilotTheme>("Assets/UIPilot/Themes/" + shot.Theme + ".asset");
        foreach (MenuType menu in Enum.GetValues(typeof(MenuType)))
            UIGeneratorModule.Generate(menu, theme);

        var cam = Camera.main;
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        var rt = new RenderTexture(shot.Width, shot.Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
        {
            antiAliasing = 8
        };
        cam.targetTexture = rt;

        AddBackdrop(cam, shot);

        var canvasGO = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);
        var canvas   = canvasGO.GetComponent<Canvas>();
        canvas.renderMode    = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera   = cam;
        canvas.planeDistance = 1f;

        var shown = PanelName(shot.Panel);
        foreach (var name in new[] { PanelName(Panel.Main), PanelName(Panel.Pause), PanelName(Panel.Settings) })
            canvasGO.transform.Find(name).gameObject.SetActive(name == shown);

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.enabled = false;
        scaler.enabled = true;
        Canvas.ForceUpdateCanvases();

        var panel = canvasGO.transform.Find(shown);
        if (shot.Panel == Panel.Settings) SetVolume(panel, 0.7f);
        if (!shot.Words)
            foreach (var text in panel.GetComponentsInChildren<TMP_Text>(true)) text.enabled = false;

        Canvas.ForceUpdateCanvases();
        SetButtonStates(panel, shot);

        cam.Render();
        Save(rt, shot.Name + ".png");
        cam.targetTexture = null;
        rt.Release();
        Log.AppendLine("rendered " + shot.Name + " " + shot.Width + "x" + shot.Height);
    }

    // The Soft Signal art, cover-fitted like CSS background-size: cover, far behind the UI.
    private static void AddBackdrop(Camera cam, Shot shot)
    {
        var go = new GameObject("StoreBackdrop", typeof(RectTransform));
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode    = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera   = cam;
        canvas.planeDistance = 50f;
        canvas.sortingOrder  = -100;

        var art = new GameObject("Art", typeof(RectTransform)).AddComponent<RawImage>();
        art.transform.SetParent(go.transform, false);
        var rect = (RectTransform)art.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        art.raycastTarget = false;

        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
        tex.LoadImage(File.ReadAllBytes(Path.Combine(Backdrops, "backdrop_" + shot.Mood + ".png")));
        tex.filterMode = FilterMode.Trilinear;
        art.texture = tex;

        // Keep the art's aspect: a wider frame keeps the floor and horizon, trims the sky.
        var artAspect  = (float)tex.width / tex.height;
        var shotAspect = (float)shot.Width / shot.Height;
        art.uvRect = shotAspect > artAspect
            ? new Rect(0f, 0f, 1f, artAspect / shotAspect)
            : new Rect((1f - shotAspect / artAspect) * 0.5f, 0f, shotAspect / artAspect, 1f);
    }

    private static string PanelName(Panel panel)
    {
        switch (panel)
        {
            case Panel.Pause:    return "UIPilot_PauseMenu_Panel";
            case Panel.Settings: return "UIPilot_Settings_Panel";
            default:             return "UIPilot_MainMenu_Panel";
        }
    }

    // What the running game shows after the player turns the volume down to 70%.
    private static void SetVolume(Transform panel, float volume)
    {
        foreach (var t in panel.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == UIGeneratorContent.GameObjects.VolumeValue)
                t.GetComponent<TMP_Text>().text = Mathf.RoundToInt(volume * 100f) + "%";
            if (t.name == UIGeneratorContent.GameObjects.VolumeFill)
            {
                var fill = (RectTransform)t;
                fill.anchorMax = new Vector2(volume, fill.anchorMax.y);
            }
        }
    }

    // Edit mode never runs the Button's transitions, so set what Play mode would show.
    private static void SetButtonStates(Transform panel, Shot shot)
    {
        var buttons = panel.GetComponentsInChildren<Button>();
        for (var i = 0; i < buttons.Length; i++)
        {
            var colors = buttons[i].colors;
            var c = i == shot.Focus ? colors.selectedColor
                  : i == shot.Hover ? colors.highlightedColor
                  : colors.normalColor;
            buttons[i].targetGraphic.canvasRenderer.SetColor(c * colors.colorMultiplier);
        }
    }

    private static void Save(RenderTexture rt, string file)
    {
        var resolved = RenderTexture.GetTemporary(rt.width, rt.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Graphics.Blit(rt, resolved);   // resolves MSAA
        RenderTexture.active = resolved;
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(resolved);
        File.WriteAllBytes(Path.Combine(Out, file), tex.EncodeToPNG());
    }

    // ── Editor captures (GUI, at 2x) ────────────────────────────────────────

    public static void SetEditorScale()
    {
        var had = EditorPrefs.HasKey(ScaleKey);
        EditorPrefs.SetInt(ScaleKey, 200);
        File.WriteAllText(Path.Combine(Out, "scale.txt"), "had key before: " + had + ", set to 200\n");
        EditorApplication.Exit(0);
    }

    private static EditorWindow _win, _hierarchy;
    private static int _frame;
    private static bool? _lampWas;
    private static Canvas _uipilotCanvas;

    public static void CaptureEditor()
    {
        Note("pixelsPerPoint: " + EditorGUIUtility.pixelsPerPoint);
        BuildIssueScene();

        _win = ScriptableObject.CreateInstance<UIPilotWindow>();
        _win.titleContent = new GUIContent("UIPilot");
        _win.ShowUtility();
        _win.position = new Rect(80, 40, 420, 560);

        var hierarchyType = Type.GetType("UnityEditor.SceneHierarchyWindow, UnityEditor.CoreModule");
        _hierarchy = (EditorWindow)ScriptableObject.CreateInstance(hierarchyType);
        _hierarchy.ShowUtility();
        _hierarchy.position = new Rect(520, 40, 340, 360);

        _lampWas = UIPilotToolbarLamp.IsShown();
        UIPilotToolbarLamp.SetShown(true);
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        _frame++;
        _win.Repaint();
        _hierarchy.Repaint();

        switch (_frame)
        {
            case 40:
                SetWin("_showHelperText", true);
                SetWin("_healthFoldout", true);
                SetWin("_healthChecksFoldout", false);
                SetWin("_buildFoldout", false);
                SetWin("_scanRepairFoldout", false);
                SetWin("_manualFoldout", false);
                ExpandAll();
                UIPilotHealthMonitor.CheckNow();
                break;
            case 120:
                GrabWindow(_win, "editor_health.png");
                GrabWindow(_hierarchy, "editor_hierarchy_lamps.png");
                GrabToolbar("editor_toolbar.png");
                SetWin("_healthChecksFoldout", true);
                _win.position = new Rect(80, 40, 420, 900);
                break;
            case 200:
                GrabWindow(_win, "editor_health_checks.png");
                BuildMenuScene();
                SetWin("_healthChecksFoldout", false);
                SetWin("_buildFoldout", true);
                SetWin("_theme", AssetDatabase.LoadAssetAtPath<UIPilotTheme>("Assets/UIPilot/Themes/Soft Club.asset"));
                SessionState.SetBool("UIPilot_QuickBuildDone", true);
                _win.position = new Rect(80, 40, 420, 760);
                _hierarchy.position = new Rect(520, 40, 380, 620);
                ExpandMenus();
                UIPilotHealthMonitor.CheckNow();
                break;
            case 300:
                GrabWindow(_win, "editor_build.png");
                GrabWindow(_hierarchy, "editor_hierarchy_menus.png");
                Finish();
                break;
        }
    }

    private static void Finish()
    {
        EditorApplication.update -= Tick;
        SessionState.EraseBool("UIPilot_QuickBuildDone");
        if (_lampWas.HasValue) UIPilotToolbarLamp.SetShown(_lampWas.Value);
        EditorPrefs.DeleteKey(ScaleKey);
        Note("toolbar lamp restored to " + _lampWas + "; UI scaling key deleted: " + !EditorPrefs.HasKey(ScaleKey));
        _win.Close();
        _hierarchy.Close();
        EditorApplication.Exit(0);
    }

    // The same five problems the guide shows, one of each kind.
    private static void BuildIssueScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var canvas = new GameObject("HUD_Canvas", typeof(RectTransform)).AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvas.gameObject.AddComponent<GraphicRaycaster>();
        UIPilotEventSystem.Create("store");
        var logic = new GameObject("GameLogic").AddComponent(Type.GetType("UIPilot_GameManager, Assembly-CSharp"));

        var menu = Box("PauseMenu", canvas.transform, new Vector2(600, 700), Vector2.zero);
        var resume = DemoButton("Btn_Resume", menu, 150);
        UnityEventTools.AddPersistentListener(resume.onClick, (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), logic, "OnResumePressed"));
        var so = new SerializedObject(resume);
        so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls.Array.data[0].m_MethodName").stringValue = "OnContinuePressed";
        so.ApplyModifiedPropertiesWithoutUndo();
        DemoButton("Btn_Options", menu, 50);
        var quit = DemoButton("Btn_Quit", menu, -50);
        quit.navigation = new Navigation { mode = Navigation.Mode.None };

        Box("Vignette", canvas.transform, new Vector2(400, 120), new Vector2(0, 50)).GetComponent<Image>().color = new Color(0, 0, 0, 0.15f);
        var footnote = new GameObject("Footnote", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
        footnote.transform.SetParent(menu, false);
        footnote.text = "Footnote"; footnote.fontSize = 10; footnote.raycastTarget = false;
        ((RectTransform)footnote.transform).anchoredPosition = new Vector2(0, -300);

        var inventory = new GameObject("Inventory", typeof(RectTransform));
        inventory.transform.SetParent(canvas.transform, false);
        ((RectTransform)inventory.transform).anchoredPosition = new Vector2(650, 0);
        inventory.AddComponent<Canvas>();
        DemoButton("Btn_Equip", inventory.transform, 0);
        Canvas.ForceUpdateCanvases();
        _uipilotCanvas = canvas;
    }

    private static void BuildMenuScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var theme = AssetDatabase.LoadAssetAtPath<UIPilotTheme>("Assets/UIPilot/Themes/Soft Club.asset");
        foreach (MenuType menu in Enum.GetValues(typeof(MenuType)))
            UIGeneratorModule.Generate(menu, theme);
        new GameObject(ScriptSetupContent.GameObjects.ManagerName);
        typeof(UIPilotWindow).GetMethod("TryQuickBuildWire", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { false });
        _uipilotCanvas = GameObject.Find(UIGeneratorContent.GameObjects.Canvas).GetComponent<Canvas>();
        Selection.activeObject = null;
    }

    private static Transform Box(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>();
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return go.transform;
    }

    private static Button DemoButton(string name, Transform parent, float y)
    {
        var go = DefaultControls.CreateButton(new DefaultControls.Resources());
        go.name = name;
        go.transform.SetParent(parent, false);
        ((RectTransform)go.transform).anchoredPosition = new Vector2(0, y);
        return go.GetComponent<Button>();
    }

    private static void SetWin(string field, object value)
    {
        typeof(UIPilotWindow).GetField(field, Priv).SetValue(_win, value);
    }

    private static void ExpandAll()
    {
        var expand = _hierarchy.GetType().GetMethod("SetExpandedRecursive", Priv | BindingFlags.Public);
        foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            expand?.Invoke(_hierarchy, new object[] { root.GetInstanceID(), true });
    }

    // Only down to the panels and the main menu's own children: the shape, not every decoration.
    private static void ExpandMenus()
    {
        var expand = _hierarchy.GetType().GetMethod("ExpandTreeViewItem", Priv | BindingFlags.Public)
                  ?? _hierarchy.GetType().GetMethod("SetExpanded", Priv | BindingFlags.Public);
        Note("expand method: " + (expand != null ? expand.Name : "none"));
        var main = _uipilotCanvas.transform.Find("UIPilot_MainMenu_Panel");
        foreach (var t in new[] { _uipilotCanvas.transform, main })
            expand?.Invoke(_hierarchy, new object[] { t.gameObject.GetInstanceID(), true });
    }

    private static void GrabToolbar(string file)
    {
        var guiView = Type.GetType("UnityEditor.GUIView, UnityEditor.CoreModule");
        foreach (var view in Resources.FindObjectsOfTypeAll(guiView))
            if (view.GetType().Name == "Toolbar")
                GrabView(view, file);
    }

    private static void GrabWindow(EditorWindow window, string file)
    {
        GrabView((Object)typeof(EditorWindow).GetField("m_Parent", Priv).GetValue(window), file);
    }

    private static void GrabView(Object view, string file)
    {
        try
        {
            var rect = (Rect)view.GetType().GetProperty("screenPosition", Priv | BindingFlags.Public).GetValue(view);
            var ppp = EditorGUIUtility.pixelsPerPoint;
            var w = Mathf.RoundToInt(rect.width * ppp);
            var h = Mathf.RoundToInt(rect.height * ppp);

            var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            view.GetType().GetMethod("GrabPixels", Priv | BindingFlags.Public).Invoke(view, new object[] { rt, new Rect(0, 0, w, h) });

            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            var px = tex.GetPixels32();
            var flipped = new Color32[px.Length];
            for (var y = 0; y < h; y++) Array.Copy(px, y * w, flipped, (h - 1 - y) * w, w);
            tex.SetPixels32(flipped);
            tex.Apply();

            File.WriteAllBytes(Path.Combine(Out, file), tex.EncodeToPNG());
            Note("OK " + file + " " + w + "x" + h);
        }
        catch (Exception e)
        {
            Note("EXCEPTION grabbing " + file + ": " + e.GetBaseException().Message);
        }
    }

    private static void Note(string line)
    {
        File.AppendAllText(Path.Combine(Out, "editor.txt"), line + "\n");
    }
}
