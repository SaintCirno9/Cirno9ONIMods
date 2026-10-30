using Harmony;

namespace SuperAnimal
{
    public class OvercrowdedPatch
    {
        [HarmonyPatch(typeof(EntityTemplates), "ExtendEntityToWildCreature")]
        public class EntityTemplatesExtendEntityToWildCreature
        {
            public static void Prefix(ref int space_required_per_creature)
            {
                space_required_per_creature = 0;
            }
        }
    }
}