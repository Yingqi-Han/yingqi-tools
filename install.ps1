param()
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $root 'build.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
$build = Join-Path $root 'build'
$guard = Join-Path $build 'PowerGuard.exe'
$installGuard = Start-Process $guard -ArgumentList 'install' -Verb RunAs -PassThru -Wait
if ($installGuard.ExitCode -ne 0) { throw "PowerGuard installation failed: $($installGuard.ExitCode)" }
$installDir = Join-Path $env:LOCALAPPDATA 'Programs\YingqiTools'
New-Item -ItemType Directory -Path $installDir -Force | Out-Null
Copy-Item (Join-Path $build 'YingqiTools.exe') $installDir -Force
Copy-Item (Join-Path $build 'KeyboardLockComponent.dll') $installDir -Force
Copy-Item (Join-Path $build 'LidWorkModeComponent.dll') $installDir -Force
$desktop = [Environment]::GetFolderPath('Desktop')
$shortcutPath = Join-Path $desktop 'Yingqi Tools.lnk'
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = Join-Path $installDir 'YingqiTools.exe'
$shortcut.WorkingDirectory = $installDir
$shortcut.IconLocation = (Join-Path $installDir 'YingqiTools.exe') + ',0'
$shortcut.Description = 'Yingqi Tools modular Windows utility toolbox'
$shortcut.Save()
$oldShortcutName = (-join ([char[]](0x952E,0x76D8,0x964D,0x6E29,0x9501))) + '.lnk'
$oldShortcut = Join-Path $desktop $oldShortcutName
if (Test-Path $oldShortcut) {
    $old = $shell.CreateShortcut($oldShortcut)
    if ($old.TargetPath -like '*KeyboardCoolDownLock*') { Remove-Item -LiteralPath $oldShortcut -Force }
}
$oldInstall = Join-Path $env:LOCALAPPDATA 'Programs\KeyboardCoolDownLock'
if (Test-Path $oldInstall) {
    $backupRoot = Join-Path $env:LOCALAPPDATA 'YingqiTools\legacy-backup'
    New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null
    $backup = Join-Path $backupRoot (Get-Date -Format 'yyyyMMdd-HHmmss')
    Move-Item -LiteralPath $oldInstall -Destination $backup
}
Start-Process (Join-Path $installDir 'YingqiTools.exe')
[pscustomobject]@{ Installed = $installDir; Shortcut = $shortcutPath; PowerGuard = 'C:\Program Files\YingqiTools\PowerGuard\PowerGuard.exe' }
