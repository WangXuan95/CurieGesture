using System;

namespace BLEServiceBrowser.Model
{
    // Enum holding supported device types.
    public enum DeviceType
    {
        GenericAccess,
        Battery,
        HeartRate
    }

    // This allows the UI to display a name along side a Guid and a type.
    public class NamedGuid
    {
        public string Name { get; set; }

        public Guid Guid { get; set; }

        public DeviceType Type { get; set; }
    }
}
