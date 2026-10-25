<#
.SYNOPSIS
    Checks that the WPF layer loads outside Revit.

.DESCRIPTION
    Creates the shared tool window from the built assembly, sets its bindable
    properties and closes it. This catches the failures that a compiler does not
    see: a malformed XAML file, a pack URI that does not resolve, or a resource
    dictionary that is not merged.

    It runs on Windows PowerShell with the WPF assemblies from the .NET Framework,
    which is the runtime Revit 2020-2024 uses, so no Revit installation is needed.

.PARAMETER Configuration
    Build configuration of the assembly to check. Defaults to Debug.

.EXAMPLE
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/smoke-ui.ps1 -Configuration Debug

.NOTES
    Run `dotnet build ArKrPlugFR.sln` first.
#>
[CmdletBinding()]
param(
    [string] $Configuration = 'Debug',

    [ValidateSet('2020', '2022', '2024')]
    [string] $RevitVersion = '2022'
)

$ErrorActionPreference = 'Stop'

if ([System.Threading.Thread]::CurrentThread.GetApartmentState() -ne 'STA') {
    throw 'WPF requires a single-threaded apartment. Run this script with Windows PowerShell (STA by default).'
}

Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$directory = Join-Path $repositoryRoot "artifacts/bin/ArKrPlugFR.Ui/$Configuration/net48"
$uiAssembly = Join-Path $directory 'ArKrPlugFR.Ui.dll'

if (-not (Test-Path -LiteralPath $uiAssembly)) {
    throw "Assembly not found at '$uiAssembly'. Build the solution for Revit $RevitVersion first."
}

Set-Location $directory

# Loading the domain assembly first keeps the type resolution identical to the
# deployment, where both assemblies sit in the same folder.
[System.Reflection.Assembly]::LoadFrom((Join-Path $directory 'ArKrPlugFR.Core.dll')) | Out-Null
[System.Reflection.Assembly]::LoadFrom($uiAssembly) | Out-Null

$window = New-Object ArKrPlugFR.Ui.Views.ToolWindow
$window.Title = 'Smoke test'
$window.Subtitle = 'XAML parsing and resource dictionary resolution'
$window.ReportCaption = 'Report'
$window.Report = 'line one'

if ($window.Report -ne 'line one') {
    throw 'The Report property did not round-trip.'
}

$window.Report = ''

if ($window.Report -ne '') {
    throw 'The Report property did not clear.'
}

$window.Close()

Write-Host "UI smoke test passed: $($window.GetType().FullName) loads, binds and closes." -ForegroundColor Green
