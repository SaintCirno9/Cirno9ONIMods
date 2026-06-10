using UnityEngine.EventSystems;

namespace ZonedSolidTransferArm;

public class ZonedSolidTransferArmGlobalZoneToolRightClick : KMonoBehaviour, IPointerClickHandler
{
    private static bool suppressNextRightClickCancel;

    public static bool ConsumeSuppressNextRightClickCancel()
    {
        if (!suppressNextRightClickCancel)
        {
            return false;
        }

        suppressNextRightClickCancel = false;
        return true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        eventData.Use();
        suppressNextRightClickCancel = true;
        ZonedSolidTransferArmGlobalZoneTool.ActivateRemoveMode();
    }
}
