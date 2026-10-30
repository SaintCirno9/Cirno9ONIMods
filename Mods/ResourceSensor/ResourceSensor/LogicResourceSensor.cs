using System;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;

namespace ResourceSensor;

[SerializationConfig(MemberSerialization.OptIn)]
public class LogicResourceSensor : Switch, ISaveLoadable, IThresholdSwitch, ISim1000ms, IMultiSliderControl, ISliderControl, ICheckboxControl
{
	public enum SensorMode
	{
		Distance,
		Room,
		Global
	}

	private bool wasOn;

	[Serialize]
	private SensorMode mode = SensorMode.Global;

	[Serialize]
	private int distance = 3;

	[Serialize]
	private bool includeStorage;

	private RangeVisualizer visualizer = null;

	[MyCmpGet]
	private TreeFilterable treeFilterable = null;

	[MyCmpGet]
	private LogicPorts logicPorts = null;

	[Serialize]
	public float countThreshold;

	[Serialize]
	public bool activateOnGreaterThan = true;

	private float currentCount;

	private KSelectable selectable;

	private Guid roomStatusGUID;

	private KBatchedAnimController animController;

	private ISliderControl[] sliders;

	private static readonly EventSystem.IntraObjectHandler<LogicResourceSensor> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<LogicResourceSensor>(delegate(LogicResourceSensor component, object data)
	{
		component.OnCopySettings(data);
	});

