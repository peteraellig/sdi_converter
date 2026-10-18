# Publishes the GUI as a ClickOnce package (x64, self-contained, engine included) and copies it to <project>\publish.
# Needs the full Visual Studio MSBuild (the ClickOnce tasks are not available in "dotnet publish").
# UseSharedCompilation=false: the shared compiler server does not see the generated editorconfig file in this setup (CS2001).
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent)).TrimEnd('\')
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vs = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
if (!$vs) { throw 'Visual Studio MSBuild not found' }
$staging = Join-Path $env:LOCALAPPDATA 'sdi_converter\gui-publish\app.publish'
if (Test-Path -LiteralPath $staging) { Remove-Item -LiteralPath $staging -Recurse -Force }
& (Join-Path $vs 'MSBuild\Current\Bin\MSBuild.exe') (Join-Path $root 'gui\SdiConverter.csproj') /restore /target:Publish /p:UseSharedCompilation=false /p:PublishProfile=ClickOnce /p:Configuration=Release /verbosity:minimal
if ($LASTEXITCODE) { throw 'ClickOnce publication failed' }
if (!(Test-Path -LiteralPath (Join-Path $staging 'SdiConverter.application'))) { throw 'SdiConverter.application was not produced' }
$final = Join-Path $root 'publish'
New-Item -ItemType Directory -Path $final -Force | Out-Null
Copy-Item -Path (Join-Path $staging '*') -Destination $final -Recurse -Force
Write-Host "Ready: $final\setup.exe"
# The stray launcher in the package root is not referenced by the manifest (the one in 'Application Files' is the entry point).
Remove-Item -LiteralPath (Join-Path $final 'Launcher.exe') -Force -ErrorAction SilentlyContinue
