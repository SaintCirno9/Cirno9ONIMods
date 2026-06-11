using HarmonyLib;

namespace CritterPlantTweaks
{
    public class SnailPatch
    {
        [HarmonyPatch(typeof(MoistureMonitor), "IsInLiquid")]
        public class MoistureMonitorIsInLiquidPatch
        {
            public static void Postfix(MoistureMonitor.Instance smi, ref bool __result)
            {
                if (__result && IsSnail(smi) && HasEnoughMucus(smi))
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(MoistureMonitor), "CanProduceLubricant")]
        public class MoistureMonitorCanProduceLubricantPatch
        {
            public static void Postfix(MoistureMonitor.Instance smi, ref bool __result)
            {
                if (!__result &&
                    IsSnail(smi) &&
                    HasEnoughMucus(smi) &&
                    !smi.effects.HasEffect(MoistureMonitor.RECENTLY_PRODUCED_LUBRICANT_EFFECT) &&
                    Grid.IsSubstantialLiquid(Grid.PosToCell(smi), 0.02f))
                {
                    __result = true;
                }
            }
        }

        private static bool IsSnail(MoistureMonitor.Instance smi)
        {
            var brain = smi.GetComponent<CreatureBrain>();
            return brain != null && brain.species == GameTags.Creatures.Species.SnailSpecies;
        }

        private static bool HasEnoughMucus(MoistureMonitor.Instance smi)
        {
            return smi.mucusAmount.value >= (smi.HasTag(GameTags.Creatures.Dry) ? 2f : 10f);
        }
    }
}
