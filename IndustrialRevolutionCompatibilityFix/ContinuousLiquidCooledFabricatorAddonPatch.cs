using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace IndustrialRevolutionCompatibilityFix
{
    [HarmonyPatch]
    internal static class ContinuousLiquidCooledFabricatorAddonPatch
    {
        private const string TargetTypeName = "RonivansLegacy_ChemicalProcessing.Content.Scripts.CustomComplexFabricators.ContinuousLiquidCooledFabricatorAddon";
        private static readonly ConditionalWeakTable<object, StatusGuids> StatusGuidsByAddon = new ConditionalWeakTable<object, StatusGuids>();

        private static bool Prepare()
        {
            return AccessTools.TypeByName(TargetTypeName) != null;
        }

        private static MethodBase TargetMethod()
        {
            Type targetType = AccessTools.TypeByName(TargetTypeName);
            return targetType == null ? null : AccessTools.Method(targetType, "UpdatePipeConnectedState");
        }

        private static bool Prefix(object __instance)
        {
            ConduitType type = ReadField<ConduitType>(__instance, "type");
            int inputCell = ReadField<int>(__instance, "inputCell");
            int outputCell = ReadField<int>(__instance, "outputCell");
            Operational operational = ReadField<Operational>(__instance, "operational");
            KSelectable selectable = ReadField<KSelectable>(__instance, "selectable");

            ConduitFlow flowManager = Conduit.GetFlowManager(type);
            bool inputConnected = flowManager.HasConduit(inputCell);
            bool outputConnected = flowManager.HasConduit(outputCell);

            operational.SetFlag(GetInputConnectedFlag(), inputConnected);
            operational.SetFlag(GetOutputConnectedFlag(), outputConnected);

            StatusGuids statusGuids = StatusGuidsByAddon.GetOrCreateValue(__instance);

            StatusItem inputStatusItem = GetInputStatusItem(type);
            RemoveDuplicateStatusItems(selectable, inputStatusItem, statusGuids.InputGuid);
            statusGuids.InputGuid = selectable.ToggleStatusItem(
                inputStatusItem,
                statusGuids.InputGuid,
                !inputConnected,
                new Tuple<ConduitType, Tag>(type, GameTags.Any));

            StatusItem outputStatusItem = GetOutputStatusItem(type);
            Guid outputGuid = ReadField<Guid>(__instance, "hasPipeOutputGuid");
            RemoveDuplicateStatusItems(selectable, outputStatusItem, outputGuid);
            outputGuid = selectable.ToggleStatusItem(
                outputStatusItem,
                outputGuid,
                !outputConnected,
                new Tuple<ConduitType, List<Tag>>(type, new List<Tag> { GameTags.Any }));
            WriteField(__instance, "hasPipeOutputGuid", outputGuid);

            return false;
        }

        private static StatusItem GetInputStatusItem(ConduitType type)
        {
            return Db.Get().BuildingStatusItems.Get(type switch
            {
                ConduitType.Gas => "M_NeedGasIn",
                ConduitType.Liquid => "M_NeedLiquidIn",
                ConduitType.Solid => "M_NeedSolidIn",
                _ => "M_NeedLiquidIn"
            });
        }

        private static StatusItem GetOutputStatusItem(ConduitType type)
        {
            return Db.Get().BuildingStatusItems.Get(type switch
            {
                ConduitType.Gas => "M_NeedGasOut",
                ConduitType.Liquid => "M_NeedLiquidOut",
                ConduitType.Solid => "M_NeedSolidOut",
                _ => "M_NeedLiquidOut"
            });
        }

        private static Operational.Flag GetInputConnectedFlag()
        {
            return (Operational.Flag)AccessTools.Field(typeof(RequireInputs), "inputConnectedFlag").GetValue(null);
        }

        private static Operational.Flag GetOutputConnectedFlag()
        {
            return (Operational.Flag)AccessTools.Field(typeof(RequireOutputs), "outputConnectedFlag").GetValue(null);
        }

        private static void RemoveDuplicateStatusItems(KSelectable selectable, StatusItem statusItem, Guid keepGuid)
        {
            StatusItemGroup statusItemGroup = selectable.GetStatusItemGroup();
            if (statusItemGroup == null)
            {
                return;
            }

            List<Guid> removeGuids = new List<Guid>();
            foreach (StatusItemGroup.Entry entry in statusItemGroup)
            {
                if (entry.item != null && entry.item.Id == statusItem.Id && entry.id != keepGuid)
                {
                    removeGuids.Add(entry.id);
                }
            }

            foreach (Guid removeGuid in removeGuids)
            {
                selectable.RemoveStatusItem(removeGuid);
            }
        }

        private static T ReadField<T>(object instance, string name)
        {
            return (T)AccessTools.Field(instance.GetType(), name).GetValue(instance);
        }

        private static void WriteField<T>(object instance, string name, T value)
        {
            AccessTools.Field(instance.GetType(), name).SetValue(instance, value);
        }

        private sealed class StatusGuids
        {
            public Guid InputGuid = Guid.Empty;
        }
    }
}
