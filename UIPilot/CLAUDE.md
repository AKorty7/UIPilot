# UIPilot brief

UIPilot is a Unity Editor tool (C#, Unity 6000.3.11f1, UGUI + TextMeshPro) that
builds themed game menus, keeps every UI in the scene healthy, and explains
clicks in Play mode. Publisher: NomadStudios. It is being resubmitted to the
Unity Asset Store after a rejection for unclear in-package docs.

The binding architecture and scope rules are in `.cursorrules`, imported here so
they always load. They win over anything below.

@.cursorrules

## Where things are

| Path | What |
|---|---|
| `Assets/UIPilot/Editor/` | All tool code. Layout is in `.cursorrules`. |
| `Assets/UIPilot/Art/`, `Art/Themes/`, `Art/Fonts/` | Shipped sprites, genre artwork, OFL fonts. Every font is listed in `Third-Party Notices.txt`. |
| `Assets/UIPilot/Themes/` | The nine preset theme assets. Regenerated from `UIGeneratorThemePresets` in place, so their GUIDs survive. |
| `Assets/UIPilot/Documentation/` | The user guide: write `UIPilot_Documentation.md`, build the PDF with `../tools/docs`. |
| `design-system/uipilot/` | The look of the window and the generated menus, and the theme rules (`pages/generated-menus.md`). Read before any visual change. |
| `../tools/art/` | Vector sources and build scripts for every sprite (`build-sprites.ps1`, `build-theme-sprites.ps1`) and for the scenes (`scene_layers.py`: layer PNGs, .metas and materials). |
| `../tools/store/` | Backdrops (`backdrop.py`, `genre_backdrops.py`), the render harness (`unity/`), store images. |
| `../tools/docs/` | The PDF builder and the guide's figures. |

## Direction, decided with the user

Dated so a later reader knows what was current.

- **2026-09-19:** UIPilot must be a tool people use daily, not once. UI Health
  (always-on checks, toolbar and Hierarchy lamps, one-click fixes) and the Click
  Debugger (what took a click in Play mode, and why) were built for that.
- **2026-09-19:** Seven themes: Soft Club (the product's own look, the user's
  taste), Soft Club Night, Ink, Fantasy RPG, JRPG Window, Pixel Retro, Sci-Fi HUD.
- **2026-09-22, all in 1.0** (the user has time and chose not to defer):
  - The genre backdrops go **into the engine**: layered scenes generated with the
    menu, swapped by Apply Theme, with a **"use your own image or video" slot** on
    the theme, because buyers replace our art with their game's.
  - **Time of day** on themes where it makes sense (Fantasy first). It is a value
    **the game sets** (the developer's code, via the generated `UIPilot_GameManager`),
    not a real-world clock, plus an editor setting on the theme. Transitions are
    driven by a shader blending day and night palettes, so no new runtime script.
  - A **Military Shooter** theme. "Realistic" means the slick COD/Battlefield feel:
    dark, desaturated, angular brackets, faint grid, condensed capitals, mono
    readouts, subtle scanning motion. The slickness is in the UI; a photoreal
    background comes from the buyer's own scene or image, never from us.
  - **More genres, one strong theme each** (horror, racing/arcade, cosy/casual,
    clean-modern are candidates), rather than more variants of one genre. Buyers
    keep a theme's chrome and swap its backdrop.
- **2026-09-22, built:** the Picture slot; the `_Scene` object with twelve Layer
  slots under a `_Wash`; the `UIPilot/Scene Layer` shader and `_UIPilotTimeOfDay`
  global; the GameManager's `timeOfDay`, `dayLengthSeconds` and `SetTimeOfDay`;
  Fantasy RPG's twelve-layer scene and the Military Shooter theme with its nine;
  Survival Horror with a ten-layer scene (later the same day). Still open: more
  genres, one theme each (racing/arcade, cosy/casual, clean-modern).
- Every new theme still has to pass: 4.5:1 text contrast measured on a render,
  focus visible without hue alone, and Apply Theme == fresh build for every
  theme pair with a control pair that differs. A scene is checked at midnight,
  dawn, noon and dusk, and the harness must set the hour after building (the
  build shows the theme's own hour).

## Where we left off

Updated 2026-09-23 (evening). Replace this section, don't append to it, when the state changes.

**Built and committed 2026-09-23: a working front end** (idea #1 of three the user approved
2026-09-23; #2 is screen-shape checks in UI Health with one-click anchor fixes, #3 more
certain checks: missing glyphs, text overflow, Constant Pixel Size, lost gamepad focus).
- Generated GameManager (template in `ScriptSetupContent`, repo copy regenerated): a
  `gameScene` field; Play loads it (`CanStreamedLevelBeLoaded`, else a Console warning),
  or hides the menu when empty; Esc/Start opens Pause in play (time 0, cursor freed and
  put back on Resume), Esc/B goes back one step; Quit asks once ("Press again to quit",
  3 s, cancelled when focus moves on) then quits; `OpenPauseMenu()` is public; a scene
  with no main menu starts in play; `OnDestroy` resets time if paused. Input System
  when `ENABLE_INPUT_SYSTEM`, else Esc and joystick button 1.
- Window: "Play loads" popup under Theme, once the scene has a main menu and a
  GameManager with the field; lists the build's scenes (active Build Profile), flags a
  scene not in the list, hints when there are none. Logic in `ScriptSetupModule`.
- Guide 5.1/5.2 rewritten, Quick start bullets, window table; `Window_Build.png` recaptured;
  PDF rebuilt (v22) and copied in.
- Verified: compile both input paths (legacy by recompiling without the define);
  a GUI Play-mode harness, 30/30 (keys via Input System virtual devices, needs
  `editorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView` and Enter Play
  Mode without domain reload, both restored); the asking label rendered in all nine
  themes (fits, widest Pixel Retro 334/440 px); the row's four states rendered.
- Two older gaps, fixed with it (user asked): a menu built after the others now gets its
  `On…Pressed` methods appended to the existing script (nothing else in it changes; a
  file declaring two types gets a Console warning instead), and Build UI / Repair wait
  for the compile before wiring (`BeginWaitingForCompile`); `WriteScript` skips an
  identical file so nothing waits for a compile that never comes. The overwrite guard
  now compares the whole file with `BuildScript` of the labels it declares, so any edit
  counts. Verified: a GUI run through two domain reloads (Main built, then Build UI with
  only Pause: Resume added and wired) and 21 pure-function checks run outside Unity by
  loading the compiled editor DLL in Windows PowerShell 5.1 (it targets the 4.8 API).
  `BuildScript` now writes '\n' only (it mixed in CRLF between methods).

**Waiting on the user**

- **Apply Theme** (reported 2026-09-22 as "does nothing visible"): the Theme field
  never took the pick. Fixed in `ae872e4` and `c7d4616` (status row under Apply,
  picker hint, Undo clears the row), rendered and checked. On 2026-09-23 the pref held
  Ink's GUID, so the picker works; the user has not said whether their menus changed.
  - If the field ever fails to take a pick from the picker window, its
    `ObjectSelectorUpdated` command is reaching another control: replace the field with
    a button that calls `EditorGUIUtility.ShowObjectPicker<UIPilotTheme>` under its own
    control ID and read `EditorGUIUtility.GetObjectPickerObject()` on that command.
  - Proposed, not approved: replace the object field with a dropdown of every
    `UIPilotTheme` in the project, by name.
- **"We need to anchor all the UI elements"** (2026-09-22). Unclear what moved: the
  user was asked which resolution or aspect and which elements, and whether idea #2
  is what they meant, with no answer yet. Ask before changing layout. Current layout, for reference: Canvas Scaler Scale With
  Screen Size 1920 × 1080, match 0.5; the panel floats 64 px from the left, top and
  bottom, 640 wide, with a 480 column inside, title at the top and actions at the
  bottom by design (`design-system/uipilot/pages/generated-menus.md`, "Layout");
  scene layers cover the panel with an AspectRatioFitter (Envelope Parent, 16:9).

**Next**

- The demo scene rebuild and the rest of the Asset Store list below (agreed order:
  cleanup before new features), then #2, #3.
- More genres, one theme each: racing/arcade, cosy/casual, clean-modern. Ask for 2–3
  reference screenshots first and look up the genre's title screens on the Game UI
  Database.
- The Military Shooter's subtle scanning motion was part of the brief and is not built.

**Not tested by hand yet:** the day cycle in Play mode (`dayLengthSeconds`,
`SetTimeOfDay`), Click Debugger clicks made by hand in the Game view, the legacy Input
Manager path (the new pause keys only compile-checked there), a real gamepad, the light
editor skin.

**Before resubmitting to the Asset Store** (think clean project): the demo scene
`Assets/UIPilot/Demo/UIPilot_Full.unity` was last saved 2026-06-18, so it still has the
old look and two unwired buttons: rebuild and save it. The package must include
`Assets/UIPilot_GameManager.cs`, which the demo scene uses. There is no asmdef and there
are no tests. Settle the version string (`.cursorrules` says v0.1.0-alpha, Player
Settings 0.1, a June commit said v0.6.0-alpha; the target is 1.0.0). The store
tagline is unconfirmed.

## References

- **Game UI Database** (gameuidatabase.com): filter Genres, Aesthetic or UI Style
  plus a screen type ("Title Screen", "Settings Menu") before designing a theme.
  Read it as page text; it is JavaScript-heavy.
- The user's taste for UIPilot's own brand is Gen X soft club. For genre themes,
  ask for 2–3 reference screenshots early; they recognise a look faster than
  they describe it.
- The other design sources are listed in the global `~/.claude/CLAUDE.md`.

## How to verify

- Compile: `node "C:/Users/xyada/.claude/skills/compile-check/compile-check.mjs" --root "<this folder>" --warnings`.
  Works with the editor open. Zero errors before reporting anything done.
- Unity runs: a temporary `Assets/_UIPilotVerify/Editor/*.cs` harness, run with
  `Unity.exe -batchmode -projectPath <this folder> -executeMethod <Type.Method>
  -logFile <file>`. GUI runs drop `-batchmode` and need a watchdog. Close the
  user's editor first (only if it has no unsaved changes) and reopen it after.
  Delete the harness in a separate command afterwards, then `git checkout` the
  `ProjectSettings/*.asset` files the run touched (line endings, UnityConnect).
- A killed Play session leaves `Temp/__Backupscenes`, which blocks the next
  launch on a recovery dialog. Delete it (after checking it is the harness's
  scene) before relaunching.
- A visual change is not done until it has been rendered in Unity and looked at.
  Render, crop, read the image; measure contrast on the pixels.
- The guide PDF: edit the Markdown, then build with `../tools/docs` (its README; it
  needs `marked@12`, which is not installed in the repo) and read the rendered pages.
- When the user reports a bug from their open editor: the Console is in
  `%LOCALAPPDATA%\Unity\Editor\Editor.log` (`Editor-prev.log` is the session before).
  EditorPrefs are in the registry under `HKCU\Software\Unity Technologies\Unity Editor 5.x`;
  each name gets an `_h<hash>` suffix, strings are stored as UTF-8 bytes, and a write
  lands at once. Unity's own C# for this version is on GitHub,
  `Unity-Technologies/UnityCsReference`, branch `6000.3` (the IMGUI object field is
  `Editor/Mono/GUI/ObjectField.cs`, its picker `Editor/Mono/ObjectSelector.cs`).

## Git

- Work on `Dev`; never commit broken code to `main`. Commit only when the user asks.
- Write the message to a file and commit with `git commit -F <file>`. Leave
  `UIPilot/.claude/` untracked.
- Unity runs append kerning records to TMP font assets (`Art/Fonts/* SDF.asset`) and
  rewrite line endings in `ProjectSettings/*.asset`. `git checkout` those before
  committing unless the change was meant.

## Skills to use here

`ui-ux-pro-max` and `frontend-design` for any menu or window design;
`better-interface` to review a finished screen; `12-principles-of-animation` for
any transition; `canvas-design` for backdrops and key art; `unity:ui-ugui`,
`unity:optimize-text-mesh-pro`, `unity:2d-pixel-perfect` when they apply;
`cyclomatic-complexity` after any function with heavy branching.
