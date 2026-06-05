using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace AdjustableSolidTransferArm
{
    public class SolidTransferArmPatch : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            LocString.CreateLocStringKeys(typeof(SolidTransferArmControlStrings.UI));
        }

        [HarmonyPatch(typeof(SolidTransferArm), "OnPrefabInit")]
        public class SolidTransferArmOnPrefabInitPatch
        {
            public static void Postfix(SolidTransferArm __instance)
            {
                if (__instance.GetComponent<RangeVisualizer>() != null)
                {
                    __instance.gameObject.AddOrGet<SolidTransferArmControl>();
                }
            }
        }

        [HarmonyPatch(typeof(SolidTransferArm), "AsyncUpdate")]
        public class SolidTransferArmAsyncUpdatePatch
        {
            private static readonly MethodInfo GridIsPhysicallyAccessibleMethod = AccessTools.Method(
                typeof(Grid),
                nameof(Grid.IsPhysicallyAccessible),
                new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) });

            private static readonly MethodInfo IsPhysicallyAccessibleMethod = AccessTools.Method(
                typeof(SolidTransferArmAsyncUpdatePatch),
                nameof(IsPhysicallyAccessible));

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if (instruction.Calls(GridIsPhysicallyAccessibleMethod))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, IsPhysicallyAccessibleMethod);
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }

            public static bool IsPhysicallyAccessible(int x, int y, int x2, int y2, bool blockingTileVisible, SolidTransferArm arm)
            {
                if (!SolidTransferArmControl.IsCellInRange(arm, x, y, x2, y2))
                {
                    return false;
                }
                if (!SolidTransferArmControl.IgnoresNoPickZone(arm) && NoPickZone.ContainsCell(Grid.XYToCell(x2, y2)))
                {
                    return false;
                }
                return SolidTransferArmControl.IsCrossWallEnabled(arm) ||
                       Grid.IsPhysicallyAccessible(x, y, x2, y2, blockingTileVisible);
            }
        }
    }
}
