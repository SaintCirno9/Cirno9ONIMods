using HarmonyLib;

namespace ColonyTweaks
{
    public class TimerSensorPatch
    {
        private const float MaxCycles = 100f;

        [HarmonyPatch(typeof(TimerSideScreen), nameof(TimerSideScreen.SetTarget))]
        public class TimerSideScreenSetTargetPatch
        {
            public static void Prefix(TimerSideScreen __instance)
            {
                Traverse.Create(__instance).Field("maxCycles").SetValue(MaxCycles);
            }
        }
    }
}
