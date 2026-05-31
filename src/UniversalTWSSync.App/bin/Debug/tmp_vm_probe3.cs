using System;
using UniversalTWSSync.App.Services;
using UniversalTWSSync.App.ViewModels;

class Program
{
    static void Main()
    {
        var vm = new MainWindowViewModel(new WindowsDeviceDiscoveryService(), new WasapiAudioSyncService());
        var d = vm.SelectedRightDevice;
        Console.WriteLine("Raw=" + d.DisplayName);
        Console.WriteLine("RawLen=" + d.DisplayName.Length);
        Console.WriteLine("Clean=" + d.CleanDisplayName);
        Console.WriteLine("CleanLen=" + d.CleanDisplayName.Length);
        foreach (var ch in d.DisplayName)
        {
            Console.Write(((int)ch) + " ");
        }
        Console.WriteLine();
    }
}
