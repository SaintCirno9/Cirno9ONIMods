using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace TemporaryFixes
{
    public sealed class ModEntry : UserMod2
    {
        private static bool customizeBuildingsPatched;
        private static bool insulatedAirLockPatched;
        private static bool roomsExpandedDecorPlantsPatched;
        private static bool doorSelfSealingEnabled;
        private static string insulatedAirLockId;
        private static MethodInfo onSimDoorOpenedMethod;
        private static MethodInfo onSimDoorClosedMethod;

        public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<KMod.Mod> mods)
        {
            base.OnAllModsLoaded(harmony, mods);
            TryPatchCustomizeBuildings(harmony);
            TryPatchInsulatedSelfSealingAirLock(harmony);
            TryFixRoomsExpandedDecorativePlants();
        }

        private static void TryPatchCustomizeBuildings(Harmony harmony)
        {
            if (customizeBuildingsPatched)
            {
                return;
            }

            Assembly customizeBuildingsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, "CustomizeBuildings", StringComparison.Ordinal));
            if (customizeBuildingsAssembly == null)
            {
                return;
            }

            doorSelfSealingEnabled = ReadDoorSelfSealingEnabled();
            if (!doorSelfSealingEnabled)
            {
                Debug.Log("[TemporaryFixes] Customize Buildings 的 DoorSelfSealing 当前未启用，跳过补丁。");
                customizeBuildingsPatched = true;
                return;
            }

            Type patchType = customizeBuildingsAssembly.GetType("CustomizeBuildings.DoorSelfSealing_Patch", false);
            MethodInfo targetMethod = AccessTools.Method(typeof(Door), "SetSimState", new[] { typeof(bool), typeof(IList<int>) });
            MethodInfo originalPrefix = patchType == null ? null : AccessTools.Method(patchType, "Prefix", new[] { typeof(bool), typeof(IList<int>), typeof(Door) });
            MethodInfo replacementPrefix = AccessTools.Method(typeof(ModEntry), nameof(CustomizeBuildingsDoorSetSimStatePrefix));
            onSimDoorOpenedMethod = AccessTools.Method(typeof(Door), "OnSimDoorOpened");
            onSimDoorClosedMethod = AccessTools.Method(typeof(Door), "OnSimDoorClosed");

            if (targetMethod == null || originalPrefix == null || replacementPrefix == null || onSimDoorOpenedMethod == null || onSimDoorClosedMethod == null)
            {
                Debug.LogWarning("[TemporaryFixes] 未能定位 Customize Buildings 的 DoorSelfSealing 补丁。");
                return;
            }

            harmony.Unpatch(targetMethod, originalPrefix);
            harmony.Patch(targetMethod, prefix: new HarmonyMethod(replacementPrefix) { priority = Priority.First });
            customizeBuildingsPatched = true;
            Debug.Log("[TemporaryFixes] 已替换 Customize Buildings 的 DoorSelfSealing 补丁，兼容新的 SetCellProperties 签名。");
        }

        private static void TryPatchInsulatedSelfSealingAirLock(Harmony harmony)
        {
            if (insulatedAirLockPatched)
            {
                return;
            }

            Assembly airLockAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, "ONIInsulatedSelfSealingAirLock", StringComparison.Ordinal));
            if (airLockAssembly == null)
            {
                return;
            }

            Type configType = airLockAssembly.GetType("ONIInsulatedSelfSealingAirLock.InsulatedSelfSealingAirLockConfig", false);

            MethodInfo setSimStateMethod = AccessTools.Method(typeof(Door), "SetSimState", new[] { typeof(bool), typeof(IList<int>) });
            MethodInfo onSpawnMethod = AccessTools.Method(typeof(Door), "OnSpawn");
            MethodInfo applyControlStateMethod = AccessTools.Method(typeof(Door), "ApplyControlState", new[] { typeof(bool) });
            MethodInfo onCleanUpMethod = AccessTools.Method(typeof(Door), "OnCleanUp");

            MethodInfo newSetSimStatePostfix = AccessTools.Method(typeof(ModEntry), nameof(InsulatedAirLockSetSimStatePostfix));
            MethodInfo newOnSpawnPostfix = AccessTools.Method(typeof(ModEntry), nameof(InsulatedAirLockOnSpawnPostfix));
            MethodInfo newApplyControlStatePostfix = AccessTools.Method(typeof(ModEntry), nameof(InsulatedAirLockApplyControlStatePostfix));
            MethodInfo newOnCleanUpPrefix = AccessTools.Method(typeof(ModEntry), nameof(InsulatedAirLockOnCleanUpPrefix));

            if (setSimStateMethod == null
                || onSpawnMethod == null
                || applyControlStateMethod == null
                || onCleanUpMethod == null
                || newSetSimStatePostfix == null
                || newOnSpawnPostfix == null
                || newApplyControlStatePostfix == null
                || newOnCleanUpPrefix == null
                || configType == null)
            {
                Debug.LogWarning("[TemporaryFixes] 未能定位 ONIInsulatedSelfSealingAirLock 的补丁入口。");
                return;
            }

            FieldInfo idField = AccessTools.Field(configType, "ID");
            insulatedAirLockId = idField?.GetValue(null) as string;
            if (string.IsNullOrEmpty(insulatedAirLockId))
            {
                Debug.LogWarning("[TemporaryFixes] 未能读取 ONIInsulatedSelfSealingAirLock 的建筑 ID。");
                return;
            }

            UnpatchAssemblyMethods(harmony, setSimStateMethod, airLockAssembly, PatchMethodType.Postfix);
            UnpatchAssemblyMethods(harmony, onSpawnMethod, airLockAssembly, PatchMethodType.Postfix);
            UnpatchAssemblyMethods(harmony, applyControlStateMethod, airLockAssembly, PatchMethodType.Postfix);
            UnpatchAssemblyMethods(harmony, onCleanUpMethod, airLockAssembly, PatchMethodType.Prefix);

            harmony.Patch(setSimStateMethod, postfix: new HarmonyMethod(newSetSimStatePostfix));
            harmony.Patch(onSpawnMethod, postfix: new HarmonyMethod(newOnSpawnPostfix));
            harmony.Patch(applyControlStateMethod, postfix: new HarmonyMethod(newApplyControlStatePostfix));
            harmony.Patch(onCleanUpMethod, prefix: new HarmonyMethod(newOnCleanUpPrefix));

            insulatedAirLockPatched = true;
            Debug.Log("[TemporaryFixes] 已替换 ONIInsulatedSelfSealingAirLock 的旧门补丁，兼容新的 SetCellProperties 签名。");
        }

        // RoomsExpanded 的植物园把装饰植物写成硬编码白名单，漏收了静杯花(WaterCups)，
        // 导致其无法计入"4 种不同装饰植物"。这里反射补回白名单。
        // 注意：发布版经 ILRepack 合并后程序集名是 RoomsExpandedMerged，但类型全名仍为
        // RoomsExpanded.RoomModdedConstraints，故直接按类型全名跨所有程序集查找，不匹配程序集名。
        private static void TryFixRoomsExpandedDecorativePlants()
        {
            if (roomsExpandedDecorPlantsPatched)
            {
                return;
            }

            Type constraintsType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("RoomsExpanded.RoomModdedConstraints", false))
                .FirstOrDefault(type => type != null);
            if (constraintsType == null)
            {
                return;
            }

            FieldInfo namesField = AccessTools.Field(constraintsType, "DecorativeNames");
            if (!(namesField?.GetValue(null) is List<string> decorativeNames))
            {
                Debug.LogWarning("[TemporaryFixes] 未能定位 RoomsExpanded 的 DecorativeNames 白名单。");
                return;
            }

            roomsExpandedDecorPlantsPatched = true;

            if (decorativeNames.Contains("WaterCups"))
            {
                return;
            }

            decorativeNames.Add("WaterCups");
            Debug.Log("[TemporaryFixes] 已向 RoomsExpanded 植物园白名单补入静杯花(WaterCups)。");
        }

        private static bool CustomizeBuildingsDoorSetSimStatePrefix(bool is_door_open, IList<int> cells, Door __instance)
        {
            if (!doorSelfSealingEnabled || __instance == null || cells == null || cells.Count == 0)
            {
                return true;
            }

            if (__instance.doorType == Door.DoorType.Internal || __instance.CurrentState == Door.ControlState.Opened)
            {
                return true;
            }

            PrimaryElement primaryElement = __instance.GetComponent<PrimaryElement>();
            if (primaryElement == null)
            {
                return true;
            }

            float mass = primaryElement.Mass / cells.Count;
            for (int i = 0; i < cells.Count; i++)
            {
                int cell = cells[i];
                World.Instance.groundRenderer.MarkDirty(cell);
                SimMessages.SetCellProperties(cell, 4, -1);
                SimMessages.SetInsulation(cell, __instance.insulationModifier);

                System.Action callback = CreateDoorCallback(__instance, is_door_open);
                if (callback == null)
                {
                    return true;
                }

                HandleVector<Game.CallbackInfo>.Handle callbackHandle = Game.Instance.callbackManager.Add(new Game.CallbackInfo(callback, false));
                SimMessages.ReplaceAndDisplaceElement(
                    cell,
                    primaryElement.ElementID,
                    is_door_open ? CellEventLogger.Instance.DoorOpen : CellEventLogger.Instance.DoorClose,
                    mass,
                    primaryElement.Temperature,
                    byte.MaxValue,
                    0,
                    callbackHandle.index);
            }

            return false;
        }

        private static void InsulatedAirLockSetSimStatePostfix(Door __instance, IList<int> cells)
        {
            if (!IsInsulatedAirLock(__instance))
            {
                return;
            }

            ApplyInsulatedAirLockSimState(__instance, cells);
        }

        private static void InsulatedAirLockOnSpawnPostfix(Door __instance)
        {
            if (!IsInsulatedAirLock(__instance))
            {
                return;
            }

            ApplyInsulatedAirLockSimState(__instance, __instance.building.PlacementCells);
        }

        private static void InsulatedAirLockApplyControlStatePostfix(Door __instance, bool force)
        {
            if (!force || !IsInsulatedAirLock(__instance))
            {
                return;
            }

            SyncInsulatedAirLockStateMachineAfterLoad(__instance);
        }

        private static void InsulatedAirLockOnCleanUpPrefix(Door __instance)
        {
            if (!IsInsulatedAirLock(__instance))
            {
                return;
            }

            foreach (int cell in __instance.building.PlacementCells)
            {
                SimMessages.ClearCellProperties(cell, 7, -1);
            }
        }

        private static bool IsInsulatedAirLock(Door door)
        {
            return door != null
                && !string.IsNullOrEmpty(insulatedAirLockId)
                && door.PrefabID() == insulatedAirLockId;
        }

        private static void ApplyInsulatedAirLockSimState(Door door, IList<int> cells)
        {
            bool allowTransmission = door.CurrentState == Door.ControlState.Opened;
            foreach (int cell in cells)
            {
                if (allowTransmission)
                {
                    SimMessages.SetInsulation(cell, 1f);
                    SimMessages.ClearCellProperties(cell, 7, -1);
                }
                else
                {
                    SimMessages.SetInsulation(cell, 0f);
                    SimMessages.SetCellProperties(cell, 7, -1);
                }
            }
        }

        private static void SyncInsulatedAirLockStateMachineAfterLoad(Door door)
        {
            if (door == null || door.CurrentState == Door.ControlState.Opened)
            {
                return;
            }

            Door.Controller.Instance controller = Traverse.Create(door).Field("controller").GetValue<Door.Controller.Instance>();
            if (controller == null || !NeedsInsulatedAirLockPostLoadSync(door, controller))
            {
                return;
            }

            Traverse.Create(door).Field("openCount").SetValue(0);
            controller.sm.isOpen.Set(false, controller);
            if (!controller.IsInsideState(controller.sm.closed))
            {
                controller.GoTo(controller.sm.closed);
            }

            ApplyInsulatedAirLockSimState(door, door.building.PlacementCells);
        }

        private static bool NeedsInsulatedAirLockPostLoadSync(Door door, Door.Controller.Instance controller)
        {
            if (door.CurrentState == Door.ControlState.Opened)
            {
                return false;
            }

            if (controller.IsInsideState(controller.sm.closed))
            {
                return false;
            }

            if (door.CurrentState == Door.ControlState.Locked
                && (controller.IsInsideState(controller.sm.locked) || controller.IsInsideState(controller.sm.locking)))
            {
                return false;
            }

            return true;
        }

        private static bool ReadDoorSelfSealingEnabled()
        {
            try
            {
                string configPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Klei",
                    "OxygenNotIncluded",
                    "mods",
                    "CustomizeBuildings.json");
                if (!File.Exists(configPath))
                {
                    return false;
                }

                string json = File.ReadAllText(configPath);
                return Regex.IsMatch(json, "\"DoorSelfSealing\"\\s*:\\s*true", RegexOptions.CultureInvariant);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[TemporaryFixes] 读取 CustomizeBuildings.json 失败: " + ex.Message);
                return false;
            }
        }

        private static System.Action CreateDoorCallback(Door door, bool isDoorOpen)
        {
            MethodInfo callbackMethod = isDoorOpen ? onSimDoorOpenedMethod : onSimDoorClosedMethod;
            return callbackMethod == null ? null : (System.Action)Delegate.CreateDelegate(typeof(System.Action), door, callbackMethod);
        }

        private static void UnpatchAssemblyMethods(Harmony harmony, MethodInfo targetMethod, Assembly patchAssembly, PatchMethodType patchType)
        {
            Patches patchInfo = Harmony.GetPatchInfo(targetMethod);
            if (patchInfo == null)
            {
                return;
            }

            IEnumerable<Patch> patches = patchType switch
            {
                PatchMethodType.Prefix => patchInfo.Prefixes,
                PatchMethodType.Postfix => patchInfo.Postfixes,
                PatchMethodType.Transpiler => patchInfo.Transpilers,
                PatchMethodType.Finalizer => patchInfo.Finalizers,
                _ => Enumerable.Empty<Patch>()
            };

            foreach (Patch patch in patches.ToList())
            {
                MethodInfo patchMethod = patch.PatchMethod;
                if (patchMethod?.DeclaringType?.Assembly == patchAssembly)
                {
                    harmony.Unpatch(targetMethod, patchMethod);
                    Debug.Log("[TemporaryFixes] 已卸载旧补丁: " + targetMethod.Name + " <- " + patchMethod.DeclaringType.FullName + "." + patchMethod.Name);
                }
            }
        }

        private enum PatchMethodType
        {
            Prefix,
            Postfix,
            Transpiler,
            Finalizer
        }
    }
}
