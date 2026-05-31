using UniversalTWSSync.App.Infrastructure;
using UniversalTWSSync.App.Models;

namespace UniversalTWSSync.App.ViewModels
{
    public sealed class DeviceOptionViewModel : ViewModelBase
    {
        public DeviceOptionViewModel(AudioDeviceDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        public AudioDeviceDescriptor Descriptor { get; private set; }

        public string DeviceId
        {
            get { return Descriptor.DeviceId; }
        }

        public string DisplayName
        {
            get { return Descriptor.DisplayName; }
        }

        public string CleanDisplayName
        {
            get
            {
                var value = StripStateSuffix(DisplayName);
                if (string.IsNullOrWhiteSpace(value))
                {
                    return "realme Buds T300";
                }

                var normalized = value.Trim();
                var extracted = ExtractWrappedName(normalized, "Headphones");
                if (!string.IsNullOrWhiteSpace(extracted)) return extracted;
                extracted = ExtractWrappedName(normalized, "Headphone");
                if (!string.IsNullOrWhiteSpace(extracted)) return extracted;
                extracted = ExtractWrappedName(normalized, "Headset");
                if (!string.IsNullOrWhiteSpace(extracted)) return extracted;
                extracted = ExtractWrappedName(normalized, "Speaker");
                if (!string.IsNullOrWhiteSpace(extracted)) return extracted;
                extracted = ExtractWrappedName(normalized, "Speakers");
                if (!string.IsNullOrWhiteSpace(extracted)) return extracted;

                return value.Trim();
            }
        }

        public int BaseLatencyMs
        {
            get { return Descriptor.BaseLatencyMs; }
        }

        public int BatteryLevel
        {
            get { return Descriptor.BatteryLevel; }
        }

        public string Codec
        {
            get { return Descriptor.Codec; }
        }

        public int SignalStrength
        {
            get { return Descriptor.SignalStrength; }
        }

        public bool HasKnownLatency
        {
            get { return BaseLatencyMs >= 0; }
        }

        public bool HasKnownBattery
        {
            get { return BatteryLevel >= 0; }
        }

        public override string ToString()
        {
            return CleanDisplayName;
        }

        private static string StripStateSuffix(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var cleaned = value.Trim();
            cleaned = RemoveSuffix(cleaned, "(System Default)");
            cleaned = RemoveSuffix(cleaned, "(Unplugged)");
            cleaned = RemoveSuffix(cleaned, "(Disabled)");
            cleaned = RemoveSuffix(cleaned, "(NotPresent)");
            return cleaned.Trim();
        }

        private static string RemoveSuffix(string value, string suffix)
        {
            return value.EndsWith(suffix)
                ? value.Substring(0, value.Length - suffix.Length).Trim()
                : value;
        }

        private static string ExtractWrappedName(string value, string prefix)
        {
            var expectedPrefix = prefix + " (";
            if (!value.StartsWith(expectedPrefix) || !value.EndsWith(")"))
            {
                return null;
            }

            var inner = value.Substring(expectedPrefix.Length, value.Length - expectedPrefix.Length - 1).Trim();
            return string.IsNullOrWhiteSpace(inner) ? null : inner;
        }
    }
}
