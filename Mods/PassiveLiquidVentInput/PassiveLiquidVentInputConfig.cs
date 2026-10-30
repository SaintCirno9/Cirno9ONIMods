using TUNING;
using UnityEngine;

namespace PassiveLiquidVentInput
{
    public class PassiveLiquidVentInputConfig : IBuildingConfig
    {
        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("PassiveLiquidVentInput", 1, 1,
                "passive_liquid_vent_input_kanim", 30, 10f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, MATERIALS.RAW_METALS,
                1600f, BuildLocationRule.Anywhere, BUILDINGS.DECOR.PENALTY.TIER1, NOISE_POLLUTION.NONE, 0.2f);
            buildingDef.Floodable = false;
            buildingDef.Overheatable = false;
            buildingDef.MaterialCategory = MATERIALS.RAW_METALS;
            buildingDef.AudioCategory = "Metal";
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
            buildingDef.SceneLayer = Grid.SceneLayer.Building;
            buildingDef.OutputConduitType = ConduitType.Liquid;
            buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
            buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
            buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, "PassiveLiquidVentInput");
            SoundEventVolumeCache.instance.AddVolume("ventliquid_kanim", "LiquidVent_squirt",
                NOISE_POLLUTION.NOISY.TIER0);
            return buildingDef;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayBehindConduits);
            go.AddOrGet<LoopingSounds>();
            go.AddOrGet<LogicOperationalController>();
            go.GetComponent<KBatchedAnimController>().initialAnim = "on";
            go.AddOrGet<Storage>().capacityKg = 500f;
            ElementConsumer elementConsumer = go.AddOrGet<ElementConsumer>();
            elementConsumer.showDescriptor = false;
            elementConsumer.configuration = ElementConsumer.Configuration.AllLiquid;
            elementConsumer.capacityKG = 100f;
            elementConsumer.consumptionRate = 100f;
            elementConsumer.storeOnConsume = true;
            elementConsumer.showInStatusPanel = false;
            elementConsumer.consumptionRadius = 1;
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.conduitType = ConduitType.Liquid;
            conduitDispenser.alwaysDispense = true;
            conduitDispenser.elementFilter = null;
            go.AddOrGet<PassiveLiquidVentInput>();
        }

        public const string ID = "PassiveLiquidVentInput";

        protected const string kanim = "passive_liquid_vent_input_kanim";
    }
}