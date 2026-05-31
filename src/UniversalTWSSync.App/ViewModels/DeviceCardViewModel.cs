using System.Windows.Media;
using UniversalTWSSync.App.Infrastructure;

namespace UniversalTWSSync.App.ViewModels
{
    public sealed class DeviceCardViewModel : ViewModelBase
    {
        private string _deviceName;
        private int _batteryLevel;
        private bool _isConnected;
        private int _latencyMs;
        private string _codec;
        private int _signalStrength;
        private Brush _indicatorBrush;

        public DeviceCardViewModel(string placeholderName)
        {
            DeviceName = placeholderName;
            ConnectionText = "Disconnected";
            IndicatorBrush = new SolidColorBrush(Color.FromRgb(251, 191, 36));
        }

        public string DeviceName
        {
            get { return _deviceName; }
            set { SetProperty(ref _deviceName, value, nameof(DeviceName)); }
        }

        public int BatteryLevel
        {
            get { return _batteryLevel; }
            set
            {
                if (SetProperty(ref _batteryLevel, value, nameof(BatteryLevel)))
                {
                    OnPropertyChanged(nameof(BatteryLabel));
                    OnPropertyChanged(nameof(BatteryProgressValue));
                    OnPropertyChanged(nameof(BatteryStatusText));
                    OnPropertyChanged(nameof(HasKnownBattery));
                }
            }
        }

        public string BatteryLabel
        {
            get { return BatteryLevel >= 0 ? BatteryLevel + "%" : "—"; }
        }

        public double BatteryProgressValue
        {
            get { return BatteryLevel >= 0 ? BatteryLevel : 0; }
        }

        public string BatteryStatusText
        {
            get { return BatteryLevel >= 0 ? "Battery reported by device" : "Battery not reported by Windows"; }
        }

        public bool HasKnownBattery
        {
            get { return BatteryLevel >= 0; }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value, nameof(IsConnected)); }
        }

        public string ConnectionText { get; private set; }

        public int LatencyMs
        {
            get { return _latencyMs; }
            set { SetProperty(ref _latencyMs, value, nameof(LatencyMs)); }
        }

        public string Codec
        {
            get { return _codec; }
            set { SetProperty(ref _codec, value, nameof(Codec)); }
        }

        public int SignalStrength
        {
            get { return _signalStrength; }
            set { SetProperty(ref _signalStrength, value, nameof(SignalStrength)); }
        }

        public Brush IndicatorBrush
        {
            get { return _indicatorBrush; }
            set { SetProperty(ref _indicatorBrush, value, nameof(IndicatorBrush)); }
        }

        public void ApplySelection(DeviceOptionViewModel device, bool connected)
        {
            if (device == null)
            {
                DeviceName = "No device selected";
                BatteryLevel = -1;
                LatencyMs = -1;
                Codec = "Telemetry unavailable";
                SignalStrength = -1;
                SetConnectionState(false);
                return;
            }

            DeviceName = device.CleanDisplayName;
            BatteryLevel = device.BatteryLevel;
            LatencyMs = device.BaseLatencyMs;
            Codec = device.Codec;
            SignalStrength = device.SignalStrength;
            SetConnectionState(connected);
        }

        public void SetConnectionState(bool connected)
        {
            IsConnected = connected;
            ConnectionText = connected ? "Connected" : "Disconnected";
            IndicatorBrush = connected
                ? new SolidColorBrush(Color.FromRgb(74, 222, 128))
                : new SolidColorBrush(Color.FromRgb(251, 191, 36));

            OnPropertyChanged(nameof(ConnectionText));
        }
    }
}
