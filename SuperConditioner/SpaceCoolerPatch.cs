using Harmony;
using STRINGS;

namespace SuperConditioner
{
    public class SpaceCoolerPatch
    {
        [HarmonyPatch(typeof(Db), "Initialize")]
        internal class SpaceCoolerTechMod
        {
            private static void Postfix()
            {
                Db.Get().Techs.Get("TemperatureModulation").unlockedItemIDs.Add("SpaceCooler");
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        internal class SpaceCoolerUI
        {
            private static void Prefix()
            {
                string[] value =
                {
                    "STRINGS.BUILDINGS.PREFABS.SPACECOOLER.NAME",
                    "空间冷却器"
                };
                Strings.Add(value);
                string[] value2 =
                {
                    "STRINGS.BUILDINGS.PREFABS.SPACECOOLER.DESC",
                    "空间冷却器在通电状态下会不断吸收热量。"
                };
                Strings.Add(value2);
                string[] array = new string[2];
                array[0] = "STRINGS.BUILDINGS.PREFABS.SPACECOOLER.EFFECT";
                string[] values =
                {
                    "吸收适度的" + UI.FormatAsLink("热量", "HEAT") + "。"
                };
                array[1] = string.Concat(values);
                Strings.Add(array);
                ModUtil.AddBuildingToPlanScreen("Utilities", "SpaceCooler");
            }
        }
    }
}