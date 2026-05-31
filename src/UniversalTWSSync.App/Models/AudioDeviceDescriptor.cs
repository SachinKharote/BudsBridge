namespace UniversalTWSSync.App.Models
{
    public sealed class AudioDeviceDescriptor
    {
        public string DeviceId { get; set; }

        public string DisplayName { get; set; }

        public int BatteryLevel { get; set; }

        public int BaseLatencyMs { get; set; }

        public string Codec { get; set; }

        public int SignalStrength { get; set; }
    }
}
