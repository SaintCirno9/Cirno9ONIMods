using HarmonyLib;
using UnityEngine;

namespace ColonyTweaks
{
    public class OvercrowdedPatch
    {
        [HarmonyPatch(typeof(EntityTemplates), nameof(EntityTemplates.ExtendEntityToWildCreature), new[] { typeof(GameObject), typeof(int), typeof(bool) })]
        public class EntityTemplatesExtendEntityToWildCreature
        {
            public static void Postfix(GameObject __result)
            {
                var def = __result.GetDef<OvercrowdingMonitor.Def>();
                if (def != null)
                {
                    def.spaceRequiredPerCreature = 0;
                }
            }
        }

        [HarmonyPatch(typeof(OvercrowdingMonitor.Instance), MethodType.Constructor, new[] { typeof(IStateMachineTarget), typeof(OvercrowdingMonitor.Def) })]
        public class OvercrowdingMonitorInstancePatch
        {
            public static void Prefix(OvercrowdingMonitor.Def def)
            {
                def.spaceRequiredPerCreature = 0;
            }
        }
    }
}
