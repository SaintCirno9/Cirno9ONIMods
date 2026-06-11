using Harmony;
using Klei.AI;

namespace ColonyTweaks
{
    public class MaterialPatch
    {
        [HarmonyPatch(typeof(LegacyModMain), "ConfigElements")]
        public class LegacyModMainConfigElements
        {
            public static void Postfix()
            {
                var plastic = ElementLoader.FindElementByHash(SimHashes.Polypropylene);
                plastic.attributeModifiers.AddRange(new[]
                {
                    new AttributeModifier(Db.Get().BuildingAttributes.Decor.Id, 20f, plastic.name),
                    new AttributeModifier(Db.Get().BuildingAttributes.OverheatTemperature.Id, 200f, plastic.name)
                });
            }
        }
    }
}