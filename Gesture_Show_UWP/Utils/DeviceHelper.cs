using BLEServiceBrowser.Model;
using BLEServiceBrowser.Model.Device;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace BLEServiceBrowser.Utils
{
    // Static class to perform device specific functions
    public static class DeviceHelper
    {
        // Initalise a list of supported devices. This list is used to populate the combo box on the UI.
        private static List<NamedGuid> DeviceList = new List<NamedGuid>() 
        {
            new NamedGuid(){ Name = "Battery", Guid = GattServiceUuids.Battery, Type = DeviceType.Battery},
            new NamedGuid(){ Name = "Generic Access", Guid = GattServiceUuids.GenericAccess, Type = DeviceType.GenericAccess},
            new NamedGuid(){ Name = "Heart Rate Monitor", Guid = GattServiceUuids.HeartRate, Type = DeviceType.HeartRate},
        };

        // Return the whole list of supported devices.
        public static List<NamedGuid> GetAllSupportedDeviceTypes()
        {
            return DeviceList;
        }

        // Return a single device object depending on name value, using a linq statement.
        public static NamedGuid GetGuid(string name)
        {
            return DeviceList.Where(r => r.Name == name).FirstOrDefault();
        }

        // Using device type decideds which device object to initilise, this allows for dynamic object creation.
        public static DeviceBase GetDeviceObject(DeviceInformation deviceInfo, DeviceType type)
        {
            DeviceBase device = null;
            // Main switch statement to handle the creation of the device objects.
            switch (type)
            {
                case DeviceType.GenericAccess:
                    device = new GenericAccessDevice();
                    break;
                case DeviceType.HeartRate:
                    device = new HeartRateMonitorDevice();
                    break;
            }

            if (device == null)
            {
                // Display error if device does not have a value and return null.
                MessageHelper.DisplayBasicMessage(StringResources.InitialisationError);
                return device;
            }

            device.Initialise(deviceInfo.Id);

            return device;
        }
    }
}
