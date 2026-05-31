using System.Collections.Generic;
using UniversalTWSSync.App.Models;

namespace UniversalTWSSync.App.Services
{
    public interface IDeviceDiscoveryService
    {
        IList<AudioDeviceDescriptor> ScanDevices();
    }
}
