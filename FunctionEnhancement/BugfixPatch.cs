using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using UnityEngine;

namespace FunctionEnhancement
{
    public class BugfixPatch
    {
        [HarmonyPatch(typeof(KAnimBatch.AtlasList), "Apply")]
        public class AtlasListApplyPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 1; i < codes.Count; i++)
                {
                    var opString = codes[i].opcode + " " + codes[i].operand;
                    if (opString == "call Void LogError(System.Object)")
                    {
                        codes[i].operand = typeof(AtlasListApplyPatch).GetMethod(nameof(DoNothing));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }

            public static void DoNothing(object obj)
            {
                return;
            }
        }
    }
}