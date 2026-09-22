# Rebuilding the UIPilot documentation PDF

The guide is written once, in Markdown:
`UIPilot/Assets/UIPilot/Documentation/UIPilot_Documentation.md`.
The PDF that ships next to it is generated from that file. Nothing in this folder
is part of the Asset Store package.

One-time setup, in this folder:

```bash
npm install marked@12
```

Rebuild (PowerShell, from this folder):

```powershell
node build-pdf.mjs ..\..\UIPilot\Assets\UIPilot\Documentation\UIPilot_Documentation.md .\images
$html = (Resolve-Path .\UIPilot_Documentation.html).Path
& "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" --headless=new --disable-gpu `
  --no-pdf-header-footer --print-to-pdf="$PWD\UIPilot_Documentation.pdf" ([System.Uri]$html).AbsoluteUri
```

Then copy `UIPilot_Documentation.pdf` over the one in the package's `Documentation` folder.

`images/` holds renders of the generated menus and cropped screenshots of the
UIPilot window, all made in Unity 6000.3.11f1 (dark skin). Replace them if the look
changes. The builder inserts figures after these headings, so keep them:
"3. Quick start", "4. What UIPilot creates", "5.4 Themes", "7. Click Debugger", "Scan & Repair" and "Manual", and before "What it checks" (the UI Health figure).

The eight `Theme_*.png` images are 960 x 540 renders of the main menu in each preset
theme, and `Theme_Fantasy_Hours.png` is Fantasy RPG's scene at midnight, dawn, noon and
dusk (the genre ones over their scenes from `tools/store/genre_backdrops.py`; Pixel Retro
halved with nearest-neighbour so its pixels stay sharp). `Click_Window.png` is the Click
Debugger section after three clicks on a generated main menu. Render a new one whenever a preset changes or one is added. The `Health_*.png` images are crops of the window, the Hierarchy and the main toolbar on a test scene with one UI Health issue of each kind.
