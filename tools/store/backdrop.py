"""Soft Signal backdrops: the "game world" the UIPilot menus are shown over in the
store images. One geometry (a measured floor, six pearls, one ring), three lights
(day, night, ink). Writes an HTML page per mood; headless Edge turns each into a PNG.

    python backdrop.py <out_dir> [width height]
"""
import math
import os
import random
import sys

W, H = (int(sys.argv[2]), int(sys.argv[3])) if len(sys.argv) > 3 else (3900, 2600)
S = W / 3900.0                      # every size below is authored at 3900 wide

HY = round(H * 0.60)                # horizon
VX = round(W * 0.64)                # vanishing point: the right of the frame holds the event
F = 1400.0 * S                      # focal length in pixels
CAM_H = 1.6                         # eye height, world units

MOODS = {
    "day": dict(
        sky=[(0, "#8B9FEA"), (0.52, "#B6CCF6"), (1, "#E1F3F2")],
        floor=[(0, "#D9EEF1"), (0.35, "#BCD2F4"), (1, "#8FA4E6")],
        grid="#FFFFFF", grid_a=0.62, grid_glow=0,
        haze="#FFFFFF", haze_a=0.85,
        light=(0.80, 0.24), light_col="#FFFFFF", light_a=(0.62, 0.85, 1.0),
        pearl=("#FFFFFF", "#D3DDF8", "#8497DC"), rim="#FFFFFF", rim_a=0.55,
        shadow="#5468C0", shadow_a=0.30, ring=("#FFFFFF", "#C9D6F8"), ring_glow=0,
        frame="#FFFFFF", stars=False, grain=0.075),
    "night": dict(
        sky=[(0, "#05081F"), (0.55, "#141A55"), (1, "#27458C")],
        floor=[(0, "#1B2A66"), (0.4, "#0C1240"), (1, "#050821")],
        grid="#6FE6FF", grid_a=0.78, grid_glow=3.2,
        haze="#56CFF2", haze_a=0.55,
        light=(0.83, 0.20), light_col="#9C7CFF", light_a=(0.50, 0.55, 0.9),
        pearl=("#B9C8FF", "#25317A", "#080C2E"), rim="#6FE6FF", rim_a=0.85,
        shadow="#000000", shadow_a=0.55, ring=("#6FE6FF", "#A06BFF"), ring_glow=6,
        frame="#E8F6FF", stars=True, grain=0.09),
    "ink": dict(
        sky=[(0, "#121826"), (0.55, "#1F2940"), (1, "#3F4E69")],
        floor=[(0, "#34425C"), (0.4, "#1C2436"), (1, "#0D111B")],
        grid="#A9B8CF", grid_a=0.32, grid_glow=0,
        haze="#1AA7A9", haze_a=0.55,
        light=(0.80, 0.22), light_col="#8C9DBE", light_a=(0.22, 0.25, 0.4),
        pearl=("#D2DAE6", "#56647E", "#161C2A"), rim="#1AA7A9", rim_a=0.55,
        shadow="#000000", shadow_a=0.45, ring=("#1AA7A9", "#56647E"), ring_glow=0,
        frame="#F3F5F8", stars=False, grain=0.08),
}

# The world: six pearls and one ring, spaced so no two silhouettes touch.
# (X, Z, radius) in world units; the ring frames the far pearl on its left.
PEARLS = [(4.25, 4.6, 1.12), (0.60, 6.4, 0.82), (2.90, 8.4, 0.52), (-1.40, 15.0, 0.60),
          (-3.20, 20.0, 0.45), (9.50, 18.0, 0.58)]
SELECTED = 2                                   # the one with the focus frame
RING = (-5.0, 40.0, 6.2, 0.62)                 # X, Z, radius, tube


def ground_y(z):
    return HY + F * CAM_H / z


def to_screen(x, y, z):
    return VX + F * x / z, HY + F * (CAM_H - y) / z


