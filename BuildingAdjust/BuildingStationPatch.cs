using HarmonyLib;
using Klei.AI;
using STRINGS;
using UnityEngine;

namespace BuildingAdjust
{
    public class BuildingStationPatch
    {
        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public class DbInitializePatch
        {
            public static void Postfix(Db __instance)
            {
                var farmTinkerEffect = __instance.effects.Get("FarmTinker");
                ReplaceModifierValue(farmTinkerEffect, 0, 2f);
                farmTinkerEffect.duration *= 2f;
                ReplaceModifierValue(__instance.effects.Get("PowerTinker"), 0, 100f);
            }
        }

        [HarmonyPatch(typeof(CreatureFeederConfig), nameof(CreatureFeederConfig.ConfigureBuildingTemplate))]
        public class CreatureFeederConfigConfigureBuildingTemplatePatch
        {
            public static void Postfix()
            {
                ApplyFeederEffect(Db.Get().effects.Get("AteFromFeeder"), CREATURES.MODIFIERS.ATE_FROM_FEEDER.NAME, true);
            }
        }

        [HarmonyPatch(typeof(RanchStationConfig), nameof(RanchStationConfig.DoPostConfigureComplete))]
        public class RanchStationConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                DoubleRanchedTimeRemaining(go);
            }
        }

        [HarmonyPatch(typeof(UnderwaterRanchStationConfig), nameof(UnderwaterRanchStationConfig.DoPostConfigureComplete))]
        public class UnderwaterRanchStationConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                DoubleRanchedTimeRemaining(go);
            }
        }

        [HarmonyPatch(typeof(IncubationMonitor.Instance), "UpdateIncubationState")]
        public class IncubationMonitorUpdateIncubationStatePatch
        {
            public static void Postfix(IncubationMonitor.Instance __instance)
            {
                var effectId = "IncubatorEffect";
                var effectName = "在孵化器中孵化";
                var effectDesc = "这个小动物蛋正在孵化器中快乐的孵化";
                var incubatorEffect = new Effect(effectId, effectName, effectDesc, 0, true, false, false);
                var incubatorModifer = new AttributeModifier(Db.Get().Amounts.Incubation.deltaAttribute.Id,
                    0.025f, effectName);
                incubatorEffect.Add(incubatorModifer);
                if (__instance.sm.incubatorIsActive.Get(__instance))
                {
                    __instance.Get<Effects>().Add(incubatorEffect, false);
                }
                else
                {
                    __instance.Get<Effects>().Remove(effectId);
                }
            }
        }

        [HarmonyPatch(typeof(CreatureDeliveryPoint), "OnPrefabInit")]
        public class CreatureDeliveryPointOnPrefabInitPatch
        {
            public static void Postfix(CreatureDeliveryPoint __instance)
            {
                __instance.GetComponent<BaggableCritterCapacityTracker>().maximumCreatures = 99;
            }
        }

        [HarmonyPatch(typeof(StaterpillarGeneratorConfig), nameof(StaterpillarGeneratorConfig.CreateBuildingDef))]
        public class StaterpillarGeneratorConfigCreateBuildingDefPatch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.GeneratorWattageRating *= 3f;
            }
        }

        private static void ReplaceModifierValue(Effect effect, int modifierIndex, float value)
        {
            var modifier = effect.SelfModifiers[modifierIndex];
            AttributeModifier replacement;
            if (modifier.DescriptionCB != null)
            {
                replacement = new AttributeModifier(modifier.AttributeId, value, modifier.NameCB, modifier.DescriptionCB,
                    modifier.IsMultiplier, modifier.UIOnly);
            }
            else
            {
                replacement = new AttributeModifier(modifier.AttributeId, value, modifier.Description,
                    modifier.IsMultiplier, modifier.UIOnly);
            }
            replacement.OverrideTimeSlice = modifier.OverrideTimeSlice;
            effect.SelfModifiers[modifierIndex] = replacement;
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

        private static void DoubleRanchedTimeRemaining(GameObject go)
        {
            var def = go.GetDef<RanchStation.Def>();
            var originalOnRanchComplete = def.OnRanchCompleteCb;
            def.OnRanchCompleteCb = (creatureGo, rancherWb) =>
            {
                originalOnRanchComplete(creatureGo, rancherWb);
                creatureGo.GetComponent<Effects>().Get("Ranched").timeRemaining *= 2f;
            };
        }
    }
}
