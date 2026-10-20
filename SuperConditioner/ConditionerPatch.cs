using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using SuperConditioner.STRINGS;
using UnityEngine;

namespace SuperConditioner
{
    public class ConditionerPatch
    {
        public static void OnLoad()
        {
            LocString.CreateLocStringKeys(typeof(UI));
        }

        [HarmonyPatch(typeof(LiquidConditionerConfig), "CreateBuildingDef")]
        public static class LiquidConditionerAdjust
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.EnergyConsumptionWhenActive /= 10;
            }
        }

        [HarmonyPatch(typeof(AirConditionerConfig), "CreateBuildingDef")]
        public static class AirConditionerAdjust
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.EnergyConsumptionWhenActive /= 10;
            }
        }

        [HarmonyPatch(typeof(LiquidConditionerConfig), "ConfigureBuildingTemplate")]
        public static class LiquidConditionerAddControl
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<ConditionerControl>();
                var conduitConsumer = go.AddOrGet<ConduitConsumer>();
                conduitConsumer.consumptionRate = 100f;
                var defaultStorage = go.AddOrGet<Storage>();
                defaultStorage.capacityKg = 2f * conduitConsumer.consumptionRate;
            }
        }

        [HarmonyPatch(typeof(AirConditionerConfig), "ConfigureBuildingTemplate")]
        public static class AirConditionerAddControl
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<ConditionerControl>();
                var conduitConsumer = go.AddOrGet<ConduitConsumer>();
                conduitConsumer.consumptionRate = 10f;
                var defaultStorage = go.AddOrGet<Storage>();
                defaultStorage.capacityKg = 2f * conduitConsumer.consumptionRate;
            }
        }


        [HarmonyPatch(typeof(AirConditioner), "UpdateState")]
        public static class ConditionerHeatAdjust
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
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
                        break;
                    }
                }

                for (int i = 0; i < codes.Count - 1; i++)
                {
                    var opString = codes[i].opcode + " " + codes[i].operand;
                    if (opString == "ldloc.s System.Single (12)" &&
                        codes[i + 1].opcode == OpCodes.Neg)
                    {
                        codes.InsertRange(i + 1, new[]
                        {
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Callvirt,
                                typeof(ConditionerHeatAdjust).GetMethod(nameof(ProduceHeat)))
                        });
                        break;
                    }
                }

                return codes.AsEnumerable();
            }

            public static float SetTargetHeat(AirConditioner conditioner)
            {
                return conditioner.gameObject.GetComponent<ConditionerControl>().TargetTemp + 273;
            }

            public static float ProduceHeat(float produceHeatAmount, AirConditioner conditioner)
            {
                var control = conditioner.gameObject.GetComponent<ConditionerControl>();
                return control.AbsorbHeat == false ? produceHeatAmount : 0f;
            }
        }
    }
}