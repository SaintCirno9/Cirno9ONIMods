using HarmonyLib;
using UnityEngine;

namespace ColonyTweaks
{
    // 修改生产建筑队列上限从99改为999
    public class FabricatorQueuePatch
    {
        // 在游戏启动时修改MAX_QUEUE_SIZE
        [HarmonyPatch(typeof(Game), "OnPrefabInit")]
        public class GameOnPrefabInitPatch
        {
            public static void Postfix()
            {
                ComplexFabricator.MAX_QUEUE_SIZE = 999;
                Debug.Log($"[ColonyTweaks] ComplexFabricator.MAX_QUEUE_SIZE 已设置为 {ComplexFabricator.MAX_QUEUE_SIZE}");
            }
        }

        // 修改输入框的最大值限制
        [HarmonyPatch(typeof(SelectedRecipeQueueScreen), "OnSpawn")]
        public class SelectedRecipeQueueScreenOnSpawnPatch
        {
            public static void Postfix(SelectedRecipeQueueScreen __instance)
            {
                if (__instance.QueueCount != null)
                {
                    __instance.QueueCount.maxValue = 999f;
                    __instance.QueueCount.minValue = 0f;

                    // 修改底层TMP_InputField的字符限制
                    if (__instance.QueueCount.field != null)
                    {
                        __instance.QueueCount.field.characterLimit = 4; // 允许输入4个字符（支持999）
                        Debug.Log($"[ColonyTweaks] QueueCount.field.characterLimit 已设置为 {__instance.QueueCount.field.characterLimit}");
                    }

                    Debug.Log($"[ColonyTweaks] SelectedRecipeQueueScreen.QueueCount 范围已设置为 {__instance.QueueCount.minValue} - {__instance.QueueCount.maxValue}");
                }
            }
        }

        // 在SetRecipeCategory时也设置一次，确保输入框已初始化
        [HarmonyPatch(typeof(SelectedRecipeQueueScreen), "SetRecipeCategory")]
        public class SelectedRecipeQueueScreenSetRecipeCategoryPatch
        {
            public static void Postfix(SelectedRecipeQueueScreen __instance)
            {
                if (__instance.QueueCount != null)
                {
                    __instance.QueueCount.maxValue = 999f;
                    __instance.QueueCount.minValue = 0f;

                    // 修改底层TMP_InputField的字符限制
                    if (__instance.QueueCount.field != null)
                    {
                        __instance.QueueCount.field.characterLimit = 4;
                    }
                }
            }
        }

        // 修改KNumberInputField的ProcessInput方法，在处理输入前修改maxValue
        [HarmonyPatch(typeof(KNumberInputField), "ProcessInput")]
        public class KNumberInputFieldProcessInputPatch
        {
            public static void Prefix(KNumberInputField __instance)
            {
                // 如果这个输入框的maxValue是99，将其改为999
                if (__instance.maxValue == 99f)
                {
                    __instance.maxValue = 999f;
                    // 同时修改字符限制
                    if (__instance.field != null && __instance.field.characterLimit == 2)
                    {
                        __instance.field.characterLimit = 4;
                    }
                }
            }
        }

        // 修改KNumberInputField的SetAmount方法，确保使用999作为上限
        [HarmonyPatch(typeof(KNumberInputField), "SetAmount")]
        public class KNumberInputFieldSetAmountPatch
        {
            public static void Prefix(KNumberInputField __instance)
            {
                // 如果这个输入框的maxValue是99，将其改为999
                if (__instance.maxValue == 99f)
                {
                    __instance.maxValue = 999f;
                    // 同时修改字符限制
                    if (__instance.field != null && __instance.field.characterLimit == 2)
                    {
                        __instance.field.characterLimit = 4;
                    }
                }
            }
        }
    }
}
