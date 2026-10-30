using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ColonyTweaks
{
    public class DropPatch
    {
        [HarmonyPatch(typeof(EntityTemplates), nameof(EntityTemplates.ExtendEntityToBasicCreature), new[] { typeof(EntityTemplates.ExtendEntityToBasicCreatureData) })]
        public static class EntityTemplatesExtendEntityToBasicCreaturePatch
        {
            public static void Postfix(GameObject __result)
            {
                var component = __result.GetComponent<Butcherable>();
                if (component == null || component.drops == null)
                {
                    return;
                }
                var drops = new Dictionary<string, float>(component.drops);
                if (!drops.ContainsKey("Meat"))
                {
                    drops["Meat"] = 0f;
                }
                drops["Meat"] += 1f;
                foreach (var drop in new List<string>(drops.Keys))
                {
                    drops[drop] *= 2f;
                }
                component.SetDrops(drops);
            }
        }
    }
}
