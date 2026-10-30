using HarmonyLib;
using KMod;
using UnityEngine;

namespace AdjustableRobotMiner
{
    public class AutoMinerPatch : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            LocString.CreateLocStringKeys(typeof(AutoMinerStrings.UI));
        }

        [HarmonyPatch(typeof(AutoMinerConfig), "DoPostConfigureComplete")]
        public static class AutoMinerConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<AutoMinerControl>();
            }
        }

        [HarmonyPatch(typeof(DualSliderSideScreen), "SetTarget")]
        public static class DualSliderSideScreen_SetTarget_Patch
        {
            public static void Prefix(DualSliderSideScreen __instance, GameObject new_target)
            {
                if (new_target.GetComponent<AutoMinerControl>() == null)
                    return;
                var sliderSets = __instance.sliderSets;
                for (var i = 0; i < sliderSets.Count; i++)
                {
                    sliderSets[i].index = i;
                }
            }
        }
    }
}