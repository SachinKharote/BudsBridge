using UniversalTWSSync.App.Models;

namespace UniversalTWSSync.App.Services
{
    public interface IAudioSyncService
    {
        SyncSessionResult Connect(AudioDeviceDescriptor leftDevice, AudioDeviceDescriptor rightDevice);

        SyncSessionResult Start(AudioDeviceDescriptor leftDevice, AudioDeviceDescriptor rightDevice, int leftDelayMs, int rightDelayMs, bool autoSyncEnabled);

        SyncSessionResult Stop();

        SyncSessionResult ApplyCalibration(AudioDeviceDescriptor leftDevice, AudioDeviceDescriptor rightDevice, int leftDelayMs, int rightDelayMs, bool autoSyncEnabled);
    }
}
