# UIPilot

![UIPilot demo](https://github.com/user-attachments/assets/70d18979-efea-4770-8092-946efcd60e15)

Stop setting up Unity UI from scratch every project.

UIPilot is a Unity Editor tool that generates, wires, validates, and
repairs standard UGUI systems in one click. No runtime footprint.
No custom frameworks. Just clean, native Unity UI — ready to go.

Then it keeps watching. **UI Health** checks every UI in your scenes,
not only UIPilot's, for the bugs that make a button silently do nothing,
and fixes most of them in one click.

---

## The problem

Every Unity project needs a main menu. Every time you build one,
you do the same thing: create a Canvas, remember CanvasScaler,
set ScaleWithScreenSize, set 1920x1080, add GraphicRaycaster,
add EventSystem, build panels, add buttons, write a GameManager,
wire onClick listeners, set them to persistent.

From memory. Correctly. Every single time.

UIPilot does all of it in one click.

---

## What you get

- **UI Health** — checks all the UI in your open scenes as you work,
  on save, before Play and on every build: click events pointing at a
  deleted or renamed method, a missing or wrong-module EventSystem,
  canvases with no raycaster, invisible images eating a button's clicks,
  controls a gamepad can never reach, and text under the Steam Deck
  minimum. Lamps on the toolbar and in the Hierarchy; one-click, undoable
  fixes. Silent when everything is fine.
- **Click Debugger** — in Play mode, click anywhere and see what took
  the click and why: the method it calls, or the image drawn over the
  button, the CanvasGroup switching it off, the missing raycaster, the
  empty On Click. Works on any UI, with either input system.
- **Quick Build** — generates MainMenu, PauseMenu, and SettingsMenu
  panels with correctly configured Canvas, CanvasScaler, EventSystem,
  buttons, and a wired GameManager script. One click.
- **Themes** — the look is one asset. Pick Soft Club, Soft Club Night,
  Ink, Fantasy RPG, JRPG Window, Pixel Retro, Sci-Fi HUD or Military
  Shooter, or duplicate one and make your own. **Apply Theme to Existing
  Menus** restyles what you already built without touching its layout or
  wiring.
- **Scenes and time of day** — a theme can put your own picture behind
  the menu, or its own layered scene that turns with the hour: Fantasy
  RPG's valley and Military Shooter's grey ridges go from midnight to
  dusk. Your game sets the hour with one call, or lets a day run on its
  own. No scripts on the layers: one shader, one global value.
- **Scan & Repair** — audits every UIPilot object in your scene and
  reports missing or broken components. Repair Scene fixes everything
  it finds automatically.
- **Manual controls** — generate, clear, discover, and wire
  individual elements for hands-on workflows.
- **Non-destructive** — UIPilot only creates and edits objects with the
  UIPilot_ prefix. The one thing it adds beyond that is an EventSystem,
  and only if your scene has none. UI Health reads all your UI but
  changes something that isn't UIPilot's only when you click its Fix.
- **Editor-only tool** — all UIPilot code lives in an Editor folder.
  The only file that ships with your game is the GameManager script it
  writes for you to own.

---

## Quick start

1. Import UIPilot into your project
2. If your project has no `Assets/TextMesh Pro` folder yet:
   **Window → TextMeshPro → Import TMP Essential Resources**
3. Open **Tools → UIPilot**
4. Tick the menus you want under **Build**, then click **Build UI**
5. The first time, wait a few seconds while Unity compiles the new
   script — the buttons are wired automatically when it finishes
6. Hit Play — your buttons are wired and ready

That's it. CanvasScaler is configured. EventSystem is there.
Listeners are persistent. GameManager stubs are written.
You just fill in your game logic.

The full guide — setup, everything that gets created, customising,
troubleshooting — is in
[`UIPilot_Documentation.pdf`](UIPilot/Assets/UIPilot/Documentation/UIPilot_Documentation.pdf).

---

## Requirements

- Unity 6.3 LTS (6000.3.x)
- UGUI + TextMeshPro (`com.unity.ugui`, included with Unity), with the
  TMP Essential Resources imported
- Any Active Input Handling setting: Input Manager, Input System, or Both
- Tested with the Built-In Render Pipeline

---

## How it works

UIPilot uses the `UIPilot_` prefix as a strict boundary.
Everything it generates is prefixed. Everything without the prefix
is never touched. This means you can drop UIPilot into an existing
project and it will coexist cleanly with whatever UI you already have.

The generated GameManager (`UIPilot_GameManager.cs`) contains method
stubs for every button — `OnPlayPressed()`, `OnQuitPressed()`, and so
on. Fill them in with your own logic. Once you have edited any of those
methods, UIPilot asks before it ever overwrites the file.

---

## Known limitations

- Tested on Unity 6.3 LTS with Built-In Render Pipeline only
- UI Toolkit is not supported in this version
- UIPilot finds its objects by name — keep the `UIPilot_` GameObject
  names (the visible text is yours to change)

---

## Support

Made by NomadStudios
📧 NomadStudios47@outlook.com

If something breaks or behaves unexpectedly, open an issue or
send an email. Every report gets a response.

---

## License

This asset is licensed for use under the
[Unity Asset Store EULA](https://unity.com/legal/as-terms).