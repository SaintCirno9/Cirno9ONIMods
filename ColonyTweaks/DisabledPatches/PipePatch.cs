using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;

namespace ColonyTweaks
{
    public class PipePatch
    {
        [HarmonyPatch(typeof(ConduitFlow), MethodType.Constructor, typeof(ConduitType), typeof(int),
            typeof(IUtilityNetworkMgr), typeof(float), typeof(float))]
        public class ConduitFlowConstructorPatch
        {
            public static void Prefix(ConduitType conduit_type, ref float max_conduit_mass)
            {
                if (conduit_type is ConduitType.Liquid)
                {
                    max_conduit_mass = ColonyTweaks.LiquidPumpRate;
                }

                if (conduit_type is ConduitType.Gas)
                {
                    max_conduit_mass = ColonyTweaks.GasPumpRate;
                }
            }
        }

        [HarmonyPatch(typeof(SolidConduitDispenser), "ConduitUpdate")]
        private static class SolidConduitDispenserConduitUpdatePatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_R4 && Math.Abs((float) codes[i].operand - 20f) < 0.1f)
                    {
                        codes[i].operand = 100f;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}