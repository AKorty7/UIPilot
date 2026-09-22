"""Backdrops for the genre themes: the "game world" each genre menu is shown over in
renders and store images. Like backdrop.py, every scene keeps the left third calm for
the menu panel and puts its event on the right. Writes one HTML page per mood; headless
Edge turns each into a PNG (see README.md).

    python genre_backdrops.py <out_dir> [width height]

Moods: ember (Fantasy RPG), overworld (JRPG Window), pixel (Pixel Retro), space (Sci-Fi HUD).
"""
import math
import os
import random
import sys

W, H = (int(sys.argv[2]), int(sys.argv[3])) if len(sys.argv) > 3 else (3900, 2600)
S = W / 3900.0


def ridge(rng, y0, amp, rough, step, x0=-40, x1=None, lift=None, floor=None, flatten=None):
    """A mountain skyline as a closed path: midpoint-displaced, filled down to `floor`
    (the bottom of the frame by default). `lift(x)` raises the line where the scene
    wants high ground. `flatten` = (centre x, half width, y, feather) levels the ground
    there, so a building stands on it instead of hovering over the bumps."""
    x1 = W + 40 if x1 is None else x1
    floor = H + 10 if floor is None else floor
    n = 1
    pts = [y0, y0]
    spread = amp
    while (x1 - x0) / n > step:
        nxt = []
        for i in range(n):
            a, b = pts[i], pts[i + 1]
            nxt += [a, (a + b) / 2 + rng.uniform(-spread, spread)]
        nxt.append(pts[-1])
        pts, n, spread = nxt, n * 2, spread * rough
    d = []
    for i, y in enumerate(pts):
        x = x0 + (x1 - x0) * i / n
        if lift:
            y -= lift(x)
        if flatten:
            fx, half, level, feather = flatten
            t = min(1.0, max(0.0, 1 - (abs(x - fx) - half) / feather))
            t = t * t * (3 - 2 * t)
            y = y * (1 - t) + level * t
        d.append(f"{'M' if i == 0 else 'L'}{x:.1f} {y:.1f}")
    return " ".join(d) + f" L{x1} {floor} L{x0} {floor} Z"


# ── The landmark ─────────────────────────────────────────────────────────────
# Every scene with a building shows the same one, so the set reads as one world: a
# symmetrical castle. Units are abstract: x from the centre line, y up from the ground.
# The vector scenes scale it; the pixel scene draws one unit as one pixel.
#
# What makes it belong in a scene, and must hold for each one:
#   - it stands on ground levelled for it (ridge `flatten`, the pixel `plateau`);
#   - it takes the colour of the ground it stands on, a touch darker, never the
#     darkest thing in the frame: distance fades a building as much as its hill;
#   - only the edges that face the scene's light are lit, in the light's colour.
CASTLE_WALL = 13                                   # height of the curtain wall
CASTLE_BLOCKS = [                                  # (left, width, height), crenellated on top
    (-31, 62, CASTLE_WALL),                        # curtain wall
    (-9, 18, 31),                                  # keep
]
CASTLE_TOWERS = [                                  # (centre, half width, wall, roof, clear of other walls from)
    (-31, 3, 19, 8, 0), (31, 3, 19, 8, 0),                          # corner turrets
    (-17, 4, 27, 11, CASTLE_WALL), (17, 4, 27, 11, CASTLE_WALL),    # flanking towers
    (0, 3, 45, 14, 31),                                             # the spire, out of the keep
]
CASTLE_WINDOWS = [(-17, 20), (17, 20), (-5, 24), (5, 24), (0, 38), (-31, 13), (31, 13)]
CASTLE_GATE = (-2, 4, 6)                           # left, width, height
CASTLE_HALF_WIDTH = 36
CASTLE_HEIGHT = 59


