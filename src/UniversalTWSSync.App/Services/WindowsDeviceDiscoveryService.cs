using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using UniversalTWSSync.App.Models;

namespace UniversalTWSSync.App.Services
{
    public sealed class WindowsDeviceDiscoveryService : IDeviceDiscoveryService
    {
        public IList<AudioDeviceDescriptor> ScanDevices()
        {
            var devices = new List<AudioDeviceDescriptor>();

            try
            {
                using (var enumerator = new MMDeviceEnumerator())
                {
                    var defaultDeviceId = string.Empty;
                    try
                    {
                        defaultDeviceId = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID;
                    }
                    catch
                    {
                    }

                    var endpoints = enumerator.EnumerateAudioEndPoints(
                        DataFlow.Render,
                        DeviceState.Active | DeviceState.Disabled | DeviceState.NotPresent | DeviceState.Unplugged);

                    foreach (var endpoint in endpoints)
                    {
                        var displayName = endpoint.FriendlyName;
                        if (endpoint.ID == defaultDeviceId)
                        {
                            displayName += " (System Default)";
                        }
                        else if (endpoint.State != DeviceState.Active)
                        {
                            displayName += " (" + endpoint.State + ")";
                        }

                        devices.Add(new AudioDeviceDescriptor
                        {
                            DeviceId = endpoint.ID,
                            DisplayName = displayName,
                            BatteryLevel = -1,
                            BaseLatencyMs = -1,
                            Codec = "Render endpoint",
                            SignalStrength = -1
                        });
                    }
                }
            }
            catch
            {
                return new List<AudioDeviceDescriptor>();
            }

            return devices
                .OrderByDescending(device => device.DisplayName.Contains("(System Default)"))
                .ThenBy(device => GetStateRank(device.DisplayName))
                .ThenByDescending(device => IsLikelyBluetooth(device.DisplayName))
                .ThenBy(device => device.DisplayName)
                .ToList();
        }

        private static int GetStateRank(string value)
        {
            var normalized = value == null ? string.Empty : value.ToLowerInvariant();

            if (!normalized.Contains("(disabled)") && !normalized.Contains("(notpresent)") && !normalized.Contains("(unplugged)"))
            {
                return 0;
            }

            if (normalized.Contains("(unplugged)"))
            {
                return 1;
            }

            if (normalized.Contains("(disabled)"))
            {
                return 2;
            }

            return 3;
        }

        private static bool IsLikelyBluetooth(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalized = value.ToLowerInvariant();
            return normalized.Contains("bluetooth")
                || normalized.Contains("a2dp")
                || normalized.Contains("buds")
                || normalized.Contains("pods")
                || normalized.Contains("head")
                || normalized.Contains("ear");
        }
    }
}
