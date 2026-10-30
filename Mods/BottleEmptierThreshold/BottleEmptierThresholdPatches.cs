using HarmonyLib;
using UnityEngine;

namespace BottleEmptierThreshold;

[HarmonyPatch(typeof(BottleEmptierConfig), nameof(BottleEmptierConfig.ConfigureBuildingTemplate))]
public static class BottleEmptierConfigConfigureBuildingTemplatePatch
{
    public static void Postfix(GameObject go)
    {
        go.AddOrGet<BottleEmptierThresholdControl>();
    }
}

[HarmonyPatch(typeof(BottleEmptierGasConfig), nameof(BottleEmptierGasConfig.ConfigureBuildingTemplate))]
public static class BottleEmptierGasConfigConfigureBuildingTemplatePatch
{
    public static void Postfix(GameObject go)
    {
        go.AddOrGet<BottleEmptierThresholdControl>();
    }
}

[HarmonyPatch(typeof(BottleEmptierConduitLiquidConfig), nameof(BottleEmptierConduitLiquidConfig.ConfigureBuildingTemplate))]
public static class BottleEmptierConduitLiquidConfigConfigureBuildingTemplatePatch
{
    public static void Postfix(GameObject go)
    {
        go.AddOrGet<BottleEmptierThresholdControl>();
    }
}

[HarmonyPatch(typeof(BottleEmptierConduitGasConfig), nameof(BottleEmptierConduitGasConfig.ConfigureBuildingTemplate))]
public static class BottleEmptierConduitGasConfigConfigureBuildingTemplatePatch
{
    public static void Postfix(GameObject go)
    {
        go.AddOrGet<BottleEmptierThresholdControl>();
    }
}

[HarmonyPatch(typeof(BottleEmptier.StatesInstance), "CreateChore")]
public static class BottleEmptierStatesInstanceCreateChorePatch
{
    public static bool Prefix(BottleEmptier.StatesInstance __instance)
    {
        var control = __instance.GetComponent<BottleEmptierThresholdControl>();
        control?.RefreshFetchChore();
        return false;
    }
}
