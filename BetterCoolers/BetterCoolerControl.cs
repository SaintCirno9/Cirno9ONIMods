using System;
using KSerialization;
using UnityEngine;

namespace BetterCoolers
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class BetterCoolerControl : KMonoBehaviour, ISingleSliderControl, ICheckboxControl
    {
        private const float LegacyDefaultTargetTemp = 293.15f;
        private const float DefaultTargetTemp = 297.15f;
        private const int ModVersion = 2;

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

        public string GetSliderTooltip(int index)=>string.Format(Strings.Get(GetSliderTooltipKey(index)), GameUtil.GetConvertedTemperature(TargetTemp), SliderUnits);

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.CONDITIONERCONTROLUISIDESCREEN.TITLE";
        public string SliderUnits => $"  {GameUtil.GetTemperatureUnitSuffix()}";

        public string CheckboxTitleKey => SliderTitleKey;
        public string CheckboxLabel => Strings.Get("STRINGS.UI.UISIDESCREENS.CONDITIONERCONTROLUISIDESCREEN.PRODUCEHEATCHECKBOXLABEL");
        public string CheckboxTooltip => Strings.Get("STRINGS.UI.UISIDESCREENS.CONDITIONERCONTROLUISIDESCREEN.PRODUCEHEATCHECKBOXTOOLTIP");

        public bool GetCheckboxValue()
        {
            return ProduceHeat;
        }

        public void SetCheckboxValue(bool value)
        {
            ProduceHeat = value;
        }

        // TargetTemp 使用开尔文温度。
        [Serialize] public float TargetTemp { get; set; } = DefaultTargetTemp;
        [Serialize] public bool ProduceHeat { get; set; }
        [Serialize] public int oldModVersion = ModVersion;

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
            ProduceHeat = sourceControl.ProduceHeat;
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-905833192, OnCopySettingsDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (oldModVersion == 0)
            {
                if (Math.Abs(TargetTemp - LegacyDefaultTargetTemp) <= 0.01f)
                {
                    TargetTemp = DefaultTargetTemp;
                }
                else
                {
                    TargetTemp = GameUtil.GetTemperatureConvertedToKelvin(TargetTemp);
                }
            }
            oldModVersion = ModVersion;
        }


        [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;
    }
}
