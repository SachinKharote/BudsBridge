using System;
using UniversalTWSSync.App.Services;
using UniversalTWSSync.App.ViewModels;

class Program
{
    static void Main()
    {
        var vm = new MainWindowViewModel(new WindowsDeviceDiscoveryService(), new WasapiAudioSyncService());
        Console.WriteLine("Count=" + vm.AvailableDevices.Count);
        Console.WriteLine("Left=" + (vm.SelectedLeftDevice == null ? "<null>" : vm.SelectedLeftDevice.CleanDisplayName));
        Console.WriteLine("Right=" + (vm.SelectedRightDevice == null ? "<null>" : vm.SelectedRightDevice.CleanDisplayName));
        foreach (var d in vm.AvailableDevices)
        {
            Console.WriteLine(d.CleanDisplayName + " | " + d.DisplayName);
        }
    }
}
