"""Builds a theme's scene layers: the white artwork each UIPilot/Scene Layer material
tints through the day. One PNG per layer (1280 x 720, white on clear), its Unity
import settings (.meta, written once so the GUID survives rebuilds), and one
material per layer with its four colours and rise. The scene is the same one the
backdrops draw (tools/store/genre_backdrops.py), taken apart into layers.

    python scene_layers.py fantasy

Writes into UIPilot/Assets/UIPilot/Art/Themes/<Theme>/. Needs headless Edge.
"""
import math
import os
import random
import subprocess
import sys
import uuid

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(os.path.dirname(HERE))
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
W, H = 1280, 720

# The backdrop script's geometry, at this frame size (it reads its size from argv).
THEME_ARG = sys.argv[1] if len(sys.argv) > 1 else "fantasy"
sys.argv = [sys.argv[0], ".", str(W), str(H)]
sys.path.insert(0, os.path.join(REPO, "tools", "store"))
import genre_backdrops as gb  # noqa: E402

# The shader every layer material uses. Its .meta is written with this GUID.
SHADER_GUID = "6a1f5c2e9d3b4e7f8a0c1d2e3f4a5b6c"
SHADER_PATH = os.path.join(REPO, "UIPilot", "Assets", "UIPilot", "Art", "Shaders", "UIPilotSceneLayer.shader")

SHADER_META = f"""fileFormatVersion: 2
guid: {SHADER_GUID}
ShaderImporter:
  externalObjects: {{}}
  defaultTextures: []
  nonModifiableTextures: []
  preprocessorOverride: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""

TEXTURE_META = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: {compression}
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID: {sprite_id}
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""

MATERIAL = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {name}
  m_Shader: {{fileID: 4800000, guid: {shader}, type: 3}}
  m_Parent: {{fileID: 0}}
  m_ModifiedSerializedProperties: 0
  m_ValidKeywords: []
  m_InvalidKeywords: []
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 0
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap: {{}}
  disabledShaderPasses: []
  m_LockedProperties:
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs:
    - _MainTex:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    m_Ints: []
    m_Floats:
    - _ColorMask: 15
    - _Rise: {rise}
    - _Stencil: 0
    - _StencilComp: 8
    - _StencilOp: 0
    - _StencilReadMask: 255
    - _StencilWriteMask: 255
    - _UseUIAlphaClip: 0
    m_Colors:
    - _Dawn: {dawn}
    - _Day: {day}
    - _Dusk: {dusk}
    - _Night: {night}
  m_BuildTextureStacks: []
"""

