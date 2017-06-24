using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BLEServiceBrowser.Model.Device
{
    /*
     * This class represents a Battery monitor. 
     * The base class handles all generic service and characteristic handlers. 
     * This class should only handle Battery Monitor based functionality.
     */
    public class BatteryMonitorDevice : DeviceBase
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

            // Check to see if currently selected Guid is equal to predefined BatteryLevel Guid.
            if (this.SelectedService.Guid == GattCharacteristicUuids.BatteryLevel)
            {
                await HandleReadBatteryLevel();
            }
        }

        // Functionality to handle battery level.
        private async Task HandleReadBatteryLevel()
        {
            // Get data is in the base class and handles getting the current service, 
            // it then attemps to get data. Data is then read and returned to the callback pointer. 
            GattReadResult data = await GetData();
            string result = DataReader.FromBuffer(data.Value).ReadUInt16().ToString();

            // Return string value to the main UI using callback from base class.
            this.ReturnResult(result);
        }
    }
}
