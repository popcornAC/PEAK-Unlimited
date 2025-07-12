# PEAK Unlimited - DLL Extraction Script for Windows
# This script extracts all necessary DLLs from PEAK and BepInEx for development

param(
    [string]$PEAK_PATH = "C:\Program Files (x86)\Steam\steamapps\common\PEAK",
    [string]$OUTPUT_PATH = ".\extracted_dlls",
    [switch]$Help
)

if ($Help) {
    Write-Host "PEAK Unlimited DLL Extraction Script" -ForegroundColor Green
    Write-Host "====================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\extract_dlls.ps1 [-PEAK_PATH <path>] [-OUTPUT_PATH <path>]"
    Write-Host ""
    Write-Host "Parameters:"
    Write-Host "  -PEAK_PATH     Path to PEAK game directory (default: C:\Program Files (x86)\Steam\steamapps\common\PEAK)"
    Write-Host "  -OUTPUT_PATH   Path to output directory for DLLs (default: .\extracted_dlls)"
    Write-Host "  -Help          Show this help message"
    Write-Host ""
    Write-Host "Example:"
    Write-Host "  .\extract_dlls.ps1 -OUTPUT_PATH C:\temp\peak_dlls"
    exit 0
}

Write-Host "PEAK Unlimited DLL Extraction Script" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""

# Check if PEAK directory exists
if (-not (Test-Path $PEAK_PATH)) {
    Write-Host "ERROR: PEAK directory not found at: $PEAK_PATH" -ForegroundColor Red
    Write-Host "Please make sure PEAK is installed and update the PEAK_PATH parameter if needed." -ForegroundColor Red
    exit 1
}

Write-Host "PEAK Path: $PEAK_PATH" -ForegroundColor Yellow
Write-Host "Output Path: $OUTPUT_PATH" -ForegroundColor Yellow
Write-Host ""

# Create output directory
if (-not (Test-Path $OUTPUT_PATH)) {
    New-Item -ItemType Directory -Path $OUTPUT_PATH -Force | Out-Null
    Write-Host "Created output directory: $OUTPUT_PATH" -ForegroundColor Green
}

# Define DLLs to extract
$gameDlls = @(
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll", 
    "UnityEngine.UI.dll",
    "Photon.Pun.dll",
    "Photon.Realtime.dll",
    "Assembly-CSharp.dll",
    "Zorro.Core.dll"
)

$bepinexDlls = @(
    "BepInEx.dll",
    "BepInEx.Configuration.dll",
    "BepInEx.Harmony.dll",
    "0Harmony.dll"
)

# Extract game DLLs
$gameManagedPath = Join-Path $PEAK_PATH "PEAK_Data\Managed"
Write-Host "Extracting game DLLs from: $gameManagedPath" -ForegroundColor Yellow

$extractedCount = 0
foreach ($dll in $gameDlls) {
    $sourcePath = Join-Path $gameManagedPath $dll
    $destPath = Join-Path $OUTPUT_PATH $dll
    
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath $destPath -Force
        Write-Host "  ✓ $dll" -ForegroundColor Green
        $extractedCount++
    } else {
        Write-Host "  ✗ $dll (not found)" -ForegroundColor Red
    }
}

# Extract BepInEx DLLs
$bepinexCorePath = Join-Path $PEAK_PATH "BepInEx\core"
Write-Host ""
Write-Host "Extracting BepInEx DLLs from: $bepinexCorePath" -ForegroundColor Yellow

foreach ($dll in $bepinexDlls) {
    $sourcePath = Join-Path $bepinexCorePath $dll
    $destPath = Join-Path $OUTPUT_PATH $dll
    
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath $destPath -Force
        Write-Host "  ✓ $dll" -ForegroundColor Green
        $extractedCount++
    } else {
        Write-Host "  ✗ $dll (not found)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Extraction completed!" -ForegroundColor Green
Write-Host "Total DLLs extracted: $extractedCount" -ForegroundColor Green
Write-Host "Output directory: $OUTPUT_PATH" -ForegroundColor Green
Write-Host ""

# Create a summary file
$summaryPath = Join-Path $OUTPUT_PATH "extraction_summary.txt"
$summary = @"
PEAK Unlimited DLL Extraction Summary
====================================
Extracted on: $(Get-Date)
PEAK Path: $PEAK_PATH
Output Path: $OUTPUT_PATH
Total DLLs: $extractedCount

Game DLLs:
$($gameDlls | ForEach-Object { "  - $_" } | Out-String)

BepInEx DLLs:
$($bepinexDlls | ForEach-Object { "  - $_" } | Out-String)

Instructions:
1. Copy all DLLs from this directory to your macOS project's 'libs/' folder
2. Make sure your macOS project's .csproj file references these DLLs correctly
3. Run 'dotnet build' on macOS to verify everything works
"@

$summary | Out-File -FilePath $summaryPath -Encoding UTF8
Write-Host "Summary saved to: $summaryPath" -ForegroundColor Cyan

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Copy all files from '$OUTPUT_PATH' to your macOS project's 'libs/' directory" -ForegroundColor White
Write-Host "2. On macOS, run 'dotnet restore' and 'dotnet build' to test" -ForegroundColor White
Write-Host "3. See DEVELOPMENT_SETUP.md for more details" -ForegroundColor White 