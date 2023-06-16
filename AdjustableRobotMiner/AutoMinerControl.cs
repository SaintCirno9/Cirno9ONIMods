using System;
using HarmonyLib;
using KSerialization;
using UnityEngine;

namespace AdjustableRobotMiner
{
    public class AutoMinerControl : KMonoBehaviour, IDualSliderControl
    {
        public int SliderDecimalPlaces(int index) => 0;

        public float GetSliderMin(int index) => 2;

        public float GetSliderMax(int index) => 64;

        public float GetSliderValue(int index)
        {
            switch (index)
            {
                case 0:
                    return minerHeight;
                case 1:
                    return minerWidth;
                default:
                    return minerHeight;
            }
        }

        public void SetSliderValue(float percent, int index)
        {
            switch (index)
            {
                case 0:
                    minerHeight = Convert.ToInt32(percent);
                    break;
                case 1:
                    minerWidth = Convert.ToInt32(percent);
                    break;
            }

            UpdateRange();
        }

        public string GetSliderTooltipKey(int index) =>
            "STRINGS.UI.UISIDESCREENS.AUTOMINERCONTROLSIDESCREEN.SLIDERTOOLTIP";


        public string GetSliderTooltip() => Strings.Get(GetSliderTooltipKey(0));

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.AUTOMINERCONTROLSIDESCREEN.TITLE";
        public string SliderUnits => "tiles";

        [MyCmpReq] public AutoMiner autoMiner;
        [MyCmpReq] public RangeVisualizer rangeVisualizer;
        [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;

        [Serialize]public int minerHeight = 9;
        [Serialize]public int minerWidth = 16;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.CopySettings, OnCopySettings);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            UpdateRange();
        }

        private void OnCopySettings(object data)
        {
            var component = ((GameObject)data).GetComponent<AutoMinerControl>();
            if (component == null) return;
            minerHeight = component.minerHeight;
            minerWidth = component.minerWidth;
            UpdateRange();
        }

        private void UpdateRange()
        {
            autoMiner.height = minerHeight;
            autoMiner.width = minerWidth;
            autoMiner.x = -autoMiner.width / 2 + 1;
            rangeVisualizer.RangeMin.x = -autoMiner.width / 2 + 1;
            rangeVisualizer.RangeMin.y = -1;
            rangeVisualizer.RangeMax.x = rangeVisualizer.RangeMin.x + autoMiner.width - 1;
            rangeVisualizer.RangeMax.y = autoMiner.height - 2;
        }
    }
}