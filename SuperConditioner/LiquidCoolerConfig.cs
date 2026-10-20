using TUNING;
using UnityEngine;

namespace SuperConditioner
{
    public class LiquidCoolerConfig : LiquidHeaterConfig
    {
        public override BuildingDef CreateBuildingDef()
        {
            var buildingDef = base.CreateBuildingDef();
            buildingDef.PrefabID = "LiquidCooler";
            buildingDef.name = "LiquidCooler";
            buildingDef.ExhaustKilowattsWhenActive =
                -(buildingDef.ExhaustKilowattsWhenActive + buildingDef.SelfHeatKilowattsWhenActive);
            buildingDef.SelfHeatKilowattsWhenActive = 0f;
            buildingDef.Overheatable = false;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();
            SpaceHeater spaceHeater = go.AddOrGet<SpaceHeater>();
            spaceHeater.SetLiquidHeater();
            spaceHeater.targetTemperature = 10358.15f;
            spaceHeater.minimumCellMass = 400f;
        }
    }
}