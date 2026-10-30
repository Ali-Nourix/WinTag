[CmdletBinding()]
param(
    [string]$ReportPath = (Join-Path $env:LOCALAPPDATA ("WinTag\SpikeReports\safe-" + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.json')),
    [switch]$IncludeControlledTarget
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') { throw 'Windows 11 is required.' }
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
try { $admin = ([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator) }
finally { $identity.Dispose() }
if ($admin) { throw 'Do not run elevated. Open the ordinary probe or a non-administrator PowerShell.' }
# No downloads, execution-policy changes, elevation, scheduled tasks or existing-app closure.
if ($ReportPath.Contains('"') -or $ReportPath.StartsWith('\\')) { throw 'Use a plain local .json report path.' }
$ReportPath = [IO.Path]::GetFullPath($ReportPath)
$root = [IO.Path]::GetPathRoot($ReportPath)
if (([IO.DriveInfo]::new($root)).DriveType -notin @([IO.DriveType]::Fixed, [IO.DriveType]::Removable)) { throw 'Network report drives are not allowed.' }
if ($PSScriptRoot.StartsWith('\\') -or ([IO.DriveInfo]::new([IO.Path]::GetPathRoot($PSScriptRoot))).DriveType -notin @([IO.DriveType]::Fixed, [IO.DriveType]::Removable)) {
    throw 'Extract and run the package from a local, non-network directory.'
}
$exe = Join-Path $PSScriptRoot 'WindowColoring.exe'
$manifest = Join-Path $PSScriptRoot 'checksums.sha256'
if (!(Test-Path -LiteralPath $exe -PathType Leaf) -or !(Test-Path -LiteralPath $manifest -PathType Leaf)) {
    throw 'Extract the complete binary package first. For source builds use the in-app safe-check button.'
}
foreach ($line in [IO.File]::ReadAllLines($manifest)) {
    if ($line -notmatch '^([a-fA-F0-9]{64})  ([A-Za-z0-9_.-]+)$') { throw 'Invalid package checksum manifest.' }
    $expected = $Matches[1]; $name = $Matches[2]
    if ($name -eq '.' -or $name -eq '..') { throw 'Invalid checksum filename.' }
    $file = Join-Path $PSScriptRoot $name
    if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $expected) { throw "Package integrity mismatch: $name" }
}
# Checksums detect accidental corruption; this unsigned package is NOT authenticated by hashes alone.
$argsString = '--safe-checks "' + $ReportPath + '"'
if ($IncludeControlledTarget) {
    $answer = Read-Host 'Allow a NEW disposable companion/guardian, DWM apply/restore on that companion only, then normal close? [y/N]'
    if ($answer -notmatch '^(y|yes)$') { throw 'Controlled experiment cancelled; no probe launched.' }
    $argsString += ' --controlled-target'
}
$process = Start-Process -FilePath $exe -ArgumentList $argsString -Wait -PassThru
if ($process.ExitCode -ne 0) { throw "Safe action returned exit code $($process.ExitCode). Inspect the local report if created. Never infer a native-test pass." }
Write-Host "Local report: $ReportPath"
Write-Host 'Visual/click-through/crash/performance gates are NOT automatically certified. No report was uploaded.'
