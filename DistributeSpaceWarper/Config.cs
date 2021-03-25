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
        }
        internal static void Init(ConfigFile config)
        {
            ////////////////////
            // General Config //
            ////////////////////
            General.WarperLocalMode = config.Bind(GENERAL_SECTION, "Default warper Local Mode", ELogisticStorage.Demand,
                "Default local logistics mode of the Warpers"
            );

            General.WarperRemoteMode = config.Bind(GENERAL_SECTION, "Default warper Remote Mode", ELogisticStorage.None,
                "Default remote logistics mode of the Warpers"
            );
            General.WarperMaxValue = config.Bind(GENERAL_SECTION, "Default warper max item value", 100,
                new ConfigDescription(
                    "Default number of items set for warper slot.",
                    new AcceptableValueRange<int>(0, 10000),
                    new { }
                )
            );
        }
    }
}