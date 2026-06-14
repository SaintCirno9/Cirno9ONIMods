using HarmonyLib;
using KMod;
using PeterHan.PLib.Actions;
using PeterHan.PLib.Core;

namespace ZonedSolidTransferArm;

public class ZonedSolidTransferArmMod : UserMod2
{
    public static PAction GlobalZoneAction { get; private set; }
    public static PAction RemoveGlobalZoneAction { get; private set; }

    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        PUtil.InitLibrary();
        LocString.CreateLocStringKeys(typeof(ZonedSolidTransferArmStrings.UI));
        RegisterToolParameterStrings();
        GlobalZoneAction = new PActionManager().CreateAction(
            "ZONEDSOLIDTRANSFERARM.GLOBALZONE.ACTION",
            ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.NAME,
            new PKeyBinding(KKeyCode.Z, Modifier.Alt));
        RemoveGlobalZoneAction = new PActionManager().CreateAction(
            "ZONEDSOLIDTRANSFERARM.GLOBALZONE.REMOVEACTION",
            ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.REMOVENAME,
            new PKeyBinding(KKeyCode.Z, Modifier.Shift));
    }

    private static void RegisterToolParameterStrings()
    {
        Strings.Add(
            "STRINGS.UI.TOOLS.FILTERLAYERS.ZONEDSOLIDTRANSFERARM_GLOBALZONE_ADD.NAME",
            ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.ADDNAME);
        Strings.Add(
            "STRINGS.UI.TOOLS.FILTERLAYERS.ZONEDSOLIDTRANSFERARM_GLOBALZONE_ADD.TOOLTIP",
            ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.ADDTOOLTIP);
        Strings.Add(
            "STRINGS.UI.TOOLS.FILTERLAYERS.ZONEDSOLIDTRANSFERARM_GLOBALZONE_REMOVE.NAME",
            ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.REMOVENAME);
        Strings.Add(
            "STRINGS.UI.TOOLS.FILTERLAYERS.ZONEDSOLIDTRANSFERARM_GLOBALZONE_REMOVE.TOOLTIP",
            ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.REMOVETOOLTIP);
        Strings.Add(
            "STRINGS.UI.TOOLS.FILTERLAYERS.ZONEDSOLIDTRANSFERARM_GLOBALZONE_TEMPORARY_CONSTRUCTION.NAME",
            ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.TEMPORARYCONSTRUCTIONNAME);
        Strings.Add(
            "STRINGS.UI.TOOLS.FILTERLAYERS.ZONEDSOLIDTRANSFERARM_GLOBALZONE_TEMPORARY_CONSTRUCTION.TOOLTIP",
            ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.TEMPORARYCONSTRUCTIONTOOLTIP);
        Strings.Add(
            "STRINGS.UI.TOOLS.FILTERLAYERS.ZONEDSOLIDTRANSFERARM_GLOBALZONE_TEMPORARY_CLEAR.NAME",
            ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.TEMPORARYCLEARNAME);
        Strings.Add(
            "STRINGS.UI.TOOLS.FILTERLAYERS.ZONEDSOLIDTRANSFERARM_GLOBALZONE_TEMPORARY_CLEAR.TOOLTIP",
            ZonedSolidTransferArmStrings.UI.TOOLS.GLOBALZONE.TEMPORARYCLEARTOOLTIP);
    }
}
