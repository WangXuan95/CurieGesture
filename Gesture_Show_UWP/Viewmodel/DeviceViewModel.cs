using BLEServiceBrowser.Model;
using System.Collections.Generic;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLEServiceBrowser.Viewmodel
{
    // Viewmodel class which is used to update the UI as the user progresses through the application.
    public class DeviceViewModel : ViewModelBase
    {
        // Selected device id
        public string SelectedDeviceID { get; set; }

        // this is the string value which is the resulot of a query and returned from the device.
        private string value;
        public string Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                this.RaisePropertyChanged("Value");
            }
        }

        // List of services which the selected device supports
        private List<NamedGuid> supportedServices;
        public List<NamedGuid> SupportedServices
        {
            get { return this.supportedServices; }
            set
            {
                this.supportedServices = value;
                this.RaisePropertyChanged("SupportedServices");
            }
        }

        // List of characteristics which the selected device supports
        private List<GattCharacteristic> supportedCharacteristics;
        public List<GattCharacteristic> SupportedCharacteristics
        {
            get { return this.supportedCharacteristics; }
            set
            {
                this.supportedCharacteristics = value;
                this.RaisePropertyChanged("SupportedCharacteristics");
            }
        }

        // The selected characteristic which is to be used.
        private GattCharacteristic selectedCharacteristic;
        public GattCharacteristic SelectedCharacteristic
        {
            get { return this.selectedCharacteristic; }
            set
            {
                this.selectedCharacteristic = value;
                this.RaisePropertyChanged("SelectedCharacteristic");
                this.RaisePropertyChanged("CharacteristicProperties");
                this.RaisePropertyChanged("HaveCharacteristic");
            }
        }

        // string value to represent the properties of a characteristic, this is used to determin which controls on the ui are enabled
        public string CharacteristicProperties 
        {
            get
            {
                if (this.SelectedCharacteristic != null)
                {
                    return this.SelectedCharacteristic.CharacteristicProperties.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        // Boolean value that when set, determins if the final panel on the UI is visible or not.
        public bool HaveCharacteristic
        {
            get { return this.SelectedCharacteristic != null; }
        }

        public DeviceViewModel()
        {
            this.SupportedServices = new List<NamedGuid>();
            this.supportedCharacteristics = new List<GattCharacteristic>();
        }

        public void CleanAll()
        {
            this.SelectedDeviceID = string.Empty;
            this.CleanCharacteristics();
            this.CleanServices();
        }

        public void CleanServices()
        {
            this.SupportedServices = new List<NamedGuid>(); ;
            this.RaisePropertyChanged("SupportedServices");
        }

        public void CleanCharacteristics()
        {
            this.SelectedCharacteristic = null;
            this.SupportedCharacteristics = new List<GattCharacteristic>();
            this.value = string.Empty;

            this.RaisePropertyChanged("SelectedCharacteristic");
            this.RaisePropertyChanged("SupportedCharacteristics");
            this.RaisePropertyChanged("CharacteristicProperties");
            this.RaisePropertyChanged("HaveCharacteristic");
            this.RaisePropertyChanged("Value");
        }
    }
}
