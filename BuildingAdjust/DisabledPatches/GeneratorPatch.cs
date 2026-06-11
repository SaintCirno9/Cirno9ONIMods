using System.Collections.Generic;
using Harmony;
using UnityEngine;

namespace BuildingAdjust
{
    public class GeneratorPatch
    {
        private static Dictionary<string, float> generatorWattageRatingDict = new()
        {
            {"ManualGenerator", 500f},
            {"WoodGasGenerator", 800f},
            {"Generator", 1000f},
            {"HydrogenGenerator", 1600f},
            {"MethaneGenerator", 2000f},
            {"SteamTurbine", 3000f},
            {"PetroleumGenerator", 2400f},
            {"SolarPanel", 500f}
        };

        public static void BatchUpdateGenerator(string buildingDefPrefabID, BuildingDef buildingDef, GameObject go)
        {
            if (generatorWattageRatingDict.ContainsKey(buildingDefPrefabID))
            {
                buildingDef.GeneratorWattageRating = generatorWattageRatingDict[buildingDefPrefabID];
            }
        }

        [HarmonyPatch(typeof(EnergyGenerator.OutputItem), MethodType.Constructor, typeof(SimHashes), typeof(float),
            typeof(bool), typeof(CellOffset), typeof(float))]
        public class EnergyGeneratorOutputItemConstructorPatch
        {
            public static void Prefix(ref float creation_rate)
            {
                creation_rate *= 10;
            }
        }
    }
}