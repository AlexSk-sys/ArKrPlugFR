<#
.SYNOPSIS
    Builds ArKrPlugFR for one Revit release and assembles a deployable folder.

.DESCRIPTION
    Produces a flat folder that contains the host, the shared assemblies and
    every tool (assembly plus its *.plugin.json manifest), ready to be copied
    next to a Revit add-in manifest.

    The Revit API is referenced through NuGet reference assemblies, so no Revit
    installation is required to build or pack.

.PARAMETER RevitVersion
    Revit release to build for. Supported values: 2020, 2022, 2024.

.PARAMETER Configuration
    Build configuration. Defaults to Release.

.PARAMETER OutputRoot
    Root folder for the result. The version-specific folder is created inside it.
    Defaults to artifacts\deploy.

.EXAMPLE
    pwsh scripts/pack.ps1 -RevitVersion 2024

.NOTES
    Requires the .NET SDK and, for the net48 target, the .NET Framework 4.8
    targeting pack that ships with Visual Studio.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('2020', '2022', '2024')]
    [string] $RevitVersion,

    [string] $Configuration = 'Release',

    [string] $OutputRoot = 'artifacts/deploy'
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repositoryRoot 'ArKrPlugFR.sln'

if (-not (Test-Path -LiteralPath $solution)) {
    throw "Solution not found at '$solution'."
}

$outputDirectory = Join-Path (Join-Path $repositoryRoot $OutputRoot) $RevitVersion

Write-Host "Building ArKrPlugFR for Revit $RevitVersion ($Configuration)..." -ForegroundColor Cyan

& dotnet build $solution --nologo --configuration $Configuration -p:RevitVersion=$RevitVersion -p:TreatWarningsAsErrors=true

if ($LASTEXITCODE -ne 0) {
    throw "The build failed with exit code $LASTEXITCODE."
}

if (Test-Path -LiteralPath $outputDirectory) {
    Remove-Item -LiteralPath $outputDirectory -Recurse -Force
}

New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null

# The host output already carries the shared assemblies (Core, Revit, Ui) and
# their dependencies, so it is the base of the deployment.
$hostOutput = Join-Path $repositoryRoot "artifacts/bin/ArKrPlugFR.Host/$Configuration/net48"

Get-ChildItem -LiteralPath $hostOutput -File |
    Where-Object { $_.Extension -in '.dll', '.xml' } |
    Copy-Item -Destination $outputDirectory -Force

# Every tool contributes its assembly and its manifest.
$toolProjects = Get-ChildItem -LiteralPath (Join-Path $repositoryRoot 'src') -Directory |
    Where-Object { $_.Name -like 'ArKrPlugFR.Plugins.*' }

foreach ($project in $toolProjects) {
    $toolOutput = Join-Path $repositoryRoot "artifacts/bin/$($project.Name)/$Configuration/net48"

    if (-not (Test-Path -LiteralPath $toolOutput)) {
        throw "Output of '$($project.Name)' not found at '$toolOutput'."
    }

    Get-ChildItem -LiteralPath $toolOutput -File |
        Where-Object { $_.Name -like "$($project.Name).*" -and $_.Extension -in '.dll', '.xml' } |
        Copy-Item -Destination $outputDirectory -Force

    Get-ChildItem -LiteralPath $toolOutput -File -Filter '*.plugin.json' |
        Copy-Item -Destination $outputDirectory -Force
}

Copy-Item -LiteralPath (Join-Path $repositoryRoot 'installer/ArKrPlugFR.addin') -Destination $outputDirectory -Force

$files = Get-ChildItem -LiteralPath $outputDirectory -File

Write-Host "Packed $($files.Count) file(s) into '$outputDirectory'." -ForegroundColor Green
$files | Select-Object -ExpandProperty Name | Sort-Object | ForEach-Object { Write-Host "  $_" }
