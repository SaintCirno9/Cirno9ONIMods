using System.Collections.Generic;
using HarmonyLib;

namespace CritterPlantTweaks
{
    public class HatchPatch
    {
        [HarmonyPatch(typeof(BaseHatchConfig), "HardRockDiet")]
        public class BaseHatchConfigHardRockDiet
        {
            public static bool Prefix(Tag poopTag, float caloriesPerKg, float producedConversionRate, string diseaseId,
                float diseasePerKgProduced, ref List<Diet.Info> __result)
            {
                __result = new List<Diet.Info>
                {
                    new(new HashSet<Tag> { SimHashes.SandStone.CreateTag() },
                        SimHashes.SandStone.CreateTag(), caloriesPerKg, producedConversionRate, diseaseId,
                        diseasePerKgProduced),
                    new(new HashSet<Tag> { SimHashes.SedimentaryRock.CreateTag() },
                        SimHashes.SedimentaryRock.CreateTag(), caloriesPerKg, producedConversionRate, diseaseId,
                        diseasePerKgProduced),
                    new(new HashSet<Tag> { SimHashes.IgneousRock.CreateTag() },
                        SimHashes.IgneousRock.CreateTag(), caloriesPerKg, producedConversionRate, diseaseId,
                        diseasePerKgProduced),
                    new(new HashSet<Tag> { SimHashes.CrushedRock.CreateTag() },
                        SimHashes.CrushedRock.CreateTag(), caloriesPerKg, producedConversionRate, diseaseId,
                        diseasePerKgProduced),
                    new(new HashSet<Tag> { SimHashes.MaficRock.CreateTag() },
                        SimHashes.MaficRock.CreateTag(), caloriesPerKg, producedConversionRate, diseaseId,
                        diseasePerKgProduced),
                    new(new HashSet<Tag> { SimHashes.Obsidian.CreateTag() },
                        SimHashes.Obsidian.CreateTag(), caloriesPerKg, producedConversionRate, diseaseId,
                        diseasePerKgProduced),
                    new(new HashSet<Tag> { SimHashes.Granite.CreateTag() },
                        SimHashes.Granite.CreateTag(), caloriesPerKg, producedConversionRate, diseaseId,
                        diseasePerKgProduced),
                    new(new HashSet<Tag> { SimHashes.Katairite.CreateTag() },
                        SimHashes.Katairite.CreateTag(), caloriesPerKg, producedConversionRate, diseaseId,
                        diseasePerKgProduced),
                };
                return false;
            }
        }
    }
}
