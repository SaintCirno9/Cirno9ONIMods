using HarmonyLib;
using UnityEngine;

namespace ColonyTweaks
{
    public class TubeWormPatch
    {
        [HarmonyPatch(typeof(TubeWormConfig), nameof(TubeWormConfig.CreatePrefab))]
        public static class TubeWormConfigCreatePrefabPatch
        {
            public static void Postfix(GameObject __result)
            {
                if (__result.GetComponent<Tinkerable>() == null)
                {
                    Tinkerable.MakeFarmTinkerable(__result);
                }
            }
        }
    }
}
