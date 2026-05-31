using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using UniversalTWSSync.App.Commands;
using UniversalTWSSync.App.Infrastructure;
using UniversalTWSSync.App.Services;

namespace UniversalTWSSync.App.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        private readonly IDeviceDiscoveryService _deviceDiscoveryService;
        private readonly IAudioSyncService _audioSyncService;
        private readonly DispatcherTimer _heartbeatTimer;

        private DeviceOptionViewModel _selectedLeftDevice;
        private DeviceOptionViewModel _selectedRightDevice;
        private int _leftDelayMs;
        private bool _isAutoSyncEnabled;
        private string _connectionSummary;
        private string _connectionDetails;
        private Brush _connectionSummaryBrush;
        private string _syncStatusTitle;
        private string _syncStatusMessage;
        private Brush _syncIndicatorBrush;
        private string _selectedAudioSource;
        private string _syncQualitySummary;
        private Brush _syncQualityBrush;
        private Brush _autoSyncIndicatorBrush;
        private string _currentOutputLabel;
        private bool _isSessionConnected;
        private bool _isSyncRunning;

        public MainWindowViewModel(IDeviceDiscoveryService deviceDiscoveryService, IAudioSyncService audioSyncService)
        {
            _deviceDiscoveryService = deviceDiscoveryService;
            _audioSyncService = audioSyncService;

            AvailableDevices = new ObservableCollection<DeviceOptionViewModel>();
            ActivityFeed = new ObservableCollection<string>();
            AudioSources = new ObservableCollection<string> { "System Audio (Windows Default Loopback)" };
            QuickActions = new ObservableCollection<QuickActionViewModel>();

            LeftEarbud = new DeviceCardViewModel("Awaiting scan");
            RightEarbud = new DeviceCardViewModel("Awaiting scan");

            ScanDevicesCommand = new RelayCommand(ScanDevices);
            ConnectCommand = new RelayCommand(ConnectDevices, CanConnectDevices);
            StartSyncCommand = new RelayCommand(StartSync, CanStartSync);
            StopSyncCommand = new RelayCommand(StopSync);
            TestAudioCommand = new RelayCommand(TestAudio);
            RecalibrateCommand = new RelayCommand(Recalibrate);
            OpenAudioSettingsCommand = new RelayCommand(OpenAudioSettings);

            QuickActions.Add(new QuickActionViewModel
            {
                Title = "Test Audio",
                Subtitle = "Trigger a short sync verification cycle",
                Glyph = "T",
                Command = TestAudioCommand
            });
            QuickActions.Add(new QuickActionViewModel
            {
                Title = "Recalibrate",
                Subtitle = "Restart the live route with updated delays",
                Glyph = "O",
                Command = RecalibrateCommand
            });
            QuickActions.Add(new QuickActionViewModel
            {
                Title = "Audio Settings",
                Subtitle = "Review the active Windows audio route",
                Glyph = "=",
                Command = OpenAudioSettingsCommand
            });

            Greeting = BuildGreeting();
            SelectedAudioSource = AudioSources[0];
            LeftDelayMs = 20;
            RightDelayMs = 0;
            IsAutoSyncEnabled = true;

            SetConnectionState("Ready to scan", "Choose two Windows-visible devices to start a session.", CreateBrush(251, 191, 36));
            SetSyncStatus("Standby", "Scan for devices, connect both endpoints, then start sync.", CreateBrush(251, 191, 36));
            SetQuality("Waiting", CreateBrush(251, 191, 36));
            CurrentOutputLabel = "Source: Windows default render loopback. Outputs: the two selected render endpoints.";

            _heartbeatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(8)
            };
            _heartbeatTimer.Tick += HeartbeatTimer_Tick;

            AddActivity("Application initialized");
            ScanDevices();
        }

        public string Greeting { get; private set; }

        public ObservableCollection<DeviceOptionViewModel> AvailableDevices { get; private set; }

        public ObservableCollection<string> ActivityFeed { get; private set; }

        public ObservableCollection<string> AudioSources { get; private set; }

        public ObservableCollection<QuickActionViewModel> QuickActions { get; private set; }

        public DeviceCardViewModel LeftEarbud { get; private set; }

        public DeviceCardViewModel RightEarbud { get; private set; }

        public RelayCommand ScanDevicesCommand { get; private set; }

        public RelayCommand ConnectCommand { get; private set; }

        public RelayCommand StartSyncCommand { get; private set; }

        public RelayCommand StopSyncCommand { get; private set; }

        public RelayCommand TestAudioCommand { get; private set; }

        public RelayCommand RecalibrateCommand { get; private set; }

        public RelayCommand OpenAudioSettingsCommand { get; private set; }

        public DeviceOptionViewModel SelectedLeftDevice
        {
            get { return _selectedLeftDevice; }
            set
            {
                if (SetProperty(ref _selectedLeftDevice, value, nameof(SelectedLeftDevice)))
                {
                    LeftEarbud.ApplySelection(value, _isSessionConnected);
                    HandleSelectionChanged();
                }
            }
        }

        public DeviceOptionViewModel SelectedRightDevice
        {
            get { return _selectedRightDevice; }
            set
            {
                if (SetProperty(ref _selectedRightDevice, value, nameof(SelectedRightDevice)))
                {
                    RightEarbud.ApplySelection(value, _isSessionConnected);
                    HandleSelectionChanged();
                }
            }
        }

        public int LeftDelayMs
        {
            get { return _leftDelayMs; }
            set
            {
                if (SetProperty(ref _leftDelayMs, value, nameof(LeftDelayMs)))
                {
                    OnPropertyChanged(nameof(LeftDelayLabel));
                    UpdateSyncFromCalibration(false);
                }
            }
        }

        public int RightDelayMs { get; private set; }

        public string LeftDelayLabel
        {
            get { return LeftDelayMs + "ms"; }
        }

        public string RightDelayLabel
        {
            get { return RightDelayMs + "ms"; }
        }

        public bool IsAutoSyncEnabled
        {
            get { return _isAutoSyncEnabled; }
            set
            {
                if (SetProperty(ref _isAutoSyncEnabled, value, nameof(IsAutoSyncEnabled)))
                {
                    AutoSyncIndicatorBrush = value ? CreateBrush(74, 222, 128) : CreateBrush(251, 191, 36);
                    UpdateSyncFromCalibration(false);
                }
            }
        }

        public string ConnectionSummary
        {
            get { return _connectionSummary; }
            private set { SetProperty(ref _connectionSummary, value, nameof(ConnectionSummary)); }
        }

        public string ConnectionDetails
        {
            get { return _connectionDetails; }
            private set { SetProperty(ref _connectionDetails, value, nameof(ConnectionDetails)); }
        }

        public Brush ConnectionSummaryBrush
        {
            get { return _connectionSummaryBrush; }
            private set { SetProperty(ref _connectionSummaryBrush, value, nameof(ConnectionSummaryBrush)); }
        }

        public string SyncStatusTitle
        {
            get { return _syncStatusTitle; }
            private set { SetProperty(ref _syncStatusTitle, value, nameof(SyncStatusTitle)); }
        }

        public string SyncStatusMessage
        {
            get { return _syncStatusMessage; }
            private set { SetProperty(ref _syncStatusMessage, value, nameof(SyncStatusMessage)); }
        }

        public Brush SyncIndicatorBrush
        {
            get { return _syncIndicatorBrush; }
            private set { SetProperty(ref _syncIndicatorBrush, value, nameof(SyncIndicatorBrush)); }
        }

        public string SelectedAudioSource
        {
            get { return _selectedAudioSource; }
            set
            {
                if (SetProperty(ref _selectedAudioSource, value, nameof(SelectedAudioSource)))
                {
                    CurrentOutputLabel = "Source: " + value + ". Live audio is captured from the current Windows default output path.";
                }
            }
        }

        public string SyncQualitySummary
        {
            get { return _syncQualitySummary; }
            private set { SetProperty(ref _syncQualitySummary, value, nameof(SyncQualitySummary)); }
        }

        public Brush SyncQualityBrush
        {
            get { return _syncQualityBrush; }
            private set { SetProperty(ref _syncQualityBrush, value, nameof(SyncQualityBrush)); }
        }

        public Brush AutoSyncIndicatorBrush
        {
            get { return _autoSyncIndicatorBrush; }
            private set { SetProperty(ref _autoSyncIndicatorBrush, value, nameof(AutoSyncIndicatorBrush)); }
        }

        public string CurrentOutputLabel
        {
            get { return _currentOutputLabel; }
            private set { SetProperty(ref _currentOutputLabel, value, nameof(CurrentOutputLabel)); }
        }

        private void ScanDevices()
        {
            AvailableDevices.Clear();

            foreach (var device in _deviceDiscoveryService.ScanDevices())
            {
                AvailableDevices.Add(new DeviceOptionViewModel(device));
            }

            if (AvailableDevices.Count > 0)
            {
                SelectedLeftDevice = AvailableDevices[0];
            }

            if (AvailableDevices.Count > 1)
            {
                SelectedRightDevice = AvailableDevices[1];
            }

            _isSessionConnected = false;
            _isSyncRunning = false;
            LeftEarbud.SetConnectionState(false);
            RightEarbud.SetConnectionState(false);

            if (AvailableDevices.Count == 0)
            {
                SetConnectionState("No audio devices found", "Windows did not return any active render endpoints for this session.", CreateBrush(251, 191, 36));
                SetSyncStatus("Waiting", "Make sure your earbuds or headphones are connected and visible to Windows.", CreateBrush(251, 191, 36));
                AddActivity("Scan found no Windows audio devices");
            }
            else
            {
                SetConnectionState("Scan complete", AvailableDevices.Count + " Windows render endpoints loaded. Select one device for each side.", CreateBrush(124, 107, 255));
                SetSyncStatus("Standby", "Devices are ready for connection and calibration.", CreateBrush(251, 191, 36));
                AddActivity("Scan completed from Windows device list");
            }
            RefreshCommands();
            UpdateSyncFromCalibration(false);
        }

        private void ConnectDevices()
        {
            var result = _audioSyncService.Connect(GetLeftDescriptor(), GetRightDescriptor());

            if (!result.Success)
            {
                _isSessionConnected = false;
                SetConnectionState(result.Title, result.Details, CreateBrush(251, 191, 36));
                SetSyncStatus("Needs input", result.Details, CreateBrush(251, 191, 36));
                AddActivity(result.Title);
                RefreshCommands();
                return;
            }

            _isSessionConnected = true;
            LeftEarbud.SetConnectionState(true);
            RightEarbud.SetConnectionState(true);
            SetConnectionState(result.Title, result.Details, CreateBrush(74, 222, 128));
            SetSyncStatus("Connected", "Endpoints are open. Start sync to route live system audio.", CreateBrush(74, 222, 128));
            AddActivity("Devices connected for live routing");
            RefreshCommands();
            UpdateSyncFromCalibration(false);
        }

        private void StartSync()
        {
            if (!_isSessionConnected)
            {
                ConnectDevices();
            }

            if (!_isSessionConnected)
            {
                return;
            }

            var result = _audioSyncService.Start(GetLeftDescriptor(), GetRightDescriptor(), LeftDelayMs, RightDelayMs, IsAutoSyncEnabled);
            if (!result.Success)
            {
                SetSyncStatus("Unable to start", result.Details, CreateBrush(251, 191, 36));
                AddActivity(result.Title);
                return;
            }

            _isSyncRunning = true;
            _heartbeatTimer.Start();
            SetSyncStatus("Active", "Live system audio is being mirrored to both selected devices.", CreateBrush(74, 222, 128));
            AddActivity("Live sync started");
            ApplyQualityText(result.Quality);
            RefreshCommands();
        }

        private void StopSync()
        {
            var result = _audioSyncService.Stop();
            _isSyncRunning = false;
            _heartbeatTimer.Stop();
            SetSyncStatus("Paused", result.Details, CreateBrush(251, 191, 36));
            AddActivity("Live sync stopped");
            ApplyQualityText(result.Quality);
            RefreshCommands();
        }

        private void TestAudio()
        {
            AddActivity("Sync test requested");
            UpdateSyncFromCalibration(true);
        }

        private void Recalibrate()
        {
            if (SelectedLeftDevice == null || SelectedRightDevice == null)
            {
                AddActivity("Recalibration skipped");
                return;
            }

            var suggestedOffset = 0;
            if (SelectedLeftDevice.BaseLatencyMs >= 0 && SelectedRightDevice.BaseLatencyMs >= 0)
            {
                suggestedOffset = SelectedRightDevice.BaseLatencyMs - SelectedLeftDevice.BaseLatencyMs;
            }

            LeftDelayMs = suggestedOffset;
            UpdateSyncFromCalibration(true);
            AddActivity("Recalibration applied");
        }

        private void OpenAudioSettings()
        {
            AddActivity("Audio route details reviewed");
            CurrentOutputLabel = "Live source is the Windows default render loopback. Selected devices receive mono mirrored playback when sync is active.";
        }

        private void UpdateSyncFromCalibration(bool announce)
        {
            if (SelectedLeftDevice == null || SelectedRightDevice == null)
            {
                return;
            }

            var result = _audioSyncService.ApplyCalibration(GetLeftDescriptor(), GetRightDescriptor(), LeftDelayMs, RightDelayMs, IsAutoSyncEnabled);
            ApplyQualityText(result.Quality);

            if (announce)
            {
                SetSyncStatus(result.Title, result.Details, CreateBrush(124, 107, 255));
            }
        }

        private void HeartbeatTimer_Tick(object sender, EventArgs e)
        {
            if (!_isSyncRunning)
            {
                return;
            }

            AddActivity("Live session heartbeat");
        }

        private void HandleSelectionChanged()
        {
            _isSessionConnected = false;
            _isSyncRunning = false;
            _heartbeatTimer.Stop();
            LeftEarbud.ApplySelection(SelectedLeftDevice, false);
            RightEarbud.ApplySelection(SelectedRightDevice, false);
            SetConnectionState("Pair selected", "Press Connect or Start Sync to initialize both live endpoints.", CreateBrush(124, 107, 255));
            SetSyncStatus("Standby", "Manual delay can be tuned before the session starts.", CreateBrush(251, 191, 36));
            RefreshCommands();
            UpdateSyncFromCalibration(false);
        }

        private void SetConnectionState(string summary, string details, Brush brush)
        {
            ConnectionSummary = summary;
            ConnectionDetails = details;
            ConnectionSummaryBrush = brush;
        }

        private void SetSyncStatus(string title, string message, Brush brush)
        {
            SyncStatusTitle = title;
            SyncStatusMessage = message;
            SyncIndicatorBrush = brush;
        }

        private void SetQuality(string summary, Brush brush)
        {
            SyncQualitySummary = "Sync Quality: " + summary;
            SyncQualityBrush = brush;
        }

        private void ApplyQualityText(string quality)
        {
            if (quality == "Excellent")
            {
                SetQuality(quality, CreateBrush(74, 222, 128));
                SetConnectionState("Both devices connected", "Excellent connection", CreateBrush(74, 222, 128));
                return;
            }

            if (quality == "Good")
            {
                SetQuality(quality, CreateBrush(124, 107, 255));
                SetConnectionState("Both devices connected", "Live routing is active. Fine-tune the offset only if you still hear echo.", CreateBrush(74, 222, 128));
                return;
            }

            if (quality == "Moderate")
            {
                SetQuality(quality, CreateBrush(251, 191, 36));
                SetConnectionState("Calibration recommended", "Live playback is running, but a smaller latency mismatch is possible.", CreateBrush(251, 191, 36));
                return;
            }

            if (quality == "Needs calibration")
            {
                SetQuality(quality, CreateBrush(251, 191, 36));
                SetConnectionState("Latency mismatch detected", "Use Recalibrate or adjust the slider until echo is reduced.", CreateBrush(251, 191, 36));
                return;
            }

            if (quality == "Live")
            {
                SetQuality(quality, CreateBrush(74, 222, 128));
                SetConnectionState("Live audio routing active", "System audio is being captured and pushed to both selected devices.", CreateBrush(74, 222, 128));
                return;
            }

            if (quality == "Ready")
            {
                SetQuality(quality, CreateBrush(124, 107, 255));
                SetConnectionState("Endpoints ready", "Both selected render devices are available for live routing.", CreateBrush(74, 222, 128));
                return;
            }

            if (quality == "Idle")
            {
                SetQuality(quality, CreateBrush(251, 191, 36));
                SetConnectionState("Sync paused", "The live route is stopped. Start sync to resume playback duplication.", CreateBrush(251, 191, 36));
                return;
            }

            SetQuality(quality, CreateBrush(251, 191, 36));
        }

        private void AddActivity(string message)
        {
            var entry = DateTime.Now.ToString("HH:mm") + "  " + message;
            ActivityFeed.Insert(0, entry);

            while (ActivityFeed.Count > 5)
            {
                ActivityFeed.RemoveAt(ActivityFeed.Count - 1);
            }
        }

        private bool CanConnectDevices()
        {
            return SelectedLeftDevice != null && SelectedRightDevice != null;
        }

        private bool CanStartSync()
        {
            return SelectedLeftDevice != null && SelectedRightDevice != null;
        }

        private void RefreshCommands()
        {
            ConnectCommand.RaiseCanExecuteChanged();
            StartSyncCommand.RaiseCanExecuteChanged();
        }

        private static Brush CreateBrush(byte red, byte green, byte blue)
        {
            return new SolidColorBrush(Color.FromRgb(red, green, blue));
        }

        private static string BuildGreeting()
        {
            var hour = DateTime.Now.Hour;

            if (hour < 12)
            {
                return "Good Morning";
            }

            if (hour < 18)
            {
                return "Good Afternoon";
            }

            return "Good Evening";
        }

        private UniversalTWSSync.App.Models.AudioDeviceDescriptor GetLeftDescriptor()
        {
            return SelectedLeftDevice == null ? null : SelectedLeftDevice.Descriptor;
        }

        private UniversalTWSSync.App.Models.AudioDeviceDescriptor GetRightDescriptor()
        {
            return SelectedRightDevice == null ? null : SelectedRightDevice.Descriptor;
        }
    }
}
