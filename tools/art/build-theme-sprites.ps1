# Builds the artwork of UIPilot's genre themes (Fantasy RPG, JRPG Window, Pixel Retro,
# Sci-Fi HUD): screenshots theme-sprites.html with a transparent background (headless
# Edge), cuts it into PNGs, and draws the two pixel-art sprites pixel by pixel.
# Run from anywhere:  powershell -File tools\art\build-theme-sprites.ps1
#
# The first build of a sprite also writes its .meta (import settings: 9-slice border,
# Pixels Per Unit, filter, compression). After that the .meta is left alone, so
# rebuilding the art never loses settings or the GUIDs that themes reference.

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$here  = Split-Path -Parent $MyInvocation.MyCommand.Path
$html  = Join-Path $here 'theme-sprites.html'
$sheet = Join-Path $here 'theme-sprites.sheet.png'
$out   = Join-Path $here '..\..\UIPilot\Assets\UIPilot\Art\Themes'
$edge  = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'

New-Item -ItemType Directory -Force $out | Out-Null
if (Test-Path $sheet) { Remove-Item -LiteralPath $sheet -Force }

# name, x, y, width, height, border (left, bottom, right, top), PPU, filter (0 point,
# 1 bilinear), compression (0 none, 1 normal). Positions must match theme-sprites.html.
$sprites = @(
    @('uipilot_fantasy_panel',     0,   0, 1024, 1536, @(160,160,160,160), 200, 1, 1),
    @('uipilot_jrpg_panel',     1040,   0,  256,  256, @( 48, 48, 48, 48), 200, 1, 0),
    @('uipilot_scifi_panel',    1312,   0,  384,  384, @(120,120,120,120), 200, 1, 0),
    @('uipilot_jrpg_cursor',    1712,   0,   72,   56, @(  0,  0,  0,  0), 200, 1, 0),
    @('uipilot_fantasy_focus',  1040, 400,  512,  112, @( 96,  0,192,  0), 200, 1, 0),
    @('uipilot_scifi_focus',    1040, 528,  512,  112, @( 64,  0,192,  0), 200, 1, 0),
    @('uipilot_fantasy_rule',   1040, 656,  960,   56, @(  0,  0,  0,  0), 200, 1, 0),
    @('uipilot_scifi_rule',     1040, 728,  960,   32, @(  0,  0,  0,  0), 200, 1, 0),
    @('uipilot_shooter_panel',  1040, 780,  384,  384, @( 96, 96, 96, 96), 200, 1, 0),
    @('uipilot_shooter_focus',  1440, 780,  512,  112, @( 64,  0,192,  0), 200, 1, 0),
    @('uipilot_shooter_rule',   1040,1180,  960,   24, @(  0,  0,  0,  0), 200, 1, 0)
)

# Pixel art, one character per pixel. Drawn at Pixels Per Unit 100 with point
# filtering; the theme scales the panel 4x (Pixel Size Multiplier 0.25) and sizes the
# cursor to 4x, so one art pixel is exactly four screen pixels at 1080p.
$palette = @{
    '.' = [System.Drawing.Color]::FromArgb(0, 0, 0, 0)
    'K' = [System.Drawing.Color]::FromArgb(255, 0x00, 0x00, 0x00)   # outline
    'W' = [System.Drawing.Color]::FromArgb(255, 0xF4, 0xF1, 0xE8)   # border, warm white
    'F' = [System.Drawing.Color]::FromArgb(255, 0x0A, 0x0A, 0x1E)   # window fill
    'A' = [System.Drawing.Color]::FromArgb(255, 0xFF, 0xFF, 0xFF)   # cursor (tinted by focus)
    'S' = [System.Drawing.Color]::FromArgb(210, 0x00, 0x00, 0x00)   # cursor's shadow
}
$pixelArt = @(
    @('uipilot_pixel_panel', @(4,4,4,4), @(
        '..KKKKKKKKKKKK..',
        '.KWWWWWWWWWWWWK.',
        'KWWWWWWWWWWWWWWK',
        'KWWFFFFFFFFFFWWK',
        'KWWFFFFFFFFFFWWK',
        'KWWFFFFFFFFFFWWK',
        'KWWFFFFFFFFFFWWK',
        'KWWFFFFFFFFFFWWK',
        'KWWFFFFFFFFFFWWK',
        'KWWFFFFFFFFFFWWK',
        'KWWFFFFFFFFFFWWK',
        'KWWFFFFFFFFFFWWK',
        'KWWFFFFFFFFFFWWK',
        'KWWWWWWWWWWWWWWK',
        '.KWWWWWWWWWWWWK.',
        '..KKKKKKKKKKKK..')),
    @('uipilot_pixel_cursor', @(0,0,0,0), @(
        'A....',
        'AA...',
        'AAA..',
        'AAAA.',
        'AAASS',
        'AASS.',
        'ASS..',
        '.S...'))
)

