using HarmonyLib;
using Klei.AI;
using STRINGS;
using UnityEngine;

namespace CritterPlantTweaks
{
    public class EffectPatch
    {
        [HarmonyPatch(typeof(EntityTemplates), nameof(EntityTemplates.ExtendEntityToWildCreature), new[] { typeof(GameObject), typeof(int), typeof(bool) })]
        public class EntityTemplatesExtendEntityToWildCreaturePatch
        {
            public static void Postfix(GameObject __result)
            {
                var def = __result.AddOrGetDef<WildnessMonitor.Def>();
                if (def is not { tameEffect: { } tameEffect, wildEffect: { } })
                {
                    return;
                }

                tameEffect.SelfModifiers.RemoveAll(modifier =>
                    modifier.AttributeId == Db.Get().CritterAttributes.Happiness.Id);
                tameEffect.Add(new AttributeModifier(Db.Get().Amounts.ScaleGrowth.deltaAttribute.Id, 1f,
                    CREATURES.MODIFIERS.TAME.NAME, true));
            }
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public class DbInitializePatch
        {
            public static void Postfix(Db __instance)
            {
                var ranchedEffect = __instance.effects.Get("Ranched");
                ranchedEffect.Add(new AttributeModifier(Db.Get().Amounts.ScaleGrowth.deltaAttribute.Id, 1f,
                    CREATURES.MODIFIERS.RANCHED.NAME, true));
                ApplyFeederEffect(__instance.effects.Get("HadMilk"), CREATURES.MODIFIERS.HADMILK.NAME, false);
                ApplyFeederEffect(__instance.effects.Get("HadInk"), CREATURES.MODIFIERS.HADINK.NAME, false);
            }
        }

        [HarmonyPatch(typeof(CreatureCalorieMonitor), "InitializeStates")]
        public class CreatureCalorieMonitorInitializeStatesPatch
        {
            public static void Postfix(CreatureCalorieMonitor __instance)
            {
                var outOfCaloriesTame = Traverse.Create(__instance).Field<Effect>("outOfCaloriesTame").Value;
                outOfCaloriesTame.SelfModifiers.RemoveAll(modifier =>
                    modifier.AttributeId == Db.Get().CritterAttributes.Happiness.Id);
            }
        }

        private static void ApplyFeederEffect(Effect effect, string modifierName, bool replaceHappiness)
        {
            var wildnessId = Db.Get().Amounts.Wildness.deltaAttribute.Id;
            var happinessId = Db.Get().CritterAttributes.Happiness.Id;
            var scaleGrowthId = Db.Get().Amounts.ScaleGrowth.deltaAttribute.Id;
            effect.SelfModifiers.RemoveAll(modifier =>
                modifier.AttributeId == wildnessId ||
                modifier.AttributeId == scaleGrowthId ||
                replaceHappiness && modifier.AttributeId == happinessId);
            effect.Add(new AttributeModifier(wildnessId, -60f / effect.duration, modifierName));
            if (replaceHappiness)
            {
                effect.Add(new AttributeModifier(happinessId, 3f, modifierName));
            }
            effect.Add(new AttributeModifier(scaleGrowthId, 0.5f, modifierName, true));
        }
    }
}
