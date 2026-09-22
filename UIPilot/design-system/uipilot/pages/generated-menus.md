# Generated menus — themes, and the default "Soft Club" (overrides MASTER for this surface)

The look of the menus UIPilot **generates**. Chosen by the project owner from reference
images: Gen X soft club compilation covers and PS3-era adverts (c. 1999–2007). The editor
window keeps MASTER's look; this file changes only what the player sees.

Where this file is silent, MASTER rules. Where they disagree, this file wins. Still
binding from MASTER: real data never placeholder; no runtime scripts beyond the generated
GameManager; every sprite white and tintable; status never by colour alone; text contrast
4.5:1; **render it and look** before calling it done.

## Themes — how the look is delivered

The look is **data, not code**. A `UIPilotTheme` asset (`Editor/Core/UIPilotTheme.cs`)
holds every colour, size, letter treatment and decoration switch. The buyer picks one in
the Build section, duplicates it to make their own, and can push it onto menus that
already exist with **Apply Theme to Existing Menus**.

| Preset (`Assets/UIPilot/Themes/`) | What it is | Made by |
|---|---|---|
| **Soft Club** (default) | the look specified in the rest of this file | `UIGeneratorThemePresets.SoftClub()` = the theme's own field defaults + Michroma + glow material |
| **Soft Club Night** | the same artwork in a dark key: navy wash `#060A24` @ 93%, panel `#091036` @ 98%, violet and cyan blooms | `SoftClubNight()` |
| **Ink** | MASTER's ink-and-teal look as a menu: opaque `#121826` panel, teal `#0C7C7E` focus **bar**, neutral `#2A3550` hover bar, TMP default font, uppercase bold title, every decoration off | `Ink()` |
| **Fantasy RPG** | parchment page in a gilt frame (`uipilot_fantasy_panel`, 9-sliced 160), Young Serif titles over Crimson Pro Bold items in iron-gall ink `#3A2514` (secondary `#553A20`, 4.5:1 even on the scorched edge), gold-leaf **bar** with a garnet lozenge, gilt rule, warm candle blooms, icons off | `FantasyRpg()` |
| **JRPG Window** | blue gradient window with a white double rim (`uipilot_jrpg_panel`, sliced 48), Work Sans Bold capitals with a drop-shadow material, pointing-glove **marker**, icons off | `JrpgWindow()` |
| **Pixel Retro** | 16 px pixel-art window (`uipilot_pixel_panel`, Pixels Per Unit 100, point filter, drawn at 0.25 = 4 screen px per art px at 1080p), Silkscreen with a hard shadow on titles, 5x8 arrow **marker** at 4x, CRT lines in the window, icons off | `PixelRetro()` |
| **Sci-Fi HUD** | clipped-corner glass slab edged in cyan `#37E0FF` (`uipilot_scifi_panel`, sliced 120), Tektur capitals with a glow material, scanning **bar**, tick-scale rule, scene frames, scanlines, registration marks | `SciFiHud()` |
| **Military Shooter** | near-black slab `#0C0F12` @ 94% with a 1 px `#2E353C` edge and an amber `#F2A33A` bar down the left (`uipilot_shooter_panel`, sliced 96), Big Shoulders Bold capitals, amber-edged light **bar**, hazard-stripe rule, registration marks, and a nine-layer scene: overcast sky, grey ridges, a radar station, a helicopter, the tactical overlay (brackets, tick ruler, reticle). Nothing glows. Reference: Modern Warfare's title overlay and Battlefield 2042's dark slab with one accent, from the Game UI Database | `MilitaryShooter()` |
| **Survival Horror** | a black `#050505` @ 94% that feathers into the dark (`uipilot_horror_panel`: one blurred shape, sliced 128; no texture, since a sliced centre stretches), Italiana titles spaced +14 over DM Mono items, a rough red `#B4161B` **marker** (`uipilot_horror_mark`), a scratched dark-red rule, icons off, and a ten-layer scene: murky sky, moon with a radial halo, bare trees drawn by a branching routine, a house on a knoll with one lit window, one soft fog gradient, and a vignette that closes in at night. Reference: Alien Isolation's spaced capitals, Alan Wake's black, from the Game UI Database | `SurvivalHorror()` |

