"""Composes UIPilot's Asset Store images as HTML pages; headless Edge renders each at
its exact size (render-pages.ps1).

Layout: the reference the user chose (Smart Mouse 2). Logo, a feature label, a big
headline, one line of explanation and three bullets on the left; one large image of
the real product on the right.

Look: Gen X soft club, the same language as the menus UIPilot generates. The Soft
Signal world seen through a periwinkle-to-aqua wash, light blooms, a scanline band,
drifting hairline frames, a ribbon of light. Text sits on cobalt glass (white on the
wash is only 2.5:1) in wide lowercase Michroma with a soft glow; body copy in Jura.

    python compose.py <out_dir> <renders> <editor> <pdf_pages> <repo_root>
"""
import math
import os
import sys
from pathlib import Path

OUT, RENDERS, EDITOR, PDF, REPO = (Path(p) for p in sys.argv[1:6])
BACKDROPS = RENDERS.parent                          # the Soft Signal worlds, without any UI
FONTS = Path(os.path.expanduser("~/.claude/skills/canvas-design/canvas-fonts"))
MICHROMA = REPO / "UIPilot/Assets/UIPilot/Art/Fonts/Michroma-Regular.ttf"

WORDMARK = "uipilot"
TAGLINE = "Game menus in one click.<br>UI that keeps working."
PUBLISHER = "NomadStudios"

# ── Tokens: the Soft Club theme's own colours ────────────────────────────────
GLASS = "rgba(35, 67, 174, 0.95)"      # cobalt glass, what all text sits on
HAIRLINE = "rgba(255, 255, 255, 0.85)"
ECHO = "rgba(255, 255, 255, 0.40)"
MINT = "#8FF0D6"                       # the one accent: labels, lamps
TEXT = "#F4FBFF"
TEXT_SOFT = "#D3E1FF"
GLOW = "0 0 14px rgba(255,255,255,0.55), 0 0 38px rgba(176,200,255,0.45)"


def url(path):
    return Path(path).resolve().as_uri()


def font_faces():
    return f"""
@font-face {{ font-family: Michroma; src: url('{url(MICHROMA)}'); }}
@font-face {{ font-family: Jura; font-weight: 300; src: url('{url(FONTS / "Jura-Light.ttf")}'); }}
@font-face {{ font-family: Jura; font-weight: 500; src: url('{url(FONTS / "Jura-Medium.ttf")}'); }}
"""


# UIPilot's mark: the focus frame of its menus with crosshair corners, three menu
# rows (the first one focused), and the UI Health lamp.
def mark(size, stroke="#FFFFFF"):
    return f"""<svg class="mark" width="{size}" height="{size}" viewBox="0 0 100 100" aria-hidden="true">
  <g fill="none" stroke="{stroke}" stroke-width="5.5" stroke-linecap="square">
    <rect x="11" y="25" width="64" height="64"/>
  </g>
  <g stroke="{stroke}" stroke-width="2.6" opacity="0.85">
    <line x1="3" y1="25" x2="19" y2="25"/><line x1="11" y1="17" x2="11" y2="33"/>
    <line x1="3" y1="89" x2="19" y2="89"/><line x1="11" y1="81" x2="11" y2="97"/>
    <line x1="67" y1="89" x2="83" y2="89"/><line x1="75" y1="81" x2="75" y2="97"/>
  </g>
  <rect x="22" y="38" width="42" height="9" fill="{stroke}"/>
  <rect x="22" y="53" width="42" height="9" fill="{stroke}" opacity="0.55"/>
  <rect x="22" y="68" width="30" height="9" fill="{stroke}" opacity="0.32"/>
  <circle cx="79" cy="21" r="12" fill="{MINT}"/>
</svg>"""


