using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ZonedSolidTransferArm;

public static class ZonedSolidTransferArmPatches
{
    private const float FetchChoreCacheDuration = 0.5f;
    private static readonly Dictionary<SolidTransferArm, CachedFetchChores> CachedFetchChoresByArm = new();
    private static readonly Dictionary<SolidTransferArm, PickupableAvailability> PickupableAvailabilityByArm = new();
    private static readonly SharedGlobalZonePickupablesCache SharedGlobalZonePickupables = new();

    private sealed class CachedFetchChores
    {
        public HashSet<int> ZoneCells;
        public int ZoneCellCount;
        public float NextRefreshTime;
        public readonly List<FetchChore> FetchChores = new();
        public readonly HashSet<FetchChore> SeenFetchChores = new();
    }

    private sealed class PickupableAvailability
    {
        public readonly HashSet<Tag> PrefabTags = new();
        public bool HasGarbage;

        public void Rebuild(List<Pickupable> pickupables)
        {
            PrefabTags.Clear();
            HasGarbage = false;
            foreach (Pickupable pickupable in pickupables)
            {
                if (pickupable?.KPrefabID == null)
                {
                    continue;
                }

                PrefabTags.Add(pickupable.KPrefabID.PrefabTag);
                if (!HasGarbage && pickupable.KPrefabID.HasTag(GameTags.Garbage))
                {
                    HasGarbage = true;
                }
            }
        }
    }

    private sealed class FetchChoreCacheBuildContext
    {
        public readonly SolidTransferArm Arm;
        public readonly HashSet<int> ZoneCells;
        public readonly List<FetchChore> FetchChores;
        public readonly HashSet<FetchChore> SeenFetchChores;

        public FetchChoreCacheBuildContext(
            SolidTransferArm arm,
            HashSet<int> zoneCells,
            List<FetchChore> fetchChores,
            HashSet<FetchChore> seenFetchChores)
        {
            Arm = arm;
            ZoneCells = zoneCells;
            FetchChores = fetchChores;
            SeenFetchChores = seenFetchChores;
        }
    }

    private sealed class SharedGlobalZonePickupablesCache
    {
        public readonly object SyncRoot = new();
        public int GlobalZoneRevision = -1;
        public float NextRefreshTime;
        public List<Pickupable> Pickupables = new();
    }

    private sealed class SharedGlobalPickupVisitorContext
    {
        public readonly List<Pickupable> Pickupables;
        public readonly HashSet<Pickupable> SeenPickupables;

        public SharedGlobalPickupVisitorContext(List<Pickupable> pickupables, HashSet<Pickupable> seenPickupables)
        {
            Pickupables = pickupables;
            SeenPickupables = seenPickupables;
        }
    }

    private static CachedFetchChores GetCachedFetchChores(SolidTransferArm arm)
    {
        if (!CachedFetchChoresByArm.TryGetValue(arm, out CachedFetchChores cachedFetchChores))
        {
            cachedFetchChores = new CachedFetchChores();
            CachedFetchChoresByArm[arm] = cachedFetchChores;
        }

        return cachedFetchChores;
    }

    private static PickupableAvailability GetPickupableAvailability(SolidTransferArm arm)
    {
        if (!PickupableAvailabilityByArm.TryGetValue(arm, out PickupableAvailability pickupableAvailability))
        {
            pickupableAvailability = new PickupableAvailability();
            PickupableAvailabilityByArm[arm] = pickupableAvailability;
        }

        return pickupableAvailability;
    }

    private static bool UsesOnlyGlobalZone(SolidTransferArm arm)
    {
        return arm.GetComponent<ZonedSolidTransferArmControl>() is { } control && control.UsesOnlyGlobalZone();
    }

    private static bool UsesGlobalZone(SolidTransferArm arm)
    {
        return arm.GetComponent<ZonedSolidTransferArmControl>() is { } control && control.UsesGlobalZone();
    }