def castle_svg(cx, ground, k, fill, lit, lit_side, window=None):
    """The castle at `k` pixels per unit. `lit` colours the edges facing the light
    (`lit_side`: -1 left, +1 right). `window` lights the windows and gate; None for day."""
    sink = 1.5                                     # a pixel or so into the ground, so no seam shows

    def box(left, top, width, height, colour, extra=""):
        return (f'<rect x="{cx + left*k:.1f}" y="{ground - top*k:.1f}" width="{width*k:.1f}" '
                f'height="{height*k:.1f}" fill="{colour}"{extra}/>')

    def poly(points, colour, extra=""):
        pts = " ".join(f"{cx + x*k:.1f},{ground - y*k:.1f}" for (x, y) in points)
        return f'<polygon points="{pts}" fill="{colour}"{extra}/>'

    body, light = [], []
    for (left, width, height) in CASTLE_BLOCKS:
        body.append(box(left, height, width, height + sink / k, fill))
        for m in range(left, left + width, 4):     # merlons: 2 on, 2 off, ending on one
            body.append(box(m, height + 2, 2, 2.2, fill))
        if height > CASTLE_WALL:                   # the wall's own ends are inside the turrets
            edge = left if lit_side < 0 else left + width - 0.8
            light.append(box(edge, height, 0.8, height - CASTLE_WALL, lit))
    for (c, half, wall, roof, clear) in CASTLE_TOWERS:
        body.append(box(c - half, wall, 2 * half, wall + sink / k, fill))
        eave = half + 1.5
        body.append(poly([(c - eave, wall), (c + eave, wall), (c, wall + roof)], fill))
        edge = c - half if lit_side < 0 else c + half - 0.8
        light.append(box(edge, wall, 0.8, wall - clear, lit))
        light.append(poly([(c + lit_side * eave, wall), (c, wall + roof), (c + lit_side * (eave - 1.4), wall)], lit))

    glow = []
    if window:
        for (x, y) in CASTLE_WINDOWS:
            glow.append(box(x - 0.65, y + 2.4, 1.3, 2.4, window, f' rx="{0.65*k:.1f}"'))
        gl, gw, gh = CASTLE_GATE
        r = gw / 2
        glow.append(f'<path d="M{cx + gl*k:.1f} {ground:.1f} V{ground - (gh - r)*k:.1f} '
                    f'A{r*k:.1f} {r*k:.1f} 0 0 1 {cx + (gl + gw)*k:.1f} {ground - (gh - r)*k:.1f} '
                    f'V{ground:.1f} Z" fill="{window}"/>')
    return ("<g>" + "".join(body) + f'<g opacity="0.75">{"".join(light)}</g>'
            + (f'<g filter="url(#blur2)" opacity="0.9">{"".join(glow)}</g>' if glow else "")
            + "".join(glow) + "</g>")


def castle_windows_svg(cx, ground, k, colour):
    """Only the castle's lit windows and gate, for a layer of their own."""
    parts = []
    for (x, y) in CASTLE_WINDOWS:
        parts.append(f'<rect x="{cx + (x - 0.65)*k:.1f}" y="{ground - (y + 2.4)*k:.1f}" width="{1.3*k:.1f}" '
                     f'height="{2.4*k:.1f}" rx="{0.65*k:.1f}" fill="{colour}"/>')
    gl, gw, gh = CASTLE_GATE
    r = gw / 2
    parts.append(f'<path d="M{cx + gl*k:.1f} {ground:.1f} V{ground - (gh - r)*k:.1f} '
                 f'A{r*k:.1f} {r*k:.1f} 0 0 1 {cx + (gl + gw)*k:.1f} {ground - (gh - r)*k:.1f} '
                 f'V{ground:.1f} Z" fill="{colour}"/>')
    glow = "".join(parts)
    return f'<g filter="url(#blur2)" opacity="0.9">{glow}</g>{glow}'


def castle_pixels(put, cx, ground, fill, lit, window):
    """The same castle, one unit to one pixel, lit from the right."""
    for (left, width, height) in CASTLE_BLOCKS:
        put(cx + left, ground - height, width, height, fill)
        for m in range(left, left + width, 4):
            put(cx + m, ground - height - 2, 2, 2, fill)
        if height > CASTLE_WALL:
            put(cx + left + width - 1, ground - height, 1, height - CASTLE_WALL, lit)
    for (c, half, wall, roof, clear) in CASTLE_TOWERS:
        put(cx + c - half, ground - wall, 2 * half, wall, fill)
        put(cx + c + half - 1, ground - wall, 1, wall - clear, lit)
        for i in range(roof):
            hw = max(1, round((half + 1) * (1 - i / roof)))
            put(cx + c - hw, ground - wall - 1 - i, 2 * hw, 1, fill)
            put(cx + c + hw - 1, ground - wall - 1 - i, 1, 1, lit)
    for (x, y) in CASTLE_WINDOWS:
        put(cx + x - 1, ground - y - 2, 2, 2, window)
    gl, gw, gh = CASTLE_GATE
    put(cx + gl, ground - gh + 1, gw, gh - 1, window)
    put(cx + gl + 1, ground - gh, gw - 2, 1, window)


