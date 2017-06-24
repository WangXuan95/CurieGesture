using BLEServiceBrowser.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BLEServiceBrowser.Model.Device
{
    /*
     * This class represents a generic device, and any functionality which is needed to get or handle data from a device.
     */

    public class GenericAccessDevice : DeviceBase
    {
        /*
        * HandleSelectedCharacteristic is overriden from the base class and is used for deciding what characteristic
        * is currenlty being selected and how the application should interact with the device.
        */
        public override async Task HandleSelectedCharacteristic(Dictionary<string, object> parameters, Action<string> callback)
        {
            // Set a callback so once the data has been recieved from the device, 
            // we have a pointer to the function which will update the UI.
            if (callback != null)
            {
                this.CallBack = callback;
            }

            // Get refrence to the currently selected service, as this saves dipping into the base class and retriveing it.
            var service = this.SelectedService.Guid;

            // Main check to see what characteristic is to be called and how to handle the call.
            if (service == CustomUuidClass.DeviceName)
            {
                await HandleReadDeviceName();
            }
            else if (service == CustomUuidClass.Appearance)
            {
                await HandleReadAppearance();
            }
            else if (service == CustomUuidClass.PrivacyFlag)
            {
                await HandleReadPrivacyFlag();
            }
            else if (service == CustomUuidClass.ReconnectionAddress)
            {
                // Some charcteristics have a write feature, and for this we need to send data from the UI to the device. 
                // This value is passed through using the parameter dictionary, and all write parameters all use the same string as a key value.
                string key = StringResources.WriteValue;

                if (parameters.ContainsKey(key))
                {
                    // Casting the object to a string so it can be passed to the device.
                    this.HandleReconnectionAddress(parameters[key].ToString());
                }
            }
            else if (service == CustomUuidClass.PeripheralPreferredConnectionParameters)
            {
                await HandleReadPeripheralPreferredConnectionParameters();
            }
        }

        // String value to be sent back to the UI layer to display to the user.
        private void HandleReconnectionAddress(string writeValue)
        {
            // Handle Reconnection Address is a write function, however generic devices do not support write and are read only.
            // Therefoe just display erro to the user.
            ReturnResult(StringResources.NotSupported);
        }

        // Queries device for the selected device name.
        private async Task HandleReadDeviceName()
        {
            // Get Data from base class.
            GattReadResult data = await GetData();

            // Read trhe buffer as a string value, from the first element of the buffer through to the last.
            string result = DataReader.FromBuffer(data.Value).ReadString(data.Value.Length);

            // Return string value to the main UI using callback from base class.
            this.ReturnResult(result);
        }

        // Handle reading a boolean value.
        private async Task HandleReadPrivacyFlag()
        {
            // Get data using generic method in base class.
            GattReadResult data = await GetData();
            // Pass returned value through a datareader, then converting to a string.
            string result = DataReader.FromBuffer(data.Value).ReadBoolean().ToString();

            // Return string value to the main UI using callback from base class.
            this.ReturnResult(result);
        }

        // Handle reading a multi value buffer.
        private async Task HandleReadPeripheralPreferredConnectionParameters()
        {
            // Get data using generic method in base class.
            GattReadResult data = await GetData();
            // Pass thorugh a datareader.
            var reader = DataReader.FromBuffer(data.Value);

            // create 4 values by reading a sing UInt16 at a time.
            UInt16 value_1 = reader.ReadUInt16();
            UInt16 value_2 = reader.ReadUInt16();
            UInt16 value_3 = reader.ReadUInt16();
            UInt16 value_4 = reader.ReadUInt16();

            // Create a comma seperated string from the values.
            string result = string.Format("{0}, {1}, {2}, {3}", value_1, value_2, value_3, value_4);

            // Return string value to the main UI using callback from base class.
            this.ReturnResult(result);
        }

        // Handle reading a string value from the device.
        private async Task HandleReadAppearance()
        {
            // Get data using generic method in base class.
            GattReadResult data = await GetData();
            // Read trhe buffer as a string value, from the first element of the buffer through to the last.
            string result = DataReader.FromBuffer(data.Value).ReadInt16().ToString();

            // Return string value to the main UI using callback from base class.
            this.ReturnResult(result);
        }
    }
}
