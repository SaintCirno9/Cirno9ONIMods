using STRINGS;

namespace ResourceSensor;

public class UI
{
	public class UISIDESCREENS
	{
		public class RESOURCE_SENSOR_SIDE_SCREEN
		{
			public static readonly LocString TITLE = "Resource Sensor";

			public static readonly LocString VALUE_NAME = "Value";

			public static LocString SLIDER_TOOLTIP = "Resources further than " + STRINGS.UI.FormatAsKeyWord("{0}") + " tiles will not be counted.";

			public static LocString DISTANCE_TITLE = "Distance";

			public static LocString CURRENT_MODE_PREFIX = "Current: ";

			public static LocString MODE_BUTTON_TOOLTIP = "Switch to {0}.";

			public static LocString DISTANCE_MODE = "Distance Mode";

			public static LocString DISTANCE_MODE_TOOLTIP = "Count resources within the configured distance.";

			public static LocString ROOM_MODE = "Room Mode";

			public static LocString ROOM_MODE_TOOLTIP = "Count resources in the same room.";

			public static LocString GLOBAL_MODE = "Global Mode";

			public static LocString GLOBAL_MODE_TOOLTIP = "Count resources in the current world.";

			public static LocString INCLUDE_STORAGE_CHECKBOX = "Include Storage Bins";

			public static LocString INCLUDE_STORAGE_CHECKBOX_TOOLTIP = "Also count matching resources stored in storage buildings.";
		}

		public class THRESHOLD_SWITCH_SIDESCREEN
		{
			public static LocString RESOURCE_TOOLTIP_ABOVE = "Will send a " + STRINGS.UI.FormatAsAutomationState("Green Signal", STRINGS.UI.AutomationState.Active) + " if " + STRINGS.UI.PRE_KEYWORD + "Total Mass" + STRINGS.UI.PST_KEYWORD + " is above <b>{0}</b>";

			public static LocString RESOURCE_TOOLTIP_BELOW = "Will send a " + STRINGS.UI.FormatAsAutomationState("Green Signal", STRINGS.UI.AutomationState.Active) + " if the " + STRINGS.UI.PRE_KEYWORD + "Total Mass" + STRINGS.UI.PST_KEYWORD + " is below <b>{0}</b>";
		}
	}

	public class BUILDINGS
	{
		public class PREFABS
		{
			public class LOGICRESOURCESENSOR
			{
				public static LocString NAME = STRINGS.UI.FormatAsLink("Resource Sensor", LogicResourceSensorConfig.ID);

				public static LocString DESCRIPTION = "Detecting resources can enable complex storage automations.";

				public static LocString EFFECT = "Sends a " + STRINGS.UI.FormatAsAutomationState("Green Signal", STRINGS.UI.AutomationState.Active) + " or a " + STRINGS.UI.FormatAsAutomationState("Red Signal", STRINGS.UI.AutomationState.Standby) + " based on count mode and material amount.";

				public static LocString LOGIC_PORT = "Material Count";

				public static LocString LOGIC_PORT_ACTIVE = "Sends a " + STRINGS.UI.FormatAsAutomationState("Green Signal", STRINGS.UI.AutomationState.Active) + " if the total mass of counted resources is greater than the selected threshold.";

				public static LocString LOGIC_PORT_INACTIVE = "Otherwise, sends a " + STRINGS.UI.FormatAsAutomationState("Red Signal", STRINGS.UI.AutomationState.Standby);

				public static LocString SIDESCREEN_TITLE = "Resource Sensor";
			}
		}
	}
}
