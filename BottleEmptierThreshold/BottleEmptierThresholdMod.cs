using System.IO;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using UnityEngine;

namespace BottleEmptierThreshold;

public class BottleEmptierThresholdMod : UserMod2
{
    private static bool generatedLocalizationTemplates;

    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        PUtil.InitLibrary();
        RegisterLocalization();
    }

    internal static void RegisterLocalization()
    {
        Localization.RegisterForTranslation(typeof(BottleEmptierThresholdStrings));
        LoadCurrentLocalization();
        LocString.CreateLocStringKeys(typeof(BottleEmptierThresholdStrings), null);
        GenerateLocalizationTemplates();
    }

    internal static void LoadCurrentLocalization()
    {
        string localeCode = Localization.GetLocale()?.Code;
        if (string.IsNullOrEmpty(localeCode))
        {
            return;
        }

        string poPath = Path.Combine(PUtil.GetModPath(typeof(BottleEmptierThresholdMod).Assembly), "translations", localeCode + ".po");
        if (!File.Exists(poPath))
        {
            return;
        }

        Localization.OverloadStrings(Localization.LoadStringsFile(poPath, false));
        Debug.Log("[BottleEmptierThreshold] Found translation file for " + localeCode + ".");
    }

    private static void GenerateLocalizationTemplates()
    {
        if (generatedLocalizationTemplates)
        {
            return;
        }

        generatedLocalizationTemplates = true;
        string modPath = PUtil.GetModPath(typeof(BottleEmptierThresholdMod).Assembly);
        string translationFolder = Path.Combine(modPath, "translations");
        Directory.CreateDirectory(translationFolder);

        Localization.GenerateStringsTemplate(
            typeof(BottleEmptierThresholdStrings),
            Path.Combine(modPath, "strings_templates"));
        Localization.GenerateStringsTemplate(
            typeof(BottleEmptierThresholdStrings).Namespace,
            typeof(BottleEmptierThresholdMod).Assembly,
            Path.Combine(modPath, "translation_template.pot"),
            null);
        Localization.GenerateStringsTemplate(
            typeof(BottleEmptierThresholdStrings).Namespace,
            typeof(BottleEmptierThresholdMod).Assembly,
            Path.Combine(translationFolder, "translation_template.pot"),
            null);
    }
}

[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
[HarmonyAfter("PeterHan.PLib")]
[HarmonyPriority(Priority.Last)]
public static class BottleEmptierThresholdLocalizationInitializePatch
{
    public static void Postfix()
    {
        BottleEmptierThresholdMod.RegisterLocalization();
    }
}
