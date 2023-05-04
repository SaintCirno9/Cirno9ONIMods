using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using KMod;
using UnityEngine;
using PeterHan.PLib.UI;

namespace AdjustableSolidTransferArm
{
    public class SolidTransferArmPatch : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            LocString.CreateLocStringKeys(typeof(SolidTransferArmControlStrings.UI));
        }

        [HarmonyPatch(typeof(SolidTransferArmConfig), "DoPostConfigureComplete")]
        public class SolidTransferArmConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<SolidTransferArmControl>();
            }
        }

        [HarmonyPatch(typeof(SolidTransferArm), "AsyncUpdate")]
        public class SolidTransferArmAsyncUpdatePatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Ldloc_1 &&
                        codes[i + 2].opcode == OpCodes.Ldloc_S && codes[i + 3].opcode == OpCodes.Ldloc_3 &&
                        codes[i + 4].opcode == OpCodes.Ldc_I4_1)
                    {
                        codes.InsertRange(i + 6, new[]
                        {
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Call,
                                typeof(SolidTransferArmAsyncUpdatePatch).GetMethod(nameof(IsPhysicallyAccessible)))
                        });
                        break;
                    }
                }

                return codes.AsEnumerable();
            }

            public static bool IsPhysicallyAccessible(bool flag, SolidTransferArm arm)
            {
                return arm.GetComponent<SolidTransferArmControl>().isCrossWall || flag;
            }
        }

        [HarmonyPatch(typeof(StationaryChoreRangeVisualizer), "UpdateVisualizers")]
        public class StationaryChoreRangeVisualizerUpdateVisualizersPatch
        {
            // public static bool Prefix(StationaryChoreRangeVisualizer __instance)
            // {
            //     var target = __instance.GetComponentInParent<SolidTransferArmControl>();
            //     return target == null || !target.hideRange;
            // }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                for (int i = 0; i < codes.Count; i++)
                {
                    var opString = codes[i].opcode + " " + codes[i].operand;
                    if (opString ==
                        "call Boolean TestLineOfSight(Int32, Int32, Int32, Int32, System.Func`2[System.Int32,System.Boolean], Boolean)")
                    {
                        codes.InsertRange(i + 1,
                            new[]
                            {
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Call,
                                    typeof(StationaryChoreRangeVisualizerUpdateVisualizersPatch).GetMethod(
                                        nameof(CanAddCell)))
                            });
                    }
                }

                return codes.AsEnumerable();
            }

            public static bool CanAddCell(bool flag, StationaryChoreRangeVisualizer visualizer)
            {
                var target = visualizer.GetComponent<SolidTransferArmControl>();
                if (target == null)
                {
                    return flag;
                }

                return target.isCrossWall || flag;
            }
        }

        // [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
        // public class DetailsScreenOnPrefabInitPatch
        // {
        //     public static void Postfix(List<DetailsScreen.SideScreenRef> ___sideScreens)
        //     {
        //         // PUIUtils.AddSideScreenContent<SolidTransferArmControlSideScreen>();
        //         // if ((___sideScreens.Last().screenPrefab is not SolidTransferArmControlSideScreen screen) ||
        //         //     ___sideScreens.Find((screenRef) => screenRef.name == "SingleCheckboxSideScreen").screenPrefab is not
        //         //         SingleCheckboxSideScreen checkboxScreen)
        //         //     return;
        //         // Debug.Log("Initing SolidTransferArmControlSideScreen");
        //         // screen.Init(checkboxScreen);
        //         if (___sideScreens.Find((screenRef) => screenRef.name == "SingleCheckboxSideScreen").screenPrefab is not
        //                 SingleCheckboxSideScreen checkboxScreen)
        //             return;
        //         var gameObject = Object.Instantiate(checkboxScreen.gameObject);
        //         gameObject.name = "SolidTransferArmControlSideScreen";
        //         var activeSelf = gameObject.activeSelf;
        //         gameObject.SetActive(false);
        //         var component = gameObject.GetComponent<SingleCheckboxSideScreen>();
        //         var screen = gameObject.AddComponent<SolidTransferArmControlSideScreen>();
        //         // screen.crossWallToggle = component.toggle;
        //         // screen.crossWallCheckMark = component.toggleCheckMark;
        //         // screen.crossWallLabel = component.label;
        //         screen.showRangeToggle = component.toggle;
        //         screen.showRangeCheckMark = component.toggleCheckMark;
        //         screen.showRangeLabel = component.label;
        //         Object.DestroyImmediate(component);
        //         gameObject.SetActive(activeSelf);
        //         screen.transform.SetParent(checkboxScreen.transform.parent);
        //         ___sideScreens.Add(new DetailsScreen.SideScreenRef
        //         {
        //             name = "SolidTransferArmControlSideScreen",
        //             screenPrefab = screen
        //         });
        //         
        //     }
        // }
    }
}