def base_css(w, h):
    return f"""
{font_faces()}
* {{ box-sizing: border-box; margin: 0; padding: 0; }}
html, body {{ width: {w}px; height: {h}px; overflow: hidden; background: #9DB6F2; }}
body {{ position: relative; color: {TEXT}; font-family: Jura, sans-serif; font-weight: 500; }}
.layer {{ position: absolute; inset: 0; }}
.world {{ width: 100%; height: 100%; object-fit: cover; object-position: 70% 60%; filter: saturate(1.05); }}
.wash {{ background: linear-gradient(165deg, rgba(141,163,236,0.80) 0%, rgba(157,182,242,0.68) 46%, rgba(146,214,236,0.60) 100%); }}
.blooms {{ background:
    radial-gradient(ellipse 42% 46% at 8% 6%, rgba(179,166,247,0.62), transparent 70%),
    radial-gradient(ellipse 46% 50% at 96% 96%, rgba(122,215,236,0.70), transparent 70%),
    radial-gradient(ellipse 34% 40% at 80% 16%, rgba(210,247,236,0.60), transparent 70%),
    radial-gradient(ellipse 38% 34% at 52% 46%, rgba(255,255,255,0.32), transparent 72%); }}
.scanband {{ top: 34%; bottom: auto; height: 24%;
  background: repeating-linear-gradient(0deg, rgba(255,255,255,0.13) 0 2px, transparent 2px 7px);
  -webkit-mask-image: linear-gradient(90deg, transparent 0%, #000 18%, #000 82%, transparent 100%),
                      linear-gradient(0deg, transparent, #000 20%, #000 80%, transparent); -webkit-mask-composite: source-in; }}
.hair {{ position: absolute; border: 2px solid rgba(255,255,255,0.45); }}
.hair.faint {{ border-color: rgba(255,255,255,0.26); }}
.cross {{ position: absolute; width: 30px; height: 30px; transform: translate(-50%, -50%); }}
.cross::before, .cross::after {{ content: ""; position: absolute; background: rgba(255,255,255,0.9); }}
.cross::before {{ left: 14px; top: 0; width: 2px; height: 30px; }}
.cross::after {{ top: 14px; left: 0; height: 2px; width: 30px; }}

/* Cobalt glass: the surface every word sits on, framed the way the menus frame it. */
.glass {{ position: absolute; background:
    repeating-linear-gradient(0deg, rgba(255,255,255,0.05) 0 1px, transparent 1px 4px), {GLASS};
  border: 2px solid {HAIRLINE}; box-shadow: 0 30px 80px rgba(30, 50, 140, 0.35); }}
.echo {{ position: absolute; border: 2px solid {ECHO}; pointer-events: none; }}
.word {{ font-family: Michroma, sans-serif; text-transform: lowercase; text-shadow: {GLOW}; line-height: 1; }}
.small {{ font-family: Michroma, sans-serif; text-transform: uppercase; letter-spacing: 0.22em; }}

/* The product image: hairline frame, echo, crosshair corners, a soft light around it. */
.frame {{ position: absolute; overflow: hidden; background: #2343AE;
  border: 2px solid {HAIRLINE}; box-shadow: 0 0 90px rgba(255,255,255,0.28), 0 40px 90px rgba(28,44,130,0.40); }}
.frame img {{ width: 100%; height: 100%; object-fit: cover; display: block; }}
.window {{ position: absolute; box-shadow: 0 26px 70px rgba(20, 30, 90, 0.55), 0 0 0 2px rgba(255,255,255,0.55); }}
.window img {{ display: block; width: 100%; height: auto; }}
"""


# The PS3-era ribbon of light: a few long strokes, one wide and blurred beneath them.
def ribbon(w, h, y0, amp, phase=0.0):
    def path(dy, a, k):
        pts = []
        for i in range(0, 41):
            x = -0.05 * w + i * (1.1 * w / 40)
            y = y0 + dy + a * math.sin(2 * math.pi * (i / 40) * k + phase)
            pts.append(f"{x:.1f},{y:.1f}")
        return "M" + " L".join(pts)

    strokes = [(0, amp, 1.15, 26, 0.28, True), (-10, amp * 0.96, 1.15, 5, 0.85, False),
               (14, amp * 0.9, 1.1, 2.5, 0.65, False), (34, amp * 1.05, 1.2, 2, 0.45, False),
               (-34, amp * 0.8, 1.05, 1.6, 0.4, False)]
    lines = []
    for dy, a, k, sw, op, blur in strokes:
        filt = ' filter="url(#rblur)"' if blur else ""
        lines.append(f'<path d="{path(dy, a, k)}" fill="none" stroke="#FFFFFF" stroke-width="{sw}" '
                     f'stroke-opacity="{op}" stroke-linecap="round"{filt}/>')
    return (f'<svg class="layer" width="{w}" height="{h}"><defs><filter id="rblur" x="-5%" y="-50%" '
            f'width="110%" height="200%"><feGaussianBlur stdDeviation="14"/></filter></defs>{"".join(lines)}</svg>')


def atmosphere(w, h, ribbon_y, frames=()):
    hair = "".join(f'<div class="hair {c}" style="left:{x}px;top:{y}px;width:{fw}px;height:{fh}px"></div>'
                   for x, y, fw, fh, c in frames)
    return (f'<div class="layer"><img class="world" src="{url(BACKDROPS / "backdrop_day.png")}"></div>'
            f'<div class="layer wash"></div><div class="layer blooms"></div><div class="layer scanband"></div>'
            f'{hair}{ribbon(w, h, ribbon_y, h * 0.045)}')


