using HarmonyLib;
using KMod;
using UnityEngine;

namespace TidalSpringRandomizer;

public class TidalSpringRandomizerMod : UserMod2
{
}

[HarmonyPatch(typeof(SmallReefGeyserConfig), nameof(SmallReefGeyserConfig.CreatePrefab))]
public static class SmallReefGeyserConfig_CreatePrefab_Patch
{
    public static void Postfix(GameObject __result)
    {
        if (__result != null)
        {
            __result.AddOrGet<TidalSpringRandomizerControl>();
        }
    }
}
