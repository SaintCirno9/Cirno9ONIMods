using static STRINGS.UI;

namespace BetterCoolers
{
    public static class BetterCoolerControlStrings
    {
        public static class UI
        {
            public static class UISIDESCREENS
            {
                public static class CONDITIONERCONTROLUISIDESCREEN
                {
                    public static LocString TITLE = "Fluid Temperature Setting";
                    
                    public static LocString TOOLTIP = "This device will set the temperature of the fluid passing through it to " + FormatAsKeyWord("{0}{1}") + ".";

                    public static LocString PRODUCEHEATCHECKBOXLABEL = "Produce Heat";

                    public static LocString PRODUCEHEATCHECKBOXTOOLTIP = "When enabled, this device keeps the heat generated or removed by changing fluid temperature, capped to the original 14°C temperature difference.";
                }
            }
        }
    }
}
