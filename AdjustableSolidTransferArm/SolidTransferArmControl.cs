using System;
using System.Collections.Concurrent;
using HarmonyLib;
using KSerialization;
using UnityEngine;

namespace AdjustableSolidTransferArm;

public class SolidTransferArmControl : KMonoBehaviour, IMultiSliderControl, ICheckboxControl
{
    private static readonly ConcurrentDictionary<SolidTransferArm, SolidTransferArmControl> Controls = new();
    private ISliderControl[] sliders;

    public static bool IsCrossWallEnabled(SolidTransferArm arm)
    {
        return Controls.TryGetValue(arm, out var control) && control.isCrossWall;
    }

    public static bool IsCellInRange(SolidTransferArm arm, int originX, int originY, int targetX, int targetY)
    {
        return !Controls.TryGetValue(arm, out var control) ||
               Math.Abs(targetX - originX) <= control.rangeX &&
               Math.Abs(targetY - originY) <= control.rangeY;
    }

    public string SidescreenTitleKey => ControlTitleKey;
    public ISliderControl[] sliderControls => sliders ??= new ISliderControl[]
    {
        new AxisSlider(this, 0),
        new AxisSlider(this, 1)
    };

    public bool SidescreenEnabled()
    {
        return true;
    }

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
        UpdateRange();
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
        rangeX = sourceControl.rangeX;
        rangeY = sourceControl.rangeY;
        range = Math.Max(rangeX, rangeY);
        isCrossWall = sourceControl.isCrossWall;
        UpdateRange();
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        NormalizeRanges();
        UpdateRange();
        Controls[solidTransferArm] = this;
    }

    protected override void OnCleanUp()
    {
        Controls.TryRemove(solidTransferArm, out _);
        base.OnCleanUp();
    }

    private void NormalizeRanges()
    {
        var defaultRange = range > 0 ? range : solidTransferArm.pickupRange;
        if (rangeX < 1)
        {
            rangeX = defaultRange;
        }
        if (rangeY < 1)
        {
            rangeY = defaultRange;
        }
        range = Math.Max(rangeX, rangeY);
    }

    private int GetRange(int axis)
    {
        return axis == 0 ? rangeX : rangeY;
    }

    private void SetRange(int axis, int value)
    {
        if (axis == 0)
        {
            rangeX = value;
        }
        else
        {
            rangeY = value;
        }
        UpdateRange();
    }

    private void UpdateRange()
    {
        NormalizeRanges();
        var maxRange = Math.Max(rangeX, rangeY);
        solidTransferArm.pickupRange = maxRange;
        Traverse.Create(solidTransferArm).Field<ChoreConsumer>("choreConsumer").Value.SetReach(maxRange);
        rangeVisualizer.RangeMin.x = -rangeX;
        rangeVisualizer.RangeMin.y = -rangeY;
        rangeVisualizer.RangeMax.x = rangeX;
        rangeVisualizer.RangeMax.y = rangeY;
        if (isCrossWall)
        {
            rangeVisualizer.BlockingCb = _ => false;
        }
        else
        {
            rangeVisualizer.BlockingCb = Grid.IsSolidCell;
        }
    }

    [Serialize] public int range;
    [Serialize] public int rangeX;
    [Serialize] public int rangeY;
    [Serialize] public bool isCrossWall;

    [MyCmpReq] public SolidTransferArm solidTransferArm;
    [MyCmpReq] public RangeVisualizer rangeVisualizer;
    [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;

    private class AxisSlider : ISliderControl
    {
        private readonly SolidTransferArmControl parent;
        private readonly int axis;

        public AxisSlider(SolidTransferArmControl parent, int axis)
        {
            this.parent = parent;
            this.axis = axis;
        }

        public string SliderTitleKey => axis == 0
            ? "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.XRANGETITLE"
            : "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.YRANGETITLE";

        public string SliderUnits => "";

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
            return 32;
        }

        public float GetSliderValue(int index)
        {
            return parent.GetRange(axis);
        }

        public void SetSliderValue(float percent, int index)
        {
            parent.SetRange(axis, Convert.ToInt32(percent));
        }

        public string GetSliderTooltipKey(int index)
        {
            return axis == 0
                ? "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.XRANGETOOLTIP"
                : "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.YRANGETOOLTIP";
        }

        public string GetSliderTooltip(int index)
        {
            return Strings.Get(GetSliderTooltipKey(index));
        }
    }
}
