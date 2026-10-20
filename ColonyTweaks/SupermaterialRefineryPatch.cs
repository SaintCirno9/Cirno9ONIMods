using HarmonyLib;
using UnityEngine;

namespace ColonyTweaks
{
    public class SupermaterialRefineryPatch
    {
        private const float VisualScale = 0.75f;

        [HarmonyPatch(typeof(SupermaterialRefineryConfig), nameof(SupermaterialRefineryConfig.CreateBuildingDef))]
        public class SupermaterialRefineryConfigCreateBuildingDefPatch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.HeightInCells = 4;
                __result.GenerateOffsets();
                Debug.Log("[ColonyTweaks] SupermaterialRefinery 高度已调整为 4 格。");
            }
        }

        [HarmonyPatch(typeof(SupermaterialRefineryConfig), nameof(SupermaterialRefineryConfig.DoPostConfigureComplete))]
        public class SupermaterialRefineryConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                ApplyScale(go);
                Debug.Log("[ColonyTweaks] SupermaterialRefinery visual scale 已设置。");
            }
        }

        [HarmonyPatch(typeof(BuildingLoader), nameof(BuildingLoader.CreateBuildingPreview))]
        public class BuildingLoaderCreateBuildingPreviewPatch
        {
            public static void Postfix(BuildingDef def, GameObject __result)
            {
                if (def != null && def.PrefabID == SupermaterialRefineryConfig.ID)
                {
                    ApplyScale(__result);
                }
            }
        }

        [HarmonyPatch(typeof(BuildingLoader), nameof(BuildingLoader.CreateBuildingUnderConstruction))]
        public class BuildingLoaderCreateBuildingUnderConstructionPatch
        {
            public static void Postfix(BuildingDef def, GameObject __result)
            {
                if (def != null && def.PrefabID == SupermaterialRefineryConfig.ID)
                {
                    ApplyScale(__result);
                }
            }
        }

        [HarmonyPatch(typeof(BuildingComplete), "OnSpawn")]
        public class BuildingCompleteOnSpawnPatch
        {
            public static void Postfix(BuildingComplete __instance)
            {
                if (__instance != null && __instance.Def != null && __instance.Def.PrefabID == SupermaterialRefineryConfig.ID)
                {
                    ApplyScale(__instance.gameObject);
                }
            }
        }

        private static void ApplyScale(GameObject go)
        {
            if (go != null)
            {
                go.transform.localScale = new Vector3(VisualScale, VisualScale, 1f);
            }
        }
    }
}