The genre sprites are drawn in `tools/art/theme-sprites.html` (pixel art in
`build-theme-sprites.ps1`) and carry their own colours: their themes tint them white.
Everything along a 9-sliced edge is the same all the way along it, ornaments sit inside the
slice border, and a panel gradient is drawn only between the slice lines so it stays linear
at any panel height. The backdrops used to judge them are `tools/store/genre_backdrops.py`.

Rules that keep this working. Break one and Apply Theme stops being safe:

1. **A theme never changes structure.** `UIGeneratorMenuBuilder` always creates every
   object, including every decoration. A theme switches decorations on and off
   (`SetActive`) and recolours; it never adds, removes, reorders or resizes layout.
2. **One styling path.** `UIGeneratorStyler.Apply(panel, theme)` is the only code that
   sets appearance. Build UI calls it as its last step and Apply Theme calls it on
   existing panels, so an applied theme is identical to a fresh build. Verified by
   rendering both and comparing. Never set a colour, font or size in the builder.
3. **The styler finds objects by name** (the `UIGeneratorContent` names) and skips what
   is missing, so a buyer who deleted a decoration gets no error.
4. **A new theme field needs three things:** a default equal to Soft Club, a tooltip, and
   a line in the styler that sets it on every apply, back to its default when the theme
   leaves it empty. Then regenerate the preset assets from their factories, in place, so
   their GUIDs survive (`EditorUtility.CopySerialized` from a fresh factory instance onto
   each existing asset, or delete them and click **Restore Default Themes**).
5. **A new preset** is a factory in `UIGeneratorThemePresets`, its asset-name constant in
   `UIPilotLabels.Theme`, an entry in `UIGeneratorThemePresets.All` (Restore reads that
   list), and its saved asset. Render all three menus over a bright scene before shipping
   it: a dark translucent wash reads as grey in Linear colour space, so dark themes need a
   near-opaque wash and panel.
