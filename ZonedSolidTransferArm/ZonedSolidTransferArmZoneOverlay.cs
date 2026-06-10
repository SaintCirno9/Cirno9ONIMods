using System.Collections.Generic;
using UnityEngine;

namespace ZonedSolidTransferArm;

public class ZonedSolidTransferArmZoneOverlay : OverlayModes.Mode
{
    public static readonly HashedString ID = "ZonedSolidTransferArmZone";

    public override HashedString ViewMode()
    {
        return ID;
    }

    public override string GetSoundName()
    {
        return "SuitRequired";
    }

    public static Color GetCellColor(SimDebugView instance, int cell)
    {
        return ZonedSolidTransferArmZoneTool.Target != null &&
               ZonedSolidTransferArmZoneTool.Target.ContainsCell(cell)
            ? ZonedSolidTransferArmControl.ZoneColor
            : Color.black;
    }

    public override List<LegendEntry> GetCustomLegendData()
    {
        return new List<LegendEntry>
        {
            new(
                ZonedSolidTransferArmStrings.UI.OVERLAYS.ZONE.MARKED,
                ZonedSolidTransferArmStrings.UI.OVERLAYS.ZONE.TOOLTIP,
                ZonedSolidTransferArmControl.ZoneColor)
        };
    }
}
