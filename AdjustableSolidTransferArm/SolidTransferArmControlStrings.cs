namespace AdjustableSolidTransferArm;

public static class SolidTransferArmControlStrings
{
    public static class UI
    {
        public static class UISIDESCREENS
        {
            public static class SOLIDTRANSFERARMCONTROLUISIDESCREEN
            {
                // public static LocString TITLE = "自动清扫器设置";
                //
                // public static LocString SLIDERTOOLTIP = "调节吸收范围";
                // public static LocString CHECKBOXLABEL = "穿墙";
                // public static LocString CHECKBOXTOOLTIP = "勾选以使清扫器可穿墙清扫";
                
                // To English
                public static LocString TITLE = "Auto-Sweeper Settings";
                
                public static LocString SLIDERTOOLTIP = "Adjust the sweep range";
                public static LocString XRANGETITLE = "X Range";
                public static LocString XRANGETOOLTIP = "Adjust the horizontal sweep range";
                public static LocString YRANGETITLE = "Y Range";
                public static LocString YRANGETOOLTIP = "Adjust the vertical sweep range";
                public static LocString LEFTRANGETITLE = "Left Range";
                public static LocString LEFTRANGETOOLTIP = "Adjust the sweep range to the left";
                public static LocString RIGHTRANGETITLE = "Right Range";
                public static LocString RIGHTRANGETOOLTIP = "Adjust the sweep range to the right";
                public static LocString DOWNRANGETITLE = "Down Range";
                public static LocString DOWNRANGETOOLTIP = "Adjust the sweep range downward";
                public static LocString UPRANGETITLE = "Up Range";
                public static LocString UPRANGETOOLTIP = "Adjust the sweep range upward";
                
                public static LocString CROSSWALLCHECKBOXLABEL = "Cross Wall";
                public static LocString CROSSWALLCHECKBOXTOOLTIP = "Check to make the sweeper sweep through the wall";
                
                public static LocString HIDERANGECHECKBOXLABEL = "Hide Range Visualizer";
                public static LocString HIDERANGECHECKBOXTOOLTIP = "Check to hide the range visualizer";

                public static LocString IGNORENOPICKZONEBUTTON = "Ignore No Pick Zone";
                public static LocString IGNORENOPICKZONEBUTTONTOOLTIP = "Make this auto-sweeper ignore no-pick zones.";
                public static LocString USENOPICKZONEBUTTON = "Use No Pick Zone";
                public static LocString USENOPICKZONEBUTTONTOOLTIP = "Make this auto-sweeper obey no-pick zones.";
            }
        }

        public static class TOOLS
        {
            public static class NOPICKZONE
            {
                public static LocString NAME = "No Pick Zone";
                public static LocString TOOLTIP = "Mark cells where auto-sweepers will not pick up items. Hold Ctrl while dragging to clear marked cells.";
            }
        }

        public static class OVERLAYS
        {
            public static class NOPICKZONE
            {
                public static LocString NAME = "No Pick Zone";
                public static LocString MARKED = "No Pick Zone";
                public static LocString TOOLTIP = "Auto-sweepers will not pick up items from marked cells.";
            }
        }
    }
}
