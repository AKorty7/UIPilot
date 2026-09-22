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


def page(body, defs=""):
    return (f'<!doctype html><html><head><meta charset="utf-8"><style>html,body{{margin:0;background:transparent}}'
            f'svg{{display:block}}</style></head><body><svg xmlns="http://www.w3.org/2000/svg" width="{W}" '
            f'height="{H}" viewBox="0 0 {W} {H}"><defs>{defs}</defs>{body}</svg></body></html>')


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

    def add(key, body, compressed, night, dawn, day, dusk, rise=0.0, defs=""):
        layers.append((key, page(body, defs), compressed, dict(night=night, dawn=dawn, day=day, dusk=dusk, rise=rise)))

    white = "#FFFFFF"
    add("sky_base", f'<rect width="{W}" height="{H}" fill="{white}"/>', False,
        colour("#232B60"), colour("#F5B48C"), colour("#BFE0F7"), colour("#F0A24E"))

    add("sky_top", f'<rect width="{W}" height="{H}" fill="url(#zenith)"/>', False,
        colour("#05061A"), colour("#6B5FA8"), colour("#3E7FD6"), colour("#4A2645"),
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
        colour("#FFD27A", 0.0), colour("#FFD9A0", 1.0), colour("#FFF8E0", 1.0), colour("#FFD27A", 1.0), rise=0.35,
        defs=f'<filter id="halo" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="{70*S}"/></filter>'
             f'<filter id="soft"><feGaussianBlur stdDeviation="{6*S}"/></filter>')

    add("clouds", f'<rect width="{W}" height="{H*0.66:.0f}" filter="url(#clouds)"/>', False,
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
        colour("#4A5A9A", 0.15), colour("#F7C9A8", 0.5), colour("#FFFFFF", 0.35), colour("#F7B267", 0.5),
        defs='<linearGradient id="mist" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff" stop-opacity="0"/>'
             '<stop offset="0.5" stop-color="#fff" stop-opacity="1"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></linearGradient>')
    return layers


THEMES = {"fantasy": ("Fantasy", "Fantasy Scene", fantasy)}


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

    for i, (name, html, compressed, mat) in enumerate(make(), start=1):
        stem = f"uipilot_{key}_scene_{i:02d}_{name}"
        html_path = os.path.join(work, stem + ".html")
        png_path = os.path.join(out, stem + ".png")
        with open(html_path, "w", encoding="utf-8") as f:
            f.write(html)
        if os.path.exists(png_path):
            os.remove(png_path)
        for _ in range(3):
            subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
                            f"--window-size={W},{H}", "--default-background-color=00000000", "--virtual-time-budget=4000",
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
