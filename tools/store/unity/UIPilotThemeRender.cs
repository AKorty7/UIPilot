// Renders every theme's menus over its backdrop. NOT part of the package: copy it into
// UIPilot/Assets/_UIPilotStore/Editor for one batch-mode run, then delete that folder.
//
//   Unity.exe -batchmode -projectPath <UIPilot> -executeMethod UIPilotThemeRender.Run -logFile <log>
//
// Environment: UIPILOT_STORE_OUT (where the PNGs go), UIPILOT_STORE_BACKDROPS (the folder
// of backdrop_<mood>.png from backdrop.py and genre_backdrops.py), and optionally
// UIPILOT_STORE_ONLY (comma-separated keys, e.g. "fantasy,pixel").
// Each theme gets: <key>_main_1080, <key>_settings_1080, <key>_pause_deck (1280 x 800),
// and the genre themes <key>_main_hero (2400 x 1600).
using System;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UIPilot.Editor.Core;
using UIPilot.Editor.Modules.UIGenerator;

public static class UIPilotThemeRender
{
    private const string ThemeFolder = "Assets/UIPilot/Themes/";
    private static string Out       => Environment.GetEnvironmentVariable("UIPILOT_STORE_OUT");
    private static string Backdrops => Environment.GetEnvironmentVariable("UIPILOT_STORE_BACKDROPS");
    private static readonly StringBuilder Log = new StringBuilder();

    private enum Panel { Main, Pause, Settings }

    private sealed class Shot
    {
        public string Name, Theme, Mood, Game;
        public int Width = 1920, Height = 1080;
        public Panel Panel = Panel.Main;
        public int Focus = 0, Hover = -1;
    }

    // Key, theme asset, backdrop mood, and a made-up game name for the title.
    private static readonly (string Key, string Theme, string Mood, string Game, bool Genre)[] Looks =
    {
        ("softclub", "Soft Club",       "day",       "Soft Signal",   false),
        ("night",    "Soft Club Night", "night",     "Soft Signal",   false),
        ("ink",      "Ink",             "ink",       "Soft Signal",   false),
        ("fantasy",  "Fantasy RPG",     "ember",     "Emberwake",     true),
        ("jrpg",     "JRPG Window",     "overworld", "Skyward Tales", true),
        ("pixel",    "Pixel Retro",     "pixel",     "Pixel Quest",   true),
        ("scifi",    "Sci-Fi HUD",      "space",     "Deep Orbit",    true),
        ("shooter",  "Military Shooter", "none",     "Blackline",     true),   // its scene is in the theme
    };

    public static void Run()
    {
        var only = Environment.GetEnvironmentVariable("UIPILOT_STORE_ONLY");
        var realName = PlayerSettings.productName;
        try
        {
            foreach (var look in Looks)
            {
                if (!string.IsNullOrEmpty(only) && !only.Split(',').Contains(look.Key)) continue;

                Shoot(new Shot { Name = look.Key + "_main_1080",     Theme = look.Theme, Mood = look.Mood, Game = look.Game, Hover = 1 });
                Shoot(new Shot { Name = look.Key + "_settings_1080", Theme = look.Theme, Mood = look.Mood, Game = look.Game, Panel = Panel.Settings, Focus = 2 });
                Shoot(new Shot { Name = look.Key + "_pause_deck",    Theme = look.Theme, Mood = look.Mood, Game = look.Game, Panel = Panel.Pause, Width = 1280, Height = 800 });
                if (look.Genre)
                    Shoot(new Shot { Name = look.Key + "_main_hero", Theme = look.Theme, Mood = look.Mood, Game = look.Game, Width = 2400, Height = 1600, Focus = 1 });
            }
        }
        catch (Exception e) { Log.AppendLine("EXCEPTION: " + e); }
        finally { PlayerSettings.productName = realName; }

        Log.AppendLine("product name restored: " + PlayerSettings.productName);
        File.WriteAllText(Path.Combine(Out, "themes.txt"), Log.ToString());
        EditorApplication.Exit(0);
    }

    private static void Shoot(Shot shot)
    {
        PlayerSettings.productName = shot.Game;
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Loaded after NewScene: a new scene unloads assets nothing refers to, and a
        // theme that has become null silently builds as Soft Club.
        var theme = AssetDatabase.LoadAssetAtPath<UIPilotTheme>(ThemeFolder + shot.Theme + ".asset");
        if (theme == null) throw new Exception("theme not found: " + shot.Theme);
        foreach (MenuType menu in Enum.GetValues(typeof(MenuType)))
            UIGeneratorModule.Generate(menu, theme);

        var cam = Camera.main;
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        var rt = new RenderTexture(shot.Width, shot.Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
        { antiAliasing = 8 };
        cam.targetTexture = rt;
        AddBackdrop(cam, shot);

        var canvasGO = GameObject.Find(UIGeneratorContent.GameObjects.Canvas);
        var canvas   = canvasGO.GetComponent<Canvas>();
        canvas.renderMode    = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera   = cam;
        canvas.planeDistance = 1f;

        var shown = PanelName(shot.Panel);
        foreach (Transform child in canvasGO.transform)
            child.gameObject.SetActive(child.name == shown);

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.enabled = false;
        scaler.enabled = true;
        Canvas.ForceUpdateCanvases();

        var panel = canvasGO.transform.Find(shown);
        if (shot.Panel == Panel.Settings) SetVolume(panel, 0.7f);
        Canvas.ForceUpdateCanvases();
        SetButtonStates(panel, shot);

        cam.Render();
        Save(rt, shot.Name + ".png");
        cam.targetTexture = null;
        rt.Release();
        Log.AppendLine("rendered " + shot.Name + " " + shot.Width + "x" + shot.Height);
    }

    // The backdrop, cover-fitted like CSS background-size: cover, far behind the UI.
    private static void AddBackdrop(Camera cam, Shot shot)
    {
        var file = Path.Combine(Backdrops, "backdrop_" + shot.Mood + ".png");
        if (!File.Exists(file)) { Log.AppendLine("  no backdrop " + file); return; }

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
        tex.LoadImage(File.ReadAllBytes(file));
        tex.filterMode = shot.Mood == "pixel" ? FilterMode.Point : FilterMode.Trilinear;
        art.texture = tex;

        // Keep the art's aspect: a wider frame keeps the ground and horizon, trims the sky.
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
}
