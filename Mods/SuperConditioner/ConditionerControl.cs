using System;
using KSerialization;
using SuperConditioner.STRINGS;
using UnityEngine;
using UnityEngine.Serialization;

namespace SuperConditioner
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ConditionerControl : KMonoBehaviour, ISingleSliderControl, ICheckboxControl
    {
        public int SliderDecimalPlaces(int index)
        {
            return 5;
        }

        public float GetSliderMin(int index)
        {
            return -273f;
        }

        public float GetSliderMax(int index)
        {
            return 9999f;
        }

        public float GetSliderValue(int index)
        {
            return TargetTemp;
        }

        public void SetSliderValue(float percent, int index)
        {
            TargetTemp = percent;
        }

        public string GetSliderTooltipKey(int index)
        {
            return "STRINGS.UI.UISIDESCREENS.CONDITIONERCONTROLUISIDESCREEN.TOOLTIP";
        }

        public string GetSliderTooltip()
        {
            return string.Format(UI.UISIDESCREENS.CONDITIONERCONTROLUISIDESCREEN.TOOLTIP, TargetTemp, SliderUnits);
        }

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.CONDITIONERCONTROLUISIDESCREEN.TITLE";
        public string SliderUnits => $"  {GameUtil.GetTemperatureUnitSuffix()}";

        public bool GetCheckboxValue()
        {
            return AbsorbHeat;
        }

        public void SetCheckboxValue(bool value)
        {
            AbsorbHeat = value;
        }

        public string CheckboxTitleKey => "STRINGS.UI.UISIDESCREENS.CONDITIONERCONTROLUISIDESCREEN.TITLE";
        public string CheckboxLabel => "吞热";
        public string CheckboxTooltip => "勾选以吞热";

        [Serialize] public float TargetTemp { get; set; } = 20f;
        [Serialize] public bool AbsorbHeat { get; set; } = true;

        private static readonly EventSystem.IntraObjectHandler<ConditionerControl> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<ConditionerControl>(
                (control, obj) => control.OnCopySettings(obj));

        private void OnCopySettings(object data)
        {
            var sourceControl = (data as GameObject)?.GetComponent<ConditionerControl>();
            if (sourceControl is null)
            {
                return;
            }

            AbsorbHeat = sourceControl.AbsorbHeat;
            TargetTemp = sourceControl.TargetTemp;
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-905833192, OnCopySettingsDelegate);
        }

        [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;
    }
}