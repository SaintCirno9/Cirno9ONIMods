$GameDll = "C:\Games\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll"
$OutputPath = Join-Path $PSScriptRoot "GameSource"

if (Test-Path $OutputPath) {
    Remove-Item -LiteralPath $OutputPath -Recurse -Force
}

ilspycmd -p -o $OutputPath $GameDll
