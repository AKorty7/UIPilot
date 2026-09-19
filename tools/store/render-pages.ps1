# Renders every page compose.py wrote to an exact-size, 24-bit PNG (no alpha, as the
# Asset Store asks). Usage: render-pages.ps1 -Pages <dir with jobs.txt> -Out <png dir>
param([Parameter(Mandatory)][string]$Pages, [Parameter(Mandatory)][string]$Out)

$edge = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $Out | Out-Null

foreach ($line in Get-Content (Join-Path $Pages "jobs.txt")) {
    $name, $w, $h = $line -split " "
    $html = Join-Path $Pages "$name.html"
    $raw  = Join-Path $Pages "$name.raw.png"
    Start-Process -FilePath $edge -Wait -ArgumentList @(
        "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
        "--run-all-compositor-stages-before-draw", "--virtual-time-budget=4000",
        "--window-size=$w,$h", "--screenshot=`"$raw`"", "`"$(([Uri]$html).AbsoluteUri)`"")

    # Flatten to 24-bit RGB at exactly the target size.
    $src = [Drawing.Image]::FromFile($raw)
    $dst = New-Object Drawing.Bitmap ([int]$w), ([int]$h), ([Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [Drawing.Graphics]::FromImage($dst)
    $g.DrawImage($src, 0, 0, [int]$w, [int]$h)
    $g.Dispose(); $src.Dispose()
    $dst.Save((Join-Path $Out "$name.png"), [Drawing.Imaging.ImageFormat]::Png)
    $dst.Dispose()
    "{0,-26} {1}x{2}" -f $name, $w, $h
}
