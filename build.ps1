[CmdletBinding()]
param(
    [string]$GameDirectory = 'D:\SteamLibrary\steamapps\common\A Dance of Fire and Ice',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$managedDirectory = Join-Path $GameDirectory 'A Dance of Fire and Ice_Data\Managed'
if (-not (Test-Path -LiteralPath (Join-Path $managedDirectory 'Assembly-CSharp.dll'))) {
    throw 'Game assembly not found. Specify -GameDirectory with your game installation path.'
}
if (-not (Test-Path -LiteralPath (Join-Path $managedDirectory 'UnityModManager\UnityModManager.dll'))) {
    throw 'Install Unity Mod Manager in the specified game directory before building.'
}

& dotnet build (Join-Path $projectRoot 'KeyLimiter.csproj') -c $Configuration "-p:GameDirectory=$GameDirectory"
if ($LASTEXITCODE -ne 0) {
    throw 'The mod build failed. The existing package has not been replaced.'
}

$distDirectory = Join-Path $projectRoot 'dist'
New-Item -ItemType Directory -Path $distDirectory -Force | Out-Null
$stagingDirectory = Join-Path $distDirectory ('.package-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stagingDirectory | Out-Null

try {
    $contentDirectory = Join-Path $stagingDirectory 'content'
    New-Item -ItemType Directory -Path $contentDirectory -Force | Out-Null

    Copy-Item -LiteralPath (Join-Path $projectRoot 'package\Info.json') -Destination $contentDirectory

    $iconPath = Join-Path $projectRoot 'package\icon.png'
    if (Test-Path -LiteralPath $iconPath) {
        Copy-Item -LiteralPath $iconPath -Destination $contentDirectory
    }
    Copy-Item -LiteralPath (Join-Path $projectRoot "bin\$Configuration\KeyLimiter.dll") -Destination $contentDirectory
    foreach ($name in @('README.md', 'LICENSE')) {
        Copy-Item -LiteralPath (Join-Path $projectRoot $name) -Destination $contentDirectory
    }

    # Build in a fresh directory so stale files never enter the archive.
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $stagedArchive = Join-Path $stagingDirectory 'KeyLimiter.zip'
    [System.IO.Compression.ZipFile]::CreateFromDirectory($contentDirectory, $stagedArchive)
    $archivePath = Join-Path $distDirectory 'KeyLimiter.zip'
    Move-Item -LiteralPath $stagedArchive -Destination $archivePath -Force
    Write-Output "Package created: $archivePath"
}
finally {
    $resolvedStaging = [System.IO.Path]::GetFullPath($stagingDirectory)
    $distPrefix = [System.IO.Path]::GetFullPath($distDirectory) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $resolvedStaging.StartsWith($distPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to remove a staging directory outside dist.'
    }
    Remove-Item -LiteralPath $resolvedStaging -Recurse -Force
}
