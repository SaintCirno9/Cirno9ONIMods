using static STRINGS.UI;

namespace SuperConditioner.STRINGS
{
    public class UI
    {
        public class UISIDESCREENS
        {
            public class CONDITIONERCONTROLUISIDESCREEN
            {
                public static LocString TITLE = "温度设置";

                public static LocString TOOLTIP = "这台设备将把流经的流体温度设置为" + FormatAsKeyWord("{0}{1}") + ".";
            }
            
            public class HEATERCONTROLUISIDESCREEN
            {
                public static LocString TITLE = "温度调节";
            }
        }
    }
}