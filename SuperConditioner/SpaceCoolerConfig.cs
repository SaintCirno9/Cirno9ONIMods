using UnityEngine;

namespace SuperConditioner
{
    public class SpaceCoolerConfig : SpaceHeaterConfig
    {
        public override BuildingDef CreateBuildingDef()
        {
            var buildingDef = base.CreateBuildingDef();
            buildingDef.PrefabID = "SpaceCooler";
            buildingDef.name = "SpaceCooler";
            buildingDef.ExhaustKilowattsWhenActive =
                -(buildingDef.ExhaustKilowattsWhenActive + buildingDef.SelfHeatKilowattsWhenActive);
            buildingDef.SelfHeatKilowattsWhenActive = 0f;
            buildingDef.Overheatable = false;
            return buildingDef;
        }
        
        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();
            go.AddOrGet<SpaceHeater>().targetTemperature = 10343.15f;
        }
    }
}