6. Every preset must pass the MASTER checks on its own: text 4.5:1 on the composed
   panel (measure it on a render, over the panel art's darkest part), focus visible
   without relying on hue alone (a frame, a bar or a cursor appears), hover distinct from
   focus. Verify Apply Theme against a fresh build for every pair of presets, with a
   control pair that must differ.
7. Themes are Editor-only assets. No scene references one, so none reaches a build.
8. **New themes start from the full Soft Club preset.** Create > UIPilot > Theme is a
   [MenuItem] in UIGeneratorModule, not a [CreateAssetMenu]: a field default cannot
   reference the font asset, so a plain new asset would lose Michroma.
9. **Scenes.** A theme may carry a scene: `sceneLayers`, up to twelve `UIPilotSceneLayer`
   entries (a white sprite, a `UIPilot/Scene Layer` material), drawn back to front in the
   `_Scene` object's Layer slots, under the `_Wash`. The material holds the layer's colour
   and alpha at Night, Dawn, Day and Dusk and a Rise; the shader eases between them by
   one global float, `_UIPilotTimeOfDay` (0 midnight, 0.5 noon), which the Editor sets
   for its preview (`UIPilotScenePreview`) and the generated GameManager sets in the game
   (`timeOfDay`, `dayLengthSeconds`, `SetTimeOfDay`). No layer has a script. Layers share
   the 16:9 frame whatever their texture size (flat colours are 4 x 4, gradients 4 x 720),
   and are built by `tools/art/scene_layers.py`, which also writes the materials. A
   `picture` on the theme replaces the layers. Colour rules: a landmark takes the colour
   of the ground it stands on; a static wash over a scene stays light (Fantasy: 30%),
   since the layers carry the mood; check the night key for contrast between landmark
   and sky, and keep a moon or sun on the open side of the screen, clear of the panel.
10. **Focus styles.** *Frame* stretches the focus art (default `uipilot_focus`) around the
   item, `focusReach` past its edges. *Bar* stretches it behind the item from the icon's
   left, and a Bar without art is a flat colour. *Marker* draws the art at `markerSize`,
   ending 8 units before the words (`UIGeneratorMenuBuilder.LabelInset`), so a wide cursor
   reaches back past the item's edge instead of crowding the text. On the settings step
   arrows a Marker theme uses the hairline frame, as the arrows are too small for a cursor.
   A Marker theme turns **icons** off: the cursor takes their place.

The sections below specify **Soft Club**, the default. The "Do not" list at the end is
Soft Club's; another preset may use bold or a second typeface if its own spec says so.

## The vibe, in buildable terms

| In the references | How it is built here |
|---|---|
| Airy periwinkle → aqua → mint wash, soft light blooms | a translucent full-screen wash + four huge blurred blobs (`uipilot_glow`) tinted from the tokens |
| Blurred, blue-tinted photography | the **game scene itself**, seen through the wash — it becomes the hazy photo |
| Thin white rectangular frames, overlapping and offset | `uipilot_frame` hairlines: one on the glass panel, one offset echo, corner crosshairs |
| Saturated blue tiles with light type | the menu column is a **cobalt glass** panel; white type sits on it |
| Horizontal scanline bands | `uipilot_scanlines` tiled inside the glass at very low alpha |
| The PS3 menu's ribbon of light | `uipilot_wave`, a static flowing ribbon across the scene side |
| Wide lowercase techno lettering (Eurostile Extended / Microgramma) | **Michroma** (SIL OFL, shipped with its licence), LowerCase *style*, open tracking |
| Soft white glow around type and edges | TMP underlay glow material on the title; halo baked into the focus sprite |

## Colour tokens

| Token | Hex | Use |
|---|---|---|
| Wash | `#9DB6F2` @ 72% | full-screen veil over the scene |
| Bloom violet / aqua / mint / light | `#B3A6F7` / `#7AD7EC` / `#D2F7EC` / `#FFFFFF` | the four background blooms, 35–60% alpha |
| Glass | `#2343AE` @ 95% | the menu panel. Near-opaque on purpose: it is the surface text is read on |
| Hairline | `#FFFFFF` @ 85% / 40% | panel frame / its offset echo and crosshairs |
| Text | `#F4FBFF` | everything on glass — 5.5:1 or better on the composed glass |
| Text soft | `#D3E1FF` | values at rest, small print — 4.6:1 on glass |
| Focus | `#FFFFFF` | tint of the focus plate (frame + halo + 10% fill) |
| Hover | `#FFFFFF` @ 45% | the same plate, fainter |
| Meter | `#FFFFFF` fill on `#FFFFFF` @ 25% track | volume meter |

There is no hue accent in this look. **Focus is light**: a white hairline frame appears
around the item with a soft halo. That is a shape change as well as a brightness change,
so focus never depends on telling two colours apart.

## Type — Michroma

| Role | Size | Treatment |
|---|---|---|
| Panel title | 58, auto-sizes down to 28, may wrap | LowerCase style, tracking +4, glow material |
| Menu item / row label | 24 | LowerCase style, tracking +6 |
| Row value | 20 | LowerCase style |
| Small print / caption | 15 | UpperCase style, tracking +18 |

Michroma is very wide: a 24 px item is as long as a 36 px LiberationSans one. Sizes above
are tuned to keep "fullscreen" and the quality level names inside the 480 column.
If the font asset is missing, fall back to the TMP default and keep everything else.

## Layout (1920 × 1080)

A floating glass panel, not a full-height band: 64 from the left, top and bottom edges,
640 wide. Inside it, the same column as MASTER (480 wide, identity at the top, actions at
the bottom, 8-point grid), inset 80. An echo frame sits 16 right and 16 down of the panel.
Row height 56, spacing 8.

## Components

| Component | Build |
|---|---|
| Focus plate | one sprite, `uipilot_focus`: crisp 1 px frame + halo + 10% fill, 9-sliced, overhanging the item by 24 on every side. One graphic, so the Button's own Color Tint drives the whole effect: clear at rest, 45% white on hover, white on focus. |
| Menu item | icon (thin line, 24) + lowercase label. No box at rest. |
| Stepper | chevron icons instead of `<` `>` glyphs, value between |
| Meter | 2 px track and fill, square ends |
| Title block | title, then a 1 px hairline rule the width of the column with a crosshair at its end |

## Do not

- Put white type on the wash. Only on glass. (White on the wash is 2.5:1.)
- Use a second typeface, bold, or italics. Michroma has one weight; that is the look.
- Animate the wave or the scanlines. That needs a runtime script.
- Add fake "data" text (matrix columns, lorem codes) as decoration. Real data only.
