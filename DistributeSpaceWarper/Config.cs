namespace DistributeSpaceWarper
{
    using BepInEx.Configuration;

    public static class Config
    {
        private static readonly string GENERAL_SECTION = "General";

        public static class General
        {
            public static ConfigEntry<ELogisticStorage> WarperLocalMode;
            public static ConfigEntry<ELogisticStorage> WarperRemoteMode;
            public static ConfigEntry<int> WarperMaxValue;
            public static ConfigEntry<bool> ShowWarperSlot;
            public static ConfigEntry<bool> WarpersRequiredToggleAutomation;
        }
        internal static void Init(ConfigFile config)
        {
            ////////////////////
            // General Config //
            ////////////////////
            
            General.ShowWarperSlot = config.Bind(GENERAL_SECTION, "ShowWarperSlot", false,
                "Should additional warper only slot be visible. " +
                "Note #1: Slot number 6+ will not take anything from incoming belts." + 
                "Note #2: Enabling this may help with mod compatibility.");
            
            General.WarperMaxValue = config.Bind(GENERAL_SECTION, "WarperMaxValue", 100,
                new ConfigDescription("Default number of items set for warper slot. Note: Should be in increments of 100. Otherwise may cause issues.",
                    new AcceptableValueRange<int>(0, 10000), new { }));
                
            General.WarperLocalMode = config.Bind(GENERAL_SECTION, "WarperLocalMode", ELogisticStorage.Demand,
                "Default local logistics mode of the Warpers"
            );

            General.WarperRemoteMode = config.Bind(GENERAL_SECTION, "WarperRemoteMode", ELogisticStorage.None,
                "Default remote logistics mode of the Warpers"
            );
            
            General.WarpersRequiredToggleAutomation =  config.Bind(GENERAL_SECTION, "WarpersRequiredToggleAutomation", true,
                "If enabled, when `Warpers Required` toggle ticked on, this will setup warper slot to default local mode." +
                "When toggle is ticked off this will set wraper slot to local supply.");
            
        }
    }
}