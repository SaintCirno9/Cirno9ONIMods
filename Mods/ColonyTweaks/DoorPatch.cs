using HarmonyLib;
using UnityEngine;

namespace ColonyTweaks
{
    public class DoorPatch
    {
        private const float DoorSpeedMultiplier = 3f;

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

                    UpdateDoorSpeed(complete);
                }
            }
        }

        private static void UpdateDoorSpeed(GameObject go)
        {
            var door = go.GetComponent<Door>();
            if (door is null)
            {
                return;
            }

            door.unpoweredAnimSpeed *= DoorSpeedMultiplier;
            door.poweredAnimSpeed *= DoorSpeedMultiplier;
        }
    }
}
