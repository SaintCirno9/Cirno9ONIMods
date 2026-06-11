using HarmonyLib;

namespace CritterPlantTweaks
{
    public class DreckoPatch
    {
        [HarmonyPatch(typeof(DreckoConfig), "CreateDrecko")]
        public class DreckoConfigDreckoConfigPatch
        {
            public static void Prefix()
            {
                Traverse.Create<DreckoConfig>().Field<float>("KG_POOP_PER_DAY_OF_PLANT").Value *= 10;
            }
        }

        [HarmonyPatch(typeof(DreckoPlasticConfig), "CreateDrecko")]
        public class DreckoPlasticConfigDreckoConfigPatch
        {
            public static void Prefix()
            {
                Traverse.Create<DreckoConfig>().Field<float>("KG_POOP_PER_DAY_OF_PLANT").Value *= 10;
            }
        }
    }
}
