using System.Collections.Generic;
using System.Reflection;
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
            private static readonly MethodInfo SetTargetHeatMethod = AccessTools.Method(
                typeof(ConditionerHeatAdjust),
                nameof(SetTargetHeat));
            private static readonly MethodInfo AdjustProducedHeatMethod = AccessTools.Method(
                typeof(ConditionerHeatAdjust),
                nameof(AdjustProducedHeat));
            private static readonly MethodInfo PrimaryElementTemperatureGetter = AccessTools.PropertyGetter(
                typeof(PrimaryElement),
                nameof(PrimaryElement.Temperature));
            private static readonly FieldInfo TemperatureDeltaField = AccessTools.Field(
                typeof(AirConditioner),
                nameof(AirConditioner.temperatureDelta));
            private static readonly FieldInfo SpecificHeatCapacityField = AccessTools.Field(
                typeof(Element),
                nameof(Element.specificHeatCapacity));

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                bool found1 = TryPatchTargetTemperature(codes);
                bool found2 = TryPatchProducedHeat(codes);

                if (!found1)
                {
                    Debug.LogError("BetterCoolers: Failed to patch AirConditioner.UpdateState target temperature");
                }

                if (!found2)
                {
                    Debug.LogError("BetterCoolers: Failed to patch AirConditioner.UpdateState produced heat");
                }

                return codes;
            }

            private static bool TryPatchTargetTemperature(List<CodeInstruction> codes)
            {
                if (PrimaryElementTemperatureGetter == null || TemperatureDeltaField == null)
                {
                    return false;
                }

                for (int i = 1; i < codes.Count - 4; i++)
                {
                    if (codes[i].Calls(PrimaryElementTemperatureGetter) &&
                        codes[i + 1].opcode == OpCodes.Ldarg_0 &&
                        LoadsField(codes[i + 2], TemperatureDeltaField) &&
                        codes[i + 3].opcode == OpCodes.Add &&
                        IsStoreLocal(codes[i + 4].opcode))
                    {
                        codes[i - 1] = CopyLabelsAndBlocks(codes[i - 1], new CodeInstruction(OpCodes.Ldarg_0));
                        codes[i] = new CodeInstruction(OpCodes.Call, SetTargetHeatMethod);
                        codes.RemoveRange(i + 1, 3);
                        return true;
                    }
                }

                return false;
            }

            private static bool TryPatchProducedHeat(List<CodeInstruction> codes)
            {
                if (SpecificHeatCapacityField == null)
                {
                    return false;
                }

                for (int i = 0; i < codes.Count; i++)
                {
                    if (!LoadsField(codes[i], SpecificHeatCapacityField))
                    {
                        continue;
                    }

                    for (int j = i + 1; j < codes.Count && j < i + 8; j++)
                    {
                        if (IsStoreLocal(codes[j].opcode))
                        {
                            codes.InsertRange(j, new[]
                            {
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Call, AdjustProducedHeatMethod)
                            });
                            return true;
                        }
                    }
                }

                return false;
            }

            private static bool LoadsField(CodeInstruction instruction, FieldInfo field)
            {
                return instruction.opcode == OpCodes.Ldfld && Equals(instruction.operand, field);
            }

            private static bool IsStoreLocal(OpCode opcode)
            {
                return opcode == OpCodes.Stloc ||
                       opcode == OpCodes.Stloc_0 ||
                       opcode == OpCodes.Stloc_1 ||
                       opcode == OpCodes.Stloc_2 ||
                       opcode == OpCodes.Stloc_3 ||
                       opcode == OpCodes.Stloc_S;
            }

            private static CodeInstruction CopyLabelsAndBlocks(CodeInstruction source, CodeInstruction replacement)
            {
                replacement.labels.AddRange(source.labels);
                replacement.blocks.AddRange(source.blocks);
                source.labels.Clear();
                source.blocks.Clear();
                return replacement;
            }

            public static float SetTargetHeat(AirConditioner conditioner)
            {
                return conditioner.gameObject.GetComponent<BetterCoolerControl>().TargetTemp;
            }

            public static float AdjustProducedHeat(float heat, AirConditioner conditioner)
            {
                var control = conditioner.gameObject.GetComponent<BetterCoolerControl>();
                if (!control.ProduceHeat)
                {
                    return 0f;
                }

                var delta = control.TargetTemp - conditioner.lastGasTemp;
                var maxDelta = Mathf.Abs(conditioner.temperatureDelta);
                var absDelta = Mathf.Abs(delta);
                if (absDelta <= maxDelta || absDelta <= 0.01f)
                {
                    return heat;
                }
                return heat * (maxDelta / absDelta);
            }
        }
    }
}
