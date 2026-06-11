using System.Linq;
using Harmony;
using UnityEngine;

namespace BuildingAdjust
{
    public class BottleFillerAutoDropPatch
    {
        [HarmonyPatch(typeof(GasBottlerConfig), "ConfigureBuildingTemplate")]
        public static class GasBottlerConfigConfigureBuildingTemplatePatch
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<BottleFillerAutoDropControl>();
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public class GeneratedBuildingsLoadGeneratedBuildingsPatch
        {
            public static void Postfix()
            {
                var bottleFiller = Assets.BuildingDefs.FirstOrDefault(def => def.PrefabID == "StormShark.BottleFiller");

                if (bottleFiller is null || bottleFiller.BuildingComplete is null)
                {
                    return;
                }

                var complete = bottleFiller.BuildingComplete;
                complete.AddOrGet<BottleFillerAutoDropControl>();
            }
        }
    }
}