	public SensorMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			if (mode != value)
			{
				mode = value;
				UpdateRangeVisualizer();
			}
		}
	}

	public int Distance
	{
		get
		{
			return distance;
		}
		set
		{
			if (distance != value)
			{
				distance = value;
				UpdateRangeVisualizer();
			}
		}
	}

	public bool IncludeStorage
	{
		get
		{
			return includeStorage;
		}
		set
		{
			includeStorage = value;
		}
	}

	public float Threshold
	{
		get
		{
			return countThreshold;
		}
		set
		{
			countThreshold = value;
		}
	}

	public bool ActivateAboveThreshold
	{
		get
		{
			return activateOnGreaterThan;
		}
		set
		{
			activateOnGreaterThan = value;
		}
	}

	public float CurrentValue => currentCount;

	public float RangeMin => 0f;

	public float RangeMax => 100000f;

	public LocString Title => new LocString(ResourceSensorPatches.Localized("Resource Sensor", "资源传感器"), "STRINGS.UI.SIDESCREENS.RESOURCE_SENSOR.TITLE");

	public LocString ThresholdValueName => new LocString(ResourceSensorPatches.Localized("Value", "数值"), "STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.VALUE_NAME");

	public string AboveToolTip => Strings.Get("STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.RESOURCE_TOOLTIP_ABOVE");

	public string BelowToolTip => Strings.Get("STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.RESOURCE_TOOLTIP_BELOW");

	public ThresholdScreenLayoutType LayoutType => ThresholdScreenLayoutType.SliderBar;

	public int IncrementScale => 100;

	public NonLinearSlider.Range[] GetRanges => NonLinearSlider.GetDefaultRange(RangeMax);

	public string SidescreenTitleKey => "STRINGS.UI.SIDESCREENS.RESOURCE_SENSOR.TITLE";

	public ISliderControl[] sliderControls => sliders ??= new ISliderControl[] { this };

	public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.DISTANCE_TITLE";

	public string SliderUnits => " Tiles";

	public string CheckboxTitleKey => SidescreenTitleKey;

	public string CheckboxLabel => Strings.Get("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.INCLUDE_STORAGE_CHECKBOX");

	public string CheckboxTooltip => Strings.Get("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.INCLUDE_STORAGE_CHECKBOX_TOOLTIP");

	public bool GetCheckboxValue()
	{
		return IncludeStorage;
	}

	public void SetCheckboxValue(bool value)
	{
		IncludeStorage = value;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		selectable = GetComponent<KSelectable>();
		Subscribe(-905833192, OnCopySettingsDelegate);
		Subscribe((int)GameHashes.RefreshUserMenu, AddResourceSensorModeButtons);
	}

	private void AddResourceSensorModeButtons(object data)
	{
		AddModeCycleButton();
	}

	private void AddModeCycleButton()
	{
		SensorMode nextMode = GetNextMode();
		string currentModeName = Strings.Get(GetModeLabelKey(mode));
		string nextModeName = Strings.Get(GetModeLabelKey(nextMode));
		Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo(
			"action_switch_toggle",
			Strings.Get("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.CURRENT_MODE_PREFIX") + currentModeName,
			() => SetModeFromUserMenu(nextMode),
			global::Action.NumActions,
			null,
			null,
			null,
			string.Format(Strings.Get("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.MODE_BUTTON_TOOLTIP"), nextModeName)), 0.45f);
	}

	private SensorMode GetNextMode()
	{
		return mode switch
		{
			SensorMode.Global => SensorMode.Room,
			SensorMode.Room => SensorMode.Distance,
			_ => SensorMode.Global
		};
	}

	private static string GetModeLabelKey(SensorMode sensorMode)
	{
		return sensorMode switch
		{
			SensorMode.Distance => "STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.DISTANCE_MODE",
			SensorMode.Room => "STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.ROOM_MODE",
			_ => "STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.GLOBAL_MODE"
		};
	}

	private void SetModeFromUserMenu(SensorMode targetMode)
	{
		if (mode == targetMode)
		{
			return;
		}
		Mode = targetMode;
		RefreshDetailsScreen();
	}

	private void RefreshDetailsScreen()
	{
		if (selectable != null && selectable.IsSelected && DetailsScreen.Instance != null)
		{
			DetailsScreen.Instance.Refresh(gameObject);
		}
	}

	private void OnCopySettings(object data)
	{
		LogicResourceSensor source = ((GameObject)data).GetComponent<LogicResourceSensor>();
		if (source != null)
		{
			countThreshold = source.countThreshold;
			activateOnGreaterThan = source.activateOnGreaterThan;
			Mode = source.Mode;
			IncludeStorage = source.IncludeStorage;
			Distance = source.Distance;
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.OnToggle += OnSwitchToggled;
		animController = GetComponent<KBatchedAnimController>();
		UpdateLogicCircuit();
		UpdateVisualState(force: true);
		wasOn = switchedOn;
		UpdateRangeVisualizer();
	}

	public void Sim1000ms(float dt)
	{
		if (treeFilterable.AcceptedTags.Count == 0)
		{
			currentCount = 0f;
			SetStateForCurrentCount();
			UpdateRangeVisualizer();
			return;
		}

		if (mode == SensorMode.Room)
		{
			UpdateRoomCount();
			return;
		}

		RemoveNotInRoomStatus();
		if (mode == SensorMode.Distance)
		{
			currentCount = CountDistance();
		}
		else if (mode == SensorMode.Global)
		{
			currentCount = CountGlobal();
		}
		SetStateForCurrentCount();
	}

	private void UpdateRoomCount()
	{
		Room currentRoom = Game.Instance.roomProber.GetRoomOfGameObject(gameObject);
		if (currentRoom == null)
		{
			AddNotInRoomStatus();
			currentCount = 0f;
			SetStateForCurrentCount();
			return;
		}

		RemoveNotInRoomStatus();
		currentCount = CountRoom(currentRoom);
		SetStateForCurrentCount();
	}

	private void AddNotInRoomStatus()
	{
		if (!selectable.HasStatusItem(Db.Get().BuildingStatusItems.NotInAnyRoom))
		{
			roomStatusGUID = selectable.AddStatusItem(Db.Get().BuildingStatusItems.NotInAnyRoom);
		}
	}

	private void RemoveNotInRoomStatus()
	{
		if (selectable.HasStatusItem(Db.Get().BuildingStatusItems.NotInAnyRoom))
		{
			selectable.RemoveStatusItem(roomStatusGUID);
		}
	}

	private void SetStateForCurrentCount()
	{
		SetState(activateOnGreaterThan ? currentCount > countThreshold : currentCount < countThreshold);
	}

	private void OnSwitchToggled(bool toggled_on)
	{
		UpdateLogicCircuit();
		UpdateVisualState();
	}

	private void UpdateLogicCircuit()
	{
		logicPorts.SendSignal(LogicSwitch.PORT_ID, switchedOn ? 1 : 0);
	}

	private void UpdateVisualState(bool force = false)
	{
		if (wasOn != switchedOn || force)
		{
			wasOn = switchedOn;
			if (switchedOn)
			{
				animController.Play("Working_pre");
				animController.Queue("Working_loop", KAnim.PlayMode.Loop);
			}
			else
			{
				animController.Play("Working_pst");
				animController.Queue("off");
			}
		}
	}

	protected override void UpdateSwitchStatus()
	{
		StatusItem status_item = (switchedOn ? Db.Get().BuildingStatusItems.LogicSensorStatusActive : Db.Get().BuildingStatusItems.LogicSensorStatusInactive);
		GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Power, status_item);
	}

	public float GetRangeMinInputField()
	{
		return RangeMin;
	}

	public float GetRangeMaxInputField()
	{
		return RangeMax;
	}

	public string Format(float value, bool units)
	{
		return $"{value} kg";
	}

	public float ProcessedSliderValue(float input)
	{
		return Mathf.RoundToInt(input);
	}

	public float ProcessedInputValue(float input)
	{
		int roundedUp = Mathf.CeilToInt(input);
		float roundedToCents = Mathf.Round(input * 100f) / 100f;
		if (Mathf.Abs(roundedToCents - (float)roundedUp) <= 0.01f)
		{
			return roundedUp;
		}
		return roundedToCents;
	}

	public LocString ThresholdValueUnits()
	{
		return "";
	}

	public bool SidescreenEnabled()
	{
		return mode == SensorMode.Distance;
	}

	public int SliderDecimalPlaces(int index)
	{
		return 0;
	}

	public float GetSliderMin(int index)
	{
		return 0f;
	}

	public float GetSliderMax(int index)
	{
		return 20f;
	}

	public float GetSliderValue(int index)
	{
		return Distance;
	}

	public void SetSliderValue(float percent, int index)
	{
		Distance = Convert.ToInt32(percent);
	}

	public string GetSliderTooltipKey(int index)
	{
		return "STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.SLIDER_TOOLTIP";
	}

	public string GetSliderTooltip(int index)
	{
		return string.Format(Strings.Get(GetSliderTooltipKey(index)), Distance);
	}

	private float CountDistance()
	{
		int portCell = logicPorts.GetPortCell(LogicSwitch.PORT_ID);
		if (Distance == 0)
		{
			return CountCell(portCell);
		}
		HashSet<GameObject> countedBuildings = new HashSet<GameObject>();
		Grid.CellToXY(portCell, out var originX, out var originY);
		int minX = originX - Distance;
		int maxX = originX + Distance;
		int minY = originY - Distance;
		int maxY = originY + Distance;
		float totalMass = 0f;
		for (int x = minX; x <= maxX; x++)
		{
			for (int y = minY; y <= maxY; y++)
			{
				int cell = Grid.XYToCell(x, y);
				if (!Grid.IsValidCell(cell) || Grid.IsSolidCell(cell))
				{
					continue;
				}
				totalMass += CountCell(cell);
				if (includeStorage)
				{
					totalMass += CountStorageBuildingAtCell(cell, countedBuildings);
				}
			}
		}
		return totalMass;
	}

	private void UpdateRangeVisualizer()
	{
		if (mode != SensorMode.Distance)
		{
			RemoveRangeVisualizer();
			return;
		}
		visualizer = gameObject.AddOrGet<RangeVisualizer>();
		visualizer.OriginOffset = Vector2I.zero;
		visualizer.TestLineOfSight = false;
		visualizer.BlockingCb = Grid.IsSolidCell;
		visualizer.BlockingTileVisible = false;
		visualizer.RangeMin = new Vector2I(-Distance, -Distance);
		visualizer.RangeMax = new Vector2I(Distance, Distance);
		int size = Distance * 2 + 1;
		visualizer.TexSize = new Vector2I(size, size);
	}

	private void RemoveRangeVisualizer()
	{
		if (visualizer == null)
		{
			visualizer = GetComponent<RangeVisualizer>();
		}
		if (visualizer != null)
		{
			Destroy(visualizer);
			visualizer = null;
		}
	}

	private float CountRoom(Room room)
	{
		float totalMass = 0f;
		int minX = room.cavity.minX;
		int maxX = room.cavity.maxX;
		int minY = room.cavity.minY;
		int maxY = room.cavity.maxY;
		HashSet<GameObject> countedBuildings = new HashSet<GameObject>();
		RoomProber roomProber = Game.Instance.roomProber;
		for (int x = minX; x <= maxX; x++)
		{
			for (int y = minY; y <= maxY; y++)
			{
				int cell = Grid.XYToCell(x, y);
				if (Grid.IsSolidCell(cell) || roomProber.GetCavityForCell(cell) != room.cavity)
				{
					continue;
				}
				totalMass += CountCell(cell);
				if (includeStorage)
				{
					totalMass += CountStorageBuildingAtCell(cell, countedBuildings);
				}
			}
		}
		return totalMass;
	}

	private float CountGlobal()
	{
		HashSet<Tag> acceptedTags = treeFilterable.AcceptedTags;
		WorldInventory worldInventory = base.gameObject.GetMyWorld().worldInventory;
		float totalMass = 0f;
		foreach (Tag tag in acceptedTags)
		{
			totalMass += worldInventory.GetTotalAmount(tag, includeRelatedWorlds: false);
		}
		return totalMass;
	}

	private float CountCell(int cell)
	{
		HashSet<Tag> acceptedTags = treeFilterable.AcceptedTags;
		GameObject firstPickupable = Grid.Objects[cell, 3];
		if (firstPickupable == null)
		{
			return 0f;
		}
		float totalMass = 0f;
		ObjectLayerListItem pickupableItem = firstPickupable.GetComponent<Pickupable>().objectLayerListItem;
		while (pickupableItem != null)
		{
			GameObject pickupableObject = pickupableItem.gameObject;
			pickupableItem = pickupableItem.nextItem;
			if (!(pickupableObject != null) || pickupableObject.TryGetComponent<MinionIdentity>(out var _) || !pickupableObject.TryGetComponent<KPrefabID>(out var prefab))
			{
				continue;
			}
			foreach (Tag tag in acceptedTags)
			{
				if (prefab.HasTag(tag))
				{
					totalMass += pickupableObject.GetComponent<PrimaryElement>().Mass;
				}
			}
		}
		return totalMass;
	}

	private float CountStorageBuildingAtCell(int cell, HashSet<GameObject> countedBuildings)
	{
		GameObject building = Grid.Objects[cell, 1];
		if (building == null || countedBuildings.Contains(building))
		{
			return 0f;
		}
		countedBuildings.Add(building);
		return CountBuilding(building);
	}

	private float CountBuilding(GameObject building)
	{
		if (building.TryGetComponent<BuildingUnderConstruction>(out var _))
		{
			return 0f;
		}
		if (!building.TryGetComponent<StorageLocker>(out var _) && !building.TryGetComponent<StorageLockerSmart>(out var _) && !building.TryGetComponent<RationBox>(out var _) && !building.TryGetComponent<Refrigerator>(out var _))
		{
			return 0f;
		}
		float totalMass = 0f;
		if (building.TryGetComponent<Storage>(out var storage))
		{
			HashSet<Tag> acceptedTags = treeFilterable.AcceptedTags;
			foreach (GameObject storedItem in storage.items)
			{
				foreach (Tag tag in acceptedTags)
				{
					if (storedItem.HasTag(tag))
					{
						totalMass += storedItem.GetComponent<PrimaryElement>().Mass;
						break;
					}
				}
			}
		}
		return totalMass;
	}
}
