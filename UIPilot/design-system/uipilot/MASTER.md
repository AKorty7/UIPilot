# UIPilot Design System — MASTER

The source of truth for how UIPilot looks: the menus it **generates** and its own
**editor window**. Read this before changing anything visual. A file in `pages/`
overrides this one for its surface; otherwise this file rules.

> **Generated menus are specified in [`pages/generated-menus.md`](pages/generated-menus.md)
> ("Soft Club": blue wash, cobalt glass, hairline frames, Michroma).** That file overrides
> everything menu-specific below — colours, type, sizes, components, states. What this
> file still decides for the menus: the principles (section 1), the sprite-kit rules (5a),
> accessibility (8), verification (9) and the don'ts (10). Sections 2–6 now describe the
> **UIPilot editor window** and the fallback look used when the art kit is missing.
>
> **The menu look is a theme asset, not code.** Buyers choose between the presets Soft
> Club (default), Soft Club Night and Ink, or duplicate one. All appearance goes through
> `UIGeneratorStyler.Apply`; the builder creates structure only. The rules are in the
> "Themes" section of that page. Read them before touching the builder, the styler or a
> preset.

Built with the `ui-ux-pro-max` skill's Master + Overrides structure. Its generated
recommendation was reviewed, not pasted:

| From the skill | Verdict |
|---|---|
| Style: **Minimalism & Swiss** — grid, clear type hierarchy, one accent, no decoration, sharp corners | **Adopted.** It is the direction below. |
| Status colours green / amber / red; text contrast 4.5:1; keyboard access; visible focus; subtle motion | **Adopted.** |
| Palette: indigo on lavender (light); fonts: Orbitron + JetBrains Mono; GSAP scroll reveals | **Rejected.** Web-landing output. UIPilot cannot ship fonts, renders in Unity, and its menus sit over a game, so they are dark. |

## 1. Principles

1. **Real, never placeholder.** Every string on screen is real project data or does
   real work: the title is the Product Name, the version line is the build's version,
   every Settings row changes a real setting. If something cannot be real, leave it out.
2. **A menu is a list, not a stack of boxes.** An item at rest is its label. Only the
   item with focus is a filled bar.
3. **The accent means one thing: focus is here.** The mouse pointer gets a neutral bar.
4. **Real artwork, from one small kit.** The menus are drawn with UIPilot's own sprite kit
   (section 5a): shaped, shaded, shadowed, with icons. Every sprite is **white and
   tintable**, so colour still lives in the tokens below and the buyer restyles by changing
   a colour, not by repainting art. No runtime scripts except the one generated
   `UIPilot_GameManager`; everything stays native UGUI the buyer can edit.
5. **Opaque, or it does not carry meaning.** Translucent fills blend differently in Linear
   and Gamma colour space. Anything a player must read sits on an opaque surface.
6. **Status is a lamp *and* a word.** Never colour alone.
7. **Look at it.** A visual change is not done until it has been rendered in Unity and
   looked at (section 9).

## 2. Colour tokens

Shared by both surfaces unless noted. Contrast is against near-white text `#F3F5F8`.

| Token | Hex | Role | Contrast |
|---|---|---|---|
| Band / ink | `#121826` | the menu column's surface (opaque) | 16.2:1 |
| Band edge | `#2B3650` | 2 px hairline finishing the band's open side | — |
| Backdrop | `#04060A` @ 65% | dims the scene; nothing legible depends on it | — |
| Accent | `#0C7C7E` | focus bar, title rule, meter fill, window accent bar | 4.6:1 |
| Accent pressed | `#095F61` | bar while a button is held | 6.9:1 |
| Hover | `#2A3550` | bar under the mouse pointer; meter track | 11.1:1 |
| Idle | accent/hover hue @ 0% alpha | a button at rest — fades alpha only, never through an off-colour | — |
| Disabled | `#0C101A` | recessed, darker than the band | — |
| Text | `#F3F5F8` | all primary text | — |
| Text muted | `#97A1B3` | values at rest, small print | 6.8:1 on band |
| Lamp OK / Caution / Fault / Idle | `#3FB950` / `#E3A008` / `#E5484D` / `#7A7F87` | editor status rows, always beside a status word | non-text |

Every text/background pair is 4.5:1 or better at any size. Do not introduce a second accent.

## 3. Type

One family, because it is the only one every project has: **LiberationSans SDF** (TMP default).

