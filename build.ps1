param()
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$build = Join-Path $root 'build'
$keyboard = Join-Path $root 'components\keyboard-cooldown-lock'
$lid = Join-Path $root 'components\lid-work-mode'
if (-not (Test-Path (Join-Path $keyboard 'build.ps1')) -or -not (Test-Path (Join-Path $lid 'build.ps1'))) { throw 'Submodules are missing. Run git submodule update --init --recursive.' }
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $keyboard 'build.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Keyboard component build failed.' }
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $lid 'build.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Lid component build failed.' }
$compiler = @((Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'), (Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe')) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $compiler) { throw 'The .NET Framework C# compiler was not found.' }
New-Item -ItemType Directory -Path $build -Force | Out-Null
Copy-Item (Join-Path $keyboard 'build\KeyboardLockComponent.dll') $build -Force
Copy-Item (Join-Path $lid 'build\LidWorkModeComponent.dll') $build -Force
Copy-Item (Join-Path $lid 'build\PowerGuard.exe') $build -Force
$app = Join-Path $build 'YingqiTools.exe'
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ "/win32manifest:$(Join-Path $root 'src\app.manifest')" "/out:$app" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll "/reference:$(Join-Path $build 'KeyboardLockComponent.dll')" "/reference:$(Join-Path $build 'LidWorkModeComponent.dll')" (Join-Path $root 'src\YingqiTools.cs')
if ($LASTEXITCODE -ne 0) { throw 'Yingqi Tools compilation failed.' }
$test = Start-Process $app -ArgumentList '--self-test' -PassThru -Wait
if ($test.ExitCode -ne 0) { throw "Self-test failed: $($test.ExitCode)" }
Get-ChildItem $build | Select-Object Name, Length, LastWriteTime
