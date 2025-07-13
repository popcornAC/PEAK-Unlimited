# PEAK Unlimited Test Runner
# This script runs tests and code analysis

param(
    [switch]$Test,
    [switch]$Analyze,
    [switch]$All,
    [switch]$Help
)

if ($Help) {
    Write-Host "PEAK Unlimited Test Runner" -ForegroundColor Green
    Write-Host "========================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\test.ps1 [-Test] [-Analyze] [-All]"
    Write-Host ""
    Write-Host "Parameters:"
    Write-Host "  -Test     Run unit tests"
    Write-Host "  -Analyze  Run code analysis"
    Write-Host "  -All      Run both tests and analysis"
    Write-Host "  -Help     Show this help message"
    exit 0
}

Write-Host "PEAK Unlimited Test Runner" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green
Write-Host ""

$runTests = $Test -or $All
$runAnalysis = $Analyze -or $All

if ($runTests) {
    Write-Host "Running unit tests..." -ForegroundColor Yellow
    dotnet test --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Tests failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "Tests completed successfully!" -ForegroundColor Green
}

if ($runAnalysis) {
    Write-Host "Running code analysis..." -ForegroundColor Yellow
    dotnet build --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Code analysis failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "Code analysis completed successfully!" -ForegroundColor Green
}

Write-Host "Test runner completed!" -ForegroundColor Green 