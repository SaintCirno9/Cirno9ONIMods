namespace ZonedSolidTransferArm;

public static class ZonedSolidTransferArmStrings
{
    public static class UI
    {
        public static class UISIDESCREENS
        {
            public static class ZONEDSOLIDTRANSFERARMCONTROL
            {
                public static LocString ENABLEZONEBUTTON = "启用区域模式";
                public static LocString ENABLEZONEBUTTONTOOLTIP = "让此自动清扫器只在指定区域内拾取物品。";
                public static LocString DISABLEZONEBUTTON = "停用区域模式";
                public static LocString DISABLEZONEBUTTONTOOLTIP = "停用此自动清扫器的区域限制，恢复原本的拾取逻辑。";
                public static LocString ADDZONEBUTTON = "添加区域";
                public static LocString ADDZONEBUTTONTOOLTIP = "为此自动清扫器添加允许拾取物品的区域。";
                public static LocString REMOVEZONEBUTTON = "删除区域";
                public static LocString REMOVEZONEBUTTONTOOLTIP = "从此自动清扫器的工作区域中删除格子。";
                public static LocString ENABLEFILTERBUTTON = "启用过滤器";
                public static LocString ENABLEFILTERBUTTONTOOLTIP = "让此自动清扫器只拾取过滤器允许的物品。";
                public static LocString DISABLEFILTERBUTTON = "关闭过滤器";
                public static LocString DISABLEFILTERBUTTONTOOLTIP = "关闭此自动清扫器的物品过滤器，恢复默认拾取。";
                public static LocString ADDGLOBALZONEBUTTON = "加入全局区域";
                public static LocString ADDGLOBALZONEBUTTONTOOLTIP = "把此建筑占用的格子加入所有区域模式自动清扫器共享的工作区域。";
                public static LocString REMOVEGLOBALZONEBUTTON = "移出全局区域";
                public static LocString REMOVEGLOBALZONEBUTTONTOOLTIP = "把此建筑占用的格子从所有区域模式自动清扫器共享的工作区域中移除。";
            }
        }

        public static class TOOLS
        {
            public static class ZONE
            {
                public static LocString NAME = "清扫器区域模式";
                public static LocString TOOLTIP = "拖拽编辑当前自动清扫器的工作区域。";
            }

            public static class GLOBALZONE
            {
                public static LocString NAME = "全局清扫器区域";
                public static LocString TOOLTIP = "编辑所有区域模式自动清扫器共享的工作区域。";
                public static LocString ADDNAME = "添加全局区域";
                public static LocString ADDTOOLTIP = "向所有区域模式自动清扫器共享的工作区域添加格子。";
                public static LocString REMOVENAME = "删除全局区域";
                public static LocString REMOVETOOLTIP = "从所有区域模式自动清扫器共享的工作区域删除格子。";
            }
        }

        public static class OVERLAYS
        {
            public static class ZONE
            {
                public static LocString NAME = "清扫器区域模式";
                public static LocString MARKED = "工作区域";
                public static LocString TOOLTIP = "当前清扫器启用区域模式时只会拾取这些格子内的物品。";
            }

            public static class GLOBALZONE
            {
                public static LocString NAME = "全局清扫器区域";
                public static LocString MARKED = "全局工作区域";
                public static LocString TOOLTIP = "启用区域模式的自动清扫器都会拾取这些格子内的物品。";
            }
        }
    }
}

