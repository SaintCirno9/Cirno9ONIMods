using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ColonyTweaks
{
    // 备用方案：使用Transpiler直接替换所有对MAX_QUEUE_SIZE的访问
    public class FabricatorQueuePatchTranspiler
    {
        // Transpiler替换IncrementRecipeQueueCount中的MAX_QUEUE_SIZE比较
        [HarmonyPatch(typeof(ComplexFabricator), "IncrementRecipeQueueCount")]
        public class IncrementRecipeQueueCountPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var maxQueueSizeField = AccessTools.Field(typeof(ComplexFabricator), "MAX_QUEUE_SIZE");

                for (int i = 0; i < codes.Count; i++)
                {
                    // 查找 ldsfld MAX_QUEUE_SIZE 指令
                    if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand is FieldInfo field && field == maxQueueSizeField)
                    {
                        // 替换为加载常量999
                        codes[i] = new CodeInstruction(OpCodes.Ldc_I4, 999);
                    }
                }

                return codes;
            }
        }

        // Transpiler替换DecrementRecipeQueueCountInternal中的MAX_QUEUE_SIZE赋值
        [HarmonyPatch(typeof(ComplexFabricator), "DecrementRecipeQueueCountInternal")]
        public class DecrementRecipeQueueCountInternalPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var maxQueueSizeField = AccessTools.Field(typeof(ComplexFabricator), "MAX_QUEUE_SIZE");

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand is FieldInfo field && field == maxQueueSizeField)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Ldc_I4, 999);
                    }
                }

                return codes;
            }
        }

        // Transpiler替换GetRemainingQueueCount中的MAX_QUEUE_SIZE返回值
        [HarmonyPatch(typeof(ComplexFabricator), "GetRemainingQueueCount")]
        public class GetRemainingQueueCountPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var maxQueueSizeField = AccessTools.Field(typeof(ComplexFabricator), "MAX_QUEUE_SIZE");

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand is FieldInfo field && field == maxQueueSizeField)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Ldc_I4, 999);
                    }
                }

                return codes;
            }
        }
    }
}
