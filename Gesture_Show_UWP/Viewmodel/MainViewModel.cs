using BLEServiceBrowser.Model;
using BLEServiceBrowser.Model.Device;
using BLEServiceBrowser.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.UI.Core;

namespace BLEServiceBrowser.Viewmodel
{
    // Main view model class which is databound to the UI, and handles all code behind interactions with the data classes.
    public class MainViewModel : ViewModelBase
    {
        // List of supoorted types used to populate the combo box.
        private List<NamedGuid> supportedTypes;
        public List<NamedGuid> SupportedTypes
        {
            get { return this.supportedTypes; }
            set
            {
                this.supportedTypes = value;
                this.RaisePropertyChanged("SupportedTypes");
            }
        }

        // List of found devices after the user has pressed the scan button.
        private ObservableCollection<DeviceInformation> foundDevices;
        public ObservableCollection<DeviceInformation> FoundDevices
        {
            get { return this.foundDevices; }
            set
            {
                this.foundDevices = value;
                this.RaisePropertyChanged("FoundDevices");
            }
        }

        // The device the user has selected from the list.
        private DeviceBase selectedDevice;

        // the selected device type
        private NamedGuid selectedDeviceType;

        // A device view model which holds all the device specific data as the user progresses through the application.
        private DeviceViewModel selectedDeviceViewModel;
        public DeviceViewModel SelectedDeviceViewModel
        {
            get { return this.selectedDeviceViewModel; }
            set
            {
                this.selectedDeviceViewModel = value;
                this.RaisePropertyChanged("SelectedDeviceViewModel");
            }
        }

        // Initialisation
        public MainViewModel()
        {
            this.SelectedDeviceViewModel = new DeviceViewModel();
            this.SupportedTypes = new List<NamedGuid>(DeviceHelper.GetAllSupportedDeviceTypes());
            this.FoundDevices = new ObservableCollection<DeviceInformation>();
        }

        // Checks to make sure the selected device type cannot be a null value.
        public void SetSelectedDevice(NamedGuid selected)
        {
            // if the value has a relevent value, set the selectedDeviceType. If this is not the case display an error.
            if (selected != null)
            {
                this.selectedDeviceType = selected;
            }
            else
            {
                MessageHelper.DisplayBasicMessage(StringResources.InitialisationError);
            }
        }

        // Get a list of devices which are the same type as the selected device type.
        public async void GetPossibleDevices()
        {
            this.SelectedDeviceViewModel.CleanAll();
            this.FoundDevices = new ObservableCollection<DeviceInformation>();

            if (this.selectedDeviceType == null)
                return;

            Guid id = Windows.Devices.Bluetooth.GenericAttributeProfile.GattServiceUuids.HeartRate;

            // Request the IsPaired property so we can display the paired status in the UI
            string[] requestedProperties = { "System.Devices.Aep.IsPaired" };

            //for bluetooth LE Devices
            string aqsFilter = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"";

            // Find a list of devices with the same Uuid as the selected device type, and populate a list.
            //DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(
            //    aqsFilter,
            //    requestedProperties,
            //    DeviceInformationKind.AssociationEndpoint);

            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(id));


            if (devices.Count > 0)
            {
                // If we have found some values add them to a list to be displayed on the UI.
                foreach (DeviceInformation item in devices)
                {
                    this.FoundDevices.Add(item);

                    DeviceBase device = DeviceHelper.GetDeviceObject(item, this.selectedDeviceType.Type);
                    if (device != null)
                    {
                        //Set the selectedDevice value to be equal to what was recieved from the get device object call.
                        this.selectedDevice = device;

                        this.SelectedDeviceViewModel.CleanServices();
                        this.SelectedDeviceViewModel.CleanCharacteristics();

                        // Get a list of supported servioces from the device, and update the viewmodel with the results.
                        this.SelectedDeviceViewModel.SupportedServices = await device.PopulateSupportedServices();
                        // Let the UI know that some data has changed.
                        this.RaisePropertyChanged("SelectedDeviceViewModel");

                    }
                    this.RaisePropertyChanged("FoundDevices");
                }
            }
            // If no device where found display an error message.
            else
            {
                MessageHelper.DisplayBasicMessage(StringResources.NoDevicesFound);
            }
        }

