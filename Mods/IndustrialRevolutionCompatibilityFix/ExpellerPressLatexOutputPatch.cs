using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace IndustrialRevolutionCompatibilityFix
{
    [HarmonyPatch]
    internal static class ExpellerPressLatexOutputPatch
    {
        private const string TargetTypeName = "Biochemistry.Buildings.Biochemistry_ExpellerPressConfig";

        private static bool Prepare()
        {
            return AccessTools.TypeByName(TargetTypeName) != null;
        }

        private static MethodBase TargetMethod()
        {
            Type targetType = AccessTools.TypeByName(TargetTypeName);
            return targetType == null
                ? null
                : AccessTools.Method(targetType, "ConfigureBuildingTemplate", new[] { typeof(GameObject), typeof(Tag) });
        }

        private static void Postfix(GameObject go)
        {
            ConduitDispenser dispenser = go.GetComponent<ConduitDispenser>();
            if (dispenser == null || dispenser.conduitType != ConduitType.Liquid)
            {
                return;
            }

            if (dispenser.elementFilter == null)
            {
                dispenser.elementFilter = new[] { SimHashes.Latex };
                return;
            }

            if (dispenser.elementFilter.Contains(SimHashes.Latex))
            {
                return;
            }

            SimHashes[] elementFilter = dispenser.elementFilter;
            Array.Resize(ref elementFilter, elementFilter.Length + 1);
            elementFilter[elementFilter.Length - 1] = SimHashes.Latex;
            dispenser.elementFilter = elementFilter;
        }
    }
}
