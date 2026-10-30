using STRINGS;

namespace PassiveLiquidVentInput
{
    public class STRINGS
    {
        public class BUILDINGS
        {
            public class PREFABS
            {
                public class PASSIVELIQUIDVENTINPUT
                {
                    public static LocString NAME = UI.FormatAsLink("Liquid Vent Input", "PASSIVELIQUIDVENTINPUT");

                    public static LocString DESC = "Passively takes liquid from the environment under a high pressure.";

                    public static LocString EFFECT =
                        "Pumps <link=\"ELEMENTS_LIQUID\">liquid</link> into <link=\"LIQUIDPIPING\">pipes</link> using high external pressure. Use to pump out excess liquid.";

                    public class DESCRIPTORS
                    {
                        public static LocString REQ_PRESSURE_DESC = "Pressure over {0}";

                        public static LocString REQ_PRESSURE_TOOLTIP =
                            "Building starts working when <style=\"KKeyword\">pressure</style> is greater than {0}";

                        public static LocString DEP_PRESSURE_DESC = "Productivity depends on pressure";

                        public static LocString DEP_PRESSURE_TOOLTIP =
                            "Productivity of the building increases by {0} every atm kg";
                    }
                }
            }
        }
    }
}