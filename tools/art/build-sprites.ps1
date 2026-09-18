# Builds the UIPilot sprite kit: screenshots uipilot-sprites.html with a transparent
# background (headless Edge), then cuts the sheet into the PNGs Unity imports.
# Run from anywhere:  powershell -File tools\art\build-sprites.ps1
# Import settings (Sprite, 9-slice borders, Pixels Per Unit 200) live in the .meta files
# next to the PNGs, so rebuilding the art never loses them.

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$here  = Split-Path -Parent $MyInvocation.MyCommand.Path
$html  = Join-Path $here 'uipilot-sprites.html'
$sheet = Join-Path $here 'uipilot-sprites.sheet.png'
$out   = Join-Path $here '..\..\UIPilot\Assets\UIPilot\Art'
$edge  = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'

New-Item -ItemType Directory -Force $out | Out-Null
if (Test-Path $sheet) { Remove-Item -LiteralPath $sheet -Force }

$uri = ([System.Uri]$html).AbsoluteUri
$p = Start-Process -FilePath $edge -PassThru -Wait -ArgumentList @(
    '--headless=new', '--disable-gpu', '--hide-scrollbars',
    '--force-device-scale-factor=1', '--window-size=1024,704',
    '--default-background-color=00000000',
    "--screenshot=`"$sheet`"", "`"$uri`"")
if (-not (Test-Path $sheet)) { throw "Edge did not write the sheet (exit $($p.ExitCode))" }

# name, x, y, width, height — must match the positions in uipilot-sprites.html
$sprites = @(
    @('uipilot_glow',                16,  16,  160, 160),
    @('uipilot_focus',              192,  16,  192, 192),
    @('uipilot_frame',              400,  16,   64,  64),
    @('uipilot_fade',               480,  16,  256,  16),
    @('uipilot_shade',              752,  16,   16, 256),
    @('uipilot_cross',              784,  16,   48,  48),
    @('uipilot_scanlines',          848,  16,   16,  16),
    @('uipilot_wave',                 0, 224, 1024, 320),
    @('uipilot_icon_play',           16, 576,   96,  96),
    @('uipilot_icon_settings',      128, 576,   96,  96),
    @('uipilot_icon_quit',          240, 576,   96,  96),
    @('uipilot_icon_back',          352, 576,   96,  96),
    @('uipilot_icon_volume',        464, 576,   96,  96),
    @('uipilot_icon_fullscreen',    576, 576,   96,  96),
    @('uipilot_icon_quality',       688, 576,   96,  96),
    @('uipilot_icon_chevron_left',  800, 576,   96,  96),
    @('uipilot_icon_chevron_right', 912, 576,   96,  96)
)
$source = [System.Drawing.Bitmap]::FromFile($sheet)
try {
    "sheet: $($source.Width)x$($source.Height), corner alpha = $($source.GetPixel(0,0).A) (0 = transparent)"
    foreach ($s in $sprites) {
        $rect = New-Object System.Drawing.Rectangle($s[1], $s[2], $s[3], $s[4])
        $cut  = $source.Clone($rect, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $cut.Save((Join-Path $out ($s[0] + '.png')), [System.Drawing.Imaging.ImageFormat]::Png)
        $cut.Dispose()
    }
}
finally { $source.Dispose() }

"wrote $($sprites.Count) sprites to $((Resolve-Path $out).Path)"
