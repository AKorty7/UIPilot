# UIPilot

![UIPilot demo](https://github.com/user-attachments/assets/70d18979-efea-4770-8092-946efcd60e15)

Stop setting up Unity UI from scratch every project.

UIPilot is a Unity Editor tool that generates, wires, validates, and
repairs standard UGUI systems in one click. No runtime footprint.
No custom frameworks. Just clean, native Unity UI — ready to go.

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

- **Quick Build** — generates MainMenu, PauseMenu, and SettingsMenu
  panels with correctly configured Canvas, CanvasScaler, EventSystem,
  buttons, and a wired GameManager script. One click.
- **Scan & Repair** — audits every UIPilot object in your scene and
  reports missing or broken components. Repair Scene fixes everything
  it finds automatically.
- **Manual controls** — generate, clear, discover, and wire
  individual elements for hands-on workflows.
- **Non-destructive** — UIPilot only ever touches objects with the
  UIPilot_ prefix. Your existing scene is never modified.
- **Zero runtime footprint** — everything runs at editor-time.
  Nothing ships with your game.

---

## Quick start

1. Import UIPilot into your project
2. Open **Window → UIPilot**
3. Select which menus you want under **Build**
4. Click **Build UI**
5. Hit Play — your buttons are wired and ready

That's it. CanvasScaler is configured. EventSystem is there.
Listeners are persistent. GameManager stubs are written.
You just fill in your game logic.

---

## Requirements

- Unity 6.3 LTS (6000.3.x)
- Built-In Render Pipeline
- TextMeshPro (included with Unity)
- UGUI (included with Unity)

---

## How it works

UIPilot uses the `UIPilot_` prefix as a strict boundary.
Everything it generates is prefixed. Everything without the prefix
is never touched. This means you can drop UIPilot into an existing
project and it will coexist cleanly with whatever UI you already have.

The generated GameManager (`UIPilot_GameManager.cs`) contains method
stubs for every button — `OnPlayPressed()`, `OnQuitPressed()`, and so
on. Fill them in with your own logic. UIPilot never overwrites a file
you have already edited unless you explicitly run Quick Clear.

---

## Known limitations

- Tested on Unity 6.3 LTS with Built-In Render Pipeline only
- UI Toolkit is not supported in this version
- On slower machines, a WarnTypeNotResolved warning may appear after
  Quick Build — click Build UI a second time after compilation
  completes and it will resolve

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