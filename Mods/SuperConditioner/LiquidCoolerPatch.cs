using Harmony;
using STRINGS;

namespace SuperConditioner
{
    public class LiquidCoolerPatch
    {
        [HarmonyPatch(typeof(Db), "Initialize")]
        internal class LiquidCoolerTechMod
        {
            private static void Postfix()
            {
                Db.Get().Techs.Get("LiquidTemperature").unlockedItemIDs.Add("LiquidCooler");
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        internal class LiquidCoolerUI
        {
            private static void Prefix()
            {
                string[] value = {
                    "STRINGS.BUILDINGS.PREFABS.LIQUIDCOOLER.NAME",
                    "液体冷却器"
                };
                Strings.Add(value);
                string[] value2 = {
                    "STRINGS.BUILDINGS.PREFABS.LIQUIDCOOLER.DESC",
                    "冷却器将液体冷却以至其凝结。"
                };
                Strings.Add(value2);
                string[] array = new string[2];
                array[0] = "STRINGS.BUILDINGS.PREFABS.LIQUIDCOOLER.EFFECT";
                string[] values = {
                    string.Concat(new[]
                    {
                        "冷却大量的",
                        UI.FormatAsLink("液体", "ELEMENTS_LIQUID"),
                        "。\n\n必须完全浸没。"
                    })
                };
                array[1] = string.Concat(values);
                Strings.Add(array);
                ModUtil.AddBuildingToPlanScreen("Utilities", "LiquidCooler");
            }
        }
    }
}