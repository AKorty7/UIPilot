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
| `Assets/UIPilot/Themes/` | The eight preset theme assets. Regenerated from `UIGeneratorThemePresets` in place, so their GUIDs survive. |
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
  Fantasy RPG's twelve-layer scene and the Military Shooter theme with its nine.
  Still open from the same decision: more genres, one theme each.
- Every new theme still has to pass: 4.5:1 text contrast measured on a render,
  focus visible without hue alone, and Apply Theme == fresh build for every
  theme pair with a control pair that differs. A scene is checked at midnight,
  dawn, noon and dusk, and the harness must set the hour after building (the
  build shows the theme's own hour).

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

## Skills to use here

`ui-ux-pro-max` and `frontend-design` for any menu or window design;
`better-interface` to review a finished screen; `12-principles-of-animation` for
any transition; `canvas-design` for backdrops and key art; `unity:ui-ugui`,
`unity:optimize-text-mesh-pro`, `unity:2d-pixel-perfect` when they apply;
`cyclomatic-complexity` after any function with heavy branching.