        // Get a reference to the selected device. 
        internal async void GetSelectedProfile(DeviceInformation selected)
        {
            DeviceBase device = DeviceHelper.GetDeviceObject(selected, this.selectedDeviceType.Type);
            if (device != null)
            {
                //Set the selectedDevice value to be equal to what was recieved from the get device object call.
                this.selectedDevice = device;

                this.SelectedDeviceViewModel.CleanServices();
                this.SelectedDeviceViewModel.CleanCharacteristics();

                // Get a list of supported servioces from the device, and update the viewmodel with the results.
                this.SelectedDeviceViewModel.SupportedServices = await device.PopulateSupportedServices();
                // Let the UI know that some data has changed.
                this.RaisePropertyChanged("SelectedDeviceViewModel");


            }
        }

        // Handle the event from the user selecting a service.
        internal async void HandleSelectedService(NamedGuid selected)
        {
            this.SelectedDeviceViewModel.CleanCharacteristics();

            // Get a list of supported characteristics and updat the viewmodel with the results.
            this.SelectedDeviceViewModel.SupportedCharacteristics = await this.selectedDevice.GetCharacteristics(selected);
            // Let the UI know that data has changed..
            this.RaisePropertyChanged("SelectedDeviceViewModel");
        }

        // Handle the user selecting a characteristic
        internal async void HandleSelectedCharacteristic(int index)
        {
            // Get the characteristic from the service, and populate the result in the device viewmodel.
            this.SelectedDeviceViewModel.SelectedCharacteristic = await this.selectedDevice.GetCharacteristic(index);
        }

        // Handle the read button click
        internal async void HandleReadCharateristic()
        {
            // Create new parameter dictionary and populate a function type, this is used later to determin which interaction with the device is needed.
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(StringResources.FunctionType, StringResources.ReadFunction);
            // Call the device specific HandleSelectedCharacteristic method passing through the parameter value and a function 
            // pointer which handles updating the device viewmodel value property.
            await this.selectedDevice.HandleSelectedCharacteristic(parameters, this.HandleCallbackResponse);
        }

        // Handle the write button click
        internal async void HandleWriteCharateristic(string input)
        {
            // Create new parameter dictionary and populate a function type, this is used later to determin which interaction with the device is needed.
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(StringResources.WriteValue, input);
            // Add an entry into the dictionary which contains the text value from tha UI text field.
            parameters.Add(StringResources.WriteFunction, StringResources.WriteValue);

            // Call the device specific HandleSelectedCharacteristic method passing through the parameter value and a function 
            // pointer which handles updating the device viewmodel value property.
            await this.selectedDevice.HandleSelectedCharacteristic(parameters, this.HandleCallbackResponse);
        }

        // Handle the notification toggle changed event.
        internal async void HandleNotificationCharateristic(bool input)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            // Create new parameter dictionary and populate a function type, this is used later to determin which interaction with the device is needed.
            parameters.Add(StringResources.ToggleValue, input);
            // Add an entry into the dictionary which contains a value that determins which toggle was changed.
            parameters.Add(StringResources.FunctionType, StringResources.NotificationFunction);

            // Call the device specific HandleSelectedCharacteristic method passing through the parameter value and a function 
            // pointer which handles updating the device viewmodel value property.
            await this.selectedDevice.HandleSelectedCharacteristic(parameters, this.HandleCallbackResponse);
        }

        // Handle the indication toggle changed event.
        internal async void HandleIndicateCharateristic(bool input)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            // Create new parameter dictionary and populate a function type, this is used later to determin which interaction with the device is needed.
            parameters.Add(StringResources.ToggleValue, input);
            // Add an entry into the dictionary which contains a value that determins which toggle was changed.
            parameters.Add(StringResources.FunctionType, StringResources.IndicateFunction);

            // Call the device specific HandleSelectedCharacteristic method passing through the parameter value and a function 
            // pointer which handles updating the device viewmodel value property.
            await this.selectedDevice.HandleSelectedCharacteristic(parameters, this.HandleCallbackResponse);
        }

        // Method which populates the device viewmodel's value propery with a result. A pointer to this function is passed down into the data classes so the all return back to this point.
        private async void HandleCallbackResponse(string result)
        {
            // The function is forced run on the UI thread as there is a possiblity that this function could be 
            // called on a different thread. For example when the data is updated from a notification call, or indicate.
            //await Task.Run(() =>
            //{
            //    this.SelectedDeviceViewModel.Value = result;
            //    this.RaisePropertyChanged("SelectedDeviceViewModel");
            //});

            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.SelectedDeviceViewModel.Value = result;
                this.RaisePropertyChanged("SelectedDeviceViewModel");
            });
        }

    }
}