# The import settings every sprite starts from; {tokens} are filled per sprite.
$metaTemplate = @'
fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
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
    filterMode: {filter}
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
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: {ppu}
  spriteBorder: {x: {left}, y: {bottom}, z: {right}, w: {top}}
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
    spriteID: {spriteId}
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
'@

function Write-MetaOnce([string]$png, [int[]]$border, [int]$ppu, [int]$filter, [int]$compression) {
    $meta = "$png.meta"
    if (Test-Path $meta) { return }
    $text = $metaTemplate.
        Replace('{guid}',        [guid]::NewGuid().ToString('N')).
        Replace('{spriteId}',    [guid]::NewGuid().ToString('N')).
        Replace('{filter}',      "$filter").
        Replace('{ppu}',         "$ppu").
        Replace('{left}',        "$($border[0])").
        Replace('{bottom}',      "$($border[1])").
        Replace('{right}',       "$($border[2])").
        Replace('{top}',         "$($border[3])").
        Replace('{compression}', "$compression")
    # Unity's YAML parser rejects a .meta whose last line has no line break.
    [System.IO.File]::WriteAllText($meta, $text.Replace("`r`n", "`n") + "`n", (New-Object System.Text.UTF8Encoding $false))
    "  new import settings: $(Split-Path -Leaf $meta)"
}

# ── Vector sprites ──────────────────────────────────────────────────────────
$uri = ([System.Uri]$html).AbsoluteUri
$p = Start-Process -FilePath $edge -PassThru -Wait -ArgumentList @(
    '--headless=new', '--disable-gpu', '--hide-scrollbars',
    '--force-device-scale-factor=1', '--window-size=2016,1536',
    '--default-background-color=00000000', '--virtual-time-budget=4000',
    "--screenshot=`"$sheet`"", "`"$uri`"")
if (-not (Test-Path $sheet)) { throw "Edge did not write the sheet (exit $($p.ExitCode))" }

$source = [System.Drawing.Bitmap]::FromFile($sheet)
try {
    "sheet: $($source.Width)x$($source.Height), corner alpha = $($source.GetPixel(2015,1535).A) (0 = transparent)"
    foreach ($s in $sprites) {
        $rect = New-Object System.Drawing.Rectangle($s[1], $s[2], $s[3], $s[4])
        $cut  = $source.Clone($rect, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $png  = Join-Path $out ($s[0] + '.png')
        $cut.Save($png, [System.Drawing.Imaging.ImageFormat]::Png)
        $cut.Dispose()
        Write-MetaOnce $png $s[5] $s[6] $s[7] $s[8]
    }
}
finally { $source.Dispose() }

# ── Pixel sprites ───────────────────────────────────────────────────────────
foreach ($art in $pixelArt) {
    $rows = $art[2]
    $bmp  = New-Object System.Drawing.Bitmap($rows[0].Length, $rows.Count, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($y = 0; $y -lt $rows.Count; $y++) {
            if ($rows[$y].Length -ne $bmp.Width) { throw "$($art[0]): row $y is not $($bmp.Width) wide" }
            for ($x = 0; $x -lt $bmp.Width; $x++) { $bmp.SetPixel($x, $y, $palette[[string]$rows[$y][$x]]) }
        }
        $png = Join-Path $out ($art[0] + '.png')
        $bmp.Save($png, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-MetaOnce $png $art[1] 100 0 0
    }
    finally { $bmp.Dispose() }
}

"wrote $($sprites.Count + $pixelArt.Count) sprites to $((Resolve-Path $out).Path)"
