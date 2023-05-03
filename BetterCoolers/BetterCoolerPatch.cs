using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace BetterCoolers
{
    public class BetterCoolerPatch : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            Debug.Log("BetterCoolers Loaded");
            LocString.CreateLocStringKeys(typeof(BetterCoolerControlStrings.UI));
        }

        [HarmonyPatch(typeof(LiquidConditionerConfig), "ConfigureBuildingTemplate")]
        public static class LiquidConditionerAddControl
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<BetterCoolerControl>();
            }
        }

        [HarmonyPatch(typeof(AirConditionerConfig), "ConfigureBuildingTemplate")]
        public static class AirConditionerAddControl
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<BetterCoolerControl>();
            }
        }


        [HarmonyPatch(typeof(AirConditioner), "UpdateState")]
        public static class ConditionerHeatAdjust
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                bool found1 = false;
                bool found2 = false;
                for (int i = 1; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Stloc_S && codes[i + 1].opcode == OpCodes.Ldloc_S &&
                        codes[i].operand == codes[i + 1].operand && codes[i - 1].opcode == OpCodes.Add)
                    {
                        codes.InsertRange(i + 1, new[]
                        {
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Callvirt,
                                typeof(ConditionerHeatAdjust).GetMethod(nameof(SetTargetHeat))),
                            new CodeInstruction(codes[i].opcode, codes[i].operand)
                        });
                        found1 = true;
                        break;
                    }
                }

                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Mul && codes[i + 1].opcode == OpCodes.Ldloc_S &&
                        codes[i + 2].opcode == OpCodes.Mul && codes[i + 3].opcode == OpCodes.Stloc_S)
                    {
                        codes.InsertRange(i + 3, new[]
                        {
                            new CodeInstruction(OpCodes.Ldc_R4, 0f),
                            new CodeInstruction(OpCodes.Mul)
                        });
                        found2 = true;
                        break;
                    }
                }

                if (!found1)
                {
                    Debug.LogError("BetterCoolers: Failed to patch AirConditioner.UpdateState, could not find target1");
                }

                if (!found2)
                {
                    Debug.LogError("BetterCoolers: Failed to patch AirConditioner.UpdateState, could not find target2");
                }

                return codes.AsEnumerable();
            }

            public static float SetTargetHeat(AirConditioner conditioner)
            {
                return conditioner.gameObject.GetComponent<BetterCoolerControl>().TargetTemp + 273f;
            }
        }
    }
}