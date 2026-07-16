#!/usr/bin/env pwsh
# Regenerates docs/openapi.json — the committed OpenAPI contract (SDD artifact #4).
#
# Source: the Minimal API (port 5001). The Controllers API currently cannot start
# (Swashbuckle 6.8.1 vs Microsoft.OpenApi 3.0.1 version conflict crashes AddSwaggerGen),
# so the Minimal API is the working generator. See docs/api.md for the drift details.
#
# Method: the Swashbuckle CLI (`dotnet swagger tofile`) does not work with this app's
# minimal-hosting model, so we boot the app with Firebase skipped and capture the
# runtime Swagger document.
#
# Usage:  pwsh ./scripts/generate-openapi.ps1
$ErrorActionPreference = 'Stop'

$root = Split-Path $PSScriptRoot -Parent
$proj = Join-Path $root 'src/QuestionRandomizer.Api.MinimalApi'
$dll  = Join-Path $proj 'bin/Debug/net10.0/QuestionRandomizer.Api.MinimalApi.dll'
$out  = Join-Path $root 'docs/openapi.json'
$port = 5099
$url  = "http://localhost:$port/swagger/v1/swagger.json"

Write-Host "Building Minimal API..."
dotnet build $proj -c Debug --nologo | Out-Null

$env:ASPNETCORE_ENVIRONMENT = 'Development'   # maps the /swagger endpoint
$env:ASPNETCORE_URLS        = "http://localhost:$port"
$env:Testing__SkipFirebase  = 'true'          # AddFirebase short-circuits; no creds needed
$env:DOTNET_ROLL_FORWARD    = 'LatestMajor'

Write-Host "Starting app on $port..."
# -WindowStyle is Windows-only and throws on Linux PowerShell (CI), so add it only there.
# $IsWindows is $true on pwsh/Windows, $false on Linux, and $null on Windows PowerShell 5.1.
$startArgs = @{ FilePath = 'dotnet'; ArgumentList = $dll; PassThru = $true }
if ($IsWindows -ne $false) { $startArgs['WindowStyle'] = 'Hidden' }
$proc = Start-Process @startArgs
try {
    $ok = $false
    for ($i = 0; $i -lt 30; $i++) {
        try { Invoke-WebRequest -Uri $url -OutFile $out -UseBasicParsing; $ok = $true; break }
        catch { Start-Sleep -Seconds 1 }
    }
    if (-not $ok) { throw "Timed out waiting for $url" }
    Write-Host "Wrote $out"
}
finally {
    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
}