def stops(pairs, alpha=1.0):
    return "".join(f'<stop offset="{o}" stop-color="{c}" stop-opacity="{alpha}"/>' for o, c in pairs)


def defs(m):
    lx, ly = m["light"]
    a0, a1, a2 = m["light_a"]
    return f"""
<defs>
  <linearGradient id="sky" x1="0" y1="0" x2="0" y2="1">{stops(m["sky"])}</linearGradient>
  <linearGradient id="floor" x1="0" y1="0" x2="0" y2="1">{stops(m["floor"])}</linearGradient>
  <linearGradient id="haze" x1="0" y1="0" x2="0" y2="1">
    <stop offset="0" stop-color="{m["haze"]}" stop-opacity="0"/>
    <stop offset="0.5" stop-color="{m["haze"]}" stop-opacity="{m["haze_a"]}"/>
    <stop offset="1" stop-color="{m["haze"]}" stop-opacity="0"/>
  </linearGradient>
  <linearGradient id="fade" x1="0" y1="{HY}" x2="0" y2="{H}" gradientUnits="userSpaceOnUse">
    <stop offset="0" stop-color="#fff" stop-opacity="0"/>
    <stop offset="0.10" stop-color="#fff" stop-opacity="0.25"/>
    <stop offset="0.55" stop-color="#fff" stop-opacity="0.85"/>
    <stop offset="1" stop-color="#fff" stop-opacity="1"/>
  </linearGradient>
  <mask id="gridmask"><rect x="0" y="{HY}" width="{W}" height="{H - HY}" fill="url(#fade)"/></mask>
  <radialGradient id="light-wide" cx="{lx}" cy="{ly}" r="0.55" gradientUnits="objectBoundingBox" gradientTransform="translate({lx},{ly}) scale(1,{W / H}) translate({-lx},{-ly})">
    <stop offset="0" stop-color="{m["light_col"]}" stop-opacity="{a0}"/>
    <stop offset="0.45" stop-color="{m["light_col"]}" stop-opacity="{a0 * 0.35}"/>
    <stop offset="1" stop-color="{m["light_col"]}" stop-opacity="0"/>
  </radialGradient>
  <radialGradient id="light-core" cx="0.5" cy="0.5" r="0.5">
    <stop offset="0" stop-color="#FFFFFF" stop-opacity="{a2}"/>
    <stop offset="0.25" stop-color="{m["light_col"]}" stop-opacity="{a1}"/>
    <stop offset="1" stop-color="{m["light_col"]}" stop-opacity="0"/>
  </radialGradient>
  <radialGradient id="pearl" cx="0.62" cy="0.30" r="0.78" fx="0.66" fy="0.24">
    <stop offset="0" stop-color="{m["pearl"][0]}"/>
    <stop offset="0.42" stop-color="{m["pearl"][1]}"/>
    <stop offset="1" stop-color="{m["pearl"][2]}"/>
  </radialGradient>
  <linearGradient id="ring" x1="0" y1="0" x2="1" y2="1">
    <stop offset="0" stop-color="{m["ring"][0]}"/>
    <stop offset="1" stop-color="{m["ring"][1]}"/>
  </linearGradient>
  <filter id="soft" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="{18 * S}"/></filter>
  <filter id="softer" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="{60 * S}"/></filter>
  <filter id="glow" x="-20%" y="-20%" width="140%" height="140%">
    <feGaussianBlur stdDeviation="{max(m["grid_glow"], 0.01) * S}" result="b"/>
    <feMerge><feMergeNode in="b"/><feMergeNode in="SourceGraphic"/></feMerge>
  </filter>
  <filter id="ringglow" x="-30%" y="-30%" width="160%" height="160%">
    <feGaussianBlur stdDeviation="{max(m["ring_glow"], 0.01) * S}" result="b"/>
    <feMerge><feMergeNode in="b"/><feMergeNode in="b"/><feMergeNode in="SourceGraphic"/></feMerge>
  </filter>
  <filter id="grain" x="0" y="0" width="100%" height="100%">
    <feTurbulence type="fractalNoise" baseFrequency="{0.85 / S}" numOctaves="3" seed="11" stitchTiles="stitch"/>
    <feColorMatrix type="saturate" values="0"/>
  </filter>
  <clipPath id="above-ground"><rect x="0" y="0" width="{W}" height="{round(ground_y(RING[1]))}"/></clipPath>
</defs>"""


