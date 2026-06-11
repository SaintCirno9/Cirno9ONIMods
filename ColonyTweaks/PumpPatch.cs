using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ColonyTweaks
{
    public class PumpPatch
    {
        private static List<string> skipBuildings = new()
        {
        };

        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildingsLoadGeneratedBuildingsPatch
        {
            public static void Postfix()
            {
                foreach (var buildingDef in Assets.BuildingDefs)
                {
                    if (buildingDef is not {BuildingComplete: { } complete})
                    {
                        continue;
                    }

                    BatchUpdatePumps(buildingDef.PrefabID, complete);
                }
            }
        }

        public static void BatchUpdatePumps(string buildingDefPrefabID, GameObject go)
        {
            if (skipBuildings.Contains(buildingDefPrefabID))
            {
                return;
            }

            if (go.GetComponent<Pump>() is not null && go.GetComponent<ElementConsumer>() is not null)
            {
                go.AddOrGet<ElementConsumerRadiusControl>();
            }
        }

        [HarmonyPatch(typeof(ElementConsumer), "OnSimRegistered")]
        public static class ElementConsumerOnSimRegisteredPatch
        {
            public static void Postfix(ElementConsumer __instance)
            {
                if (__instance.GetComponent<ElementConsumerRadiusControl>() is not null)
                {
                    __instance.GetComponent<ElementConsumerRadiusControl>().ApplyStoredRadius();
                    __instance.RefreshConsumptionRate();
                }
            }
        }

        [HarmonyPatch(typeof(ElementConsumer), "OnSimRegister")]
        public static class ElementConsumerOnSimRegisterPatch
        {
            public static void Prefix(ElementConsumer __instance)
            {
                __instance.GetComponent<ElementConsumerRadiusControl>()?.ApplyStoredRadius();
            }
        }

        [HarmonyPatch(typeof(Pump), "IsPumpable")]
        public static class PumpIsPumpablePatch
        {
            public static bool Prefix(Pump __instance, Element.State expected_state, int radius, ref bool __result)
            {
                var consumer = __instance.GetComponent<ElementConsumer>();
                if (consumer is null || consumer.GetComponent<ElementConsumerRadiusControl>() is null)
                {
                    return true;
                }

                var sampleCell = Grid.PosToCell(__instance.transform.GetPosition() + consumer.sampleCellOffset);
                __result = HasPumpableElement(sampleCell, expected_state, radius);
                return false;
            }
        }

        [HarmonyPatch(typeof(Pump), nameof(Pump.Sim1000ms))]
        public static class PumpSim1000msPatch
        {
            public static void Prefix(Pump __instance)
            {
                SyncPumpCapacityWithConduit(__instance);
            }
        }

        private static void SyncPumpCapacityWithConduit(Pump pump)
        {
            var consumer = pump.GetComponent<ElementConsumer>();
            if (consumer is null || consumer.GetComponent<ElementConsumerRadiusControl>() is null)
            {
                return;
            }

            var dispenser = pump.GetComponent<ConduitDispenser>();
            var storage = pump.GetComponent<Storage>();
            var conduitFlow = dispenser?.GetConduitManager();
            if (storage is null || conduitFlow is null)
            {
                return;
            }

            var conduitCapacity = Traverse.Create(conduitFlow).Field<float>("MaxMass").Value;
            if (conduitCapacity <= 0f || float.IsNaN(conduitCapacity) || float.IsInfinity(conduitCapacity))
            {
                return;
            }

            var changed = false;
            if (Mathf.Abs(consumer.consumptionRate - conduitCapacity) > 0.001f)
            {
                consumer.consumptionRate = conduitCapacity;
                changed = true;
            }

            var storageCapacity = conduitCapacity * 2f;
            if (Mathf.Abs(storage.capacityKg - storageCapacity) > 0.001f)
            {
                storage.capacityKg = storageCapacity;
                changed = true;
            }

            if (changed)
            {
                consumer.RefreshConsumptionRate();
            }
        }

        private static bool HasPumpableElement(int sampleCell, Element.State expectedState, int radius)
        {
            if (radius < 1 || !Grid.IsValidCell(sampleCell))
            {
                return false;
            }

            var sampleElement = Grid.Element[sampleCell];
            if (sampleElement is null || sampleElement.IsSolid)
            {
                return false;
            }

            var seen = new HashSet<int> { sampleCell };
            var costs = new Dictionary<int, int>
            {
                [sampleCell] = 0
            };
            var queue = new Queue<int>();
            queue.Enqueue(sampleCell);

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                if (Grid.Element[cell].IsState(expectedState))
                {
                    return true;
                }

                var cost = costs[cell];
                if (cost >= radius - 1)
                {
                    continue;
                }

                EnqueueIfPassable(Grid.CellLeft(cell), cost + 1, seen, costs, queue);
                EnqueueIfPassable(Grid.CellRight(cell), cost + 1, seen, costs, queue);
                EnqueueIfPassable(Grid.CellAbove(cell), cost + 1, seen, costs, queue);
                EnqueueIfPassable(Grid.CellBelow(cell), cost + 1, seen, costs, queue);
            }

            return false;
        }

        private static void EnqueueIfPassable(int cell, int cost, ISet<int> seen, IDictionary<int, int> costs,
            Queue<int> queue)
        {
            if (!Grid.IsValidCell(cell) || seen.Contains(cell))
            {
                return;
            }

            var element = Grid.Element[cell];
            if (element is null || element.IsSolid)
            {
                return;
            }

            seen.Add(cell);
            costs[cell] = cost;
            queue.Enqueue(cell);
        }
    }
}
