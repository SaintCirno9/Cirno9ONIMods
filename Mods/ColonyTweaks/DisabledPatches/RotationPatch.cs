using Harmony;
using UnityEngine;

namespace ColonyTweaks
{
    public class RotationPatch
    {
        [HarmonyPatch(typeof(AirFilterConfig), "CreateBuildingDef")]
        public class AirFilterConfigCreateBuildingDefPatch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.PermittedRotations = PermittedRotations.R360;
            }
        }

        [HarmonyPatch(typeof(SunLampConfig), "CreateBuildingDef")]
        public class SunLampConfigCreateBuildingDefPatch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.PermittedRotations = PermittedRotations.FlipH;
            }
        }

        [HarmonyPatch(typeof(PowerTransformerConfig), "CreateBuildingDef")]
        public class PowerTransformerConfigCreateBuildingDefPatch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.PermittedRotations = PermittedRotations.R360;
            }
        }

        [HarmonyPatch(typeof(PowerTransformerSmallConfig), "CreateBuildingDef")]
        public class PowerTransformerSmallConfigCreateBuildingDefPatch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.PermittedRotations = PermittedRotations.R360;
            }
        }
    }
}