using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TUNING;
using UnityEngine;

namespace ColonyTweaks
{
    public class RonivansFridgePatch
    {
        private const float CapacityMultiplier = 100f;

        private static readonly HashSet<string> FridgeIds = new()
        {
            "AIO_FridgeLarge",
            "SimpleFridge",
            "FridgePod",
            "HightechSmallFridge",
            "HightechBigFridge",
            "SpaceBox"
        };

        private static readonly Dictionary<string, float> OriginalCapacities = new();

        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public class GeneratedBuildingsLoadGeneratedBuildingsPatch
        {
            public static void Postfix()
            {
                foreach (var buildingDef in Assets.BuildingDefs)
                {
                    if (buildingDef is not { BuildingComplete: { } complete } ||
                        !FridgeIds.Contains(buildingDef.PrefabID))
                    {
                        continue;
                    }

                    UpdateFoodStorageCapacity(buildingDef.PrefabID, complete);
                }
            }
        }

        private static void UpdateFoodStorageCapacity(string prefabId, GameObject complete)
        {
            foreach (var storage in complete.GetComponents<Storage>())
            {
                if (storage.storageFilters is null ||
                    !storage.storageFilters.Any(STORAGEFILTERS.FOOD.Contains))
                {
                    continue;
                }

                if (!OriginalCapacities.TryGetValue(prefabId, out var originalCapacity))
                {
                    originalCapacity = storage.capacityKg;
                    OriginalCapacities[prefabId] = originalCapacity;
                }

                storage.capacityKg = originalCapacity * CapacityMultiplier;
            }
        }
    }
}
