using HarmonyLib;

namespace ColonyTweaks
{
    public class CrabPatch
    {
        [HarmonyPatch(typeof(ThreatMonitor.Instance), nameof(ThreatMonitor.Instance.CheckForThreats))]
        public class ThreatMonitorCheckForThreatsPatch
        {
            public static bool Prefix(ThreatMonitor.Instance __instance, ref bool __result)
            {
                if (!IsTameCrab(__instance))
                {
                    return true;
                }

                __instance.ClearMainThreat();
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(ThreatMonitor.Instance), nameof(ThreatMonitor.Instance.WillFight))]
        public class ThreatMonitorWillFightPatch
        {
            public static void Postfix(ThreatMonitor.Instance __instance, ref bool __result)
            {
                if (IsTameCrab(__instance))
                {
                    __result = false;
                }
            }
        }

        private static bool IsTameCrab(ThreatMonitor.Instance instance)
        {
            var brain = instance.GetComponent<CreatureBrain>();
            var prefabId = instance.GetComponent<KPrefabID>();
            return brain != null &&
                   prefabId != null &&
                   brain.species == GameTags.Creatures.Species.CrabSpecies &&
                   !prefabId.HasTag(GameTags.Creatures.Wild);
        }
    }
}
