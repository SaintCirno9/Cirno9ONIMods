using HarmonyLib;
using KMod;
using TUNING;

namespace CritterPlantTweaks
{
    public class CritterPlantTweaksMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            ApplyTuning();
        }

        private static void ApplyTuning()
        {
            CREATURES.CONVERSION_EFFICIENCY.BAD_1 += 1f;
            CREATURES.CONVERSION_EFFICIENCY.BAD_2 += 1f;
            CREATURES.CONVERSION_EFFICIENCY.NORMAL += 1f;
            CREATURES.CONVERSION_EFFICIENCY.GOOD_1 += 1f;
            CREATURES.CONVERSION_EFFICIENCY.GOOD_2 += 1f;
            CREATURES.CONVERSION_EFFICIENCY.GOOD_3 += 1f;
            StaterpillarTuning.POOP_CONVERSTION_RATE *= 10;
        }
    }
}
