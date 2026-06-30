# Генерирует AppIcon.ico из logo.png для установщика и exe.
param(
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$brandDir = Join-Path $RepoRoot 'Assets\Brand'
$logoPath = Join-Path $brandDir 'logo.png'
$rootLogo = Join-Path $RepoRoot 'logo.png'
$icoPath = Join-Path $brandDir 'AppIcon.ico'

if (-not (Test-Path $brandDir)) {
    New-Item -ItemType Directory -Path $brandDir | Out-Null
}

if (-not (Test-Path $logoPath) -and (Test-Path $rootLogo)) {
    Copy-Item $rootLogo $logoPath -Force
}

if (-not (Test-Path $logoPath)) {
    throw "Не найден logo.png в Assets\Brand или в корне проекта."
}

$bitmap = [System.Drawing.Bitmap]::FromFile($logoPath)
try {
    $icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
    $fileStream = [System.IO.File]::Create($icoPath)
    try {
        $icon.Save($fileStream)
    }
    finally {
        $fileStream.Close()
        $icon.Dispose()
    }
}
finally {
    $bitmap.Dispose()
}

Write-Host "Иконка: $icoPath"
