using HarmonyLib;
using UnityEngine;

namespace ColonyTweaks
{
    public class OxyfernPatch
    {
        [HarmonyPatch(typeof(OxyfernConfig), "CreatePrefab")]
        public static class OxyfernConfigCreatePrefabPatch
        {
            public static void Postfix(GameObject __result)
            {
                var elementConsumer = __result.GetComponent<ElementConsumer>();
                elementConsumer.consumptionRadius = 6;
            }
        }
    }
}
