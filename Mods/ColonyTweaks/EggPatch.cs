using HarmonyLib;
using Klei.AI;

namespace ColonyTweaks
{
    public class EggPatch
    {
        [HarmonyPatch(typeof(IncubationMonitor), "InitializeStates")]
        public class IncubationMonitorInitializeStatesPatch
        {
            public static void Postfix(IncubationMonitor __instance)
            {
                var suppressedEffect = Traverse.Create(__instance).Field<Effect>("suppressedEffect").Value;
                suppressedEffect.SelfModifiers.RemoveAll(modifier =>
                    modifier.AttributeId == Db.Get().Amounts.Viability.deltaAttribute.Id);
            }
        }
    }
}
