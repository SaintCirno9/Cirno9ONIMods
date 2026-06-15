using System.IO;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Actions;
using PeterHan.PLib.Core;
using UnityEngine;

namespace ZonedSolidTransferArm;

public class ZonedSolidTransferArmMod : UserMod2
{
    public static PAction GlobalZoneAction { get; private set; }
    public static PAction RemoveGlobalZoneAction { get; private set; }
    private static bool generatedLocalizationTemplates;

    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        PUtil.InitLibrary();
        RegisterLocalization();
        GlobalZoneAction = new PActionManager().CreateAction(
            "ZONEDSOLIDTRANSFERARM.GLOBALZONE.ACTION",
            ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.NAME),
            new PKeyBinding(KKeyCode.Z, Modifier.Alt));
        RemoveGlobalZoneAction = new PActionManager().CreateAction(
            "ZONEDSOLIDTRANSFERARM.GLOBALZONE.REMOVEACTION",
            ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.REMOVENAME),
            new PKeyBinding(KKeyCode.Z, Modifier.Shift));
    }

    internal static void RegisterLocalization()
    {
        Localization.RegisterForTranslation(typeof(ZonedSolidTransferArmStrings));
        LoadCurrentLocalization();
        LocString.CreateLocStringKeys(typeof(ZonedSolidTransferArmStrings), null);
        GenerateLocalizationTemplates();
    }

    internal static void LoadCurrentLocalization()
    {
        string localeCode = Localization.GetLocale()?.Code;
        if (string.IsNullOrEmpty(localeCode))
        {
            return;
        }

        string poPath = Path.Combine(PUtil.GetModPath(typeof(ZonedSolidTransferArmMod).Assembly), "translations", localeCode + ".po");
        if (!File.Exists(poPath))
        {
            return;
        }

        Localization.OverloadStrings(Localization.LoadStringsFile(poPath, false));
        Debug.Log("[ZonedSolidTransferArm] Found translation file for " + localeCode + ".");
    }

    private static void GenerateLocalizationTemplates()
    {
        if (generatedLocalizationTemplates)
        {
            return;
        }
        generatedLocalizationTemplates = true;

        string modPath = PUtil.GetModPath(typeof(ZonedSolidTransferArmMod).Assembly);
        string translationFolder = Path.Combine(modPath, "translations");
        Directory.CreateDirectory(translationFolder);

        Localization.GenerateStringsTemplate(
            typeof(ZonedSolidTransferArmStrings),
            Path.Combine(modPath, "strings_templates"));
        Localization.GenerateStringsTemplate(
            typeof(ZonedSolidTransferArmStrings).Namespace,
            typeof(ZonedSolidTransferArmMod).Assembly,
            Path.Combine(modPath, "translation_template.pot"),
            null);
        Localization.GenerateStringsTemplate(
            typeof(ZonedSolidTransferArmStrings).Namespace,
            typeof(ZonedSolidTransferArmMod).Assembly,
            Path.Combine(translationFolder, "translation_template.pot"),
            null);
    }
}

[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
[HarmonyAfter("PeterHan.PLib")]
[HarmonyPriority(Priority.Last)]
public static class ZonedSolidTransferArmLocalizationInitializePatch
{
    public static void Postfix()
    {
        ZonedSolidTransferArmMod.RegisterLocalization();
    }
}