def cross(x, y):
    return f'<div class="cross" style="left:{x}px;top:{y}px"></div>'


def glass(x, y, w, h, inner="", echo=18):
    return (f'<div class="echo" style="left:{x + echo}px;top:{y + echo}px;width:{w}px;height:{h}px"></div>'
            f'<div class="glass" style="left:{x}px;top:{y}px;width:{w}px;height:{h}px">{inner}</div>'
            f'{cross(x + w + echo, y)}{cross(x, y + h + echo)}')


def framed_image(x, y, w, h, image, position="left center", filt="none", echo=18):
    return (f'<div class="echo" style="left:{x + echo}px;top:{y + echo}px;width:{w}px;height:{h}px"></div>'
            f'<div class="frame" style="left:{x}px;top:{y}px;width:{w}px;height:{h}px">'
            f'<img src="{url(image)}" style="object-position:{position};filter:{filt}"></div>'
            f'{cross(x + w + echo, y)}{cross(x, y + h + echo)}')


# A real editor capture floating over the product image. crop_from and crop_to are
# rows of the capture, in its own pixels, when only part of the window is shown.
def window_html(image, left, top, width, source_width, crop_from=0, crop_to=None):
    scale = width / source_width
    height = f"height:{(crop_to - crop_from) * scale:.0f}px;overflow:hidden;" if crop_to else ""
    return (f'<div class="window" style="left:{left}px;top:{top}px;width:{width}px;{height}">'
            f'<img src="{url(image)}" style="margin-top:-{crop_from * scale:.0f}px"></div>')


def page(w, h, css, body):
    return (f"<!doctype html><html><head><meta charset='utf-8'><style>{base_css(w, h)}{css}</style></head>"
            f"<body>{body}</body></html>")


# ── Screenshots (2400 x 1600) ───────────────────────────────────────────────
W, H = 2400, 1600
TILE = dict(x=96, y=96, w=900, h=1408)            # the glass column, left
PANEL = dict(x=1110, y=196, w=1214, h=1208)       # the product image, right
SLIDE_FRAMES = [(1010, 120, 1180, 1330, "faint"), (1520, 1040, 820, 470, "")]

SLIDE_CSS = f"""
.inner {{ position: absolute; inset: 0; padding: 92px 80px 0 80px; }}
.title {{ display: flex; align-items: flex-start; gap: 22px; font-size: 84px; }}
.title .mark {{ margin-top: -18px; }}
.rule {{ position: relative; margin-top: 44px; height: 2px; width: 740px; background: rgba(255,255,255,0.6); }}
.rule::after {{ content: ""; position: absolute; right: -14px; top: -14px; width: 30px; height: 30px;
  background: linear-gradient(rgba(255,255,255,0.9), rgba(255,255,255,0.9)) 14px 0 / 2px 30px no-repeat,
              linear-gradient(rgba(255,255,255,0.9), rgba(255,255,255,0.9)) 0 14px / 30px 2px no-repeat; }}
.label {{ margin-top: 140px; display: flex; align-items: center; gap: 18px; color: {MINT}; font-size: 24px; }}
.label::before {{ content: ""; border-left: 14px solid {MINT}; border-top: 9px solid transparent;
  border-bottom: 9px solid transparent; }}
h1 {{ margin-top: 30px; font-weight: 400; font-size: 70px; line-height: 1.22; width: 750px; }}
.sub {{ margin-top: 44px; width: 730px; font-size: 40px; line-height: 1.4; color: {TEXT_SOFT}; }}
ul {{ margin-top: 44px; list-style: none; }}
li {{ display: flex; align-items: center; gap: 26px; font-size: 38px; line-height: 1.25; margin-top: 24px; }}
li::before {{ content: ""; flex: 0 0 14px; height: 14px; background: {MINT};
  box-shadow: 0 0 12px rgba(143,240,214,0.9), 0 0 0 6px rgba(143,240,214,0.14); }}
.foot {{ position: absolute; left: 80px; bottom: 64px; font-size: 18px; color: {TEXT_SOFT}; }}
"""


def slide(label, headline, sub, bullets, visual):
    t = TILE
    items = "".join(f"<li>{b}</li>" for b in bullets)
    inner = (f'<div class="inner"><div class="title word">{WORDMARK}{mark(76)}</div><div class="rule"></div>'
             f'<div class="label small">{label}</div><h1 class="word">{headline}</h1>'
             f'<p class="sub">{sub}</p><ul>{items}</ul></div><div class="foot small">{PUBLISHER}</div>')
    body = (atmosphere(W, H, H * 0.80, SLIDE_FRAMES) + visual
            + glass(t["x"], t["y"], t["w"], t["h"], inner))
    return page(W, H, SLIDE_CSS, body)


