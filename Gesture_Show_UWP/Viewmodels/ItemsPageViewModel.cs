using BLEServiceBrowser.DataModel;
using BLEServiceBrowser.DataModel.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace BLEServiceBrowser.Viewmodels
{
    public class ItemsPageViewModel : ViewModelBase
    {
        private IBluetoothService BluetoothService;

        private ObservableCollection<BluetoothDevice> deviceList;
        public ObservableCollection<BluetoothDevice> DeviceList 
        {
            get {return this.deviceList;}
            set 
            {
                this.deviceList = value;
                this.RaisePropertyChanged("DeviceList");
            }
        }

        private ObservableCollection<BluetoothDeviceService> serviceList;
        public ObservableCollection<BluetoothDeviceService> ServiceList
        {
            get { return this.serviceList; }
            set
            {
                this.serviceList = value;
                this.RaisePropertyChanged("ServiceList");
            }
        }

        private ObservableCollection<BluetoothServiceCharateristics> serviceCharateristicsList;
        public ObservableCollection<BluetoothServiceCharateristics> ServiceCharateristicsList
        {
            get { return this.serviceCharateristicsList; }
            set
            {
                this.serviceCharateristicsList = value;
                this.RaisePropertyChanged("ServiceCharateristicsList");
            }
        }

        private BluetoothServiceCharateristicsDescriptors descriptor;
        public BluetoothServiceCharateristicsDescriptors Descriptor 
        {
            get { return this.descriptor;}
            set 
            {
                this.descriptor = value;
                this.RaisePropertyChanged("Descriptor");
            }
        }

        private bool haveDescriptors;
        public bool HaveDescriptors 
        {
            get { return this.haveDescriptors;}
            set 
            {
                this.haveDescriptors = value;
                this.RaisePropertyChanged("HaveDescriptors");
            }
        }

        public ItemsPageViewModel()
        {
            this.DeviceList = new ObservableCollection<BluetoothDevice>();
            this.ServiceList = new ObservableCollection<BluetoothDeviceService>();
            this.ServiceCharateristicsList = new ObservableCollection<BluetoothServiceCharateristics>();

#if DEBUG
            this.BluetoothService = new BluetoothWrapper_Dummy();
#else
            //Connect to real bluetooth wrapper
#endif
        }

        internal void ScanForDevices()
        {
            this.IsBusy = true;
            this.BluetoothService.StartScan(this.HandleDeviceCallback);
        }

        private void HandleDeviceCallback(ObservableCollection<BluetoothDevice> devices)
        {
            this.IsBusy = false;

            if (devices != null)
            {
                this.DeviceList = devices;
            }
        }

        internal void GetSelectedDeviceServices(string id)
        {
            this.IsBusy = true;
            this.BluetoothService.GetServicesScan(id, this.HandleServiceCallback);
        }

        private void HandleServiceCallback(ObservableCollection<BluetoothDeviceService> services)
        {
            this.IsBusy = false;
            if (services != null)
            {
                this.ServiceList = services;
            }
        }

        internal void GetSelectedServicesCharacteristics(string id)
        {
            this.IsBusy = true;
            this.BluetoothService.GetCharacteristicsScan(id, this.HandleCharacteristicsCallback);
        }

        private void HandleCharacteristicsCallback(ObservableCollection<BluetoothServiceCharateristics> characteristics)
        {
            this.IsBusy = false;
            if (characteristics != null)
            {
                this.ServiceCharateristicsList = characteristics;
            }
        }

        internal void GetDescriptors(string id)
        {
            this.IsBusy = true;
            this.BluetoothService.GetDescriptorsScan(id, this.HandletDescriptorsCallback);
        }

        private void HandletDescriptorsCallback(BluetoothServiceCharateristicsDescriptors descriptor)
        {
            this.IsBusy = false;
            if (descriptor != null)
            {
                this.Descriptor = descriptor;
                this.HaveDescriptors = true;
            }
        }

        internal void Reset()
        {
            this.ServiceList.Clear();
            this.ServiceCharateristicsList.Clear();
            this.HaveDescriptors = false;
            if (this.Descriptor != null)
            {
                this.Descriptor = new BluetoothServiceCharateristicsDescriptors();
            }
        }
    }
}
