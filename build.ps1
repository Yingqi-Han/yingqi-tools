param([switch]$LockedMode)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$dotnet = $env:YINGQI_DOTNET
if (-not $dotnet) { $dotnet = (Get-Command dotnet -ErrorAction Stop).Source }
if (-not (Test-Path -LiteralPath $dotnet)) { throw 'Set YINGQI_DOTNET to a valid .NET 10 SDK dotnet.exe.' }
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'

$keyboard = Join-Path $root 'components\keyboard-cooldown-lock'
$lid = Join-Path $root 'components\lid-work-mode'
$requiredProjects = @(
    (Join-Path $keyboard 'src\KeyboardLockComponent\KeyboardLockComponent.csproj'),
    (Join-Path $lid 'src\LidWorkModeComponent\LidWorkModeComponent.csproj'),
    (Join-Path $lid 'src\PowerGuard\PowerGuard.csproj')
)
foreach ($project in $requiredProjects) {
    if (-not (Test-Path -LiteralPath $project)) { throw 'Submodules are missing or stale. Run git submodule update --init --recursive.' }
}

$build = [System.IO.Path]::GetFullPath((Join-Path $root 'build'))
$rootPrefix = [System.IO.Path]::GetFullPath($root).TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
if (-not $build.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) { throw "Unsafe build path: $build" }
New-Item -ItemType Directory -Force -Path $build | Out-Null
Get-ChildItem -LiteralPath $build -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force

$icon = Join-Path $root 'src\YingqiTools\Assets\YingqiTools.ico'
& $dotnet run --project (Join-Path $root 'tools\IconBuilder\IconBuilder.csproj') -c Release -- $icon
if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $icon)) { throw 'Application icon generation failed.' }

$solution = Join-Path $root 'YingqiTools.slnx'
$restoreArgs = @('restore', $solution)
if ($LockedMode) { $restoreArgs += '--locked-mode' }
& $dotnet @restoreArgs
if ($LASTEXITCODE -ne 0) { throw 'Restore failed.' }
& $dotnet build $solution -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
& $dotnet test (Join-Path $root 'tests\YingqiTools.Tests\YingqiTools.Tests.csproj') -c Release --no-build --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }

$publish = Join-Path $build 'publish'
& $dotnet publish (Join-Path $root 'src\YingqiTools\YingqiTools.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false --no-restore -o $publish
if ($LASTEXITCODE -ne 0) { throw 'Yingqi Tools publish failed.' }
$guardProject = Join-Path $lid 'src\PowerGuard\PowerGuard.csproj'
$guardRestoreArgs = @('restore', $guardProject)
if ($LockedMode) { $guardRestoreArgs += '--locked-mode' }
& $dotnet @guardRestoreArgs
if ($LASTEXITCODE -ne 0) { throw 'PowerGuard restore failed.' }
& $dotnet publish $guardProject -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false --no-restore -o (Join-Path $build 'guard')
if ($LASTEXITCODE -ne 0) { throw 'PowerGuard publish failed.' }

Get-ChildItem -LiteralPath $publish -Filter '*.pdb' -File -ErrorAction SilentlyContinue | Remove-Item -Force
Copy-Item -LiteralPath (Join-Path $build 'guard\PowerGuard.exe') -Destination $publish -Force
Copy-Item -LiteralPath (Join-Path $root 'LICENSE') -Destination $publish -Force
Copy-Item -LiteralPath (Join-Path $root 'THIRD-PARTY-NOTICES.md') -Destination $publish -Force

$selfTest = Start-Process (Join-Path $publish 'YingqiTools.exe') -ArgumentList '--self-test' -PassThru -Wait
if ($selfTest.ExitCode -ne 0) { throw "Self-test failed: $($selfTest.ExitCode)" }
Get-ChildItem -LiteralPath $publish -File | Select-Object Name, Length, LastWriteTime
