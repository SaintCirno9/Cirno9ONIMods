using Harmony;
using UnityEngine;

namespace ColonyTweaks
{
    public class ElectrolyzerPatch
    {
        [HarmonyPatch(typeof(Electrolyzer), "OverPressure")]
        public static class ElectrolyzerOverPressurePatch
        {
            public static bool Prefix(bool __result)
            {
                __result = false;
                return false;
            }
        }
    }
}