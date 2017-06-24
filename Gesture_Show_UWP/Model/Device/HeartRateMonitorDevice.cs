using BLEServiceBrowser.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BLEServiceBrowser.Model.Device
{
    /*
     * This class represents a heart rate monitor object. All servcie based functions are handled in the base class.
     * This class should only handle things which are specifically for a heart rate monitor.
     */
    public class HeartRateMonitorDevice : DeviceBase
    {
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
            if (service == GattCharacteristicUuids.HeartRateMeasurement)
            {
                // Passing through the toggle state from the UI. This allows the handler function to either attatch or detatch a value changed event.
                string key = StringResources.ToggleValue;
                if (parameters.ContainsKey(key))
                {
                    await HandleHeartRateMeasurment(Convert.ToBoolean(parameters[key]));
                }
            }
            else if (service == GattCharacteristicUuids.BodySensorLocation)
            {
                await HandleSensorLocationData();
            }
            else if (service == GattCharacteristicUuids.HeartRateControlPoint)
            {
                // Passing through the write value from UI.
                string key = StringResources.WriteValue;
                if (parameters.ContainsKey(key))
                {
                    await HandleSensorControlPoint(parameters[key].ToString());
                }
            }
        }

        // Write function for adding control points to a server
        private async Task HandleSensorControlPoint(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                // Get current service from base class.
                var service = await GetService();
                // Get current characteristic from current service using the selected values from the base class.
                var characteristic = service.GetCharacteristics(this.SelectedService.Guid)[this.SelectedIndex];

                //Create an instance of a data writer which will write to the relevent buffer.
                DataWriter writer = new DataWriter();
                byte[] toWrite = System.Text.Encoding.UTF8.GetBytes(data);
                writer.WriteBytes(toWrite);

                // Attempt to write the data to the device, and whist doing so get the status.
                GattCommunicationStatus status = await characteristic.WriteValueAsync(writer.DetachBuffer());

                // Displays a message box to tell user if the write operation was successful or not.
                if (status == GattCommunicationStatus.Success)
                {
                    MessageHelper.DisplayBasicMessage("Sensor control point has been written.");
                }
                else
                {
                    MessageHelper.DisplayBasicMessage("There was a problem writing the sensor control value, Please try again later.");
                }
            }
        }

        // Read opperation which rwads the sensor location from the device.
        private async Task HandleSensorLocationData()
        {
            string result = string.Empty;
            
            // use generic get data function from base class.
            GattReadResult data = await GetData();

            // Read the single byte data recieved from the device and convert to a string to show on the main UI.
            result = DataReader.FromBuffer(data.Value).ReadByte().ToString();
            this.ReturnResult(result);
        }

        // Notify operation which handles creating a value changed event.
        private async Task HandleHeartRateMeasurment(bool handleNotofication)
        {
            // Get service object from base class.
            var service = await GetService();
            // Get current characteristic from current service using the selected values from the base class.
            var characteristic = service.GetCharacteristics(this.SelectedService.Guid)[this.SelectedIndex];
            // Check to see if we are attching or detatching event.
            if (handleNotofication)
            {
                // Attach a listener and assign a pointer to a function which will handle the data as it comes into the application.
                characteristic.ValueChanged += Characteristic_ValueChanged;

                // Tell the device we want to register for the indicate updates, and return the staus of the registration.
                GattCommunicationStatus status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

                // Check to see if the registration was successful by checking the status, if not display a message telling the user. 
                if (status == GattCommunicationStatus.Unreachable)
                {
                    MessageHelper.DisplayBasicMessage("Your device is currently unavailible, please try again later.");
                }
            }
            else
            {
                // Remove the pointer to the local function to stop processing updates.
                characteristic.ValueChanged -= Characteristic_ValueChanged;
                ReturnResult("");
            }
        }

        // Handle the recieved data from the notification event.
        void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            string result = string.Empty;
            const byte HEART_RATE_VALUE_FORMAT = 0x01;

            var data = new byte[args.CharacteristicValue.Length];
            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);

            ushort heartRate;

            if (data.Length >= 2)
            {
                byte flags = data[0];
                bool isHeartRateValueSizeLong = ((flags & HEART_RATE_VALUE_FORMAT) != 0);

                int currentOffset = 1;

                if (isHeartRateValueSizeLong)
                {
                    heartRate = (ushort)((data[currentOffset + 1] << 8) + data[currentOffset]);
                    currentOffset += 2;
                }
                else
                {
                    heartRate = data[currentOffset];
                    currentOffset++;
                }

                result =string.Format("{0}", heartRate);
            }

            this.ReturnResult(result);
        }
    }
}