def page(body, defs="", bg="#000"):
    return f"""<!doctype html><html><head><meta charset="utf-8"><style>
html,body{{margin:0;padding:0;background:{bg}}}svg{{display:block}}</style></head><body>
<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}">
<defs>{defs}</defs>{body}</svg></body></html>"""


def grain(opacity, seed=7):
    return (f'<filter id="grain" x="0" y="0" width="100%" height="100%"><feTurbulence type="fractalNoise" '
            f'baseFrequency="0.9" numOctaves="2" seed="{seed}"/><feColorMatrix type="saturate" values="0"/></filter>',
            f'<rect width="{W}" height="{H}" filter="url(#grain)" opacity="{opacity}" style="mix-blend-mode:overlay"/>')


# ── Ember: a valley at dusk, a castle on a crag, sparks rising ──────────────
def ember():
    rng = random.Random(4)
    hy = H * 0.62
    sun = (W * 0.73, H * 0.50)
    defs = f"""
<linearGradient id="sky" x1="0" y1="0" x2="0" y2="1">
  <stop offset="0" stop-color="#1C1230"/><stop offset="0.38" stop-color="#5A2A3E"/>
  <stop offset="0.62" stop-color="#C0583A"/><stop offset="0.80" stop-color="#F0A24E"/>
  <stop offset="1" stop-color="#FFD58A"/></linearGradient>
<radialGradient id="sun" cx="{sun[0]}" cy="{sun[1]}" r="{1300*S}" gradientUnits="userSpaceOnUse">
  <stop offset="0" stop-color="#FFF1C2" stop-opacity="1"/><stop offset="0.08" stop-color="#FFD27A" stop-opacity="0.9"/>
  <stop offset="0.35" stop-color="#F08A3C" stop-opacity="0.35"/><stop offset="1" stop-color="#F08A3C" stop-opacity="0"/></radialGradient>
<linearGradient id="mist" x1="0" y1="0" x2="0" y2="1">
  <stop offset="0" stop-color="#F7B267" stop-opacity="0"/><stop offset="0.5" stop-color="#F7B267" stop-opacity="0.45"/>
  <stop offset="1" stop-color="#F7B267" stop-opacity="0"/></linearGradient>
<filter id="soft"><feGaussianBlur stdDeviation="{6*S}"/></filter>
<filter id="blur2"><feGaussianBlur stdDeviation="{2.2*S}"/></filter>
<filter id="clouds" x="0" y="0" width="100%" height="100%">
  <feTurbulence type="fractalNoise" baseFrequency="{0.0009/S} {0.004/S}" numOctaves="5" seed="12"/>
  <feColorMatrix type="matrix" values="0 0 0 0 1  0 0 0 0 0.72  0 0 0 0 0.52  2.6 0 0 0 -1.25"/></filter>
"""
    g_defs, g_body = grain(0.10)
    defs += g_defs
    body = [f'<rect width="{W}" height="{H}" fill="url(#sky)"/>',
            f'<rect width="{W}" height="{H*0.66}" filter="url(#clouds)" opacity="0.55"/>',
            f'<rect width="{W}" height="{H}" fill="url(#sun)"/>',
            f'<circle cx="{sun[0]}" cy="{sun[1]}" r="{95*S}" fill="#FFF3CF" filter="url(#soft)"/>']

    # The castle's crag: broad shoulders rising to a top levelled for the walls.
    castle_x, castle_k = W * 0.84, 5.2 * S
    crag_rise = 380 * S
    crag_top = hy - 40 * S - crag_rise

    def crag(x):
        d = max(0.0, abs(x - castle_x) - 150 * S)
        f = max(0.0, 1 - d / (700 * S)) ** 1.7
        ledges = round(f * 6) / 6                  # rock breaks in steps, not in a curve
        rough = (16 * math.sin(x / (41 * S)) + 10 * math.sin(x / (19 * S) + 1.3) + 6 * math.sin(x / (8 * S) + 0.5)) * S
        return crag_rise * (0.6 * f + 0.4 * ledges) + rough * min(1.0, 4 * f)

    # Layers of ridges, far to near: lighter and warmer far away (aerial perspective).
    layers = [
        (hy - 160 * S, 120 * S, "#B55A45", 0.85, None, None),
        (hy - 40 * S, 170 * S, "#7E3A3C", 1.0, crag,
         (castle_x, CASTLE_HALF_WIDTH * castle_k + 14 * S, crag_top, 110 * S)),
        (hy + 150 * S, 140 * S, "#4A2233", 1.0, None, None),
        (hy + 420 * S, 200 * S, "#26121F", 1.0, lambda x: 180 * S * math.exp(-((x - W * 0.15) / (500 * S)) ** 2), None),
    ]
    for i, (y0, amp, col, op, lift, flatten) in enumerate(layers):
        body.append(f'<path d="{ridge(rng, y0, amp, 0.55, 14*S, lift=lift, flatten=flatten)}" fill="{col}" opacity="{op}"/>')
        if i == 1:
            # On its ridge and of its ridge: the same rock colour a shade down, the
            # sunward edges catching the light, then the sun's haze over both.
            body.append(castle_svg(castle_x, crag_top, castle_k, "#6E3138", "#F6A95E", -1, "#FFC774"))
            body.append(f'<rect width="{W}" height="{H}" fill="url(#sun)" opacity="0.30"/>')
        if i < 3:
            body.append(f'<rect x="0" y="{y0 - 60*S}" width="{W}" height="{260*S}" fill="url(#mist)" opacity="{0.55 - i*0.12}"/>')

    # Sparks drifting up on the right.
    sparks = []
    for _ in range(140):
        x = rng.uniform(W * 0.45, W)
        y = rng.uniform(H * 0.25, H)
        r = rng.uniform(1.2, 4.2) * S
        a = rng.uniform(0.35, 0.95)
        sparks.append(f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{r:.1f}" fill="#FFC872" opacity="{a:.2f}"/>')
    body.append('<g filter="url(#blur2)">' + "".join(sparks) + "</g>")
    body.append(g_body)
    return page("".join(body), defs, "#1C1230")


# ── Overworld: bright sky, towering clouds, sea, green hills, a far tower ────
def overworld():
    rng = random.Random(9)
    hy = H * 0.64
    defs = f"""
<linearGradient id="sky" x1="0" y1="0" x2="0" y2="1">
  <stop offset="0" stop-color="#2F6FE0"/><stop offset="0.55" stop-color="#6FA8F2"/>
  <stop offset="1" stop-color="#CFE7FA"/></linearGradient>
<linearGradient id="sea" x1="0" y1="0" x2="0" y2="1">
  <stop offset="0" stop-color="#7FB6E8"/><stop offset="1" stop-color="#2D6FC4"/></linearGradient>
<filter id="cloud" x="-20%" y="-20%" width="140%" height="140%">
  <feGaussianBlur stdDeviation="{10*S}"/></filter>
<filter id="cloudTex" x="0" y="0" width="100%" height="100%">
  <feTurbulence type="fractalNoise" baseFrequency="{0.0008/S} {0.006/S}" numOctaves="4" seed="3"/>
  <feColorMatrix type="matrix" values="0 0 0 0 1  0 0 0 0 1  0 0 0 0 1  2.2 0 0 0 -1.05"/></filter>
<filter id="haze"><feGaussianBlur stdDeviation="{1.6*S}"/></filter>
"""
    g_defs, g_body = grain(0.06, 11)
    defs += g_defs
    body = [f'<rect width="{W}" height="{H}" fill="url(#sky)"/>',
            f'<rect width="{W}" height="{H*0.5}" filter="url(#cloudTex)" opacity="0.18"/>']

    # Cumulus: clusters of lit discs over shadowed discs.
    def cumulus(cx, cy, scale, n):
        """A towering cloud: puffs stacked into a dome over a flat, shadowed base,
        lit from the upper right."""
        shade, lit, core = [], [], []
        for _ in range(n):
            u = rng.uniform(-1, 1)
            dx = u * 560 * scale
            top = (1 - u * u) * 420 * scale
            dy = -rng.uniform(0, top)
            r = rng.uniform(90, 170) * scale * (1.1 - 0.5 * abs(u))
            shade.append(f'<circle cx="{cx+dx:.0f}" cy="{cy+dy+22*scale:.0f}" r="{r:.0f}" fill="#9FB8DE"/>')
            lit.append(f'<circle cx="{cx+dx+14*scale:.0f}" cy="{cy+dy-12*scale:.0f}" r="{r*0.9:.0f}" fill="#FFFFFF"/>')
            core.append(f'<circle cx="{cx+dx+30*scale:.0f}" cy="{cy+dy-30*scale:.0f}" r="{r*0.55:.0f}" fill="#FFFFFF"/>')
        cid = f"cb{int(cx)}"
        clip = (f'<clipPath id="{cid}"><rect x="{cx-900*scale:.0f}" y="{cy-900*scale:.0f}" '
                f'width="{1800*scale:.0f}" height="{1000*scale:.0f}"/></clipPath>')
        return (f'{clip}<g clip-path="url(#{cid})"><g filter="url(#cloud)">{"".join(shade)}{"".join(lit)}'
                f'<g opacity="0.7">{"".join(core)}</g></g></g>')

    body.append(cumulus(W * 0.78, H * 0.52, 1.35 * S, 60))
    body.append(cumulus(W * 0.45, H * 0.60, 0.65 * S, 34))
    body.append(cumulus(W * 0.16, H * 0.30, 0.45 * S, 24))
    body.append(f'<rect x="0" y="{hy}" width="{W}" height="{H-hy}" fill="url(#sea)"/>')

    # A far island: cliffs up to a level top, the castle on it in the island's own
    # hazy blue, a crystal floating over the spire. Lit from the upper right, like the clouds.
    ix, ik = W * 0.80, 4.2 * S
    island_rise = 120 * S
    island_top = hy + 2 * S - island_rise

    def cliffs(x):
        d = max(0.0, abs(x - ix) - 210 * S)
        cliff = island_rise * max(0.0, 1 - d / (130 * S)) ** 0.5
        shore = 30 * S * max(0.0, 1 - d / (430 * S)) ** 1.3
        return max(cliff, shore) + 5 * S * math.sin(x / (23 * S)) * min(1.0, cliff / island_rise * 3)

    shore = ridge(rng, hy + 2 * S, 10 * S, 0.5, 12 * S, x0=W * 0.62, x1=W * 0.98, floor=hy + 6 * S,
                  lift=cliffs, flatten=(ix, CASTLE_HALF_WIDTH * ik + 30 * S, island_top, 90 * S))
    island = (f'<g filter="url(#haze)"><path d="{shore}" fill="#6D93C4"/>'
              + castle_svg(ix, island_top, ik, "#5C82B7", "#DCEBFA", 1) + "</g>")
    # Its reflection first, so the glitter lies over it.
    body.append(f'<g transform="translate(0 {2*hy:.1f}) scale(1 -1)" opacity="0.20">{island}</g>')
    # Sun glitter on the sea.
    for _ in range(260):
        x = rng.gauss(W * 0.78, 340 * S)
        y = rng.uniform(hy + 8 * S, hy + 360 * S)
        w = rng.uniform(14, 60) * S * (1 + (y - hy) / (300 * S))
        body.append(f'<rect x="{x:.0f}" y="{y:.0f}" width="{w:.0f}" height="{2.4*S:.1f}" fill="#FFFFFF" opacity="{rng.uniform(0.3,0.8):.2f}"/>')
    body.append(island)
    gx, gy = ix, island_top - CASTLE_HEIGHT * ik - 62 * S          # the crystal's centre
    body.append(f'<circle cx="{gx}" cy="{gy}" r="{64*S}" fill="#7FDCF5" opacity="0.45" filter="url(#cloud)"/>')
    body.append(f'<polygon points="{gx},{gy-46*S} {gx+19*S},{gy} {gx},{gy+46*S} {gx-19*S},{gy}" fill="#4FBFE6"/>')
    body.append(f'<polygon points="{gx},{gy-46*S} {gx+19*S},{gy} {gx},{gy+46*S}" fill="#C4F2FF"/>')
    # Near hills.
    body.append(f'<path d="{ridge(rng, H*0.86, 90*S, 0.5, 16*S, lift=lambda x: 180*S*math.exp(-((x-W*0.08)/(700*S))**2))}" fill="#3E9A52"/>')
    body.append(f'<path d="{ridge(rng, H*0.93, 60*S, 0.5, 16*S)}" fill="#2B7A3E"/>')
    body.append(g_body)
    return page("".join(body), defs, "#2F6FE0")


# ── Pixel: a 300x200 pixel night, drawn on the grid and scaled up ────────────
def pixel():
    rng = random.Random(21)
    gw, gh = 300, 200
    px = W / gw
    rects = []

    def put(x, y, w, h, col):
        rects.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="{col}"/>')

    # Banded sky, dithered at each band edge.
    bands = ["#0B0C24", "#131438", "#1D1D4C", "#2A2860", "#3B3474", "#5A4486", "#835193"]
    band_h = 16
    for i, col in enumerate(bands):
        put(0, i * band_h, gw, band_h, col)
        if i + 1 < len(bands):
            nxt = bands[i + 1]
            for x in range(0, gw, 2):
                put(x + (i % 2), (i + 1) * band_h - 1, 1, 1, nxt)
                put(x + ((i + 1) % 2), (i + 1) * band_h, 1, 1, col)
    put(0, len(bands) * band_h, gw, gh, bands[-1])
    # Stars.
    for _ in range(90):
        x, y = rng.randrange(gw), rng.randrange(0, 80)
        put(x, y, 1, 1, rng.choice(["#FFFFFF", "#F8D030", "#A8C8FF"]))
    for _ in range(6):
        x, y = rng.randrange(120, gw - 4), rng.randrange(6, 60)
        put(x, y - 1, 1, 3, "#FFFFFF"); put(x - 1, y, 3, 1, "#FFFFFF")
    # Moon with craters.
    mx, my, mr = 238, 38, 14
    for y in range(-mr, mr + 1):
        for x in range(-mr, mr + 1):
            if x * x + y * y <= mr * mr:
                col = "#F4F1D8" if (x + 5) ** 2 + (y + 4) ** 2 > 16 and (x - 5) ** 2 + (y - 6) ** 2 > 9 else "#D8D2B0"
                put(mx + x, my + y, 1, 1, col)

    def skyline(y0, amp, col, seed, step=3, plateau=None):
        """A band of hills. `plateau` = (centre x, half width, y) raises a mound with a
        level top there, for the castle to stand on."""
        r = random.Random(seed)
        y = y0
        for x in range(0, gw, step):
            y = max(y0 - amp, min(y0 + amp, y + r.choice([-2, -1, 0, 0, 1, 2])))
            top = y
            if plateau:
                centre, half, level = plateau
                top = min(y, level + int(max(0, abs(x + step / 2 - centre) - half) * 0.6))
            put(x, top, step, gh - top, col)

    # The castle stands on the far hills, in their colour a shade down, its right
    # edges lit by the moon. The near hills and the pines pass in front of its foot.
    cx, ground = 214, 126
    skyline(130, 18, "#2A2456", 3, plateau=(cx, CASTLE_HALF_WIDTH + 2, ground))
    castle_pixels(put, cx, ground, "#221C49", "#4A4186", "#F8D030")
    skyline(146, 12, "#1E1A42", 5, 2)
    # Pines in front.
    r = random.Random(8)
    for x in range(0, gw, 7):
        h = r.randrange(14, 26)
        base = 176 + r.randrange(-3, 3)
        for k in range(h):
            w = max(1, (k * 5) // h)
            put(x + 3 - w, base - h + k, 2 * w + 1, 1, "#0C1A22")
        put(x + 3, base, 1, 4, "#0C1A22")
    put(0, 178, gw, gh - 178, "#09121A")
    # Fireflies.
    for _ in range(18):
        put(rng.randrange(150, gw), rng.randrange(150, 196), 1, 1, "#C8F070")

    body = (f'<g transform="scale({px:.4f})" shape-rendering="crispEdges">' + "".join(rects) + "</g>")
    return page(body, "", "#0B0C24")


# ── Space: a planet's limb, a ring, a nebula, a station catching the light ───
def space():
    rng = random.Random(33)
    pc = (W * 0.86, H * 0.90)
    pr = 760 * S
    defs = f"""
<radialGradient id="deep" cx="{W*0.7}" cy="{H*0.4}" r="{W*0.9}" gradientUnits="userSpaceOnUse">
  <stop offset="0" stop-color="#0A1E33"/><stop offset="1" stop-color="#01040A"/></radialGradient>
<filter id="nebula" x="0" y="0" width="100%" height="100%">
  <feTurbulence type="fractalNoise" baseFrequency="{0.0011/S}" numOctaves="6" seed="41"/>
  <feColorMatrix type="matrix" values="0 0 0 0 0.12  0 0 0 0 0.62  0 0 0 0 0.78  2.4 0 0 0 -1.15"/></filter>
<filter id="nebula2" x="0" y="0" width="100%" height="100%">
  <feTurbulence type="fractalNoise" baseFrequency="{0.0017/S}" numOctaves="5" seed="7"/>
  <feColorMatrix type="matrix" values="0 0 0 0 0.45  0 0 0 0 0.20  0 0 0 0 0.62  2.6 0 0 0 -1.35"/></filter>
<radialGradient id="planet" cx="{pc[0]-700*S}" cy="{pc[1]-900*S}" r="{pr*1.5}" gradientUnits="userSpaceOnUse">
  <stop offset="0" stop-color="#3C7C8C"/><stop offset="0.45" stop-color="#123447"/><stop offset="1" stop-color="#02070D"/></radialGradient>
<radialGradient id="atmo" cx="{pc[0]}" cy="{pc[1]}" r="{pr*1.06}" gradientUnits="userSpaceOnUse">
  <stop offset="0.93" stop-color="#6FF0FF" stop-opacity="0"/><stop offset="0.965" stop-color="#6FF0FF" stop-opacity="0.55"/>
  <stop offset="1" stop-color="#6FF0FF" stop-opacity="0"/></radialGradient>
<filter id="bands" x="0" y="0" width="100%" height="100%">
  <feTurbulence type="fractalNoise" baseFrequency="{0.0006/S} {0.012/S}" numOctaves="3" seed="5"/>
  <feColorMatrix type="matrix" values="0 0 0 0 0.6  0 0 0 0 0.9  0 0 0 0 1  1.8 0 0 0 -0.85"/></filter>
<clipPath id="pclip"><circle cx="{pc[0]}" cy="{pc[1]}" r="{pr}"/></clipPath>
<filter id="glow"><feGaussianBlur stdDeviation="{3*S}"/></filter>
"""
    g_defs, g_body = grain(0.08, 17)
    defs += g_defs
    body = [f'<rect width="{W}" height="{H}" fill="url(#deep)"/>',
            f'<rect width="{W}" height="{H}" filter="url(#nebula)" opacity="0.55"/>',
            f'<rect width="{W}" height="{H}" filter="url(#nebula2)" opacity="0.35"/>']
    stars = []
    for _ in range(900):
        x, y = rng.uniform(0, W), rng.uniform(0, H)
        r = rng.choice([0.7, 0.9, 1.1, 1.4, 2.0]) * S * 1.6
        stars.append(f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{r:.1f}" fill="#DDF8FF" opacity="{rng.uniform(0.3,1):.2f}"/>')
    body.append("".join(stars))
    for _ in range(9):
        x, y = rng.uniform(W * 0.4, W), rng.uniform(0, H * 0.6)
        body.append(f'<g opacity="0.9"><circle cx="{x:.0f}" cy="{y:.0f}" r="{10*S:.1f}" fill="#9AF6FF" filter="url(#glow)"/>'
                    f'<rect x="{x-60*S:.0f}" y="{y-1*S:.1f}" width="{120*S:.0f}" height="{2*S:.1f}" fill="#DDF8FF" opacity="0.6"/>'
                    f'<rect x="{x-1*S:.1f}" y="{y-60*S:.0f}" width="{2*S:.1f}" height="{120*S:.0f}" fill="#DDF8FF" opacity="0.6"/></g>')
    # Ring behind the planet (far half), the planet, then the ring's near half.
    def ring(far):
        rot = f'transform="rotate(-14 {pc[0]} {pc[1]})"'
        return (f'<ellipse cx="{pc[0]}" cy="{pc[1]}" rx="{pr*1.9}" ry="{pr*0.36}" {rot} fill="none" '
                f'stroke="#8FE9F5" stroke-opacity="{0.30 if far else 0.62}" stroke-width="{44*S}"/>'
                f'<ellipse cx="{pc[0]}" cy="{pc[1]}" rx="{pr*2.05}" ry="{pr*0.40}" {rot} fill="none" '
                f'stroke="#8FE9F5" stroke-opacity="{0.14 if far else 0.32}" stroke-width="{14*S}"/>')
    body.append(ring(True))
    body.append(f'<circle cx="{pc[0]}" cy="{pc[1]}" r="{pr}" fill="url(#planet)"/>')
    body.append(f'<g clip-path="url(#pclip)"><rect width="{W}" height="{H}" filter="url(#bands)" opacity="0.25"/></g>')
    body.append(f'<circle cx="{pc[0]}" cy="{pc[1]}" r="{pr*1.06}" fill="url(#atmo)"/>')
    body.append(f'<g clip-path="url(#nearHalf)">{ring(False)}</g>')
    defs_near = (f'<clipPath id="nearHalf"><rect x="{-W}" y="{pc[1]}" width="{W*3}" height="{H*2}" '
                 f'transform="rotate(-14 {pc[0]} {pc[1]})"/></clipPath>')
    # A ring station in the planet's light. Solid, not outlined: it is lit from the upper
    # left like the planet, and fades to the colour of the space behind it in shadow.
    sx, sy, s = W * 0.755, H * 0.40, 0.78 * S
    station_defs = f"""
<linearGradient id="hull" x1="0" y1="0" x2="1" y2="1">
  <stop offset="0" stop-color="#CFE6EC"/><stop offset="0.35" stop-color="#5E8797"/>
  <stop offset="1" stop-color="#0B1A27"/></linearGradient>
<linearGradient id="array" x1="0" y1="0" x2="1" y2="1">
  <stop offset="0" stop-color="#2C6F94"/><stop offset="1" stop-color="#081C30"/></linearGradient>
<clipPath id="ringNear"><rect x="{sx-160*s}" y="{sy}" width="{320*s}" height="{80*s}"/></clipPath>
"""
    ring_band = (f'<ellipse cx="{sx}" cy="{sy}" rx="{112*s}" ry="{34*s}" fill="none" stroke="url(#hull)" stroke-width="{13*s}"/>'
                 f'<ellipse cx="{sx}" cy="{sy - 3*s}" rx="{112*s}" ry="{34*s}" fill="none" stroke="#E6F8FC" '
                 f'stroke-opacity="0.45" stroke-width="{1.6*s}"/>')
    st = [f'<g transform="rotate(-16 {sx:.0f} {sy:.0f})">']
    st.append(f'<rect x="{sx-300*s}" y="{sy-3.5*s}" width="{600*s}" height="{7*s}" fill="url(#hull)"/>')       # truss
    for side in (-1, 1):                                                                                          # solar arrays
        ax = sx + side * 150 * s - (150 * s if side < 0 else 0)
        st.append(f'<rect x="{ax}" y="{sy-44*s}" width="{150*s}" height="{88*s}" fill="url(#array)"/>')
        for i in range(1, 5):
            st.append(f'<rect x="{ax + i*30*s - 0.8*s}" y="{sy-44*s}" width="{1.6*s}" height="{88*s}" fill="#7FD9F0" opacity="0.35"/>')
        st.append(f'<rect x="{ax}" y="{sy-0.8*s}" width="{150*s}" height="{1.6*s}" fill="#7FD9F0" opacity="0.35"/>')
        st.append(f'<rect x="{ax}" y="{sy-44*s}" width="{150*s}" height="{1.6*s}" fill="#BFEFFA" opacity="0.6"/>')
    st.append(ring_band)                                                                                          # ring, far side
    st.append(f'<rect x="{sx-30*s}" y="{sy-64*s}" width="{60*s}" height="{128*s}" rx="{16*s}" fill="url(#hull)"/>')  # hub
    st.append(f'<rect x="{sx-20*s}" y="{sy-84*s}" width="{40*s}" height="{24*s}" rx="{6*s}" fill="url(#hull)"/>')     # dock
    st.append(f'<g clip-path="url(#ringNear)">{ring_band}</g>')                                                   # ring, near side
    for (wx, wy) in [(-12, -30), (0, -30), (12, -30), (-12, 26), (0, 26)]:
        st.append(f'<rect x="{sx + wx*s - 2.5*s}" y="{sy + wy*s}" width="{5*s}" height="{3*s}" fill="#FFE2A8" opacity="0.9"/>')
    st.append(f'<circle cx="{sx}" cy="{sy-92*s}" r="{7*s}" fill="#FF5A5A" filter="url(#glow)"/>')
    st.append(f'<circle cx="{sx}" cy="{sy-92*s}" r="{2.4*s}" fill="#FFD0D0"/>')
    st.append('</g>')
    body.append("".join(st))
    defs += station_defs
    body.append(g_body)
    return page("".join(body), defs + defs_near, "#01040A")


MOODS = {"ember": ember, "overworld": overworld, "pixel": pixel, "space": space}

if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else "."
    os.makedirs(out, exist_ok=True)
    for name, make in MOODS.items():
        path = os.path.join(out, f"backdrop_{name}.html")
        with open(path, "w", encoding="utf-8") as f:
            f.write(make())
        print(path)
