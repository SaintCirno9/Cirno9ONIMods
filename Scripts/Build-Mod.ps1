[CmdletBinding()]
param(
    [string[]]$ModName,
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',
    [switch]$Deploy
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$projects = @(if ($ModName) {
    foreach ($name in $ModName) {
        if ($name -notmatch '^[A-Za-z0-9_]+$') {
            throw "无效的模组名：$name"
        }
        $path = Join-Path $repositoryRoot "Mods/$name/$name.csproj"
        if (!(Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "找不到在用工程：$path"
        }
        [xml]$projectXml = Get-Content -LiteralPath $path -Raw
        if (!$projectXml.Project.Sdk) {
            throw "该工程已停用，不纳入构建和部署：$name"
        }
        $path
    }
} else {
    Join-Path $repositoryRoot 'Cirno9OniMods.sln'
})

foreach ($project in $projects) {
    & dotnet build $project -c $Configuration "-p:DeployMods=$($Deploy.IsPresent.ToString().ToLowerInvariant())" --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "构建失败（退出码 $LASTEXITCODE）：$project"
    }
}