def sky(m):
    lx, ly = m["light"]
    core = 520 * S
    out = [f'<rect width="{W}" height="{HY + 2}" fill="url(#sky)"/>',
           f'<rect width="{W}" height="{H}" fill="url(#light-wide)"/>',
           f'<circle cx="{lx * W}" cy="{ly * H}" r="{core}" fill="url(#light-core)"/>']
    if m["stars"]:
        out.append(stars())
    return "\n".join(out)


# A measured field, not a scatter: stars on a jittered lattice, dimmer toward the horizon.
def stars():
    rnd = random.Random(20260919)
    dots = []
    step = 118 * S
    y = 40 * S
    while y < HY - 60 * S:
        x = (rnd.random() * step)
        while x < W:
            jx, jy = rnd.uniform(-0.45, 0.45) * step, rnd.uniform(-0.45, 0.45) * step
            depth = 1 - (y / HY)
            if rnd.random() < 0.55:
                r = (0.9 + rnd.random() ** 3 * 2.6) * S
                a = (0.25 + 0.75 * rnd.random()) * (0.35 + 0.65 * depth)
                dots.append(f'<circle cx="{x + jx:.1f}" cy="{y + jy:.1f}" r="{r:.2f}" fill="#E8F2FF" opacity="{a:.2f}"/>')
            x += step
        y += step
    # four glints, the brightest points of the sky
    for gx, gy in [(0.56, 0.12), (0.72, 0.38), (0.93, 0.09), (0.47, 0.31)]:
        cx, cy, L = gx * W, gy * H, 26 * S
        dots.append(f'<g stroke="#FFFFFF" stroke-width="{1.6 * S}" opacity="0.9">'
                    f'<line x1="{cx - L}" y1="{cy}" x2="{cx + L}" y2="{cy}"/>'
                    f'<line x1="{cx}" y1="{cy - L}" x2="{cx}" y2="{cy + L}"/></g>'
                    f'<circle cx="{cx}" cy="{cy}" r="{2.4 * S}" fill="#FFFFFF"/>')
    return "\n".join(dots)


def ring(m):
    x, z, radius, tube = RING
    cx, cy = to_screen(x, radius * 0.78, z)
    r = F * radius / z
    w = F * tube / z
    return (f'<g clip-path="url(#above-ground)" filter="url(#ringglow)">'
            f'<circle cx="{cx:.1f}" cy="{cy:.1f}" r="{r:.1f}" fill="none" stroke="url(#ring)" stroke-width="{w:.1f}"/>'
            f'<circle cx="{cx:.1f}" cy="{cy:.1f}" r="{r - w * 0.18:.1f}" fill="none" stroke="#FFFFFF" stroke-opacity="0.55" stroke-width="{1.4 * S}"/>'
            f'</g>')


def floor(m):
    out = [f'<rect x="0" y="{HY}" width="{W}" height="{H - HY}" fill="url(#floor)"/>',
           f'<rect x="0" y="{HY - 90 * S}" width="{W}" height="{180 * S}" fill="url(#haze)"/>']
    return "\n".join(out)


