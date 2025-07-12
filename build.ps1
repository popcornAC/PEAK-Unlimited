# PEAK Unlimited Build Script
# This script builds the mod and optionally copies it to the game directory

param(
    [string]$PEAK_PATH = "C:\Program Files (x86)\Steam\steamapps\common\PEAK",
    [switch]$Deploy,
    [switch]$Clean
)

Write-Host "PEAK Unlimited Build Script" -ForegroundColor Green
Write-Host "==========================" -ForegroundColor Green

# Set environment variable for the build
$env:PEAK_PATH = $PEAK_PATH
Write-Host "Using PEAK_PATH: $PEAK_PATH" -ForegroundColor Yellow

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning build artifacts..." -ForegroundColor Yellow
    dotnet clean
    if (Test-Path "src\PEAKUnlimited\bin") {
        Remove-Item "src\PEAKUnlimited\bin" -Recurse -Force
    }
    if (Test-Path "src\PEAKUnlimited\obj") {
        Remove-Item "src\PEAKUnlimited\obj" -Recurse -Force
    }
}

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Package restore failed!" -ForegroundColor Red
    exit 1
}

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green

# Deploy if requested
if ($Deploy) {
    $pluginPath = "src\PEAKUnlimited\bin\Release\netstandard2.1\PEAKUnlimited.dll"
    $targetPath = "$PEAK_PATH\BepInEx\plugins\PEAKUnlimited.dll"
    
    if (Test-Path $pluginPath) {
        if (Test-Path "$PEAK_PATH\BepInEx\plugins") {
            Write-Host "Deploying to game directory..." -ForegroundColor Yellow
            Copy-Item $pluginPath $targetPath -Force
            Write-Host "Deployed to: $targetPath" -ForegroundColor Green
        } else {
            Write-Host "BepInEx plugins directory not found. Please install BepInEx first." -ForegroundColor Red
        }
    } else {
        Write-Host "Plugin DLL not found at: $pluginPath" -ForegroundColor Red
    }
}

Write-Host "Build script completed!" -ForegroundColor Green 