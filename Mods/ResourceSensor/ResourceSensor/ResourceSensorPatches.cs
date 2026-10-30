using System;
using Database;
using HarmonyLib;
using UnityEngine;

namespace ResourceSensor;

public class ResourceSensorPatches
{
	[HarmonyPatch(typeof(GeneratedBuildings))]
	[HarmonyPatch("LoadGeneratedBuildings")]
	public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
	{
		public static void Prefix()
		{
			AddLocalizedStrings();
			ModUtil.AddBuildingToPlanScreen("Automation", LogicResourceSensorConfig.ID);
		}
	}

	[HarmonyPatch(typeof(Techs))]
	[HarmonyPatch("Init")]
	public static class Database_Techs_Init_Patch
	{
		public static void Postfix(Techs __instance)
		{
			__instance.TryGet("GenericSensors").unlockedItemIDs.Add(LogicResourceSensorConfig.ID);
		}
	}

	private static void AddLocalizedStrings()
	{
		string buildingKey = "STRINGS.BUILDINGS.PREFABS." + LogicResourceSensorConfig.ID.ToUpperInvariant();
		Add(buildingKey + ".NAME", STRINGS.UI.FormatAsLink(Localized("Resource Sensor", "资源传感器"), LogicResourceSensorConfig.ID));
		Add(buildingKey + ".DESC", Localized("Detecting resources can enable complex storage automations.", "检测资源数量可以实现更复杂的存储自动化。"));
		Add(buildingKey + ".EFFECT", Localized(
			"Sends a " + STRINGS.UI.FormatAsAutomationState("Green Signal", STRINGS.UI.AutomationState.Active) + " or a " + STRINGS.UI.FormatAsAutomationState("Red Signal", STRINGS.UI.AutomationState.Standby) + " based on count mode and material amount.",
			"根据计数模式和材料质量输出" + STRINGS.UI.FormatAsAutomationState("绿色信号", STRINGS.UI.AutomationState.Active) + "或" + STRINGS.UI.FormatAsAutomationState("红色信号", STRINGS.UI.AutomationState.Standby) + "。"));
		Add(buildingKey + ".LOGIC_PORT", Localized("Material Count", "材料计数"));
		Add(buildingKey + ".LOGIC_PORT_ACTIVE", Localized(
			"Sends a " + STRINGS.UI.FormatAsAutomationState("Green Signal", STRINGS.UI.AutomationState.Active) + " if the total mass of counted resources is greater than the selected threshold.",
			"当计入资源的总质量大于所选阈值时输出" + STRINGS.UI.FormatAsAutomationState("绿色信号", STRINGS.UI.AutomationState.Active) + "。"));
		Add(buildingKey + ".LOGIC_PORT_INACTIVE", Localized(
			"Otherwise, sends a " + STRINGS.UI.FormatAsAutomationState("Red Signal", STRINGS.UI.AutomationState.Standby),
			"否则输出" + STRINGS.UI.FormatAsAutomationState("红色信号", STRINGS.UI.AutomationState.Standby) + "。"));
		Add("STRINGS.UI.SIDESCREENS.RESOURCE_SENSOR.TITLE", Localized("Resource Sensor", "资源传感器"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.VALUE_NAME", Localized("Value", "数值"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.DISTANCE_TITLE", Localized("Distance", "距离"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.SLIDER_TOOLTIP", Localized(
			"Resources further than " + STRINGS.UI.FormatAsKeyWord("{0}") + " tiles will not be counted.",
			"不会计入距离超过" + STRINGS.UI.FormatAsKeyWord("{0}") + "格的资源。"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.CURRENT_MODE_PREFIX", Localized("Current: ", "当前："));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.MODE_BUTTON_TOOLTIP", Localized("Switch to {0}.", "切换为{0}。"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.DISTANCE_MODE", Localized("Distance Mode", "距离模式"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.DISTANCE_MODE_TOOLTIP", Localized("Count resources within the configured distance.", "统计指定距离内的资源。"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.ROOM_MODE", Localized("Room Mode", "房间模式"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.ROOM_MODE_TOOLTIP", Localized("Count resources in the same room.", "统计同一房间内的资源。"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.GLOBAL_MODE", Localized("Global Mode", "全局模式"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.GLOBAL_MODE_TOOLTIP", Localized("Count resources in the current world.", "统计当前世界内的资源。"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.INCLUDE_STORAGE_CHECKBOX", Localized("Include Storage Bins", "计入存储箱"));
		Add("STRINGS.UI.UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.INCLUDE_STORAGE_CHECKBOX_TOOLTIP", Localized("Also count matching resources stored in storage buildings.", "同时统计存储建筑中符合筛选的资源。"));
		Add("STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.RESOURCE_TOOLTIP_ABOVE", Localized(
			"Will send a " + STRINGS.UI.FormatAsAutomationState("Green Signal", STRINGS.UI.AutomationState.Active) + " if " + STRINGS.UI.PRE_KEYWORD + "Total Mass" + STRINGS.UI.PST_KEYWORD + " is above <b>{0}</b>",
			"当" + STRINGS.UI.PRE_KEYWORD + "总质量" + STRINGS.UI.PST_KEYWORD + "高于 <b>{0}</b> 时输出" + STRINGS.UI.FormatAsAutomationState("绿色信号", STRINGS.UI.AutomationState.Active)));
		Add("STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.RESOURCE_TOOLTIP_BELOW", Localized(
			"Will send a " + STRINGS.UI.FormatAsAutomationState("Green Signal", STRINGS.UI.AutomationState.Active) + " if the " + STRINGS.UI.PRE_KEYWORD + "Total Mass" + STRINGS.UI.PST_KEYWORD + " is below <b>{0}</b>",
			"当" + STRINGS.UI.PRE_KEYWORD + "总质量" + STRINGS.UI.PST_KEYWORD + "低于 <b>{0}</b> 时输出" + STRINGS.UI.FormatAsAutomationState("绿色信号", STRINGS.UI.AutomationState.Active)));
	}

	private static void Add(string key, string value)
	{
		Strings.Add(key, value);
	}

	internal static string Localized(string en, string zh)
	{
		string code = Localization.GetCurrentLanguageCode();
		return !string.IsNullOrEmpty(code) && code.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? zh : en;
	}

	[HarmonyPatch(typeof(SingleCheckboxSideScreen), "IsValidForTarget")]
	public static class SingleCheckboxSideScreen_IsValidForTarget_Patch
	{
		public static void Postfix(GameObject target, ref bool __result)
		{
			if (__result && target.GetComponent<LogicResourceSensor>() is { } sensor)
			{
				__result = sensor.Mode != LogicResourceSensor.SensorMode.Global;
			}
		}
	}
}
