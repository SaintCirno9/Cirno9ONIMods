using Harmony;

namespace BuildingAdjust
{
    public class GeyserPatch
    {
        [HarmonyPatch(typeof(GeyserConfigurator.GeyserInstanceConfiguration), "GetEmitRate")]
        internal class GeyserEmitRatePatch
        {
            public static void Postfix(ref float __result)
            {
                __result *= 5f;
            }
        }
        [HarmonyPatch(typeof(GeyserConfigurator.GeyserInstanceConfiguration),"GetMaxPressure")]
        public static class GeyserPressurePatch
        {
            public static void Postfix(ref float __result)
            {
                __result = float.MaxValue;
            }
        }
    }
}