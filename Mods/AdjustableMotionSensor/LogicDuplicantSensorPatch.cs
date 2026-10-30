using KMod;
using HarmonyLib;
using UnityEngine;

namespace AdjustableMotionSensor
{
    public class LogicDuplicantSensorPatch: UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            LocString.CreateLocStringKeys(typeof(LogicDuplicantSensorStrings.UI));
        }
        
        [HarmonyPatch(typeof(LogicDuplicantSensorConfig), "DoPostConfigureComplete")]
        public class LogicDuplicantSensorConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<LogicDuplicantSensorControl>();
            }
        }
    }
}