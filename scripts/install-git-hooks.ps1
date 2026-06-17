param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$sampleHookPath = Join-Path $repositoryRoot "scripts\git-hooks\pre-push.sample"
$gitHooksDirectory = Join-Path $repositoryRoot ".git\hooks"
$targetHookPath = Join-Path $gitHooksDirectory "pre-push"

if (-not (Test-Path $sampleHookPath)) {
    throw "The hook template was not found at '$sampleHookPath'."
}

if (-not (Test-Path $gitHooksDirectory)) {
    throw "The Git hooks directory was not found at '$gitHooksDirectory'."
}

if ((Test-Path $targetHookPath) -and -not $Force) {
    throw "A pre-push hook already exists at '$targetHookPath'. Use -Force to overwrite it."
}

Copy-Item -Path $sampleHookPath -Destination $targetHookPath -Force

Write-Host "Installed pre-push hook at '$targetHookPath'." -ForegroundColor Green
