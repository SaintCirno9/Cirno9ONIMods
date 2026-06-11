using Harmony;
using UnityEngine;

namespace BuildingAdjust
{
    public class BatteryPatch
    {
        [HarmonyPatch(typeof(BatteryConfig), "DoPostConfigureComplete")]
        public class BatteryConfigDoPostConfigureComplete
        {
            public static void Postfix(GameObject go)
            {
                var battery = go.AddOrGet<Battery>();
                battery.capacity *= 15;
                battery.joulesLostPerSecond = 0;
            }
        }

        [HarmonyPatch(typeof(BatteryMediumConfig), "DoPostConfigureComplete")]
        public class BatteryMediumConfigDoPostConfigureComplete
        {
            public static void Postfix(GameObject go)
            {
                var battery = go.AddOrGet<Battery>();
                battery.capacity *= 15;
                battery.joulesLostPerSecond = 0;
            }
        }

        [HarmonyPatch(typeof(BatterySmartConfig), "DoPostConfigureComplete")]
        public class BatterySmartConfigDoPostConfigureComplete
        {
            public static void Postfix(GameObject go)
            {
                var battery = go.AddOrGet<Battery>();
                battery.capacity *= 60;
                battery.joulesLostPerSecond = 0;
            }
        }
    }
}