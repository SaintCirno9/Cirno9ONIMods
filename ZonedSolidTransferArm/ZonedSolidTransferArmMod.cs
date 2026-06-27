using System.IO;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Actions;
using PeterHan.PLib.Core;
using UnityEngine;

namespace ZonedSolidTransferArm;

// 已知行为：抓取逻辑不校验 world id，传送臂可跨世界（含其他星球、火箭内部）抓取区域内物品，此为有意设计。
// 副作用：火箭内部世界被拆除后，落在其 Grid 区域内的区域格子（裸 int 索引）不会被清理，存档也会固化。
// Grid 不缩小、IsValidCell 仍返回 true，这些格子会被持续扫描（数量少，性能可忽略）；若区域被新建火箭复用，臂会转而抓取新火箭内的物品。
// 目前接受此行为，不做清理。如需精确化，可监听火箭内部世界销毁事件剔除对应格子。
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