    [HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
    public static class PlayerControllerOnPrefabInitPatch
    {
        public static void Postfix(PlayerController __instance)
        {
            List<InterfaceTool> tools = new(__instance.tools);
            GameObject toolObject = new GameObject("ZonedSolidTransferArmZoneTool");
            ZonedSolidTransferArmZoneTool tool = toolObject.AddComponent<ZonedSolidTransferArmZoneTool>();
            toolObject.transform.SetParent(__instance.gameObject.transform);
            toolObject.SetActive(true);
            toolObject.SetActive(false);
            tools.Add(tool);

            GameObject globalToolObject = new GameObject("ZonedSolidTransferArmGlobalZoneTool");
            ZonedSolidTransferArmGlobalZoneTool globalTool = globalToolObject.AddComponent<ZonedSolidTransferArmGlobalZoneTool>();
            globalToolObject.transform.SetParent(__instance.gameObject.transform);
            globalToolObject.SetActive(true);
            globalToolObject.SetActive(false);
            tools.Add(globalTool);

            __instance.tools = tools.ToArray();
        }
    }


    [HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
    public static class ToolMenuCreateBasicToolsPatch
    {
        public static void Postfix(ToolMenu __instance)
        {
            ToolMenu.ToolCollection collection = new(
                ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.NAME),
                "icon_action_store",
                ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.TOOLTIP));
            new ToolMenu.ToolInfo(
                ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.NAME),
                "icon_action_store",
                ZonedSolidTransferArmMod.GlobalZoneAction.GetKAction(),
                "ZonedSolidTransferArmGlobalZoneTool",
                collection,
                ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.TOOLTIP),
                OnSelectGlobalZoneAddTool);
            __instance.basicTools.Add(collection);
        }

        private static void OnSelectGlobalZoneAddTool(object data)
        {
            ZonedSolidTransferArmGlobalZoneTool.SetEditMode(ZonedSolidTransferArmGlobalZoneTool.EditMode.Add);
        }
    }

    [HarmonyPatch(typeof(ToolMenu), "OnSpawn")]
    public static class ToolMenuOnSpawnPatch
    {
        public static void Postfix(ToolMenu __instance)
        {
            foreach (ToolMenu.ToolCollection collection in __instance.basicTools)
            {
                if (collection.tools.Count == 1 &&
                    collection.tools[0].toolName == "ZonedSolidTransferArmGlobalZoneTool" &&
                    collection.toggle != null)
                {
                    collection.toggle.AddOrGet<ZonedSolidTransferArmGlobalZoneToolRightClick>();
                    break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ToolParameterMenu), "CreateToggleGameObject")]
    public static class ToolParameterMenuCreateToggleGameObjectPatch
    {
        public static void Postfix(ToolParameterMenu.ToggleData data, GameObject __result)
        {
            if (!TryGetParameterStrings(data.name, out LocString label, out LocString tooltip))
            {
                return;
            }

            LocText text = __result.GetComponentInChildren<LocText>();
            text.text = ZonedSolidTransferArmStrings.Text(label);

            ToolTip toolTip = __result.GetComponentInChildren<ToolTip>();
            if (toolTip != null)
            {
                toolTip.SetSimpleTooltip(ZonedSolidTransferArmStrings.Text(tooltip));
            }
        }

        private static bool TryGetParameterStrings(string name, out LocString label, out LocString tooltip)
        {
            switch (name)
            {
                case ZonedSolidTransferArmGlobalZoneTool.AddParameter:
                    label = ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.ADDNAME;
                    tooltip = ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.ADDTOOLTIP;
                    return true;
                case ZonedSolidTransferArmGlobalZoneTool.RemoveParameter:
                    label = ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.REMOVENAME;
                    tooltip = ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.REMOVETOOLTIP;
                    return true;
                case ZonedSolidTransferArmGlobalZoneTool.TemporaryConstructionParameter:
                    label = ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.TEMPORARYCONSTRUCTIONNAME;
                    tooltip = ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.TEMPORARYCONSTRUCTIONTOOLTIP;
                    return true;
                case ZonedSolidTransferArmGlobalZoneTool.TemporaryClearParameter:
                    label = ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.TEMPORARYCLEARNAME;
                    tooltip = ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.TEMPORARYCLEARTOOLTIP;
                    return true;
                default:
                    label = null;
                    tooltip = null;
                    return false;
            }
        }
    }

    [HarmonyPatch(typeof(ToolMenu), nameof(ToolMenu.OnKeyUp))]
    public static class ToolMenuOnKeyUpPatch
    {
        public static bool Prefix(KButtonEvent e)
        {
            if (ZonedSolidTransferArmGlobalZoneToolRightClick.ConsumeSuppressNextRightClickCancel() &&
                e.TryConsume(global::Action.MouseRight))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OnKeyDown))]
    public static class PlayerControllerOnKeyDownPatch
    {
        public static bool Prefix(KButtonEvent e)
        {
            if (e.TryConsume(ZonedSolidTransferArmMod.RemoveGlobalZoneAction.GetKAction()))
            {
                ZonedSolidTransferArmGlobalZoneTool.ActivateRemoveMode();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SolidTransferArm), "OnPrefabInit")]
    public static class SolidTransferArmOnPrefabInitPatch
    {
        public static void Postfix(SolidTransferArm __instance)
        {
            if (__instance.GetComponent<RangeVisualizer>() != null)
            {
                __instance.gameObject.AddOrGet<ZonedSolidTransferArmControl>();
                ZonedSolidTransferArmPickFilter.Configure(__instance.gameObject);
                __instance.gameObject.AddOrGet<ZonedSolidTransferArmTemperatureFilter>();
            }
        }
    }

    [HarmonyPatch(typeof(SolidTransferArm), "OnCleanUp")]
    public static class SolidTransferArmOnCleanUpPatch
    {
        public static void Prefix(SolidTransferArm __instance)
        {
            CachedFetchChoresByArm.Remove(__instance);
            PickupableAvailabilityByArm.Remove(__instance);
        }
    }

    [HarmonyPatch(typeof(Constructable), "OnSpawn")]
    public static class ConstructableOnSpawnPatch
    {
        public static void Postfix(Constructable __instance)
        {
            Building building = __instance.GetComponent<Building>();
            if (building == null)
            {
                return;
            }

            ZonedSolidTransferArmGlobalZone.AddTemporaryConstructionCells(building.PlacementCells);
        }
    }

    [HarmonyPatch(typeof(Clearable), nameof(Clearable.MarkForClear))]
    public static class ClearableMarkForClearPatch
    {
        public static void Postfix(Clearable __instance)
        {
            KPrefabID prefabID = __instance.GetComponent<KPrefabID>();
            if (prefabID == null || !prefabID.HasTag(GameTags.Garbage))
            {
                return;
            }

            int cell = Grid.PosToCell(__instance);
            ZonedSolidTransferArmGlobalZone.AddTemporaryClearCell(cell);
        }
    }

    // 建筑本体菜单入口暂时停用，保留实现给以后需要时恢复。
    public static class BuildingCompleteOnPrefabInitPatch
    {
        public static void Postfix(BuildingComplete __instance)
        {
            if (IsTargetBuilding(__instance))
            {
                __instance.gameObject.AddOrGet<ZonedSolidTransferArmGlobalZoneUserMenu>();
            }
        }

        private static bool IsTargetBuilding(BuildingComplete building)
        {
            Storage storage = building.GetComponent<Storage>();
            return storage is { showInUI: true } ||
                   building.GetComponent<ComplexFabricator>() != null ||
                   building.GetComponent<Fabricator>() != null;
        }
    }

    [HarmonyPatch(typeof(TreeFilterableSideScreen), "IsValidForTarget")]
    public static class TreeFilterableSideScreenIsValidForTargetPatch
    {
        public static void Postfix(GameObject target, ref bool __result)
        {
            if (__result && target.GetComponent<ZonedSolidTransferArmPickFilter>() is { } filter)
            {
                __result = filter.IsFilterEnabled();
            }
        }
    }

    [HarmonyPatch(typeof(SolidTransferArm), "FindFetchTarget")]
    public static class SolidTransferArmFindFetchTargetPatch
    {
        private static readonly MethodInfo FindFetchTargetMethod = AccessTools.Method(
            typeof(FetchManager),
            nameof(FetchManager.FindFetchTarget),
            new[] { typeof(List<Pickupable>), typeof(Storage), typeof(FetchChore) });

        private static readonly MethodInfo FilteredFindFetchTargetMethod = AccessTools.Method(
            typeof(SolidTransferArmFindFetchTargetPatch),
            nameof(FindFetchTarget));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(FindFetchTargetMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, FilteredFindFetchTargetMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static Pickupable FindFetchTarget(
            List<Pickupable> pickupables,
            Storage destination,
            FetchChore chore,
            SolidTransferArm arm)
        {
            foreach (Pickupable pickupable in pickupables)
            {
                if (ZonedSolidTransferArmPickFilter.AllowsPickup(arm, pickupable) &&
                    ZonedSolidTransferArmTemperatureFilter.AllowsPickup(arm, pickupable) &&
                    FetchManager.IsFetchablePickup(pickupable, chore, destination))
                {
                    return pickupable;
                }
            }
            return null;
        }
    }

    [HarmonyPatch(typeof(SelectTool), nameof(SelectTool.Activate))]
    public static class SelectToolActivatePatch
    {
        public static void Postfix()
        {
            ZonedSolidTransferArmZoneTool.RestorePendingSelection();
        }
    }

    [HarmonyPatch(typeof(SimDebugView), "OnPrefabInit")]
    public static class SimDebugViewOnPrefabInitPatch
    {
        public static void Postfix(Dictionary<HashedString, Func<SimDebugView, int, Color>> ___getColourFuncs)
        {
            if (!___getColourFuncs.ContainsKey(ZonedSolidTransferArmZoneOverlay.ID))
            {
                ___getColourFuncs.Add(ZonedSolidTransferArmZoneOverlay.ID, ZonedSolidTransferArmZoneOverlay.GetCellColor);
            }
            if (!___getColourFuncs.ContainsKey(ZonedSolidTransferArmGlobalZoneOverlay.ID))
            {
                ___getColourFuncs.Add(ZonedSolidTransferArmGlobalZoneOverlay.ID, ZonedSolidTransferArmGlobalZoneOverlay.GetCellColor);
            }
        }
    }

    [HarmonyPatch(typeof(OverlayScreen), "RegisterModes")]
    public static class OverlayScreenRegisterModesPatch
    {
        private static readonly MethodInfo RegisterModeMethod = AccessTools.Method(typeof(OverlayScreen), "RegisterMode");

        public static void Postfix(OverlayScreen __instance)
        {
            RegisterModeMethod.Invoke(__instance, new object[] { new ZonedSolidTransferArmZoneOverlay() });
            RegisterModeMethod.Invoke(__instance, new object[] { new ZonedSolidTransferArmGlobalZoneOverlay() });
        }
    }

    [HarmonyPatch(typeof(OverlayLegend), "OnSpawn")]
    public static class OverlayLegendOnSpawnPatch
    {
        public static void Prefix(List<OverlayLegend.OverlayInfo> ___overlayInfoList)
        {
            ___overlayInfoList.Add(new OverlayLegend.OverlayInfo
            {
                name = ZonedSolidTransferArmStrings.UI.OVERLAYS.ZONE.NAME.key.String,
                mode = ZonedSolidTransferArmZoneOverlay.ID,
                infoUnits = new List<OverlayLegend.OverlayInfoUnit>(),
                isProgrammaticallyPopulated = true
            });
            ___overlayInfoList.Add(new OverlayLegend.OverlayInfo
            {
                name = ZonedSolidTransferArmStrings.UI.OVERLAYS.GLOBALZONE.NAME.key.String,
                mode = ZonedSolidTransferArmGlobalZoneOverlay.ID,
                infoUnits = new List<OverlayLegend.OverlayInfoUnit>(),
                isProgrammaticallyPopulated = true
            });
        }
    }


    [HarmonyPatch(typeof(SaveGame), "OnPrefabInit")]
    public static class SaveGameOnPrefabInitPatch
    {
        public static void Postfix(SaveGame __instance)
        {
            ZonedSolidTransferArmGlobalZone.Clear();
            __instance.gameObject.AddOrGet<ZonedSolidTransferArmGlobalZoneSaveData>();
        }
    }
    [HarmonyPatch(typeof(SolidTransferArm), "AsyncUpdate")]
    public static class SolidTransferArmAsyncUpdatePatch
    {
        private const int ScenePartitionerNodeSize = 16;
        private static readonly FieldInfo ReachableCellsField = AccessTools.Field(typeof(SolidTransferArm), "reachableCells");
        private static readonly FieldInfo PickupablesField = AccessTools.Field(typeof(SolidTransferArm), "pickupables");
        private static readonly FieldInfo KPrefabIDField = AccessTools.Field(typeof(SolidTransferArm), "kPrefabID");

        public static bool Prefix(SolidTransferArm __instance, ref bool __result)
        {
            if (!ZonedSolidTransferArmControl.TryGetZoneSnapshot(__instance, out HashSet<int> zoneCells))
            {
                return true;
            }

            __result = AsyncUpdateZone(__instance, zoneCells);
            return false;
        }

        private static bool AsyncUpdateZone(SolidTransferArm arm, HashSet<int> zoneCells)
        {
            HashSet<int> reachableCells = (HashSet<int>)ReachableCellsField.GetValue(arm);
            List<Pickupable> pickupables = (List<Pickupable>)PickupablesField.GetValue(arm);
            ZonedSolidTransferArmControl control = arm.GetComponent<ZonedSolidTransferArmControl>();

            ListPool<int, SolidTransferArm>.PooledList oldReachable = ListPool<int, SolidTransferArm>.Allocate();
            oldReachable.AddRange(reachableCells);
            reachableCells.Clear();
            MinionGroupProber.Get().Vacate(oldReachable);

            pickupables.Clear();
            if (control != null && UsesGlobalZone(arm))
            {
                FillPickupablesFromSharedGlobalZone(arm, pickupables);
            }

            if (control is { CellCount: > 0 })
            {
                HashSet<int> localCells = new(control.Cells);
                if (localCells.Count > 0)
                {
                    VisitPickupablesInZoneCells(arm, localCells, pickupables);
                }
            }
            else if (zoneCells.Count > 0 && !UsesOnlyGlobalZone(arm))
            {
                VisitPickupablesInZoneCells(arm, zoneCells, pickupables);
            }

            GetPickupableAvailability(arm).Rebuild(pickupables);

            oldReachable.Recycle();
            return true;
        }

        private static void FillPickupablesFromSharedGlobalZone(SolidTransferArm arm, List<Pickupable> pickupables)
        {
            int carrierID = ((KPrefabID)KPrefabIDField.GetValue(arm)).InstanceID;
            foreach (Pickupable pickupable in GetSharedGlobalZonePickupables())
            {
                if (CanBePickedUpByZonedTransferArm(pickupable, carrierID) &&
                    ZonedSolidTransferArmPickFilter.AllowsPickup(arm, pickupable) &&
                    ZonedSolidTransferArmTemperatureFilter.AllowsPickup(arm, pickupable))
                {
                    pickupables.Add(pickupable);
                }
            }
        }

        private static void VisitPickupablesInZoneCells(SolidTransferArm arm, HashSet<int> reachableCells, List<Pickupable> pickupables)
        {
            PickupVisitorContext context = new PickupVisitorContext(
                arm,
                (KPrefabID)KPrefabIDField.GetValue(arm),
                reachableCells,
                pickupables);
            foreach (Pickupable pickupable in pickupables)
            {
                if (pickupable != null)
                {
                    context.Seen.Add(pickupable);
                }
            }
            HashSet<int> visitedNodes = new();
            foreach (int cell in reachableCells)
            {
                if (!Grid.IsValidCell(cell))
                {
                    continue;
                }

                Grid.CellToXY(cell, out int x, out int y);
                int nodeX = x / ScenePartitionerNodeSize;
                int nodeY = y / ScenePartitionerNodeSize;
                int nodeKey = nodeY * Grid.WidthInCells + nodeX;
                if (!visitedNodes.Add(nodeKey))
                {
                    continue;
                }

                GameScenePartitioner.Instance.ReadonlyVisitEntries(
                    nodeX * ScenePartitionerNodeSize,
                    nodeY * ScenePartitionerNodeSize,
                    ScenePartitionerNodeSize,
                    ScenePartitionerNodeSize,
                    GameScenePartitioner.Instance.pickupablesLayer,
                    VisitPickupable,
                    context);
                GameScenePartitioner.Instance.ReadonlyVisitEntries(
                    nodeX * ScenePartitionerNodeSize,
                    nodeY * ScenePartitionerNodeSize,
                    ScenePartitionerNodeSize,
                    ScenePartitionerNodeSize,
                    GameScenePartitioner.Instance.storedPickupablesLayer,
                    VisitPickupable,
                    context);
            }
        }

        private static Util.IterationInstruction VisitPickupable(object obj, PickupVisitorContext context)
        {
            if (obj is not Pickupable pickupable)
            {
                return Util.IterationInstruction.Continue;
            }

            bool inZone = context.ReachableCells.Contains(pickupable.cachedCell);
            bool conveyable = Assets.IsTagSolidTransferArmConveyable(pickupable.KPrefabID.PrefabTag);
            bool canTransfer = CanBePickedUpByZonedTransferArm(pickupable, context.KPrefabID.InstanceID);
            bool allowedByFilter = ZonedSolidTransferArmPickFilter.AllowsPickup(context.Arm, pickupable);
            bool allowedByTemperature = ZonedSolidTransferArmTemperatureFilter.AllowsPickup(context.Arm, pickupable);
            if (inZone && conveyable && canTransfer && allowedByFilter && allowedByTemperature)
            {
                if (context.Seen.Add(pickupable))
                {
                    context.Pickupables.Add(pickupable);
                }
            }
            return Util.IterationInstruction.Continue;
        }

        private static bool CanBePickedUpByZonedTransferArm(Pickupable pickupable, int carrierID)
        {
            if (pickupable.UnreservedFetchAmount <= 0f && pickupable.FindReservedAmount(carrierID) <= 0f)
            {
                return false;
            }
            if (pickupable.IsEntombed)
            {
                return false;
            }
            if (pickupable.KPrefabID.HasTag(GameTags.StoredPrivate))
            {
                return false;
            }
            if (pickupable.KPrefabID.HasTag(GameTags.Creatures.ReservedByCreature))
            {
                return false;
            }
            if (pickupable.KPrefabID.HasTag(GameTags.Creature) &&
                !pickupable.KPrefabID.HasTag(GameTags.Creatures.Deliverable))
            {
                return false;
            }
            return true;
        }

        private class PickupVisitorContext
        {
            public readonly SolidTransferArm Arm;
            public readonly KPrefabID KPrefabID;
            public readonly HashSet<int> ReachableCells;
            public readonly List<Pickupable> Pickupables;
            public readonly HashSet<Pickupable> Seen = new();

            public PickupVisitorContext(
                SolidTransferArm arm,
                KPrefabID kPrefabID,
                HashSet<int> reachableCells,
                List<Pickupable> pickupables)
            {
                Arm = arm;
                KPrefabID = kPrefabID;
                ReachableCells = reachableCells;
                Pickupables = pickupables;
            }
        }

        private static IEnumerable<Pickupable> GetSharedGlobalZonePickupables()
        {
            float currentGameTime = GameClock.Instance?.GetTime() ?? 0f;
            int globalZoneRevision = ZonedSolidTransferArmGlobalZone.Revision;
            if (SharedGlobalZonePickupables.GlobalZoneRevision != globalZoneRevision ||
                currentGameTime >= SharedGlobalZonePickupables.NextRefreshTime)
            {
                lock (SharedGlobalZonePickupables.SyncRoot)
                {
                    if (SharedGlobalZonePickupables.GlobalZoneRevision != globalZoneRevision ||
                        currentGameTime >= SharedGlobalZonePickupables.NextRefreshTime)
                    {
                        RebuildSharedGlobalZonePickupables(currentGameTime, globalZoneRevision);
                    }
                }
            }

            return SharedGlobalZonePickupables.Pickupables;
        }

        private static void RebuildSharedGlobalZonePickupables(float currentGameTime, int globalZoneRevision)
        {
            List<Pickupable> pickupables = new();
            HashSet<Pickupable> seenPickupables = new();

            HashSet<int> visitedNodes = new();
            SharedGlobalPickupVisitorContext sharedContext = new(pickupables, seenPickupables);
            foreach (int cell in ZonedSolidTransferArmGlobalZone.MarkedCells)
            {
                if (!Grid.IsValidCell(cell))
                {
                    continue;
                }

                Grid.CellToXY(cell, out int x, out int y);
                int nodeX = x / ScenePartitionerNodeSize;
                int nodeY = y / ScenePartitionerNodeSize;
                int nodeKey = nodeY * Grid.WidthInCells + nodeX;
                if (!visitedNodes.Add(nodeKey))
                {
                    continue;
                }

                GameScenePartitioner.Instance.ReadonlyVisitEntries(
                    nodeX * ScenePartitionerNodeSize,
                    nodeY * ScenePartitionerNodeSize,
                    ScenePartitionerNodeSize,
                    ScenePartitionerNodeSize,
                    GameScenePartitioner.Instance.pickupablesLayer,
                    VisitSharedGlobalPickupable,
                    sharedContext);
                GameScenePartitioner.Instance.ReadonlyVisitEntries(
                    nodeX * ScenePartitionerNodeSize,
                    nodeY * ScenePartitionerNodeSize,
                    ScenePartitionerNodeSize,
                    ScenePartitionerNodeSize,
                    GameScenePartitioner.Instance.storedPickupablesLayer,
                    VisitSharedGlobalPickupable,
                    sharedContext);
            }

            SharedGlobalZonePickupables.Pickupables = pickupables;
            SharedGlobalZonePickupables.GlobalZoneRevision = globalZoneRevision;
            SharedGlobalZonePickupables.NextRefreshTime = currentGameTime + FetchChoreCacheDuration;
        }

        private static Util.IterationInstruction VisitSharedGlobalPickupable(
            object obj,
            SharedGlobalPickupVisitorContext state)
        {
            if (obj is not Pickupable pickupable)
            {
                return Util.IterationInstruction.Continue;
            }

            if (!ZonedSolidTransferArmGlobalZone.ContainsCell(pickupable.cachedCell))
            {
                return Util.IterationInstruction.Continue;
            }

            if (!Assets.IsTagSolidTransferArmConveyable(pickupable.KPrefabID.PrefabTag))
            {
                return Util.IterationInstruction.Continue;
            }

            if (state.SeenPickupables.Add(pickupable))
            {
                state.Pickupables.Add(pickupable);
            }

            return Util.IterationInstruction.Continue;
        }
    }

    [HarmonyPatch(typeof(SolidTransferArm), nameof(SolidTransferArm.IsCellReachable))]
    public static class SolidTransferArmIsCellReachablePatch
    {
        public static void Postfix(SolidTransferArm __instance, int cell, ref bool __result)
        {
            if (ZonedSolidTransferArmControl.TryGetZoneSnapshot(__instance, out HashSet<int> zoneCells))
            {
                __result = zoneCells.Contains(cell);
            }
        }
    }

    [HarmonyPatch(typeof(ChoreConsumer), nameof(ChoreConsumer.FindNextChore))]
    public static class ChoreConsumerFindNextChorePatch
    {
        private static readonly FieldInfo PreconditionSnapshotField =
            AccessTools.Field(typeof(ChoreConsumer), "preconditionSnapshot");
        private static readonly FieldInfo LastSuccessfulPreconditionSnapshotField =
            AccessTools.Field(typeof(ChoreConsumer), "lastSuccessfulPreconditionSnapshot");
        private static readonly MethodInfo ChooseChoreMethod = AccessTools.Method(typeof(ChoreConsumer), "ChooseChore");
        private const int ScenePartitionerNodeSize = 16;

        public static bool Prefix(ChoreConsumer __instance, ref Chore.Precondition.Context out_context, ref bool __result)
        {
            if (__instance.consumerState?.solidTransferArm == null ||
                !ZonedSolidTransferArmControl.TryGetZoneSnapshot(
                    __instance.consumerState.solidTransferArm,
                    out HashSet<int> zoneCells))
            {
                return true;
            }

            __result = FindNextZoneChore(__instance, zoneCells, ref out_context);
            return false;
        }

        private static bool FindNextZoneChore(
            ChoreConsumer consumer,
            HashSet<int> zoneCells,
            ref Chore.Precondition.Context outContext)
        {
            ChoreConsumer.PreconditionSnapshot preconditionSnapshot =
                (ChoreConsumer.PreconditionSnapshot)PreconditionSnapshotField.GetValue(consumer);
            ChoreConsumer.PreconditionSnapshot lastSuccessfulPreconditionSnapshot =
                (ChoreConsumer.PreconditionSnapshot)LastSuccessfulPreconditionSnapshotField.GetValue(consumer);
            preconditionSnapshot.Clear();
            consumer.consumerState.Refresh();

            if (zoneCells.Count > 0)
            {
                foreach (FetchChore fetchChore in GetFetchChoresInZone(consumer.consumerState.solidTransferArm, zoneCells))
                {
                    VisitFetchChore(fetchChore, consumer);
                }
            }

            preconditionSnapshot.succeededContexts.Sort();
            List<Chore.Precondition.Context> succeededContexts = preconditionSnapshot.succeededContexts;
            object[] args = { outContext, succeededContexts };
            bool found = (bool)ChooseChoreMethod.Invoke(consumer, args);
            outContext = (Chore.Precondition.Context)args[0];
            if (found)
            {
                preconditionSnapshot.CopyTo(lastSuccessfulPreconditionSnapshot);
            }

            return found;
        }

        private static IEnumerable<FetchChore> GetFetchChoresInZone(SolidTransferArm arm, HashSet<int> zoneCells)
        {
            CachedFetchChores cachedFetchChores = GetCachedFetchChores(arm);
            float currentGameTime = GameClock.Instance?.GetTime() ?? 0f;
            bool needsRefresh = cachedFetchChores.ZoneCells != zoneCells ||
                cachedFetchChores.ZoneCellCount != zoneCells.Count ||
                currentGameTime >= cachedFetchChores.NextRefreshTime;
            if (needsRefresh)
            {
                RebuildFetchChoreCache(arm, zoneCells, cachedFetchChores, currentGameTime);
            }

            return cachedFetchChores.FetchChores;
        }

        private static void RebuildFetchChoreCache(
            SolidTransferArm arm,
            HashSet<int> zoneCells,
            CachedFetchChores cachedFetchChores,
            float currentGameTime)
        {
            cachedFetchChores.FetchChores.Clear();
            cachedFetchChores.SeenFetchChores.Clear();

            HashSet<int> visitedNodes = new();
            FetchChoreCacheBuildContext context = new(
                arm,
                zoneCells,
                cachedFetchChores.FetchChores,
                cachedFetchChores.SeenFetchChores);
            foreach (int cell in zoneCells)
            {
                if (!Grid.IsValidCell(cell))
                {
                    continue;
                }

                Grid.CellToXY(cell, out int x, out int y);
                int nodeX = x / ScenePartitionerNodeSize;
                int nodeY = y / ScenePartitionerNodeSize;
                int nodeKey = nodeY * Grid.WidthInCells + nodeX;
                if (!visitedNodes.Add(nodeKey))
                {
                    continue;
                }

                GameScenePartitioner.Instance.VisitEntries(
                    nodeX * ScenePartitionerNodeSize,
                    nodeY * ScenePartitionerNodeSize,
                    ScenePartitionerNodeSize,
                    ScenePartitionerNodeSize,
                    GameScenePartitioner.Instance.fetchChoreLayer,
                    VisitFetchChoreCacheBuilder,
                    context);
            }

            cachedFetchChores.ZoneCells = zoneCells;
            cachedFetchChores.ZoneCellCount = zoneCells.Count;
            cachedFetchChores.NextRefreshTime = currentGameTime + FetchChoreCacheDuration;
        }

        private static Util.IterationInstruction VisitFetchChoreCacheBuilder(object obj, FetchChoreCacheBuildContext context)
        {
            if (obj is not FetchChore fetchChore || fetchChore.target == null || fetchChore.isNull)
            {
                return Util.IterationInstruction.Continue;
            }

            int cell = Grid.PosToCell(fetchChore.gameObject);
            if (context.ZoneCells.Contains(cell) && context.SeenFetchChores.Add(fetchChore))
            {
                context.FetchChores.Add(fetchChore);
            }

            return Util.IterationInstruction.Continue;
        }

        private static void VisitFetchChore(FetchChore fetchChore, ChoreConsumer consumer)
        {
            if (fetchChore.target == null || fetchChore.isNull)
            {
                return;
            }

            if (!ShouldEvaluateFetchChore(consumer.consumerState.solidTransferArm, fetchChore))
            {
                return;
            }

            ChoreConsumer.PreconditionSnapshot preconditionSnapshot =
                (ChoreConsumer.PreconditionSnapshot)PreconditionSnapshotField.GetValue(consumer);
            fetchChore.CollectChoresFromGlobalChoreProvider(
                consumer.consumerState,
                preconditionSnapshot.succeededContexts,
                preconditionSnapshot.failedContexts,
                false);
        }

        private static bool ShouldEvaluateFetchChore(SolidTransferArm arm, FetchChore fetchChore)
        {
            if (!PickupableAvailabilityByArm.TryGetValue(arm, out PickupableAvailability pickupableAvailability))
            {
                return true;
            }

            if (fetchChore.requiredTag == GameTags.Garbage && !pickupableAvailability.HasGarbage)
            {
                return false;
            }

            if (fetchChore.criteria != FetchChore.MatchCriteria.MatchID)
            {
                return true;
            }

            foreach (Tag tag in fetchChore.tags)
            {
                if (pickupableAvailability.PrefabTags.Contains(tag))
                {
                    return true;
                }
            }

            return false;
        }
    }
}


