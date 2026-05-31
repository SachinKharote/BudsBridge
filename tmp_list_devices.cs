using System;
using NAudio.CoreAudioApi;

class Program
{
    static void Main()
    {
        var e = new MMDeviceEnumerator();
        foreach (MMDevice d in e.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active | DeviceState.Disabled | DeviceState.NotPresent | DeviceState.Unplugged))
        {
            Console.WriteLine(d.FriendlyName + " | " + d.State + " | " + d.ID);
        }
    }
}
