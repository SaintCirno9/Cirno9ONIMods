using Harmony;
using UnityEngine;

namespace BuildingAdjust
{
    public class VentPatch
    {
        [HarmonyPatch(typeof(GasVentConfig), "ConfigureBuildingTemplate")]
        public class GasVentConfigConfigureBuildingTemplatePatch
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<VentControl>();
            }
        }

        [HarmonyPatch(typeof(GasVentHighPressureConfig), "ConfigureBuildingTemplate")]
        public class GasVentHighPressureConfigConfigureBuildingTemplatePatch
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<VentControl>();
            }
        }

        [HarmonyPatch(typeof(LiquidVentConfig), "ConfigureBuildingTemplate")]
        public class LiquidVentConfigConfigureBuildingTemplatePatch
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<VentControl>();
            }
        }

        [HarmonyPatch(typeof(Vent), "IsValidOutputCell")]
        public static class VentIsValidOutputCellPatch
        {
            public static bool Prefix(Vent __instance,ref bool __result)
            {
                if (__instance.GetComponent<VentControl>().ignorePressure)
                {
                    __result = true;
                    return false;
                }

                return true;
            }
        }
    }
}