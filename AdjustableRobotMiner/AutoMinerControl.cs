using System;
using HarmonyLib;
using UnityEngine;

namespace AdjustableRobotMiner
{
    public class AutoMinerControl : KMonoBehaviour, IDualSliderControl
    {
        public int SliderDecimalPlaces(int index) => 0;

        public float GetSliderMin(int index) => 1;

        public float GetSliderMax(int index) => 64;

        public float GetSliderValue(int index)
        {
            switch (index)
            {
                case 0:
                    return autoMiner.height;
                case 1:
                    return autoMiner.width;
                default:
                    return autoMiner.height;
            }
        }

        public void SetSliderValue(float percent, int index)
        {
            switch (index)
            {
                case 0:
                    autoMiner.height = Convert.ToInt32(percent);
                    break;
                case 1:
                    autoMiner.width = Convert.ToInt32(percent);
                    break;
            }

            UpdateVisualizers();
        }

        public string GetSliderTooltipKey(int index) =>
            "STRINGS.UI.UISIDESCREENS.AUTOMINERCONTROLSIDESCREEN.SLIDERTOOLTIP";


        public string GetSliderTooltip() => Strings.Get(GetSliderTooltipKey(0));

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.AUTOMINERCONTROLSIDESCREEN.TITLE";
        public string SliderUnits => "tiles";
        
        [MyCmpReq] public AutoMiner autoMiner;
        [MyCmpReq] public StationaryChoreRangeVisualizer rangeVisualizer;
        [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.CopySettings, OnCopySettings);
        }

        private void OnCopySettings(object data)
        {
            var component = ((GameObject)data).GetComponent<AutoMinerControl>();
            if (component == null) return;
            autoMiner.height = component.autoMiner.height;
            autoMiner.width = component.autoMiner.width;
        }

        private void UpdateVisualizers()
        {
            rangeVisualizer.width = autoMiner.width;
            rangeVisualizer.height = autoMiner.height;
            AccessTools.Method(typeof(StationaryChoreRangeVisualizer), "UpdateVisualizers")
                .Invoke(rangeVisualizer, null);
        }
    }
}