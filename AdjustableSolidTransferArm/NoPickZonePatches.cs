using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AdjustableSolidTransferArm
{
    public static class NoPickZonePatches
    {
        [HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
        public static class PlayerControllerOnPrefabInitPatch
        {
            public static void Postfix(PlayerController __instance)
            {
                List<InterfaceTool> tools = new List<InterfaceTool>(__instance.tools);
                GameObject toolObject = new GameObject("NoPickZoneTool");
                NoPickZoneTool tool = toolObject.AddComponent<NoPickZoneTool>();
                toolObject.transform.SetParent(__instance.gameObject.transform);
                toolObject.SetActive(true);
                toolObject.SetActive(false);
                tools.Add(tool);
                __instance.tools = tools.ToArray();
            }
        }

        [HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
        public static class ToolMenuCreateBasicToolsPatch
        {
            public static void Postfix(ToolMenu __instance)
            {
                __instance.basicTools.Add(ToolMenu.CreateToolCollection(
                    SolidTransferArmControlStrings.UI.TOOLS.NOPICKZONE.NAME,
                    "icon_action_cancel",
                    global::Action.Clear,
                    "NoPickZoneTool",
                    SolidTransferArmControlStrings.UI.TOOLS.NOPICKZONE.TOOLTIP,
                    false));
            }
        }

        [HarmonyPatch(typeof(SimDebugView), "OnPrefabInit")]
        public static class SimDebugViewOnPrefabInitPatch
        {
            public static void Postfix(Dictionary<HashedString, Func<SimDebugView, int, Color>> ___getColourFuncs)
            {
                if (!___getColourFuncs.ContainsKey(NoPickZoneOverlay.ID))
                {
                    ___getColourFuncs.Add(NoPickZoneOverlay.ID, NoPickZoneOverlay.GetCellColor);
                }
            }
        }

        [HarmonyPatch(typeof(OverlayScreen), "RegisterModes")]
        public static class OverlayScreenRegisterModesPatch
        {
            private static readonly MethodInfo RegisterModeMethod = AccessTools.Method(typeof(OverlayScreen), "RegisterMode");

            public static void Postfix(OverlayScreen __instance)
            {
                RegisterModeMethod.Invoke(__instance, new object[] { new NoPickZoneOverlay() });
            }
        }

        [HarmonyPatch(typeof(OverlayLegend), "OnSpawn")]
        public static class OverlayLegendOnSpawnPatch
        {
            public static void Prefix(List<OverlayLegend.OverlayInfo> ___overlayInfoList)
            {
                ___overlayInfoList.Add(new OverlayLegend.OverlayInfo
                {
                    name = SolidTransferArmControlStrings.UI.OVERLAYS.NOPICKZONE.NAME,
                    mode = NoPickZoneOverlay.ID,
                    infoUnits = new List<OverlayLegend.OverlayInfoUnit>(),
                    isProgrammaticallyPopulated = true
                });
            }
        }

        [HarmonyPatch(typeof(SaveGame), "OnPrefabInit")]
        public static class SaveGameOnPrefabInitPatch
        {
            public static void Postfix(SaveGame __instance)
            {
                NoPickZone.Clear();
                __instance.gameObject.AddOrGet<NoPickZoneSaveData>();
            }
        }
    }
}
