using BLEServiceBrowser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLEServiceBrowser.Utils
{
    // STatic class to handle service specific functionality
    public static class ServiceHandler
    {
        /* Initialise a list of supported functionality, this is to get a roung the way Microsoft wad creadted the SDK. This is because there does not seem to be functionality to 
        * query a device for possible services, so we compare all services and then tha characteristic count
        */
        private static List<NamedGuid> ServiceCharacteristics = new List<NamedGuid>()
            {
                new NamedGuid(){Name = "BatteryLevel", Guid = GattCharacteristicUuids.BatteryLevel},
                new NamedGuid(){Name = "BloodPressureFeature", Guid = GattCharacteristicUuids.BloodPressureFeature},
                new NamedGuid(){Name = "BloodPressureMeasurement", Guid = GattCharacteristicUuids.BloodPressureMeasurement},
                new NamedGuid(){Name = "BodySensorLocation", Guid = GattCharacteristicUuids.BodySensorLocation},
                new NamedGuid(){Name = "CscFeature", Guid = GattCharacteristicUuids.CscFeature},
                new NamedGuid(){Name = "CscMeasurement", Guid = GattCharacteristicUuids.CscMeasurement},
                new NamedGuid(){Name = "GlucoseFeature", Guid = GattCharacteristicUuids.GlucoseFeature},
                new NamedGuid(){Name = "GlucoseMeasurement", Guid = GattCharacteristicUuids.GlucoseMeasurement},
                new NamedGuid(){Name = "GlucoseMeasurementContext", Guid = GattCharacteristicUuids.GlucoseMeasurementContext},
                new NamedGuid(){Name = "HeartRateControlPoint", Guid = GattCharacteristicUuids.HeartRateControlPoint},
                new NamedGuid(){Name = "101 Dectected! Press to Start", Guid = GattCharacteristicUuids.HeartRateMeasurement},
                new NamedGuid(){Name = "IntermediateCuffPressure", Guid = GattCharacteristicUuids.IntermediateCuffPressure},
                new NamedGuid(){Name = "IntermediateTemperature", Guid = GattCharacteristicUuids.IntermediateTemperature},
                new NamedGuid(){Name = "MeasurementInterval", Guid = GattCharacteristicUuids.MeasurementInterval},
                new NamedGuid(){Name = "RecordAccessControlPoint", Guid = GattCharacteristicUuids.RecordAccessControlPoint},
                new NamedGuid(){Name = "RscFeature", Guid = GattCharacteristicUuids.RscFeature},
                new NamedGuid(){Name = "RscMeasurement", Guid = GattCharacteristicUuids.RscMeasurement},
                new NamedGuid(){Name = "SCControlPoint", Guid = GattCharacteristicUuids.SCControlPoint},
                new NamedGuid(){Name = "SensorLocation", Guid = GattCharacteristicUuids.SensorLocation},
                new NamedGuid(){Name = "TemperatureMeasurement", Guid = GattCharacteristicUuids.TemperatureMeasurement},
                new NamedGuid(){Name = "TemperatureType", Guid = GattCharacteristicUuids.TemperatureType},
                new NamedGuid(){Name = "DeviceName", Guid = CustomUuidClass.DeviceName},
                new NamedGuid(){Name = "Appearance", Guid = CustomUuidClass.Appearance},
                new NamedGuid(){Name = "PeripheralPreferredConnectionParameters", Guid = CustomUuidClass.PeripheralPreferredConnectionParameters},
                new NamedGuid(){Name = "ReconnectionAddress", Guid = CustomUuidClass.ReconnectionAddress},
                new NamedGuid(){Name = "PrivacyFlag", Guid = CustomUuidClass.PrivacyFlag},
            };

        // Get the total number of supported services.
        public static int GetTotalGuid()
        {
            return ServiceCharacteristics.Count;
        }

        // Get a single service by name.
        public static NamedGuid GetGuid(string name)
        {
            return ServiceCharacteristics.Where(r => r.Name == name).FirstOrDefault();
        }

        // Return a single service by index.
        public static NamedGuid GetGuid(int index)
        {
            try
            {
                return ServiceCharacteristics[index];
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    // created a custom service class to give the aqbility of adding services which have not been added by Microsoft.
    public static class CustomUuidClass  
    {
        public static Guid DeviceName
        {
            get { return new Guid("{00002a00-0000-1000-8000-00805f9b34fb}"); }
        }

        public static Guid Appearance
        {
            get { return new Guid("{00002a01-0000-1000-8000-00805f9b34fb}"); }
        }

        public static Guid PrivacyFlag
        {
            get { return new Guid("{00002A02-0000-1000-8000-00805f9b34fb}"); }
        }

        public static Guid ReconnectionAddress
        {
            get { return new Guid("{00002A03-0000-1000-8000-00805f9b34fb}"); }
        }

        public static Guid PeripheralPreferredConnectionParameters
        {
            get { return new Guid("{00002A04-0000-1000-8000-00805f9b34fb}"); }
        }
    }
}
