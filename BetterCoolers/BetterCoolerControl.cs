using System;
using KSerialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace BetterCoolers
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class BetterCoolerControl : KMonoBehaviour, ISingleSliderControl
    {
        public int SliderDecimalPlaces(int index)
        {
            return 2;
        }

        public float GetSliderMin(int index)
        {
            return GameUtil.GetConvertedTemperature(0f);
        }

        public float GetSliderMax(int index)
        {
            return GameUtil.GetConvertedTemperature(5000f);
        }

        public float GetSliderValue(int index)
        {
            return GameUtil.GetConvertedTemperature(TargetTemp);
        }

        public void SetSliderValue(float percent, int index)
        {
            TargetTemp = GameUtil.GetTemperatureConvertedToKelvin(percent);
        }

        public string GetSliderTooltipKey(int index)
        {
            return "STRINGS.UI.UISIDESCREENS.CONDITIONERCONTROLUISIDESCREEN.TOOLTIP";
        }

        public string GetSliderTooltip()
        {
            return string.Format(Strings.Get(GetSliderTooltipKey(0)), TargetTemp, SliderUnits);
        }

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.CONDITIONERCONTROLUISIDESCREEN.TITLE";
        public string SliderUnits => $"  {GameUtil.GetTemperatureUnitSuffix()}";

        // TargetTemp is in Kelvin
        [Serialize] public float TargetTemp { get; set; } = 293.15f;
        [Serialize] public int oldModVersion;
        private readonly int modVersion = 1;

        private static readonly EventSystem.IntraObjectHandler<BetterCoolerControl> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<BetterCoolerControl>(
                (control, obj) => control.OnCopySettings(obj));

        private void OnCopySettings(object data)
        {
            var sourceControl = (data as GameObject)?.GetComponent<BetterCoolerControl>();
            if (sourceControl is null)
            {
                return;
            }

            TargetTemp = sourceControl.TargetTemp;
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-905833192, OnCopySettingsDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (oldModVersion == 0 && Math.Abs(TargetTemp - 293.15f) > 0.01f)
            {
                TargetTemp = GameUtil.GetTemperatureConvertedToKelvin(TargetTemp);
            }
            oldModVersion = modVersion;
        }


        [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;
    }
}