def panel(image, position="left center", filt="none"):
    p = PANEL
    return framed_image(p["x"], p["y"], p["w"], p["h"], image, position, filt)


def cascade(images):
    """Three renders stepping down to the right, each framed like the menus."""
    return "".join(framed_image(1150 + i * 150, 240 + i * 270, 880, 587, image, echo=14)
                   for i, image in enumerate(images))


def pages_visual():
    p = PANEL
    sheet = "position:absolute;background:#fff;overflow:hidden;box-shadow:0 30px 80px rgba(28,44,130,0.45),0 0 0 2px rgba(255,255,255,0.8)"
    return (f'<div style="{sheet};left:{p["x"] + 50}px;top:{p["y"] + 10}px;width:760px;height:1076px">'
            f'<img src="{url(PDF / "page01.png")}" style="width:100%"></div>'
            f'<div style="{sheet};left:{p["x"] + 460}px;top:{p["y"] + 150}px;width:760px;height:1076px">'
            f'<img src="{url(PDF / "page09.png")}" style="width:100%"></div>')


# The right end of Unity's main toolbar at 2x, where the lamp sits.
def toolbar_strip(left, top):
    width = 780
    return (f'<div class="window" style="left:{left}px;top:{top}px;width:{width}px;height:72px;overflow:hidden;background:#191919">'
            f'<img src="{url(EDITOR / "editor_toolbar.png")}" style="width:1920px;max-width:none;margin-left:-{1920 - width}px"></div>')


def slides():
    r, e = RENDERS, EDITOR
    return {
        "screenshot_01_overview": slide(
            "Overview", "game menus, built and wired for you",
            "Main, pause and settings menus in standard Unity UI, connected to a script you own.",
            ["Build everything in one click", "Three looks, fully editable", "Keyboard and gamepad ready"],
            panel(r / "main_day.png") + window_html(e / "editor_build.png", 1790, 800, 520, 840, 0, 936)),
        "screenshot_02_build": slide(
            "Build UI", "three menus in one click",
            "Canvas, EventSystem, buttons and a GameManager script, set up and connected.",
            ["Every button wired to a method", "Picks the right input module", "Safe to run again"],
            panel(r / "pause_day.png") + window_html(e / "editor_hierarchy_menus.png", 1826, 640, 500, 760, 0, 628)),
        "screenshot_03_themes": slide(
            "Themes", "pick a look, or make your own",
            "One theme asset styles every menu. Switch it, then apply it in one click.",
            ["Soft Club, Soft Club Night and Ink", "Duplicate a theme to edit it", "Restyle menus you already built"],
            cascade([r / "main_ink.png", r / "main_night.png", r / "main_day.png"])),
        "screenshot_04_settings": slide(
            "Settings menu", "settings that already work",
            "Volume, fullscreen and quality change for real, and are saved between sessions.",
            ["No placeholder buttons", "Saved with PlayerPrefs", "Add your own rows"],
            panel(r / "settings_day.png")),
        "screenshot_05_ui_health": slide(
            "UI Health", "catches broken ui as you work",
            "Checks every UI in your scene, not only the menus UIPilot builds.",
            ["Clicks that call deleted methods", "Buttons hidden under invisible images", "Missing raycasters and EventSystems"],
            panel(BACKDROPS / "backdrop_night.png", "62% center", "brightness(0.85)")
            + window_html(e / "editor_health.png", 1266, 330, 900, 840, 0, 812)),
        "screenshot_06_fixes": slide(
            "One-click fixes", "fix it in one click. undo any time.",
            "Lamps in the Hierarchy and on the main toolbar show where to look.",
            ["Every fix is one Ctrl+Z", "Warns before Play and on build", "Quiet when everything is fine"],
            panel(BACKDROPS / "backdrop_night.png", "62% center", "brightness(0.85)")
            + window_html(e / "editor_hierarchy_lamps.png", 1290, 340, 700, 680, 0, 580)
            + toolbar_strip(1470, 1060)),
        "screenshot_07_handheld": slide(
            "Gamepad and Steam Deck", "ready for gamepads and handhelds",
            "Navigation and text size are checked against Steam Deck's 9 px minimum.",
            ["Focus shown with a clear frame", "Unreachable buttons flagged", "Small text caught early"],
            panel(r / "main_day_focus2.png") + window_html(e / "editor_health_checks.png", 1690, 860, 640, 840, 586, 764)),
        "screenshot_08_docs": slide(
            "Documentation", "a clear guide in the package",
            "A 14-page illustrated PDF, from setup to troubleshooting.",
            ["Quick start in about a minute", "Every object explained", "A troubleshooting table"],
            pages_visual()),
    }


