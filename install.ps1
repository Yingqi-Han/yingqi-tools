param(
    [string]$InstallDirectory = 'D:\Programs\YingqiTools',
    [switch]$SkipBuild,
    [switch]$DoNotLaunch
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
if (-not $SkipBuild) {
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $root 'build.ps1') -LockedMode
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
}

$publish = Join-Path $root 'build\publish'
$sourceApp = Join-Path $publish 'YingqiTools.exe'
$sourceGuard = Join-Path $publish 'PowerGuard.exe'
if (-not (Test-Path -LiteralPath $sourceApp) -or -not (Test-Path -LiteralPath $sourceGuard)) { throw 'Publish output is incomplete.' }

$installedGuard = Join-Path $env:ProgramFiles 'YingqiTools\PowerGuard\PowerGuard.exe'
$guardUpToDate = $false
if (Test-Path -LiteralPath $installedGuard) {
    $sourceVersion = [version](Get-Item -LiteralPath $sourceGuard).VersionInfo.FileVersion
    $installedVersion = [version](Get-Item -LiteralPath $installedGuard).VersionInfo.FileVersion
    $guardUpToDate = $installedVersion -ge $sourceVersion
}
if (-not $guardUpToDate) {
    $guardInstall = Start-Process $sourceGuard -ArgumentList 'install' -Verb RunAs -PassThru -Wait
    if ($guardInstall.ExitCode -ne 0) { throw "PowerGuard installation failed: $($guardInstall.ExitCode)" }
}

$resolvedInstall = [System.IO.Path]::GetFullPath($InstallDirectory)
if ([System.IO.Path]::GetPathRoot($resolvedInstall) -eq $resolvedInstall) { throw "Unsafe installation path: $resolvedInstall" }
New-Item -ItemType Directory -Force -Path $resolvedInstall | Out-Null
Copy-Item -LiteralPath $sourceApp -Destination (Join-Path $resolvedInstall 'YingqiTools.exe') -Force
Copy-Item -LiteralPath (Join-Path $publish 'LICENSE') -Destination $resolvedInstall -Force
Copy-Item -LiteralPath (Join-Path $publish 'THIRD-PARTY-NOTICES.md') -Destination $resolvedInstall -Force
$dataDirectory = Join-Path $resolvedInstall 'Data\ClipboardHistory'
New-Item -ItemType Directory -Force -Path $dataDirectory | Out-Null

$desktop = [Environment]::GetFolderPath('Desktop')
$shortcutPath = Join-Path $desktop 'Yingqi Tools.lnk'
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = Join-Path $resolvedInstall 'YingqiTools.exe'
$shortcut.WorkingDirectory = $resolvedInstall
$shortcut.IconLocation = (Join-Path $resolvedInstall 'YingqiTools.exe') + ',0'
$shortcut.Description = 'Yingqi Tools modular Windows utility toolbox'
$shortcut.Save()

$oldShortcutName = (-join ([char[]](0x952E,0x76D8,0x964D,0x6E29,0x9501))) + '.lnk'
$oldShortcut = Join-Path $desktop $oldShortcutName
if (Test-Path -LiteralPath $oldShortcut) {
    $old = $shell.CreateShortcut($oldShortcut)
    if ($old.TargetPath -like '*KeyboardCoolDownLock*') { Remove-Item -LiteralPath $oldShortcut -Force }
}

if (-not $DoNotLaunch) { Start-Process (Join-Path $resolvedInstall 'YingqiTools.exe') }
[pscustomobject]@{
    Installed = $resolvedInstall
    Shortcut = $shortcutPath
    PowerGuard = $installedGuard
}
