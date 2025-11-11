$projectPath = "$PSScriptRoot/../src/Commandry.Mcp.StdIO"
$csprojPath = "$projectPath/Commandry.Mcp.StdIO.csproj"
$binPath = "$projectPath/bin/Release"
$releasesDirectory = "$PSScriptRoot/../release"

$releaseVersion = ([xml](Get-Content $csprojPath)).Project.PropertyGroup.Version
$releasePlatform = "win-x64"
$releaseBinName = "commandry-mcp-stdio"
$releaseShortName = "$releaseBinName-$releasePlatform"
$releaseFullName = "$releaseBinName-$releaseVersion-$releasePlatform"
$releasePath = "$releasesDirectory/$releaseFullName"
$releaseLatestPath = "$releasesDirectory/$releaseShortName"

New-Item -ItemType Directory -Path $releasesDirectory -ErrorAction:SilentlyContinue
New-Item -ItemType Directory -Path $releasePath -Force
New-Item -ItemType Junction -Path $releaseLatestPath -Target $binPath -Force

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')