MATERIAL_META = """fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 2100000
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def colour(hex6, alpha=1.0):
    r, g, b = (int(hex6[i:i + 2], 16) / 255 for i in (1, 3, 5))
    return f"{{r: {r:.4f}, g: {g:.4f}, b: {b:.4f}, a: {alpha:.3f}}}"


# Texture sizes. A layer is always stretched to the 16:9 frame in Unity, so a flat
# colour needs 4 x 4 pixels and a vertical gradient 4 x 720; only drawn detail
# needs the full frame. Sizes are (width, height) of the PNG; the drawing stays
# in the 1280 x 720 frame and is scaled (non-uniformly, for the thin ones).
FULL, HALF, FLAT, VGRAD = (W, H), (W // 2, H // 2), (4, 4), (4, H)


def page(body, defs="", size=FULL):
    w, h = size
    return (f'<!doctype html><html><head><meta charset="utf-8"><style>html,body{{margin:0;background:transparent}}'
            f'svg{{display:block}}</style></head><body><svg xmlns="http://www.w3.org/2000/svg" width="{w}" '
            f'height="{h}" viewBox="0 0 {W} {H}" preserveAspectRatio="none"><defs>{defs}</defs>{body}</svg></body></html>')


# ── Fantasy: the ember valley, taken apart ───────────────────────────────────
def fantasy():
    """Returns [(key, html, compressed, material dict)] back to front."""
    S = gb.S
    rng = random.Random(4)                       # the backdrop's own seed: the same hills
    hy = H * 0.62
    castle_x, castle_k = W * 0.84, 5.2 * S
    crag_rise = 380 * S
    crag_top = hy - 40 * S - crag_rise

    def crag(x):
        d = max(0.0, abs(x - castle_x) - 150 * S)
        f = max(0.0, 1 - d / (700 * S)) ** 1.7
        ledges = round(f * 6) / 6
        rough = (16 * math.sin(x / (41 * S)) + 10 * math.sin(x / (19 * S) + 1.3) + 6 * math.sin(x / (8 * S) + 0.5)) * S
        return crag_rise * (0.6 * f + 0.4 * ledges) + rough * min(1.0, 4 * f)

    ridges = [
        ("far_hills",  hy - 160 * S, 120 * S, None, None),
        ("crag",       hy - 40 * S, 170 * S, crag, (castle_x, gb.CASTLE_HALF_WIDTH * castle_k + 14 * S, crag_top, 110 * S)),
        ("mid_hills",  hy + 150 * S, 140 * S, None, None),
        ("near_hills", hy + 420 * S, 200 * S, lambda x: 180 * S * math.exp(-((x - W * 0.15) / (500 * S)) ** 2), None),
    ]
    ridge_paths = {}
    for key, y0, amp, lift, flatten in ridges:   # drawn in the backdrop's order, so the rng matches
        ridge_paths[key] = gb.ridge(rng, y0, amp, 0.55, 14 * S, lift=lift, flatten=flatten)

    layers = []

    def add(key, body, compressed, night, dawn, day, dusk, rise=0.0, defs="", size=FULL):
        layers.append((key, page(body, defs, size), compressed, size, dict(night=night, dawn=dawn, day=day, dusk=dusk, rise=rise)))

    white = "#FFFFFF"
    add("sky_base", f'<rect width="{W}" height="{H}" fill="{white}"/>', False,
        colour("#232B60"), colour("#F5B48C"), colour("#BFE0F7"), colour("#F0A24E"), size=FLAT)

    add("sky_top", f'<rect width="{W}" height="{H}" fill="url(#zenith)"/>', False,
        colour("#05061A"), colour("#6B5FA8"), colour("#3E7FD6"), colour("#4A2645"), size=VGRAD,
        defs='<linearGradient id="zenith" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff" stop-opacity="1"/>'
             '<stop offset="0.62" stop-color="#fff" stop-opacity="0"/></linearGradient>')

    stars = []
    srng = random.Random(7)
    for _ in range(170):
        x, y = srng.uniform(0, W), srng.uniform(0, H * 0.58)
        r = srng.choice([0.7, 0.9, 1.1, 1.5]) * S * 3
        stars.append(f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{r:.1f}" fill="{white}" opacity="{srng.uniform(0.45, 1):.2f}"/>')
    for _ in range(8):
        x, y = srng.uniform(W * 0.05, W * 0.95), srng.uniform(H * 0.04, H * 0.4)
        stars.append(f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{6*S:.1f}" fill="{white}" filter="url(#soft)" opacity="0.8"/>'
                     f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{2*S:.1f}" fill="{white}"/>')
    add("stars", "".join(stars), True,
        colour("#FFFFFF", 1.0), colour("#FFFFFF", 0.25), colour("#FFFFFF", 0.0), colour("#FFFFFF", 0.2),
        defs=f'<filter id="soft"><feGaussianBlur stdDeviation="{5*S}"/></filter>')

    # The moon: drawn where it sits at dawn and dusk, moved up at night by its rise.
    mx, my, mr = W * 0.56, H * 0.50, 44 * S * 3      # on the open side, clear of the panel
    add("moon", f'<circle cx="{mx}" cy="{my}" r="{mr*1.6:.0f}" fill="{white}" opacity="0.18" filter="url(#glow)"/>'
                f'<circle cx="{mx}" cy="{my}" r="{mr:.0f}" fill="{white}" mask="url(#bite)"/>', True,
        colour("#EEF1FA", 1.0), colour("#EEF1FA", 0.35), colour("#EEF1FA", 0.0), colour("#EEF1FA", 0.3), rise=-0.35,
        defs=f'<filter id="glow"><feGaussianBlur stdDeviation="{18*S}"/></filter>'
             f'<mask id="bite"><rect width="{W}" height="{H}" fill="#fff"/>'
             f'<circle cx="{mx + mr*0.45:.0f}" cy="{my - mr*0.25:.0f}" r="{mr*0.86:.0f}" fill="#000"/></mask>')

    # The sun: on the horizon line at dawn and dusk, high at noon, below at night.
    sx, sy = W * 0.73, H * 0.52
    add("sun", f'<circle cx="{sx}" cy="{sy}" r="{240*S:.0f}" fill="{white}" opacity="0.28" filter="url(#halo)"/>'
               f'<circle cx="{sx}" cy="{sy}" r="{95*S:.0f}" fill="{white}" filter="url(#soft)"/>', False,
        colour("#FFD27A", 0.0), colour("#FFD9A0", 1.0), colour("#FFF8E0", 1.0), colour("#FFD27A", 1.0), rise=0.35, size=HALF,
        defs=f'<filter id="halo" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="{70*S}"/></filter>'
             f'<filter id="soft"><feGaussianBlur stdDeviation="{6*S}"/></filter>')

    add("clouds", f'<rect width="{W}" height="{H*0.66:.0f}" filter="url(#clouds)"/>', True,
        colour("#2A2E58", 0.3), colour("#F7C9A8", 0.6), colour("#FFFFFF", 0.55), colour("#F0A24E", 0.55),
        defs=f'<filter id="clouds" x="0" y="0" width="100%" height="100%">'
             f'<feTurbulence type="fractalNoise" baseFrequency="{0.0009/S} {0.004/S}" numOctaves="5" seed="12"/>'
             f'<feColorMatrix type="matrix" values="0 0 0 0 1  0 0 0 0 1  0 0 0 0 1  2.6 0 0 0 -1.25"/></filter>')

    add("far_hills", f'<path d="{ridge_paths["far_hills"]}" fill="{white}"/>', True,
        colour("#161A44"), colour("#8E6E9E"), colour("#7FA3C9"), colour("#B55A45"))

    castle_body = gb.castle_svg(castle_x, crag_top, castle_k, white, white, -1, None)
    add("crag_castle", f'<path d="{ridge_paths["crag"]}" fill="{white}"/>' + castle_body, True,
        colour("#090B22"), colour("#5E4C78"), colour("#557A55"), colour("#7E3A3C"))

    add("windows", gb.castle_windows_svg(castle_x, crag_top, castle_k, white), True,
        colour("#FFC774", 1.0), colour("#FFC774", 0.35), colour("#FFC774", 0.0), colour("#FFB85C", 0.85),
        defs=f'<filter id="blur2"><feGaussianBlur stdDeviation="{2.2*S}"/></filter>')

    add("mid_hills", f'<path d="{ridge_paths["mid_hills"]}" fill="{white}"/>', True,
        colour("#0E1030"), colour("#43365C"), colour("#3F6B3E"), colour("#4A2233"))

    add("near_hills", f'<path d="{ridge_paths["near_hills"]}" fill="{white}"/>', True,
        colour("#07081C"), colour("#2A2240"), colour("#2C4F2E"), colour("#26121F"))

    mist = "".join(f'<rect x="0" y="{y0 - 60*S:.0f}" width="{W}" height="{260*S:.0f}" fill="url(#mist)" opacity="{op}"/>'
                   for (y0, op) in [(hy - 160 * S, 0.55), (hy - 40 * S, 0.43), (hy + 150 * S, 0.31)])
    add("mist", mist, False,
        colour("#4A5A9A", 0.15), colour("#F7C9A8", 0.5), colour("#FFFFFF", 0.35), colour("#F7B267", 0.5), size=VGRAD,
        defs='<linearGradient id="mist" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff" stop-opacity="0"/>'
             '<stop offset="0.5" stop-color="#fff" stop-opacity="1"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></linearGradient>')
    return layers


# ── Shooter: an overcast valley, a radar station on the ridge, the overlay ──
def shooter():
    """Desaturated and cold: the tactical-overlay look. A radar station stands on the
    mid ridge where the fantasy castle would; a helicopter holds over the valley."""
    S = gb.S
    rng = random.Random(17)
    hy = H * 0.60
    station_x, station_k = W * 0.78, 4.6 * S

    def shoulder(x):
        d = max(0.0, abs(x - station_x) - 120 * S)
        return 300 * S * max(0.0, 1 - d / (900 * S)) ** 1.5

    ridges = [
        ("far_ridge",  hy - 150 * S, 110 * S, None, None),
        ("mid_ridge",  hy - 20 * S, 150 * S, shoulder, (station_x, 70 * S, hy - 20 * S - 300 * S, 90 * S)),
        ("near_ridge", hy + 380 * S, 180 * S, lambda x: 160 * S * math.exp(-((x - W * 0.12) / (520 * S)) ** 2), None),
    ]
    paths = {k: gb.ridge(rng, y0, amp, 0.55, 14 * S, lift=lift, flatten=flat) for (k, y0, amp, lift, flat) in ridges}
    ground = hy - 20 * S - 300 * S

    layers = []

    def add(key, body, compressed, night, dawn, day, dusk, rise=0.0, defs="", size=FULL):
        layers.append((key, page(body, defs, size), compressed, size, dict(night=night, dawn=dawn, day=day, dusk=dusk, rise=rise)))

    white = "#FFFFFF"
    add("sky_base", f'<rect width="{W}" height="{H}" fill="{white}"/>', False,
        colour("#1B2230"), colour("#6E6F78"), colour("#9AA7B5"), colour("#7D6A5C"), size=FLAT)
    add("sky_top", f'<rect width="{W}" height="{H}" fill="url(#zenith)"/>', False,
        colour("#080B12"), colour("#2C3140"), colour("#5C6E85"), colour("#2E2A33"), size=VGRAD,
        defs='<linearGradient id="zenith" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff" stop-opacity="1"/>'
             '<stop offset="0.7" stop-color="#fff" stop-opacity="0"/></linearGradient>')
    add("clouds", f'<rect width="{W}" height="{H*0.7:.0f}" filter="url(#overcast)"/>', True,
        colour("#2A3140", 0.5), colour("#8C8A90", 0.6), colour("#D3D8DE", 0.7), colour("#8A7566", 0.6),
        defs=f'<filter id="overcast" x="0" y="0" width="100%" height="100%">'
             f'<feTurbulence type="fractalNoise" baseFrequency="{0.0012/S} {0.003/S}" numOctaves="5" seed="31"/>'
             f'<feColorMatrix type="matrix" values="0 0 0 0 1  0 0 0 0 1  0 0 0 0 1  2.2 0 0 0 -0.95"/></filter>')
    add("far_ridge", f'<path d="{paths["far_ridge"]}" fill="{white}"/>', True,
        colour("#151B27"), colour("#4E5160"), colour("#6E7C8C"), colour("#5A4E4C"))

    # The radar station: a blockhouse, a lattice mast, a dish on top, a fence line.
    k = station_k
    cx = station_x
    st = [f'<rect x="{cx - 26*k:.1f}" y="{ground - 9*k:.1f}" width="{30*k:.1f}" height="{10*k:.1f}" fill="{white}"/>',
          f'<rect x="{cx - 22*k:.1f}" y="{ground - 13*k:.1f}" width="{10*k:.1f}" height="{4.5*k:.1f}" fill="{white}"/>',
          f'<polygon points="{cx + 6*k:.1f},{ground + 1:.1f} {cx + 14*k:.1f},{ground + 1:.1f} {cx + 11.2*k:.1f},{ground - 44*k:.1f} {cx + 8.8*k:.1f},{ground - 44*k:.1f}" fill="{white}"/>',
          f'<rect x="{cx + 9.4*k:.1f}" y="{ground - 52*k:.1f}" width="{1.2*k:.1f}" height="{9*k:.1f}" fill="{white}"/>',
          f'<path d="M{cx + 3*k:.1f} {ground - 44*k:.1f} A{9*k:.1f} {9*k:.1f} 0 0 1 {cx + 17*k:.1f} {ground - 52*k:.1f} L{cx + 10*k:.1f} {ground - 46*k:.1f} Z" fill="{white}"/>',
          f'<rect x="{cx - 34*k:.1f}" y="{ground - 3.2*k:.1f}" width="{60*k:.1f}" height="{0.5*k:.1f}" fill="{white}"/>']
    for i in range(9):
        st.append(f'<rect x="{cx - 34*k + i*7.5*k:.1f}" y="{ground - 3.2*k:.1f}" width="{0.5*k:.1f}" height="{3.2*k:.1f}" fill="{white}"/>')
    for i in range(1, 6):
        y = ground - 44 * k + i * 7 * k
        st.append(f'<rect x="{cx + 6.2*k:.1f}" y="{y:.1f}" width="{7.6*k:.1f}" height="{0.45*k:.1f}" fill="{white}"/>')
    add("mid_ridge_station", f'<path d="{paths["mid_ridge"]}" fill="{white}"/>' + "".join(st), True,
        colour("#0E121B"), colour("#3B3E4B"), colour("#556372"), colour("#3F3736"))

    # A helicopter holding over the valley, rotor as a thin blur.
    hx, hyy, hk = W * 0.58, H * 0.30, 2.6 * S
    heli = (f'<ellipse cx="{hx}" cy="{hyy}" rx="{22*hk:.1f}" ry="{8*hk:.1f}" fill="{white}"/>'
            f'<rect x="{hx + 14*hk:.1f}" y="{hyy - 2.5*hk:.1f}" width="{40*hk:.1f}" height="{4*hk:.1f}" fill="{white}"/>'
            f'<rect x="{hx + 50*hk:.1f}" y="{hyy - 12*hk:.1f}" width="{3*hk:.1f}" height="{14*hk:.1f}" fill="{white}"/>'
            f'<rect x="{hx - 2*hk:.1f}" y="{hyy - 14*hk:.1f}" width="{4*hk:.1f}" height="{7*hk:.1f}" fill="{white}"/>'
            f'<rect x="{hx - 60*hk:.1f}" y="{hyy - 15*hk:.1f}" width="{120*hk:.1f}" height="{1.6*hk:.1f}" fill="{white}" opacity="0.7"/>'
            f'<rect x="{hx - 10*hk:.1f}" y="{hyy + 9*hk:.1f}" width="{26*hk:.1f}" height="{1.6*hk:.1f}" fill="{white}"/>')
    add("helicopter", heli, True,
        colour("#0A0D14"), colour("#2A2E3A"), colour("#3E4956"), colour("#2C2726"))

    add("near_ridge", f'<path d="{paths["near_ridge"]}" fill="{white}"/>', True,
        colour("#070910"), colour("#1F222B"), colour("#2F3A44"), colour("#221E1E"))

    mist = "".join(f'<rect x="0" y="{y0 - 60*S:.0f}" width="{W}" height="{260*S:.0f}" fill="url(#mist)" opacity="{op}"/>'
                   for (y0, op) in [(hy - 150 * S, 0.5), (hy - 20 * S, 0.4)])
    add("mist", mist, False,
        colour("#39445A", 0.25), colour("#B9B5BA", 0.45), colour("#E6EAEE", 0.4), colour("#B39C8C", 0.4), size=VGRAD,
        defs='<linearGradient id="mist" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff" stop-opacity="0"/>'
             '<stop offset="0.5" stop-color="#fff" stop-opacity="1"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></linearGradient>')

    # The tactical overlay: brackets, a tick ruler, a faint reticle. Constant, cool grey.
    ox0, ox1 = W * 0.44, W * 0.96
    oy0, oy1 = H * 0.10, H * 0.90
    L = 34 * S * 3
    ov = [f'<g fill="none" stroke="{white}" stroke-width="{1.6:.1f}" stroke-linecap="square">'
          f'<polyline points="{ox0},{oy0 + L} {ox0},{oy0} {ox0 + L},{oy0}"/>'
          f'<polyline points="{ox1 - L},{oy0} {ox1},{oy0} {ox1},{oy0 + L}"/>'
          f'<polyline points="{ox0},{oy1 - L} {ox0},{oy1} {ox0 + L},{oy1}"/>'
          f'<polyline points="{ox1 - L},{oy1} {ox1},{oy1} {ox1},{oy1 - L}"/>'
          f'<line x1="{ox0 + L*1.6:.0f}" y1="{oy0 + 26*S*3:.0f}" x2="{ox1 - L*1.6:.0f}" y2="{oy0 + 26*S*3:.0f}"/>'
          f'<circle cx="{W*0.72:.0f}" cy="{H*0.56:.0f}" r="{H*0.17:.0f}"/></g>']
    for i in range(0, 41):
        x = ox0 + L * 1.6 + (ox1 - ox0 - L * 3.2) * i / 40
        h = 10 if i % 5 == 0 else 5
        ov.append(f'<rect x="{x:.1f}" y="{oy0 + 26*S*3:.0f}" width="1.4" height="{h}" fill="{white}"/>')
    rc, rr = (W * 0.72, H * 0.56), H * 0.17
    for (dx, dy) in [(1, 0), (-1, 0), (0, 1), (0, -1)]:
        ov.append(f'<rect x="{rc[0] + dx*rr - (14 if dx else 0.7):.1f}" y="{rc[1] + dy*rr - (14 if dy else 0.7):.1f}" '
                  f'width="{28 if dx else 1.4}" height="{28 if dy else 1.4}" fill="{white}"/>')
    ov.append(f'<circle cx="{rc[0]}" cy="{rc[1]}" r="2.2" fill="{white}"/>')
    add("overlay", "".join(ov), True,
        colour("#C9D2DB", 0.55), colour("#C9D2DB", 0.45), colour("#C9D2DB", 0.5), colour("#C9D2DB", 0.5))
    return layers


# ── Horror: dead trees, fog, a house with one lit window, the dark closing in ──
def bare_tree(rng, x, ground, height, white, k=1.0):
    """A leafless tree as a set of tapering strokes, branching at random."""
    out = []

    def branch(x0, y0, angle, length, width, depth):
        x1 = x0 + math.cos(angle) * length
        y1 = y0 - math.sin(angle) * length
        out.append(f'<line x1="{x0:.1f}" y1="{y0:.1f}" x2="{x1:.1f}" y2="{y1:.1f}" stroke="{white}" '
                   f'stroke-width="{max(0.6, width):.1f}" stroke-linecap="round"/>')
        if depth == 0:
            return
        n = 2 if rng.random() < 0.75 else 3
        for _ in range(n):
            a = angle + rng.uniform(-0.75, 0.75) + (0.25 if rng.random() < 0.5 else -0.25)
            branch(x1, y1, a, length * rng.uniform(0.55, 0.75), width * 0.62, depth - 1)

    branch(x, ground, math.pi / 2 + rng.uniform(-0.12, 0.12), height * 0.42, height * 0.05 * k, 5)
    return "".join(out)


def horror():
    S = gb.S
    rng = random.Random(66)
    hy = H * 0.66
    house_x = W * 0.78

    def knoll(x):
        return 150 * S * max(0.0, 1 - abs(x - house_x) / (620 * S)) ** 1.3

    far = gb.ridge(rng, hy - 120 * S, 70 * S, 0.5, 14 * S)
    mid = gb.ridge(rng, hy + 10 * S, 60 * S, 0.5, 14 * S, lift=knoll, flatten=(house_x, 90 * S, hy + 10 * S - 150 * S, 80 * S))
    near = gb.ridge(rng, hy + 330 * S, 120 * S, 0.5, 14 * S)
    ground = hy + 10 * S - 150 * S

    layers = []

    def add(key, body, compressed, night, dawn, day, dusk, rise=0.0, defs="", size=FULL):
        layers.append((key, page(body, defs, size), compressed, size, dict(night=night, dawn=dawn, day=day, dusk=dusk, rise=rise)))

    white = "#FFFFFF"
    add("sky_base", f'<rect width="{W}" height="{H}" fill="{white}"/>', False,
        colour("#12161F"), colour("#4C5550"), colour("#8E9591"), colour("#4A2A26"), size=FLAT)
    add("sky_top", f'<rect width="{W}" height="{H}" fill="url(#zenith)"/>', False,
        colour("#04050A"), colour("#1E2622"), colour("#5F6A66"), colour("#1A0F10"), size=VGRAD,
        defs='<linearGradient id="zenith" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff" stop-opacity="1"/>'
             '<stop offset="0.75" stop-color="#fff" stop-opacity="0"/></linearGradient>')

    mx, my, mr = W * 0.60, H * 0.50, 40 * S * 3
    add("moon", f'<circle cx="{mx}" cy="{my}" r="{mr*3:.0f}" fill="url(#halo)"/>'
                f'<circle cx="{mx}" cy="{my}" r="{mr:.0f}" fill="{white}" filter="url(#soften)"/>', False,
        colour("#C9CBD2", 1.0), colour("#C9CBD2", 0.25), colour("#C9CBD2", 0.0), colour("#C9CBD2", 0.35), rise=-0.32, size=HALF,
        defs=f'<radialGradient id="halo"><stop offset="0.3" stop-color="{white}" stop-opacity="0.28"/>'
             f'<stop offset="0.55" stop-color="{white}" stop-opacity="0.08"/><stop offset="1" stop-color="{white}" stop-opacity="0"/></radialGradient>'
             f'<filter id="soften" x="-20%" y="-20%" width="140%" height="140%"><feGaussianBlur stdDeviation="{2*S}"/></filter>')

    add("clouds", f'<rect width="{W}" height="{H*0.8:.0f}" filter="url(#murk)"/>', True,
        colour("#151A22", 0.7), colour("#3A423F", 0.65), colour("#B9BFBB", 0.6), colour("#2E1B1B", 0.7),
        defs=f'<filter id="murk" x="0" y="0" width="100%" height="100%">'
             f'<feTurbulence type="fractalNoise" baseFrequency="{0.0014/S} {0.0035/S}" numOctaves="5" seed="41"/>'
             f'<feColorMatrix type="matrix" values="0 0 0 0 1  0 0 0 0 1  0 0 0 0 1  2.4 0 0 0 -1.1"/></filter>')

    trng = random.Random(3)
    far_trees = "".join(bare_tree(trng, x, hy - 120 * S + 6 * S, trng.uniform(150, 240) * S, white, 0.8)
                        for x in [W * f for f in (0.05, 0.13, 0.22, 0.31, 0.42, 0.50, 0.61, 0.69, 0.88, 0.96)])
    add("far_trees", f'<path d="{far}" fill="{white}"/>' + far_trees, True,
        colour("#070A10"), colour("#2C332F"), colour("#6F7773"), colour("#2C1A18"))

    # The house on its knoll: a gable, a chimney, a porch, dead trees either side.
    k = 4.2 * S
    hx = house_x
    house = [f'<rect x="{hx - 30*k:.1f}" y="{ground - 22*k:.1f}" width="{60*k:.1f}" height="{23*k:.1f}" fill="{white}"/>',
             f'<polygon points="{hx - 34*k:.1f},{ground - 22*k:.1f} {hx + 34*k:.1f},{ground - 22*k:.1f} {hx:.1f},{ground - 46*k:.1f}" fill="{white}"/>',
             f'<rect x="{hx + 14*k:.1f}" y="{ground - 44*k:.1f}" width="{6*k:.1f}" height="{14*k:.1f}" fill="{white}"/>',
             f'<rect x="{hx - 40*k:.1f}" y="{ground - 9*k:.1f}" width="{80*k:.1f}" height="{1.2*k:.1f}" fill="{white}"/>',
             f'<rect x="{hx - 40*k:.1f}" y="{ground - 9*k:.1f}" width="{1.2*k:.1f}" height="{9*k:.1f}" fill="{white}"/>',
             f'<rect x="{hx + 38.8*k:.1f}" y="{ground - 9*k:.1f}" width="{1.2*k:.1f}" height="{9*k:.1f}" fill="{white}"/>']
    house_trees = bare_tree(trng, hx - 70 * k, ground + 2, 260 * S, white) + bare_tree(trng, hx + 62 * k, ground + 4, 220 * S, white, 0.9)
    add("hill_house", f'<path d="{mid}" fill="{white}"/>' + "".join(house) + house_trees, True,
        colour("#04060A"), colour("#1B211E"), colour("#4C5450"), colour("#1A0E0E"))

    add("window", f'<rect x="{hx - 14*k:.1f}" y="{ground - 17*k:.1f}" width="{8*k:.1f}" height="{9*k:.1f}" fill="{white}"/>'
                  f'<rect x="{hx - 14*k:.1f}" y="{ground - 17*k:.1f}" width="{8*k:.1f}" height="{9*k:.1f}" fill="{white}" filter="url(#soft)" opacity="0.7"/>', True,
        colour("#FFC66B", 1.0), colour("#FFC66B", 0.5), colour("#FFC66B", 0.15), colour("#FFC66B", 0.9),
        defs=f'<filter id="soft" x="-100%" y="-100%" width="300%" height="300%"><feGaussianBlur stdDeviation="{8*S}"/></filter>')

    near_trees = "".join(bare_tree(trng, x, hy + 330 * S + 10 * S, trng.uniform(300, 420) * S, white, 1.2)
                         for x in [W * f for f in (0.47, 0.58, 0.72, 0.93)])
    add("near_trees", f'<path d="{near}" fill="{white}"/>' + near_trees, True,
        colour("#020305"), colour("#0F1412"), colour("#2E3532"), colour("#0C0708"))

    fog = f'<rect x="0" y="{hy - 260*S:.0f}" width="{W}" height="{H - (hy - 260*S):.0f}" fill="url(#fog)"/>'
    add("fog", fog, False,
        colour("#6B7388", 0.22), colour("#B9C0B6", 0.35), colour("#E4E8E4", 0.35), colour("#6E5654", 0.3), size=VGRAD,
        defs='<linearGradient id="fog" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff" stop-opacity="0"/>'
             '<stop offset="0.45" stop-color="#fff" stop-opacity="1"/><stop offset="0.8" stop-color="#fff" stop-opacity="0.85"/>'
             '<stop offset="1" stop-color="#fff" stop-opacity="0.6"/></linearGradient>')

    add("vignette", f'<rect width="{W}" height="{H}" fill="url(#vig)"/>', False,
        colour("#000000", 0.8), colour("#000000", 0.6), colour("#000000", 0.45), colour("#000000", 0.7), size=HALF,
        defs=f'<radialGradient id="vig" cx="0.62" cy="0.5" r="0.75"><stop offset="0.35" stop-color="#fff" stop-opacity="0"/>'
             f'<stop offset="1" stop-color="#fff" stop-opacity="1"/></radialGradient>')
    return layers


THEMES = {"fantasy": ("Fantasy", "Fantasy Scene", fantasy),
          "shooter": ("Shooter", "Shooter Scene", shooter),
          "horror":  ("Horror",  "Horror Scene",  horror)}


def write_once(path, text):
    if not os.path.exists(path):
        with open(path, "w", encoding="utf-8", newline="\n") as f:
            f.write(text)
        return True
    return False


def build(key):
    folder, material_prefix, make = THEMES[key]
    out = os.path.join(REPO, "UIPilot", "Assets", "UIPilot", "Art", "Themes", folder)
    work = os.path.join(HERE, "build", key)
    os.makedirs(out, exist_ok=True)
    os.makedirs(work, exist_ok=True)

    if write_once(SHADER_PATH + ".meta", SHADER_META):
        print("shader .meta written")

    for i, (name, html, compressed, size, mat) in enumerate(make(), start=1):
        stem = f"uipilot_{key}_scene_{i:02d}_{name}"
        html_path = os.path.join(work, stem + ".html")
        png_path = os.path.join(out, stem + ".png")
        with open(html_path, "w", encoding="utf-8") as f:
            f.write(html)
        if os.path.exists(png_path):
            os.remove(png_path)
        for _ in range(3):
            subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
                            f"--window-size={size[0]},{size[1]}", "--default-background-color=00000000", "--virtual-time-budget=4000",
                            f"--screenshot={png_path}", "file:///" + html_path.replace("\\", "/")],
                           check=False, capture_output=True)
            if os.path.exists(png_path):
                break
        if not os.path.exists(png_path):
            raise SystemExit("Edge wrote no screenshot for " + stem)
        write_once(png_path + ".meta", TEXTURE_META.format(guid=uuid.uuid4().hex, sprite_id=uuid.uuid4().hex,
                                                            compression=1 if compressed else 0))

        title = f"{material_prefix} {i:02d} {name.replace('_', ' ').title()}"
        mat_path = os.path.join(out, title + ".mat")
        with open(mat_path, "w", encoding="utf-8", newline="\n") as f:
            f.write(MATERIAL.format(name=title, shader=SHADER_GUID, **mat))
        write_once(mat_path + ".meta", MATERIAL_META.format(guid=uuid.uuid4().hex))
        print(f"{stem}.png  +  {title}.mat")


if __name__ == "__main__":
    build(THEME_ARG)
