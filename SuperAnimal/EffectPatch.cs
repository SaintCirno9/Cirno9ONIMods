using System.Linq;
using Harmony;
using Klei.AI;
using STRINGS;
using UnityEngine;

namespace SuperAnimal
{
    public class EffectPatch
    {
        [HarmonyPatch(typeof(EntityTemplates), "ExtendEntityToWildCreature")]
        public class EntityTemplatesExtendEntityToWildCreaturePatch
        {
            public static void Postfix(GameObject __result)
            {
                var def = __result.AddOrGetDef<WildnessMonitor.Def>();
                if (def is not {tameEffect: { } tameEffect, wildEffect: { } wildEffect})
                {
                    return;
                }

                if (tameEffect.SelfModifiers.FirstOrDefault(modifier =>
                    modifier.AttributeId == Db.Get().CritterAttributes.Happiness.Id) is { } happinessModifier)
                {
                    happinessModifier.SetValue(5);
                    tameEffect.Add(new AttributeModifier(Db.Get().Amounts.ScaleGrowth.deltaAttribute.Id, 1f,
                        CREATURES.MODIFIERS.TAME.NAME, true));
                }


                /*if (wildEffect.SelfModifiers.FirstOrDefault(modifier =>
                    modifier.AttributeId == Db.Get().CritterAttributes.Metabolism.Id) is { } metabolismModifier)
                {
                    metabolismModifier.SetValue(5f);
                }*/
            }
        }

        [HarmonyPatch(typeof(ModifierSet), "LoadEffects")]
        public class ModifierSetLoadEffectsPatch
        {
            public static void Postfix(ModifierSet __instance)
            {
                var ranchedEffect = __instance.effects.Get("Ranched");
                /*ranchedEffect.Add(new AttributeModifier(Db.Get().Amounts.Fertility.deltaAttribute.Id, 6f,
                    CREATURES.MODIFIERS.RANCHED.NAME, true));
                ranchedEffect.Add(new AttributeModifier(Db.Get().CritterAttributes.Metabolism.Id, 100f,
                    CREATURES.MODIFIERS.RANCHED.NAME));*/
                ranchedEffect.Add(new AttributeModifier(Db.Get().Amounts.ScaleGrowth.deltaAttribute.Id, 1f,
                    CREATURES.MODIFIERS.RANCHED.NAME, true));
                /*var eggSongEffect = __instance.effects.Get("EggSong");
                var incubationModifier = eggSongEffect.SelfModifiers.FirstOrDefault(modifier =>
                    modifier.AttributeId == Db.Get().Amounts.Incubation.deltaAttribute.Id);
                if (incubationModifier is not null)
                {
                    incubationModifier.SetValue(9);
                }*/

                __instance.effects.Get("FarmTinker").SelfModifiers[0].SetValue(2f);
                __instance.effects.Get("PowerTinker").SelfModifiers[0].SetValue(100f);
            }
        }

        [HarmonyPatch(typeof(CreatureCalorieMonitor), "InitializeStates")]
        public class CreatureCalorieMonitorInitializeStatesPatch
        {
            public static void Postfix(CreatureCalorieMonitor __instance)
            {
                var wildHungryEffect = new Effect("WildHungryEffect",
                    "代谢减缓",
                    "这只小动物的代谢由于饥饿减缓了", 0.0f, true, false, false);
                wildHungryEffect.Add(new AttributeModifier(Db.Get().CritterAttributes.Metabolism.Id, -20f,
                    "代谢减缓"));
                var tameHungryEffect = new Effect("TameHungryEffect",
                    "代谢减缓",
                    "这只小动物的代谢由于饥饿减缓了", 0.0f, true, false, false);
                tameHungryEffect.Add(new AttributeModifier(Db.Get().CritterAttributes.Metabolism.Id, -90f,
                    "代谢减缓"));
                var hungryState = Traverse.Create(__instance).Field<CreatureCalorieMonitor.HungryStates>("hungry").Value
                    .hungry;
                var stateTarget = Traverse.Create(hungryState).Method("GetStateTarget")
                    .GetValue<StateMachine<CreatureCalorieMonitor, CreatureCalorieMonitor.Instance, IStateMachineTarget,
                        CreatureCalorieMonitor.Def>.TargetParameter>();
                hungryState.ToggleEffect((sim =>
                {
                    var prefabID = stateTarget.Get<KPrefabID>(sim);
                    return prefabID.HasTag(GameTags.Creatures.Wild) ? wildHungryEffect : tameHungryEffect;
                }));
            }
        }

        [HarmonyPatch(typeof(IncubationMonitor.Instance), "UpdateIncubationState")]
        public class IncubationMonitorInitializeStatesPatch
        {
            public static void Postfix(IncubationMonitor.Instance __instance, bool stored, EggIncubator incubator)
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

        /*[HarmonyPatch(typeof(CritterAttributes), MethodType.Constructor, typeof(ResourceSet))]
        public class CritterAttributesConstructorPatch
        {
            public static void Postfix(CritterAttributes __instance)
            {
                __instance.Metabolism.SetFormatter(new ToPercentAttributeFormatter(1000f));
            }
        }

        [HarmonyPatch(typeof(CreatureCalorieMonitor), "UpdateMetabolismCalorieModifier")]
        public class CreatureCalorieMonitorUpdateMetabolismCalorieModifier
        {
            public static void Postfix(CreatureCalorieMonitor.Instance smi)
            {
                if (smi.metabolism.GetTotalValue() > 1)
                {
                    smi.deltaCalorieMetabolismModifier.SetValue(-smi.metabolism.GetTotalValue() / 100);
                }
            }
        }*/
    }
}