using HarmonyLib;
using KSerialization;
using UnityEngine;

namespace AdjustableMotionSensor
{
    public class LogicDuplicantSensorControl : KMonoBehaviour, IIntSliderControl
    {
        public int SliderDecimalPlaces(int index) => 0;

        public float GetSliderMin(int index) => 1;

        public float GetSliderMax(int index) => 16;

        public float GetSliderValue(int index) => sensor.pickupRange;

        public void SetSliderValue(float percent, int index)
        {
            var newRange = (int)percent;
            sensor.pickupRange = newRange;
            var pickupableExtents = Traverse.Create(sensor).Field<Extents>("pickupableExtents").Value;
            var xy = Grid.CellToXY(sensor.NaturalBuildingCell());
            if (pickupableExtents.x < xy.x)
            {
                pickupableExtents.x = xy.x - newRange / 2;
            }
            else if (pickupableExtents.x > xy.x)
            {
                pickupableExtents.x = xy.x + newRange / 2;
            }

            if (pickupableExtents.y < xy.y)
            {
                pickupableExtents.y = xy.y - newRange / 2;
            }
            else if (pickupableExtents.y > xy.y)
            {
                pickupableExtents.y = xy.y + newRange / 2;
            }

            pickupableExtents.width = newRange + 1;
            pickupableExtents.height = newRange + 1;
            AccessTools.Field(typeof(LogicDuplicantSensor), "pickupableExtents").SetValue(sensor, pickupableExtents);
            rangeVisualizer.x = -newRange / 2;
            rangeVisualizer.y = 0;
            rangeVisualizer.height = newRange + 1;
            rangeVisualizer.width = newRange + 1;
            UpdateVisualizers();
            sensor.Sim200ms(0f);
            sensor.Sim1000ms(0f);
        }

        public string GetSliderTooltipKey(int index)
        {
            return "STRINGS.UI.UISIDESCREENS.LOGICDUPLICANTSENSORCONTROLSIDESCREEN.SLIDERTOOLTIP";
        }

        public string GetSliderTooltip() => string.Format(Strings.Get(GetSliderTooltipKey(0)), sensor.pickupRange);

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.LOGICDUPLICANTSENSORCONTROLSIDESCREEN.TITLE";
        public string SliderUnits => "tiles";

        private void UpdateVisualizers()
        {
            AccessTools.Method(typeof(StationaryChoreRangeVisualizer), "UpdateVisualizers")
                .Invoke(rangeVisualizer, null);
        }

        [MyCmpReq] public LogicDuplicantSensor sensor;
        [MyCmpReq] public StationaryChoreRangeVisualizer rangeVisualizer;
    }
}