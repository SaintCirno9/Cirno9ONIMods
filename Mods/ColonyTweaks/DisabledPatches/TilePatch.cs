using Harmony;
using UnityEngine;

namespace ColonyTweaks
{
    public class TilePatch
    {
        public static void BatchUpdateTileProperties(string buildingDefPrefabID, GameObject go)
        {
            foreach (var occupier in go.GetComponents<SimCellOccupier>())
            {
                occupier.strengthMultiplier *= 2;
                occupier.movementSpeedMultiplier *= 2;
            }

            foreach (var ladder in go.GetComponents<Ladder>())
            {
                ladder.downwardsMovementSpeedMultiplier *= 2f;
                ladder.upwardsMovementSpeedMultiplier *= 2f;
            }
        }
    }
}