| Role | Size | Treatment |
|---|---|---|
| Panel title | 64, auto-sizes down to 36, may wrap to 2 lines | Bold, **UpperCase style** (not typed in caps), tracking +8, bottom-left |
| Menu item / row label | 30 | Regular, sentence case |
| Row value / step glyph | 26 / 30 | Regular |
| Small print | 20 | Regular, muted |

- Single-line labels use **Middle** alignment (`TextAlignmentOptions.Left/Center/Right`),
  never *Midline*: Midline centres on glyph shape, so a word with a descender drifts
  off its row's baseline.
- Capitals + tracking are for panel titles only. No all-caps labels, no tracked eyebrows.

## 4. Space and layout (reference 1920 × 1080, 8-point grid)

| Constant | Value |
|---|---|
| Column inset (left, top, bottom) | 120 |
| Column width | 480 |
| Band width | 720 (inset + column + inset) |
| Row / item height | 64 |
| Column spacing | 8 |
| Title box | 96 high |
| Rule | 72 × 4, then 32 gap |
| Focus-bar overhang (left) | 24 — labels share the title's left edge, the bar has padding |
| Stepper | 48 button · 144 value · 48 button, right-aligned in the row |
| Meter | 4 high, 16 inset each side, under the value |
| Small print | 56 above the band's foot |

**Composition:** identity at the top of the band (title, rule), actions at the bottom
(buttons), a flexible spacer between. Settings rows sit directly under the title and
Back stays at the foot, where every panel keeps its way out. The scene stays visible,
dimmed, on the right two thirds.

## 5. Components

| Component | Anatomy | Notes |
|---|---|---|
| **Menu item** | `UIPilot_Btn_<Label>` → `Bar` (Image, target graphic, raycast target, overhangs left) + `Text` | Label = object-name suffix = method stem: `UIPilot_Btn_Play` → `OnPlayPressed()` |
| **Stepper row** | `UIPilot_Settings_Row_<Name>` → `Label`, `UIPilot_Btn_<Name>Down`, value text, `UIPilot_Btn_<Name>Up` | Created left-to-right so hierarchy order = visual order = focus order |
| **Value button** | a menu item with a right-aligned muted value text | Pressing it changes the value (Fullscreen On/Off) |
| **Meter** | track (Hover colour) + fill (Accent); fill width = `anchorMax.x` | No sprite needed, so no Filled image type |
| **Rule** | 72 × 4 accent bar under the title | Shows the accent before anything is hovered |
| **Small print** | muted 20 px text pinned to the band's foot, outside the layout group | Real version, kept live by the GameManager |

All controls are **Buttons calling no-argument methods**, so they flow through the
existing discover → wire → audit pipeline and ordinary UI navigation (keyboard, gamepad).

### Editor components: UI Health

UI Health shows up in three places outside the window's own rows. All three use the
same 8 px square lamp and the same four lamp colours as the status rows. None of them
invents a new colour.

| Component | Anatomy | Notes |
|---|---|---|
| **Issue row** (window) | lamp · name as a link (selects the targets, accent on hover) / muted detail · mini **Fix** · status word | Fix appears only when the change is safe to make and undoable. The word is *Broken* or *Warning*, never colour alone. |
| **Summary row** | lamp · bold "All clear" or "N issues to look at in: scenes" / muted detail · mini **Check Now** | Idle grey lamp in Play mode ("Not checked yet"). |
| **Toolbar lamp** | 16 px icon (8 px lamp centred) + "UI" or "UI  N" | Unity hides package toolbar items by default. While it is hidden, the window shows a tip with **Show on Toolbar** (primary) and **Not now**. **Lamp on Unity's main toolbar** under Checks is the lasting switch. If Unity's internals change, both fall back to written instructions. Tooltip carries the words. |
| **Hierarchy lamp** | 8 px lamp on the object, 4 px lamp on each ancestor, 30 px from the row's right edge | The small lamp keeps a folded Hierarchy honest. Tooltip gives name and detail. Clear of the prefab arrow. |

Rule: **silent when all is well**. No Console line, no Hierarchy lamp, and a green toolbar
lamp with no number. A check that cannot be sure stays quiet: a false alarm costs more
than a miss.

## 5a. The sprite kit

Vector sources live in `tools/art/` (outside the Unity project); the PNGs they produce
live in `Assets/UIPilot/Art/`. Authored at 2× (Pixels Per Unit 200), white on
transparent, 9-sliced where they stretch. Never bake a colour into a sprite.

