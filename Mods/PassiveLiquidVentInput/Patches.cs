using System;
using System.IO;
using System.Reflection;
using Harmony;

namespace PassiveLiquidVentInput
{
    public class Patches
    {
        public static void OnLoad()
        {
        }

        private static void Localize()
        {
            InitLocalization(typeof(STRINGS), "", false);
        }

        public static void InitLocalization(Type locstring_tree_root, string filename_prefix = "",
            bool writeStringsTemplate = false)
        {
            ModUtil.RegisterForTranslation(locstring_tree_root);
            var executingAssembly = Assembly.GetExecutingAssembly();
            var name = executingAssembly.GetName().Name;
            var path = Path.Combine(Path.GetDirectoryName(executingAssembly.Location) ?? string.Empty,
                "translations");
            var locale = Localization.GetLocale();
            if (locale != null)
            {
                try
                {
                    string text = Path.Combine(path, filename_prefix + locale.Code + ".po");
                    if (File.Exists(text))
                    {
                        Debug.Log(name + ": Localize file found " + text);
                        Localization.OverloadStrings(Localization.LoadStringsFile(text, false));
                    }
                }
                catch
                {
                    Debug.LogWarning(name + " Failed to load localization.");
                }
            }

            LocString.CreateLocStringKeys(typeof(STRINGS), "");
        }


        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch("LoadGeneratedBuildings")]
        public static class GeneratedBuildingsLoadGeneratedBuildingsPatch
        {
            // Token: 0x0600002A RID: 42 RVA: 0x00002307 File Offset: 0x00000507
            public static void Prefix()
            {
                ModUtil.AddBuildingToPlanScreen("Plumbing", "PassiveLiquidVentInput");
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class DbInitializePatch
        {
            public static void Postfix()
            {
                Db.Get().Techs.Get("LiquidPiping").unlockedItemIDs.Add("PassiveLiquidVentInput");
            }
        }

        [HarmonyPatch(typeof(Localization))]
        [HarmonyPatch("Initialize")]
        private class LocalisationInitializePatch
        {
            public static void Postfix()
            {
                Localize();
            }
        }
    }
}