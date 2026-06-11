using System;
using System.Collections.Generic;
using Harmony;
using KSerialization;
using UnityEngine;

namespace ColonyTweaks
{
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
            return 256;
        }

        public float GetSliderValue(int index)
        {
            return range;
        }

        public void SetSliderValue(float percent, int index)
        {
            range = Convert.ToInt32(percent);
            UpdateRange();
        }

        public string GetSliderTooltipKey(int index)
        {
            return "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.SLIDERTOOLTIP";
        }

        public string GetSliderTooltip()
        {
            return STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.SLIDERTOOLTIP;
        }

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.TITLE";
        public string SliderUnits => "";

        public bool GetCheckboxValue()
        {
            return isCrossWall;
        }

        public void SetCheckboxValue(bool value)
        {
            isCrossWall = value;
        }

        public string CheckboxTitleKey => "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.TITLE";
        public string CheckboxLabel => STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.CHECKBOXLABEL;
        public string CheckboxTooltip => STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.CHECKBOXTOOLTIP;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-905833192, OnCopySettings);
            Subscribe(493375141, OnRefreshUserMenu);
        }

        private void OnCopySettings(object data)
        {
            if (data is GameObject go && go.GetComponent<SolidTransferArmControl>() is { } sourceControl)
            {
                range = sourceControl.range;
                isCrossWall = sourceControl.isCrossWall;
                UpdateRange();
            }
        }

        public void OnRefreshUserMenu(object _)
        {
            var text = showRange ? "隐藏作用范围" : "显示作用范围";
            var tooltipText = showRange
                ? "隐藏清扫器作用范围"
                : "显示清扫器作用范围";
            var button = new KIconButtonMenu.ButtonInfo("action_building_disabled", text,
                () => showRange = !showRange, Action.CinemaZoomSpeedMinus, OnRefreshUserMenu, null, null,
                tooltipText);
            Game.Instance.userMenu.AddButton(gameObject, button);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            armDict.Add(solidTransferArm, this);
            visualizerDict.Add(stationaryChoreRangeVisualizer, this);
            if (range < 1)
            {
                range = solidTransferArm.pickupRange;
            }
            else
            {
                UpdateRange();
            }
        }

        protected override void OnCleanUp()
        {
            base.OnCleanUp();
            armDict.Remove(solidTransferArm);
            visualizerDict.Remove(stationaryChoreRangeVisualizer);
        }

        private void UpdateRange()
        {
            solidTransferArm.pickupRange = range;
            Traverse.Create(solidTransferArm).Field<ChoreConsumer>("choreConsumer").Value.SetReach(range);
            var rangeVisualizer = gameObject.AddOrGet<StationaryChoreRangeVisualizer>();
            rangeVisualizer.x = -range;
            rangeVisualizer.y = -range;
            rangeVisualizer.width = range * 2 + 1;
            rangeVisualizer.height = range * 2 + 1;
        }

        [Serialize] public int range;
        [Serialize] public bool isCrossWall;
        [Serialize] public bool showRange;

        [MyCmpReq] public SolidTransferArm solidTransferArm;
        [MyCmpReq] public StationaryChoreRangeVisualizer stationaryChoreRangeVisualizer;
        [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;

        public static Dictionary<SolidTransferArm, SolidTransferArmControl> armDict = new();
        public static Dictionary<StationaryChoreRangeVisualizer, SolidTransferArmControl> visualizerDict = new();
    }
}