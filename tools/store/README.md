# Asset Store images

The key images and screenshots for UIPilot's store page. The finished files are in `out/`.
Nothing in this folder is part of the package.

| File | Size | Text allowed by Unity |
|---|---|---|
| `key_cover.png` | 1950 x 1300 | title, publisher, tagline |
| `key_card.png` | 420 x 280 | title, publisher |
| `key_icon.png` | 160 x 160 | none |
| `key_social.png` | 1200 x 630 | none |
| `screenshot_01` to `_08` | 2400 x 1600 | any; one feature each |

Every image is a 24-bit PNG with no alpha, as the store asks.

**Layout** follows the reference the user chose, Smart Mouse 2: logo, feature label,
headline, one line and three bullets on the left, and a large product image on the right.
**Look** is the Gen X soft club style of the menus UIPilot generates (see
`design-philosophy.md`, written with the canvas-design skill).

## How they are made

1. **Backdrops.** `backdrop.py` draws the Soft Signal world (a measured floor, six pearls, one
   ring) in day, night and ink lights. `genre_backdrops.py` draws a scene for each genre
   theme: ember (a valley at dusk), overworld (sky and sea), pixel (a 300 x 200 pixel night)
   and space (a ringed planet and a station). The three with a building share one castle,
   drawn once in the script, standing on ground levelled for it and coloured and lit by
   its scene. Headless Edge screenshots each page at 3900 x 2600.
2. **Unity renders.** `unity/UIPilotStoreRender.cs` is copied into
   `UIPilot/Assets/_UIPilotStore/Editor/` for one run, then deleted.
   - `RenderGame` (batch mode) builds the real menus in each theme over the backdrops. It
     temporarily names the project "Soft Signal", a made-up game, and puts the real name back
     afterwards.
   - `SetEditorScale` (batch mode) switches Unity's UI scaling to 200%. The next launch,
     `CaptureEditor`, grabs the window, Hierarchy and toolbar at 2x. It then deletes the scaling
     preference and hides the toolbar lamp again.

   Set the environment variables `UIPILOT_STORE_OUT` and `UIPILOT_STORE_BACKDROPS` first.
   - `unity/UIPilotThemeRender.cs` (`UIPilotThemeRender.Run`, batch mode, same two variables)
     renders all nine themes over their backdrops (a theme with a scene of its own,
     Fantasy RPG, Military Shooter and Survival Horror, draws that scene over the backdrop): main menu and settings at 1080p, the
     pause menu at Steam Deck size, and a 2400 x 1600 main menu for the genre themes. The
     theme pictures in the user guide come from it.
3. **Compose.** Run `compose.py <pages> <renders> <editor> <pdf pages> <repo root>`, then
   `render-pages.ps1 -Pages <pages> -Out out`. The PDF pages are the user guide rendered at
   2000 px wide.

Fonts: Michroma (in the package), and Jura from the canvas-design skill's `canvas-fonts`.
Both are SIL OFL.

To change copy, edit the `slides()` table and `TAGLINE` in `compose.py`, then repeat step 3.
Steps 1 and 2 only need re-running if the menus or the backdrops change.
