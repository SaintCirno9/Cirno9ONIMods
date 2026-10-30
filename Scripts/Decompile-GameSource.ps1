[CmdletBinding()]
param([string]$GameRootDir)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (!$GameRootDir) {
    $configPath = Join-Path $repositoryRoot 'GamePaths.local.props'
    if (!(Test-Path -LiteralPath $configPath -PathType Leaf)) {
        throw '请先配置 GamePaths.local.props 中的 GameRootDir，或传入 -GameRootDir。'
    }
    [xml]$config = Get-Content -LiteralPath $configPath -Raw
    $GameRootDir = [string]$config.Project.PropertyGroup.GameRootDir
}
if (!$GameRootDir) {
    throw 'GameRootDir 不能为空。'
}
$gameDll = Join-Path $GameRootDir 'OxygenNotIncluded_Data/Managed/Assembly-CSharp.dll'
if (!(Test-Path -LiteralPath $gameDll -PathType Leaf)) {
    throw "找不到游戏程序集：$gameDll"
}
$null = Get-Command ilspycmd -ErrorAction Stop
$outputPath = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'GameSource'))
if ([IO.Path]::GetDirectoryName($outputPath) -ne [IO.Path]::GetFullPath($repositoryRoot)) {
    throw "反编译目标不在仓库根目录内：$outputPath"
}
if (Test-Path -LiteralPath $outputPath) {
    Write-Host "将现有反编译源码移入回收站：$outputPath"
    Add-Type -AssemblyName Microsoft.VisualBasic.Core
    [Microsoft.VisualBasic.FileIO.FileSystem]::DeleteDirectory(
        $outputPath,
        [Microsoft.VisualBasic.FileIO.UIOption]::OnlyErrorDialogs,
        [Microsoft.VisualBasic.FileIO.RecycleOption]::SendToRecycleBin
    )
}
& ilspycmd -p -o $outputPath $gameDll
if ($LASTEXITCODE -ne 0) {
    throw "反编译失败（退出码 $LASTEXITCODE）：$gameDll"
}
