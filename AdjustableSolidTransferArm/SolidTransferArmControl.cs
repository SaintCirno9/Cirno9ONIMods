using System;
using System.Collections.Generic;
using HarmonyLib;
using KSerialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace AdjustableSolidTransferArm;

public class SolidTransferArmControl : KMonoBehaviour, IIntSliderControl, ICheckboxControl
{
    public int SliderDecimalPlaces(int index)
    {
        return 0;
    }

    public float GetSliderMin(int index)
    {
        return 1;
    }

    public float GetSliderMax(int index)
    {
        return 16;
    }

    public float GetSliderValue(int index)
    {
        return range;
    }

    public void SetSliderValue(float percent, int index)
    {
        range = Convert.ToInt32(percent);
        UpdateRange();
        UpdateVisualizers();
    }

    public string GetSliderTooltipKey(int index)
    {
        return "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.SLIDERTOOLTIP";
    }

    public string GetSliderTooltip()
    {
        return Strings.Get(GetSliderTooltipKey(0));
    }

    public string SliderTitleKey => ControlTitleKey;
    public string SliderUnits => "";
    public string ControlTitleKey => "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.TITLE";
    
    public string HideRangeCheckboxLabel =>
        Strings.Get("STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.HIDERANGECHECKBOXLABEL");

    public string HideRangeCheckboxTooltip =>
        Strings.Get("STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.HIDERANGECHECKBOXTOOLTIP");

    public string CrossWallCheckboxLabel =>
        Strings.Get("STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.CROSSWALLCHECKBOXLABEL");

    public string CrossWallCheckboxTooltip =>
        Strings.Get("STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.CROSSWALLCHECKBOXTOOLTIP");

    public string CheckboxTitleKey => ControlTitleKey;
    public string CheckboxLabel => CrossWallCheckboxLabel;
    public string CheckboxTooltip => CrossWallCheckboxTooltip;
    
    public bool GetCheckboxValue()
    {
        return isCrossWall;
    }

    public void SetCheckboxValue(bool value)
    {
        isCrossWall = value;
        UpdateVisualizers();
    }

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        Subscribe(-905833192, OnCopySettings);
    }

    private void OnCopySettings(object data)
    {
        if (data is not GameObject go || go.GetComponent<SolidTransferArmControl>() is not
                { } sourceControl) return;
        range = sourceControl.range;
        isCrossWall = sourceControl.isCrossWall;
        UpdateRange();
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        if (range < 1)
        {
            range = solidTransferArm.pickupRange;
        }
        else
        {
            UpdateRange();
        }
    }


    private void UpdateRange()
    {
        solidTransferArm.pickupRange = range;
        Traverse.Create(solidTransferArm).Field<ChoreConsumer>("choreConsumer").Value.SetReach(range);
        rangeVisualizer.x = -range;
        rangeVisualizer.y = -range;
        rangeVisualizer.width = range * 2 + 1;
        rangeVisualizer.height = range * 2 + 1;
    }
    
    private void UpdateVisualizers()
    {
        AccessTools.Method(typeof(StationaryChoreRangeVisualizer), "UpdateVisualizers")
            .Invoke(rangeVisualizer, null);
    }
    
    private void ClearVisualizers()
    {
        AccessTools.Method(typeof(StationaryChoreRangeVisualizer), "ClearVisualizers")
            .Invoke(rangeVisualizer, null);
    }

    [Serialize] public int range;
    [Serialize] public bool isCrossWall;
    // [Serialize] public bool hideRange;

    [MyCmpReq] public SolidTransferArm solidTransferArm;
    [MyCmpReq] public StationaryChoreRangeVisualizer rangeVisualizer;
    [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;
    // public void SetHideRange(bool value)
    // {
    //     hideRange = value;
    //     if (hideRange)
    //     {
    //         ClearVisualizers();
    //         return;
    //     }
    //     UpdateVisualizers();
    // }
    //
    // public bool GetHideRange()
    // {
    //     return hideRange;
    // }
}