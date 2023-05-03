using KSerialization;
using UnityEngine;

namespace BetterCoolers
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class BetterCoolerControl : KMonoBehaviour, ISingleSliderControl
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
            return string.Format(Strings.Get(GetSliderTooltipKey(0)), TargetTemp, SliderUnits);
        }

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.CONDITIONERCONTROLUISIDESCREEN.TITLE";
        public string SliderUnits => $"  {GameUtil.GetTemperatureUnitSuffix()}";

        [Serialize] public float TargetTemp { get; set; } = 20f;

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

        [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;
    }
}