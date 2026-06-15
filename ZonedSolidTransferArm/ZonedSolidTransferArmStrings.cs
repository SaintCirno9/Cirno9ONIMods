namespace ZonedSolidTransferArm;

public static class ZonedSolidTransferArmStrings
{
    public static string Text(LocString value)
    {
        return value.text;
    }

    public static class UI
    {
        public static class UISIDESCREENS
        {
            public static class ZONEDSOLIDTRANSFERARMCONTROL
            {
                public static LocString ENABLEZONEBUTTON = "Enable Zone Mode";
                public static LocString ENABLEZONEBUTTONTOOLTIP = "Let this auto sweeper only pick up items inside the assigned zone.";
                public static LocString DISABLEZONEBUTTON = "Disable Zone Mode";
                public static LocString DISABLEZONEBUTTONTOOLTIP = "Disable zone restrictions for this auto sweeper and restore the original pickup logic.";
                public static LocString ADDZONEBUTTON = "Add Zone";
                public static LocString ADDZONEBUTTONTOOLTIP = "Add a zone where this auto sweeper can pick up items.";
                public static LocString REMOVEZONEBUTTON = "Remove Zone";
                public static LocString REMOVEZONEBUTTONTOOLTIP = "Remove cells from this auto sweeper's working zone.";
                public static LocString ENABLEGLOBALZONEBUTTON = "Enable Global Zone";
                public static LocString ENABLEGLOBALZONEBUTTONTOOLTIP = "Let this auto sweeper treat the global sweeper zone as part of its working zone.";
                public static LocString DISABLEGLOBALZONEBUTTON = "Disable Global Zone";
                public static LocString DISABLEGLOBALZONEBUTTONTOOLTIP = "Let this auto sweeper use only its own zone and ignore the global sweeper zone.";
                public static LocString ENABLEFILTERBUTTON = "Enable Filter";
                public static LocString ENABLEFILTERBUTTONTOOLTIP = "Let this auto sweeper only pick up items allowed by the filter.";
                public static LocString DISABLEFILTERBUTTON = "Disable Filter";
                public static LocString DISABLEFILTERBUTTONTOOLTIP = "Disable the item filter for this auto sweeper and restore default pickup.";
                public static LocString ENABLETEMPERATUREFILTERBUTTON = "Enable Temperature Filter";
                public static LocString ENABLETEMPERATUREFILTERBUTTONTOOLTIP = "Let this auto sweeper only pick up items within the specified temperature range.";
                public static LocString DISABLETEMPERATUREFILTERBUTTON = "Disable Temperature Filter";
                public static LocString DISABLETEMPERATUREFILTERBUTTONTOOLTIP = "Disable the temperature filter for this auto sweeper and restore default pickup.";
                public static LocString TEMPERATUREFILTERTITLE = "Temperature Filter";
                public static LocString MINTEMPERATURELABEL = "Minimum Temperature";
                public static LocString MINTEMPERATURETOOLTIP = "The minimum item temperature allowed for pickup.";
                public static LocString MAXTEMPERATURELABEL = "Maximum Temperature";
                public static LocString MAXTEMPERATURETOOLTIP = "The maximum item temperature allowed for pickup.";
                public static LocString ADDGLOBALZONEBUTTON = "Add to Global Zone";
                public static LocString ADDGLOBALZONEBUTTONTOOLTIP = "Add this building's occupied cells to the shared working zone used by all zone-mode auto sweepers.";
                public static LocString REMOVEGLOBALZONEBUTTON = "Remove from Global Zone";
                public static LocString REMOVEGLOBALZONEBUTTONTOOLTIP = "Remove this building's occupied cells from the shared working zone used by all zone-mode auto sweepers.";
            }
        }

        public static class TOOLS
        {
            public static class ZONE
            {
                public static LocString NAME = "Sweeper Zone Mode";
                public static LocString TOOLTIP = "Drag to edit the working zone for the current auto sweeper.";
            }

            public static class GLOBALZONE
            {
                public static LocString NAME = "Global Sweeper Zone";
                public static LocString TOOLTIP = "Edit the shared working zone used by all zone-mode auto sweepers.";
                public static LocString ADDNAME = "Add Global Zone";
                public static LocString ADDTOOLTIP = "Add cells to the shared working zone used by all zone-mode auto sweepers.";
                public static LocString REMOVENAME = "Remove Global Zone";
                public static LocString REMOVETOOLTIP = "Remove cells from the shared working zone used by all zone-mode auto sweepers.";
                public static LocString TEMPORARYCONSTRUCTIONNAME = "Auto-add Build Zone";
                public static LocString TEMPORARYCONSTRUCTIONTOOLTIP = "Automatically add the planned building area to the global sweeper zone while a building ghost is placed.";
                public static LocString TEMPORARYCLEARNAME = "Auto-add Sweep Zone";
                public static LocString TEMPORARYCLEARTOOLTIP = "Automatically add the item's cell to the global sweeper zone when marking items for sweeping.";
            }
        }

        public static class OVERLAYS
        {
            public static class ZONE
            {
                public static LocString NAME = "Sweeper Zone Mode";
                public static LocString MARKED = "Working Zone";
                public static LocString TOOLTIP = "Auto sweepers with zone mode enabled will only pick up items in these cells.";
            }

            public static class GLOBALZONE
            {
                public static LocString NAME = "Global Sweeper Zone";
                public static LocString MARKED = "Global Working Zone";
                public static LocString TOOLTIP = "Auto sweepers with zone mode enabled will pick up items in these cells.";
            }
        }
    }
}

