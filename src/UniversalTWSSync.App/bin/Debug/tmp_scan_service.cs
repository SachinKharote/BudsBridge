using System;
using UniversalTWSSync.App.Services;

class Program
{
    static void Main()
    {
        var service = new WindowsDeviceDiscoveryService();
        foreach (var d in service.ScanDevices())
        {
            Console.WriteLine(d.DisplayName);
        }
    }
}
