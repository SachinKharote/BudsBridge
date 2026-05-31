using System;
using NAudio.CoreAudioApi;
class Program
{
    static void Main()
    {
        var e = new MMDeviceEnumerator();
        var d = e.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        Console.WriteLine(d.FriendlyName + " | " + d.ID);
    }
}
