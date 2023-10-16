using System.Collections.Generic;
using System.Linq;
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
                var control = arm.GetComponent<SolidTransferArmControl>();
                if (control == null) return flag;
                return control.isCrossWall || flag;
            }
        }
    }
}