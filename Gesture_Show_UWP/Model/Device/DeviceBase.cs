using BLEServiceBrowser.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLEServiceBrowser.Model.Device
{
    /*
     * Base class for all device types, any functionality which is generic 
     * across a group of devices is found in thyis class, this includes getting the currently selected service, 
     * and getting generic data from a characteristic.
     */

    public abstract class DeviceBase
    {
        // Selected device id
        public string DeviceID { get; set; }

        // Selected service 
        internal NamedGuid SelectedService { get; set; }
        // Selected index from the list of Uuid's on the main UI.
        internal int SelectedIndex;
        // Callback to return data once it has been recieved from device.
        internal Action<string> CallBack;
        // TODO: move to constants class so can be used across the application.
        internal string AuthorizeError = StringResources.AccessDenied;

        // Initialise object by setting the device id
        public void Initialise(string deviceID)
        {
            this.DeviceID = deviceID;
        }

        // Retrieves a list of supported services from a hard coded list. 
        // This is then return so ythe UI can be updated. 
        // TODO: poss move to Device Helper class?
        public async Task<List<NamedGuid>> PopulateSupportedServices()
        {
            // Initalise empty collection of named Guid's
            List<NamedGuid> services = new List<NamedGuid>();

            try
            {
                // Get current service using the current device id
                var service = await GetService();

                // Itterate through the list of availible services checking the number of 
                // possible characteristics, if there is more than 1, add it to the list to be returned. 
                for (int i = 0; i < Utils.ServiceHandler.GetTotalGuid(); i++)
                {
                    NamedGuid guid = Utils.ServiceHandler.GetGuid(i);
                    var characteristic = service.GetCharacteristics(guid.Guid);

                    if (characteristic.Count > 0)
                    {
                        services.Add(guid);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.DisplayBasicMessage(ex.Message);
            }
            // Return a list of Named Guid objects which have more then 0 characteristics
            return services;
        }

        // Return a list of Characteristics from a service, and return a list to be displayed on the UI.
        public async Task<List<GattCharacteristic>> GetCharacteristics(NamedGuid guid)
        {
            // Initalise empty collection of GattCharacteristic objects.
            List<GattCharacteristic> characteristics = new List<GattCharacteristic>();
            // Set this selected service value to the value recieved from the UI so it can be used later.
            this.SelectedService = guid;

            try
            {
                // Get current service using the current device id
                var service = await GetService();
                // Get a list of characteristics from the service and add them to the charateristics list.
                var characteristic = service.GetCharacteristics(guid.Guid);
                foreach (var item in characteristic)
                {
                    characteristics.Add(item);
                }
            }
            catch (Exception e)
            {
                MessageHelper.DisplayBasicMessage(StringResources.CharacteristicsError+ " : " + e.Message);
            }
            // return list to be displayed on the UI.
            return characteristics;
        }

        // Gets a single characteristic objec and returns it.
        public async Task<GattCharacteristic> GetCharacteristic(int selectedIndex)
        {
            // set defualt value to null, so can be tested by any calling functions
            GattCharacteristic characteristic = null;
            try
            {
                // Get current service using the current device id
                var service = await GetService();
                // get a characteristic object using the currently selected service uuid.
                // the selected index value is used by the UI, for the device index.
                characteristic = service.GetCharacteristics(this.SelectedService.Guid)[selectedIndex];
            }
            catch (Exception e)
            {
                MessageHelper.DisplayBasicMessage(StringResources.CharacteristicsError + " : " + e.Message);
            }
            return characteristic;
        }

        // Abstract function which must be overriden in any objet which is derived from this class. 
        // This allows new device classes to handle characteristics differently.
        public abstract Task HandleSelectedCharacteristic(Dictionary<string, object> parameters, Action<string> callback);

        // Get currently selected service using the device id. 
        // This is called everytime before an attempt to get data is made, due to the possiblity of the device disconnecting.
        internal async Task<GattDeviceService> GetService()
        {
            // Attempt to get the servcei using device id.
            var service = await GattDeviceService.FromIdAsync(this.DeviceID);

            if (service == null)
            {
                // Error accessing the service.
                throw new UnauthorizedAccessException();
            }
            else
            {
                // Have succesfully retrieved the service, return it.
                return service;
            }
        }

        // Generic get data function, this is not a one function fits all solution. 
        // However despite this it does cover a majority of instances.
        internal async Task<GattReadResult> GetData()
        {
            // returned as the result.
            GattReadResult readValue = null;

            try
            {
                // Call to get the current service
                var service = await GetService();
                // Get the current characteristic and attempt to read the data from the device.
                var characteristic = service.GetCharacteristics(this.SelectedService.Guid)[this.SelectedIndex];
                readValue = await characteristic.ReadValueAsync();
            }
            catch (Exception)
            {
                MessageHelper.DisplayBasicMessage(AuthorizeError);
            }
            // Return read data from the device.
            return readValue;
        }

        // Using the Callback pointer, return any data to the UI facing viewmodel to update the UI. 
        internal void ReturnResult(string result)
        {
            if (this.CallBack != null)
            {
                this.CallBack(result);
            }
        }
    }
}
