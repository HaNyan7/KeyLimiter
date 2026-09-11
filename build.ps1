[CmdletBinding()]
param(
    [string]$GameDirectory = 'D:\SteamLibrary\steamapps\common\A Dance of Fire and Ice',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$PythonCommand = 'python'
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

& $PythonCommand (Join-Path $projectRoot 'decorations\generate.py')
if ($LASTEXITCODE -ne 0) {
    throw 'Image generation failed. Install decorations/requirements.txt and try again.'
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
    $imageDirectory = Join-Path $contentDirectory 'decorations'
    $fontLicenseDirectory = Join-Path $imageDirectory 'assets'
    New-Item -ItemType Directory -Path $fontLicenseDirectory -Force | Out-Null

    foreach ($name in @('Info.json', 'icon.png')) {
        Copy-Item -LiteralPath (Join-Path $projectRoot "package\$name") -Destination $contentDirectory
    }
    Copy-Item -LiteralPath (Join-Path $projectRoot "bin\$Configuration\KeyLimiter.dll") -Destination $contentDirectory
    foreach ($name in @('README.md', 'LICENSE', 'THIRD_PARTY_NOTICES.md')) {
        Copy-Item -LiteralPath (Join-Path $projectRoot $name) -Destination $contentDirectory
    }
    Copy-Item -LiteralPath (Join-Path $projectRoot 'decorations\assets\Pretendard-LICENSE.txt') -Destination $fontLicenseDirectory

    foreach ($count in 1..24) {
        Copy-Item -LiteralPath (Join-Path $projectRoot "decorations\build\$count.png") -Destination $imageDirectory
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