# The measuring ground: ranks every world unit, files every world unit, all hairline.
def grid(m):
    lines = []
    sw = 2.0 * S
    z = 2.0
    while z < 140:
        y = ground_y(z)
        if y < H:
            lines.append(f'<line x1="0" y1="{y:.2f}" x2="{W}" y2="{y:.2f}"/>')
        z += 1.0
    z_bottom = F * CAM_H / (H - HY)
    for k in range(-80, 81):
        xb = VX + F * k / z_bottom
        lines.append(f'<line x1="{xb:.2f}" y1="{H}" x2="{VX}" y2="{HY}"/>')
    glow = ' filter="url(#glow)"' if m["grid_glow"] else ""
    return (f'<g mask="url(#gridmask)" stroke="{m["grid"]}" stroke-opacity="{m["grid_a"]}" '
            f'stroke-width="{sw:.2f}" shape-rendering="geometricPrecision"{glow}>' + "".join(lines) + "</g>")


def pearls(m):
    out = []
    for i, (x, z, radius) in sorted(enumerate(PEARLS), key=lambda p: -p[1][1]):
        gy = ground_y(z)
        cx = VX + F * x / z
        r = F * radius / z
        cy = gy - r
        out.append(f'<ellipse cx="{cx:.1f}" cy="{gy:.1f}" rx="{r * 1.05:.1f}" ry="{r * 0.2:.1f}" '
                   f'fill="{m["shadow"]}" opacity="{m["shadow_a"]}" filter="url(#soft)"/>')
        out.append(f'<circle cx="{cx:.1f}" cy="{cy:.1f}" r="{r:.1f}" fill="url(#pearl)"/>')
        out.append(f'<circle cx="{cx:.1f}" cy="{cy:.1f}" r="{r - 1.2 * S:.1f}" fill="none" stroke="{m["rim"]}" '
                   f'stroke-opacity="{m["rim_a"]}" stroke-width="{2.2 * S:.2f}"/>')
        if i == SELECTED:
            out.append(focus_frame(cx, cy, r, m))
    return "\n".join(out)


# UIPilot's focus frame, in the world: a square hairline with crosshair corners.
def focus_frame(cx, cy, r, m):
    pad = 26 * S
    half = r + pad
    x0, y0, x1, y1 = cx - half, cy - half, cx + half, cy + half
    arm = 18 * S
    sw = 2.2 * S
    marks = []
    for px, py in [(x0, y0), (x1, y0), (x0, y1), (x1, y1)]:
        marks.append(f'<line x1="{px - arm:.1f}" y1="{py:.1f}" x2="{px + arm:.1f}" y2="{py:.1f}"/>'
                     f'<line x1="{px:.1f}" y1="{py - arm:.1f}" x2="{px:.1f}" y2="{py + arm:.1f}"/>')
    return (f'<g stroke="{m["frame"]}" fill="none" stroke-width="{sw:.2f}">'
            f'<rect x="{x0:.1f}" y="{y0:.1f}" width="{half * 2:.1f}" height="{half * 2:.1f}" stroke-opacity="0.95"/>'
            f'<rect x="{x0 + 10 * S:.1f}" y="{y0 + 10 * S:.1f}" width="{half * 2:.1f}" height="{half * 2:.1f}" stroke-opacity="0.35"/>'
            + "".join(marks) + "</g>")


def grain(m):
    return (f'<rect width="{W}" height="{H}" filter="url(#grain)" '
            f'style="mix-blend-mode:overlay" opacity="{m["grain"]}"/>')


def page(mood):
    m = MOODS[mood]
    svg = "\n".join([
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}">',
        defs(m), sky(m), ring(m), floor(m), grid(m), pearls(m), grain(m), "</svg>"])
    return ("<!doctype html><html><head><meta charset='utf-8'><style>html,body{margin:0;padding:0;"
            f"width:{W}px;height:{H}px;overflow:hidden;background:#000}}svg{{display:block}}</style></head><body>"
            + svg + "</body></html>")


if __name__ == "__main__":
    out = sys.argv[1]
    os.makedirs(out, exist_ok=True)
    for mood in MOODS:
        with open(os.path.join(out, f"backdrop_{mood}.html"), "w", encoding="utf-8") as fh:
            fh.write(page(mood))
    print("wrote", ", ".join(f"backdrop_{m}.html" for m in MOODS), f"at {W}x{H}")
