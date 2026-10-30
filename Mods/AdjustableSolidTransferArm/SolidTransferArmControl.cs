using System;
using System.Collections.Concurrent;
using HarmonyLib;
using KSerialization;
using UnityEngine;

namespace AdjustableSolidTransferArm;

public class SolidTransferArmControl : KMonoBehaviour, IMultiSliderControl, ICheckboxControl, ISidescreenButtonControl
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
               control.IsLocalOffsetInRange(targetX - originX, targetY - originY);
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
        return enableSettingsSideScreen;
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

    public string SidescreenButtonText =>
        Strings.Get(enableSettingsSideScreen
            ? "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.HIDESETTINGSBUTTON"
            : "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.SHOWSETTINGSBUTTON");

    public string SidescreenButtonTooltip =>
        Strings.Get("STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.SHOWSETTINGSBUTTONTOOLTIP");

    public string SidescreenTitle => SidescreenButtonText;

    public void SetButtonTextOverride(ButtonMenuTextOverride textOverride)
    {
    }

    bool ISidescreenButtonControl.SidescreenEnabled()
    {
        return true;
    }

    public bool SidescreenButtonInteractable()
    {
        return true;
    }

    public void OnSidescreenButtonPressed()
    {
        enableSettingsSideScreen = !enableSettingsSideScreen;
        RefreshSelected();
    }

    public int HorizontalGroupID()
    {
        return -1;
    }

    public int ButtonSideScreenSortOrder()
    {
        return 1;
    }

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        Subscribe(-905833192, OnCopySettings);
        Subscribe(493375141, OnRefreshUserMenu);
        Subscribe(-1643076535, OnRotated);
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

    private void OnRotated(object data)
    {
        UpdateRange();
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
        UpdateRangeVisualizerBounds();
        if (isCrossWall)
        {
            rangeVisualizer.BlockingCb = _ => false;
        }
        else
        {
            rangeVisualizer.BlockingCb = Grid.IsSolidCell;
        }
    }

    private bool IsLocalOffsetInRange(int worldOffsetX, int worldOffsetY)
    {
        var offset = ToLocalOffset(worldOffsetX, worldOffsetY);
        return offset.x >= -rangeLeft &&
               offset.x <= rangeRight &&
               offset.y >= -rangeDown &&
               offset.y <= rangeUp;
    }

    private Vector2I ToLocalOffset(int worldOffsetX, int worldOffsetY)
    {
        var orientation = rotatable == null ? Orientation.Neutral : rotatable.GetOrientation();
        return orientation switch
        {
            Orientation.R90 => new Vector2I(-worldOffsetY, worldOffsetX),
            Orientation.R180 => new Vector2I(-worldOffsetX, -worldOffsetY),
            Orientation.R270 => new Vector2I(worldOffsetY, -worldOffsetX),
            Orientation.FlipH => new Vector2I(-worldOffsetX, worldOffsetY),
            Orientation.FlipV => new Vector2I(worldOffsetX, -worldOffsetY),
            _ => new Vector2I(worldOffsetX, worldOffsetY)
        };
    }

    private void UpdateRangeVisualizerBounds()
    {
        var corners = new[]
        {
            ToWorldOffset(-rangeLeft, -rangeDown),
            ToWorldOffset(-rangeLeft, rangeUp),
            ToWorldOffset(rangeRight, -rangeDown),
            ToWorldOffset(rangeRight, rangeUp)
        };
        rangeVisualizer.RangeMin.x = Math.Min(Math.Min(corners[0].x, corners[1].x), Math.Min(corners[2].x, corners[3].x));
        rangeVisualizer.RangeMin.y = Math.Min(Math.Min(corners[0].y, corners[1].y), Math.Min(corners[2].y, corners[3].y));
        rangeVisualizer.RangeMax.x = Math.Max(Math.Max(corners[0].x, corners[1].x), Math.Max(corners[2].x, corners[3].x));
        rangeVisualizer.RangeMax.y = Math.Max(Math.Max(corners[0].y, corners[1].y), Math.Max(corners[2].y, corners[3].y));
    }

    private Vector2I ToWorldOffset(int localOffsetX, int localOffsetY)
    {
        var orientation = rotatable == null ? Orientation.Neutral : rotatable.GetOrientation();
        return orientation switch
        {
            Orientation.R90 => new Vector2I(localOffsetY, -localOffsetX),
            Orientation.R180 => new Vector2I(-localOffsetX, -localOffsetY),
            Orientation.R270 => new Vector2I(-localOffsetY, localOffsetX),
            Orientation.FlipH => new Vector2I(-localOffsetX, localOffsetY),
            Orientation.FlipV => new Vector2I(localOffsetX, -localOffsetY),
            _ => new Vector2I(localOffsetX, localOffsetY)
        };
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
    [Serialize] public bool enableSettingsSideScreen;

    [MyCmpReq] public SolidTransferArm solidTransferArm;
    [MyCmpReq] public RangeVisualizer rangeVisualizer;
    [MyCmpGet] public Rotatable rotatable;
    [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;

    private void RefreshSelected()
    {
        var selectable = GetComponent<KSelectable>();
        if (selectable != null && selectable.IsSelected)
        {
            SelectTool.Instance.Select(null, true);
            SelectTool.Instance.Select(selectable, true);
        }
    }

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
