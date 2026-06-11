using HarmonyLib;
using Klei.AI;

namespace BuildingAdjust
{
    public class PowerTinkerPatch
    {
        private const string PowerTinkerEffectId = "PowerTinker";

        [HarmonyPatch(typeof(Effects), nameof(Effects.Sim1000ms))]
        public class EffectsSim1000msPatch
        {
            public static void Postfix(Effects __instance, float dt)
            {
                var powerTinker = __instance.Get(PowerTinkerEffectId);
                if (powerTinker is null)
                {
                    return;
                }

                var tinkerable = __instance.GetComponent<Tinkerable>();
                if (tinkerable is null || tinkerable.addedEffect != PowerTinkerEffectId)
                {
                    return;
                }

                var operational = __instance.GetComponent<Operational>();
                if (operational is null || operational.IsActive)
                {
                    return;
                }

                powerTinker.timeRemaining += dt;
            }
        }
    }
}
