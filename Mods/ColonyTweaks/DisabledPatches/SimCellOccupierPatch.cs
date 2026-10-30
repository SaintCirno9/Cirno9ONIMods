using System;
using System.Collections.Generic;
using Harmony;
using STRINGS;
using UnityEngine;

namespace ColonyTweaks
{
    public class SimCellOccupierPatch
    {
        public static void BatchAddSimCellOccupierControl(string buildingDefPrefabID, GameObject go)
        {
            if (buildingDefPrefabID is "Door" or "Ladder" or "LadderFast")
            {
                go.AddOrGet<SimCellOccupierControl>();
            }

            /*if (buildingDefPrefabID is "PressureDoor" or "ManualPressureDoor")
            {
                go.AddOrGet<SimCellOccupierControl>().defaultFluidImpermeable = true;
            }*/
        }
    }
}