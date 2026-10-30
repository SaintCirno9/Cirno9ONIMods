# Cirno9OniMods

SaintCirno9 的《缺氧》模组源码仓库。

## 目录

- `Mods/`：全部 20 个模组工程；根目录解决方案只构建当前维护的 12 个工程。
- `Scripts/`：构建、显式部署和游戏源码反编译入口。
- `GameSource/`：原版反编译源码，本机参考资料，不提交。
- `External/ModSources/`：其他 Mod 的参考源码，不提交。
- `Artifacts/bin/<工程>/<配置>/`：程序集、模组元数据和资源。
- `Artifacts/obj/<工程>/`：NuGet 还原及编译中间文件。
- `.tmp/`：临时记录与验证产物，不提交。

`Mods/` 下的 `AdjustableMotionSensor`、`AdjustableRobotMiner`、`BiggerCritterDropOff`、`FunctionEnhancement`、`PassiveLiquidVentInput`、`SuperAnimal`、`SuperConditioner`、`SuperElectrolyzer` 是已停用工程，仅移动目录、内容保留原样，不纳入解决方案或公共构建配置。

## 本机配置

复制 `GamePaths.local.props.example` 为 `GamePaths.local.props`，填写 `GameRootDir`（游戏安装目录）和 `ModDevRoot`（本地 Dev 模组根目录）。使用绝对路径；本机配置已被 Git 忽略。也可通过 MSBuild 的 `-p:GameRootDir=...`、`-p:ModDevRoot=...` 覆盖。

## 构建与部署

默认使用 Release，构建不会写入游戏 Mod 目录：

```powershell
pwsh -File ./Scripts/Build-Mod.ps1
pwsh -File ./Scripts/Build-Mod.ps1 -ModName ColonyTweaks
dotnet build ./Cirno9OniMods.sln -c Release -p:DeployMods=false
```

明确指定 `-Deploy` 才会构建并部署；省略 `-ModName` 时处理全部在用模组：

```powershell
pwsh -File ./Scripts/Build-Mod.ps1 -ModName ColonyTweaks -Deploy
```

部署会重建 `ModDevRoot/<工程名>/`，复制合并后的程序集、元数据及该工程声明的翻译/动画资源。PLib 合并步骤仍由各工程的 `ILRepack.targets` 管理；最终验收使用 Release。构建与部署成功不代表已完成游戏内验收。

## 更新游戏源码

```powershell
pwsh -File ./Scripts/Decompile-GameSource.ps1
```

需要已安装 `ilspycmd`。脚本读取本机 `GameRootDir`，将现有 `GameSource/` 移入回收站后重新反编译；也可传入 `-GameRootDir`。
