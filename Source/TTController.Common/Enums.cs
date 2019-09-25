namespace TTController.Common
{
    public enum ColorGenerationMethod
    {
        PerPort,
        SpanPorts
    }

    public enum SensorMixFunction
    {
        Minimum,
        Maximum,
        Average
    }

    public enum LedCountHandling
    {
        DoNothing,
        Lerp,
        Nearest,
        Wrap,
        Trim,
        Copy
    }

    public enum DeviceType
    {
        Default,
        RiingTrio,
        RiingDuo,
        FloeRiing,
        PurePlus
    }

    public static class EnumExtensions
    {
        public static int GetLedCount(this DeviceType type) => type switch
        {
            DeviceType.Default => 12,
            DeviceType.RiingDuo => 18,
            DeviceType.RiingTrio => 30,
            DeviceType.FloeRiing => 6,
            DeviceType.PurePlus => 9,
            _ => 12
        };

        public static int[] GetZones(this DeviceType type) => type switch
        { 
            DeviceType.Default => new int[] { 12 },
            DeviceType.RiingDuo => new int[] { 12, 6 },
            DeviceType.RiingTrio => new int[] { 12, 12, 6 },
            DeviceType.FloeRiing => new int[] { 6 },
            DeviceType.PurePlus => new int[] { 9 },
            _ => new int[] { 12 }
        };
    }
}
