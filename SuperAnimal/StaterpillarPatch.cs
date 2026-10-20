using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using UnityEngine;

namespace SuperAnimal
{
    public class StaterpillarPatch
    {
        public static void OnLoad()
        {
            StaterpillarTuning.POOP_CONVERSTION_RATE *= 10;
        }

        [HarmonyPatch(typeof(StaterpillarGeneratorConfig), "CreateBuildingDef")]
        public class StaterpillarGeneratorConfigCreateBuildingDefPatch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.GeneratorWattageRating *= 3;
            }
        }
    }
}