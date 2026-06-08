using System.Collections.Generic;
using System.Linq;
using TUNING;
using UnityEngine;

namespace ResourceSensor;

public class LogicResourceSensorConfig : IBuildingConfig
{
	public static string ID = "LogicResourceSensor";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef(ID, 1, 1, "resourceSensor_kanim", 30, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER0, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.PENALTY.TIER0);
		obj.Overheatable = false;
		obj.Floodable = false;
		obj.Entombable = false;
		obj.ViewMode = OverlayModes.Logic.ID;
		obj.AudioCategory = "Metal";
		obj.SceneLayer = Grid.SceneLayer.Building;
		obj.AlwaysOperational = true;
		string key = "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant();
		obj.LogicOutputPorts = new List<LogicPorts.Port> { LogicPorts.Port.OutputPort(LogicSwitch.PORT_ID, new CellOffset(0, 0), Strings.Get(key + ".LOGIC_PORT"), Strings.Get(key + ".LOGIC_PORT_ACTIVE"), Strings.Get(key + ".LOGIC_PORT_INACTIVE"), show_wire_missing_icon: true) };
		SoundEventVolumeCache.instance.AddVolume("switchgaspressure_kanim", "PowerSwitch_on", NOISE_POLLUTION.NOISY.TIER3);
		SoundEventVolumeCache.instance.AddVolume("switchgaspressure_kanim", "PowerSwitch_off", NOISE_POLLUTION.NOISY.TIER3);
		GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		base.ConfigureBuildingTemplate(go, prefab_tag);
		Storage storage = go.AddOrGet<Storage>();
		storage.allowItemRemoval = false;
		storage.showDescriptor = false;
		storage.storageFilters = STORAGEFILTERS.FOOD.Concat(STORAGEFILTERS.NOT_EDIBLE_SOLIDS).ToList();
		storage.allowSettingOnlyFetchMarkedItems = false;
		storage.showInUI = true;
		storage.showCapacityStatusItem = false;
		storage.showCapacityAsMainStatus = false;
		go.AddOrGet<TreeFilterable>().showUserMenu = true;
		go.AddOrGet<CopyBuildingSettings>();
		go.AddOrGet<LogicResourceSensor>().manuallyControlled = false;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits);
	}
}
