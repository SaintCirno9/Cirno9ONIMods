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
        var offsetX = targetX - originX;
        var offsetY = targetY - originY;
        return !Controls.TryGetValue(arm, out var control) ||
               offsetX >= -control.rangeLeft &&
               offsetX <= control.rangeRight &&
               offsetY >= -control.rangeDown &&
               offsetY <= control.rangeUp;
    }

    public static bool IgnoresNoPickZone(SolidTransferArm arm)
    {
        return Controls.TryGetValue(arm, out var control) && control.ignoreNoPickZone;
    }

    public string SidescreenTitleKey => ControlTitleKey;
    public ISliderControl[] sliderControls => sliders ??= new ISliderControl[]
    {
        new DirectionSlider(this, 0),
        new DirectionSlider(this, 1),
        new DirectionSlider(this, 2),
        new DirectionSlider(this, 3)
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
        Subscribe(493375141, OnRefreshUserMenu);
    }

    private void OnRefreshUserMenu(object data)
    {
        var button = ignoreNoPickZone
            ? new KIconButtonMenu.ButtonInfo(
                "action_move_to_storage",
                Strings.Get("STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.USENOPICKZONEBUTTON"),
                ToggleNoPickZoneIgnore,
                global::Action.NumActions,
                null,
                null,
                null,
                Strings.Get("STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.USENOPICKZONEBUTTONTOOLTIP"))
            : new KIconButtonMenu.ButtonInfo(
                "action_move_to_storage",
                Strings.Get("STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.IGNORENOPICKZONEBUTTON"),
                ToggleNoPickZoneIgnore,
                global::Action.NumActions,
                null,
                null,
                null,
                Strings.Get("STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.IGNORENOPICKZONEBUTTONTOOLTIP"));
        Game.Instance.userMenu.AddButton(gameObject, button, 0.45f);
    }

    private void ToggleNoPickZoneIgnore()
    {
        ignoreNoPickZone = !ignoreNoPickZone;
    }

    private void OnCopySettings(object data)
    {
        if (data is not GameObject go || go.GetComponent<SolidTransferArmControl>() is not
                { } sourceControl) return;
        rangeLeft = sourceControl.rangeLeft;
        rangeRight = sourceControl.rangeRight;
        rangeDown = sourceControl.rangeDown;
        rangeUp = sourceControl.rangeUp;
        rangeX = Math.Max(rangeLeft, rangeRight);
        rangeY = Math.Max(rangeDown, rangeUp);
        range = Math.Max(rangeX, rangeY);
        isCrossWall = sourceControl.isCrossWall;
        ignoreNoPickZone = sourceControl.ignoreNoPickZone;
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
        if (rangeLeft < 1)
        {
            rangeLeft = rangeX;
        }
        if (rangeRight < 1)
        {
            rangeRight = rangeX;
        }
        if (rangeDown < 1)
        {
            rangeDown = rangeY;
        }
        if (rangeUp < 1)
        {
            rangeUp = rangeY;
        }
        rangeX = Math.Max(rangeLeft, rangeRight);
        rangeY = Math.Max(rangeDown, rangeUp);
        range = Math.Max(rangeX, rangeY);
    }

    private int GetRange(int direction)
    {
        return direction switch
        {
            0 => rangeLeft,
            1 => rangeRight,
            2 => rangeDown,
            _ => rangeUp
        };
    }

    private void SetRange(int direction, int value)
    {
        switch (direction)
        {
            case 0:
                rangeLeft = value;
                break;
            case 1:
                rangeRight = value;
                break;
            case 2:
                rangeDown = value;
                break;
            default:
                rangeUp = value;
                break;
        }
        UpdateRange();
    }

    private void UpdateRange()
    {
        NormalizeRanges();
        var maxRange = Math.Max(Math.Max(rangeLeft, rangeRight), Math.Max(rangeDown, rangeUp));
        solidTransferArm.pickupRange = maxRange;
        Traverse.Create(solidTransferArm).Field<ChoreConsumer>("choreConsumer").Value.SetReach(maxRange);
        rangeVisualizer.RangeMin.x = -rangeLeft;
        rangeVisualizer.RangeMin.y = -rangeDown;
        rangeVisualizer.RangeMax.x = rangeRight;
        rangeVisualizer.RangeMax.y = rangeUp;
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
    [Serialize] public int rangeLeft;
    [Serialize] public int rangeRight;
    [Serialize] public int rangeDown;
    [Serialize] public int rangeUp;
    [Serialize] public bool isCrossWall;
    [Serialize] public bool ignoreNoPickZone;

    [MyCmpReq] public SolidTransferArm solidTransferArm;
    [MyCmpReq] public RangeVisualizer rangeVisualizer;
    [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;

    private class DirectionSlider : ISliderControl
    {
        private readonly SolidTransferArmControl parent;
        private readonly int direction;

        public DirectionSlider(SolidTransferArmControl parent, int direction)
        {
            this.parent = parent;
            this.direction = direction;
        }

        public string SliderTitleKey => direction switch
        {
            0 => "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.LEFTRANGETITLE",
            1 => "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.RIGHTRANGETITLE",
            2 => "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.DOWNRANGETITLE",
            _ => "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.UPRANGETITLE"
        };

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
            return parent.GetRange(direction);
        }

        public void SetSliderValue(float percent, int index)
        {
            parent.SetRange(direction, Convert.ToInt32(percent));
        }

        public string GetSliderTooltipKey(int index)
        {
            return direction switch
            {
                0 => "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.LEFTRANGETOOLTIP",
                1 => "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.RIGHTRANGETOOLTIP",
                2 => "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.DOWNRANGETOOLTIP",
                _ => "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.UPRANGETOOLTIP"
            };
        }

        public string GetSliderTooltip(int index)
        {
            return Strings.Get(GetSliderTooltipKey(index));
        }
    }
}
