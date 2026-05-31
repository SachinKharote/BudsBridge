using System;
using UniversalTWSSync.App.Models;
using UniversalTWSSync.App.ViewModels;

class Program
{
    static void Main()
    {
        var vm = new DeviceOptionViewModel(new AudioDeviceDescriptor { DisplayName = "Speaker (Realtek(R) Audio)" });
        Console.WriteLine("Display=" + vm.DisplayName);
        Console.WriteLine("Clean=" + vm.CleanDisplayName);
        Console.WriteLine("Len=" + vm.CleanDisplayName.Length);
        foreach (var ch in vm.CleanDisplayName)
        {
            Console.Write(((int)ch) + " ");
        }
        Console.WriteLine();
    }
}
