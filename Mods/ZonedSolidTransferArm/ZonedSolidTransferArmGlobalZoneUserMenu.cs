using UnityEngine;

namespace ZonedSolidTransferArm;

public class ZonedSolidTransferArmGlobalZoneUserMenu : KMonoBehaviour
{
    [MyCmpReq]
    public Building building;

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        Subscribe(493375141, OnRefreshUserMenu);
    }

    private void OnRefreshUserMenu(object data)
    {
        bool inGlobalZone = IsBuildingAreaInGlobalZone();
        Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo(
            inGlobalZone ? "action_cancel" : "action_move_to_storage",
            ZonedSolidTransferArmStrings.Text(inGlobalZone
                ? ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.REMOVEGLOBALZONEBUTTON
                : ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ADDGLOBALZONEBUTTON),
            ToggleBuildingAreaGlobalZone,
            global::Action.NumActions,
            null,
            null,
            null,
            ZonedSolidTransferArmStrings.Text(inGlobalZone
                ? ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.REMOVEGLOBALZONEBUTTONTOOLTIP
                : ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ADDGLOBALZONEBUTTONTOOLTIP)), 0.45f);
    }

    private void ToggleBuildingAreaGlobalZone()
    {
        if (IsBuildingAreaInGlobalZone())
        {
            ZonedSolidTransferArmGlobalZone.RemoveCells(building.PlacementCells);
        }
        else
        {
            ZonedSolidTransferArmGlobalZone.AddCells(building.PlacementCells);
        }
        Game.Instance.Trigger(1980521255, gameObject);
    }

    private bool IsBuildingAreaInGlobalZone()
    {
        foreach (int cell in building.PlacementCells)
        {
            if (!ZonedSolidTransferArmGlobalZone.ContainsPersistentCell(cell))
            {
                return false;
            }
        }
        return true;
    }
}