# ── Key images ─────────────────────────────────────────────────────────────
def cover():
    w, h = 1950, 1300
    css = f"""
.inner {{ position: absolute; inset: 0; padding: 300px 64px 0 70px; }}
.title {{ display: flex; align-items: flex-start; gap: 20px; font-size: 104px; }}
.title .mark {{ margin-top: -22px; }}
.rule {{ margin-top: 48px; height: 2px; width: 600px; background: rgba(255,255,255,0.6); }}
.tag {{ margin-top: 52px; font-size: 44px; line-height: 1.34; }}
.foot {{ position: absolute; left: 70px; bottom: 64px; font-size: 22px; color: {TEXT_SOFT}; }}
"""
    inner = (f'<div class="inner"><div class="title word">{WORDMARK}{mark(92)}</div><div class="rule"></div>'
             f'<div class="tag">{TAGLINE}</div></div><div class="foot small">{PUBLISHER}</div>')
    body = (atmosphere(w, h, h * 0.78, [(830, 90, 1000, 1110, "faint"), (1300, 930, 580, 300, "")])
            + framed_image(900, 150, 960, 1000, RENDERS / "main_day.png")
            + glass(80, 80, 740, 1110, inner))
    return page(w, h, css, body)


def card():
    w, h = 420, 280
    css = f"""
.cross {{ display: none; }}
.inner {{ position: absolute; inset: 0; padding: 70px 18px 0 22px; }}
.title {{ font-size: 34px; }}
.rule {{ margin-top: 16px; height: 1px; width: 170px; background: rgba(255,255,255,0.6); }}
.foot {{ position: absolute; left: 22px; bottom: 20px; font-size: 11px; letter-spacing: 0.18em; color: {TEXT_SOFT}; }}
.glass, .frame, .echo {{ border-width: 1px; }}
"""
    inner = (f'<div class="inner"><div class="title word">{WORDMARK}</div><div class="rule"></div></div>'
             f'<div class="foot small">{PUBLISHER}</div>')
    body = (atmosphere(w, h, h * 0.8, [])
            + framed_image(262, 26, 138, 222, RENDERS / "main_day_wordless.png", "3% center", echo=6)
            + glass(14, 14, 228, 246, inner, echo=6))
    return page(w, h, css, body)


def icon():
    w = h = 160
    css = f"""
body {{ background:
    radial-gradient(ellipse 70% 60% at 85% 10%, rgba(210,247,236,0.45), transparent 70%),
    radial-gradient(ellipse 70% 70% at 10% 95%, rgba(179,166,247,0.45), transparent 70%),
    repeating-linear-gradient(0deg, rgba(255,255,255,0.06) 0 1px, transparent 1px 4px), #2343AE; }}
.edge {{ position: absolute; inset: 8px; border: 1px solid rgba(255,255,255,0.55); }}
.mark {{ position: absolute; left: 24px; top: 20px; filter: drop-shadow(0 0 6px rgba(255,255,255,0.55)); }}
"""
    return page(w, h, css, f'<div class="edge"></div>{mark(114)}')


def social():
    w, h = 1200, 630
    css = ".glass .mark { position: absolute; left: 34px; top: 30px; }"
    body = (atmosphere(w, h, h * 0.8, [(380, 40, 770, 560, "faint")])
            + framed_image(330, 64, 820, 480, RENDERS / "social_day_wordless.png", echo=12)
            + glass(60, 64, 210, 210, mark(140), echo=12))
    return page(w, h, css, body)


SIZES = {"key_cover": (1950, 1300), "key_card": (420, 280), "key_icon": (160, 160), "key_social": (1200, 630)}

if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    jobs = {name: (html, (W, H)) for name, html in slides().items()}
    jobs.update({"key_cover": (cover(), SIZES["key_cover"]), "key_card": (card(), SIZES["key_card"]),
                 "key_icon": (icon(), SIZES["key_icon"]), "key_social": (social(), SIZES["key_social"])})
    with open(OUT / "jobs.txt", "w", encoding="utf-8") as manifest:
        for name, (html, (w, h)) in jobs.items():
            (OUT / f"{name}.html").write_text(html, encoding="utf-8")
            manifest.write(f"{name} {w} {h}\n")
    print(f"wrote {len(jobs)} pages")
