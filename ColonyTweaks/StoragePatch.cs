using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ColonyTweaks
{
    public class StoragePatch
    {
        private static List<string> skipBuildings = new()
        {
            "GasBottler", "StormShark.BottleFiller"
        };

        private static List<Tag> neededFilter = new()
        {
            GameTags.Liquid,
            GameTags.LiquidSource,
            GameTags.GasSource,
            GameTags.Breathable,
            GameTags.Unbreathable
        };

        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public class GeneratedBuildingsLoadGeneratedBuildingsPatch
        {
            public static void Postfix()
            {
                foreach (var buildingDef in Assets.BuildingDefs)
                {
                    if (buildingDef is not {BuildingComplete: { } complete})
                    {
                        continue;
                    }

                    BatchUpdateStorages(buildingDef.PrefabID, complete);
                }
            }
        }

        public static void BatchUpdateStorages(string buildingDefPrefabID, GameObject go)
        {
            if (skipBuildings.Contains(buildingDefPrefabID))
            {
                return;
            }

            // StorageControl 暂时禁用。
            // if (buildingDefPrefabID is "StormShark.CanisterInserter" or "StormShark.BottleInserter")
            // {
            //     go.AddComponent<StorageControl>();
            // }

            foreach (var storage in go.GetComponents<Storage>())
            {
                if ((buildingDefPrefabID.Contains("StorageLocker") || IsSolidLoader(go)) && storage.storageFilters is not null)
                {
                    storage.storageFilters.AddRange(neededFilter);
                    storage.storageFilters = storage.storageFilters.Distinct().ToList();
                }

                // 容量扩大暂时禁用。
                // if (buildingDefPrefabID.Contains("StorageLocker") ||
                //     buildingDefPrefabID is "SolidConduitInbox" or "SolidConduitOutbox")
                // {
                //     storage.capacityKg *= 1000;
                // }
                //
                // if (buildingDefPrefabID.Contains("Reservoir"))
                // {
                //     storage.capacityKg *= 1000;
                //     foreach (var consumer in go.GetComponents<ConduitConsumer>())
                //     {
                //         consumer.capacityKG = storage.capacityKg;
                //     }
                // }
                //
                // if (buildingDefPrefabID.EndsWith("Feeder"))
                // {
                //     storage.capacityKg *= 1000;
                // }
            }
        }

        private static bool IsSolidLoader(GameObject go)
        {
            return go.GetComponent<SolidConduitInbox>() is not null || go.GetComponent<SolidConduitDispenser>() is not null;
        }

        [HarmonyPatch(typeof(Storage), "OnPrefabInit")]
        public class StorageOnPrefabInitPatch
        {
            private static void Postfix(ref Storage __instance)
            {
                var storedItemModifiers = Traverse.Create(__instance).Field("defaultStoredItemModifers")
                    .GetValue<List<Storage.StoredItemModifier>>();
                if (!storedItemModifiers.Contains(Storage.StoredItemModifier.Seal))
                {
                    storedItemModifiers.Add(Storage.StoredItemModifier.Seal);
                    __instance.SetDefaultStoredItemModifiers(storedItemModifiers);
                }
            }
        }

    }
}