| Asset | What it is | Used for |
|---|---|---|
| `uipilot_glow` | soft blurred blob | the four background blooms, each in a different tint |
| `uipilot_focus` | hairline frame + halo (a blurred *outline*, so the interior stays clear) + 6% fill, 9-slice border 72 | the focus plate of every button — one graphic, so Color Tint drives it |
| `uipilot_frame` | 1-unit hairline rectangle, 9-slice border 8 | glass frame, its echo, the frames drifting over the scene |
| `uipilot_cross` | registration mark | frame corners, end of the title rule |
| `uipilot_scanlines` | 2-on / 2-off stripes, a repeating **texture** (RawImage), not a sprite | inside the glass, and a band across the scene |
| `uipilot_wave` | flowing ribbon with a blurred leading edge | the PS3-style ribbon of light |
| `uipilot_fade`, `uipilot_shade` | horizontal / vertical alpha gradients | spare: soft edges |
| `uipilot_icon_*` | 24-grid line icons, 1.7 stroke, round caps and joins | play, settings, quit, back, volume, fullscreen, quality, chevrons |
| `Michroma SDF` + `Michroma SDF Glow` | TMP font asset (dynamic) and its soft-halo material preset | all menu lettering; the glow preset on titles |

Rebuild the sprites with `tools/art/build-sprites.ps1` (headless Edge screenshots the SVG
sheet on a transparent page, then the sheet is cut up). Import settings — Sprite, borders,
Pixels Per Unit 200, no mipmaps, uncompressed — live in the `.meta` files and survive a
rebuild. Michroma is SIL OFL: its licence ships beside it and is listed in
`Assets/UIPilot/Third-Party Notices.txt`. Any new third-party asset needs the same.

Icon rules: one family, one stroke weight, one size token (**24** beside a label, **20**
in a step button); an icon sits beside a visible label and never replaces it, except the
chevrons, whose buttons are named for the method they call; icon colour is the text colour.

## 6. States

| State | Bar colour | When |
|---|---|---|
| Idle | transparent | at rest |
| Hover | Hover `#2A3550` | mouse pointer over it |
| Focus | Accent `#0C7C7E` | selected by keyboard / gamepad, or last clicked |
| Pressed | Accent pressed `#095F61` | while held |
| Disabled | Disabled `#0C101A` | not interactable |

Driven only by the Button's own **Color Tint** transition (Image white, colours in the
ColorBlock). Fade 0.08 s. State changes never move or resize anything.

## 7. Motion

Colour fades of 80 ms on state change, nothing else. They run on unscaled time, so
they still work while the game is paused (`Time.timeScale = 0`).

## 8. Accessibility

- Text contrast ≥ 4.5:1 everywhere (section 2).
- The first button of whichever panel opens takes focus, so keyboard and gamepad work
  with no setup; focus is cleared when every panel is hidden.
- Status is never colour alone: lamp + word (OK / Warning / Missing / Broken / Error /
  Fixed / Failed / Working / Ready / Stalled).
- Sizes are tuned for desktop and console. On phones the 64-high rows fall under a
  48 dp touch target; this is documented in the user guide, not hidden.

## 9. Verifying a visual change

Reading code is not looking. Render it:

- **Generated menus:** a temporary `Assets/_UIPilotVerify/Editor` script run with
  `Unity.exe -batchmode -executeMethod`. Build the menus in a new scene, switch the canvas to
  Screen Space – Camera, render to a 1920 × 1080 RenderTexture over a *lit 3D scene* (a flat
  background hides blending faults), write PNGs, exit without saving.
- **Editor window:** needs a real (non-batch) launch. `CreateInstance` a second window and
  `ShowUtility` it — leave the user's docked tab and EditorPrefs alone — then grab it with
  `m_Parent.GrabPixels` and flip it vertically.
- Delete the harness afterwards. Then rebuild the guide's figures (`tools/docs`).

## 10. Do not

- Add placeholder copy ("Game Title", "Your text here", lorem ipsum).
- Add a second accent, or art from outside the kit: a coloured sprite, a second icon
  family, an icon font, a second light direction.
- Give idle items a visible box, or make hover and focus look the same.
- Put meaning on a translucent surface.
- Use *Midline* alignment for single-line labels.
- Add a runtime MonoBehaviour. Behaviour belongs in the generated `UIPilot_GameManager`.
