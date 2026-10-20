using System.Collections.Generic;
using System.Linq;
using Harmony;
using UnityEngine;

namespace SuperAnimal
{
    public class DropPatch
    {
        [HarmonyPatch(typeof(EntityTemplates), "ExtendEntityToBasicCreature")]
        public static class EntityTemplatesExtendEntityToBasicCreaturePatch
        {
            public static void Postfix(GameObject template)
            {
                var component = template.AddOrGet<Butcherable>();
                var drops = component.drops;
                drops = drops.AddToArray("Meat");
                drops = drops.AddRangeToArray(drops);
                component.SetDrops(drops);
                
            }
        }
    }
}