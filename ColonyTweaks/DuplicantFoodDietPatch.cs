using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TUNING;

namespace ColonyTweaks
{
    // 让所有动物都能吃复制人食物。
    //
    // 原理：动物运行时的食谱（Diet）来自 DietManager.CollectSaveDiets / CollectDiets，
    // 这两个静态方法扫描所有 prefab、把每个物种的 Diet 收集进字典，之后
    // Stomach（消化）和 SolidConsumerMonitor（觅食）都通过 DietManager.GetPrefabDiet 读取它。
    // 因此只要在这两个方法的 Postfix 里给每份 Diet 追加“所有复制人食物”的 EatSolid 条目，
    // 就能一次性覆盖全部动物，无需逐物种打补丁。
    //
    // 注意：Diet.infos 是私有 set 且会触发派生数组重算，无法原地修改，必须用现有 infos
    // 加上新条目重新 new 一个 Diet（构造函数会重新计算 solidEdiblesInfo 等，使 CanEatAnySolid 生效）。
    public class DuplicantFoodDietPatch
    {
        [HarmonyPatch(typeof(DietManager), nameof(DietManager.CollectSaveDiets))]
        public class DietManagerCollectSaveDietsPatch
        {
            public static void Postfix(Dictionary<Tag, Diet> __result)
            {
                AddDuplicantFoodToAll(__result);
            }
        }

        [HarmonyPatch(typeof(DietManager), nameof(DietManager.CollectDiets))]
        public class DietManagerCollectDietsPatch
        {
            public static void Postfix(Dictionary<Tag, Diet> __result)
            {
                AddDuplicantFoodToAll(__result);
            }
        }

        private static void AddDuplicantFoodToAll(Dictionary<Tag, Diet> diets)
        {
            if (diets == null)
            {
                return;
            }

            foreach (Tag key in diets.Keys.ToList())
            {
                Diet updated = WithDuplicantFood(diets[key]);
                if (updated != null)
                {
                    diets[key] = updated;
                }
            }
        }

        private static Diet WithDuplicantFood(Diet original)
        {
            if (original == null || original.infos == null)
            {
                return null;
            }

            List<Diet.Info> infos = new List<Diet.Info>(original.infos);

            // 收集动物本来就能吃的 tag，避免重复条目（Diet 构造函数对重复 tag 会 LogError）。
            HashSet<Tag> existing = new HashSet<Tag>();
            foreach (Diet.Info info in infos)
            {
                foreach (Tag tag in info.consumedTags)
                {
                    existing.Add(tag);
                }
            }

            // 复用该动物原本的排泄物；没有就退回泥土。
            Tag poopTag = Tag.Invalid;
            foreach (Diet.Info info in infos)
            {
                if (info.producedElement != Tag.Invalid)
                {
                    poopTag = info.producedElement;
                    break;
                }
            }

            if (poopTag == Tag.Invalid)
            {
                poopTag = SimHashes.Dirt.CreateTag();
            }

            // 作弊开关：开启后把复制人食物的排泄量对齐到该动物原版食谱的最大产出质量。
            // 每周期排泄质量 = 燃烧卡路里 × (producedConversionRate / caloriesPerKg)，
            // 故取原食谱里最大的 producedConversionRate / caloriesPerKg 比值作为对齐基准。
            bool boost = ModConfig.Instance.BoostCritterPoopFromDuplicantFood;
            float maxRatio = 0f;
            if (boost)
            {
                foreach (Diet.Info info in original.infos)
                {
                    if (info.producedElement != Tag.Invalid && info.caloriesPerKg > 0f)
                    {
                        float ratio = info.producedConversionRate / info.caloriesPerKg;
                        if (ratio > maxRatio)
                        {
                            maxRatio = ratio;
                        }
                    }
                }
            }

            bool added = false;
            foreach (EdiblesManager.FoodInfo food in EdiblesManager.GetAllLoadedFoodTypes())
            {
                if (food.CaloriesPerUnit <= 0f)
                {
                    continue;
                }

                Tag foodTag = new Tag(food.Id);
                if (!existing.Add(foodTag))
                {
                    continue;
                }

                float conversionRate = (boost && maxRatio > 0f)
                    ? maxRatio * food.CaloriesPerUnit
                    : CREATURES.CONVERSION_EFFICIENCY.NORMAL;

                infos.Add(new Diet.Info(
                    new HashSet<Tag> { foodTag },
                    poopTag,
                    food.CaloriesPerUnit,
                    conversionRate,
                    food_type: Diet.Info.FoodType.EatSolid));
                added = true;
            }

            if (!added)
            {
                return null;
            }

            return new Diet(infos.ToArray());
        }
    }
}
