using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BLEServiceBrowser.DataModel.Services
{
    public class BluetoothWrapper_Dummy : IBluetoothService
    {
        public async void StartScan(Action <ObservableCollection<BluetoothDevice>> callback)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (callback != null)
            {
                callback(GetDeviceList());
            }
        }

        public async void GetServicesScan(string deviceId, Action<ObservableCollection<BluetoothDeviceService>> callback)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (callback != null)
            {
                callback(GetServices());
            }
        }

        public async void GetDescriptorsScan(string characteristicsId, Action<BluetoothServiceCharateristicsDescriptors> callback)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (callback != null)
            {
                callback(GetDescriptors());
            }
        }

        private BluetoothServiceCharateristicsDescriptors GetDescriptors()
        {
            return new BluetoothServiceCharateristicsDescriptors() 
            {
                Property = "B:0 R:1 w:0 W:0 N:1 I:0 A:0 E:0",
                Value = string.Empty,
                Descriptors = string.Empty,
            };
        }

        public async void GetCharacteristicsScan(string serviceId, Action<ObservableCollection<BluetoothServiceCharateristics>> callback)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (callback != null)
            {
                callback(GetCharacteristics());
            }
        }

        private ObservableCollection<BluetoothDevice> GetDeviceList()
        {
            ObservableCollection<BluetoothDevice> deviceList = new ObservableCollection<BluetoothDevice>();

            for (int i = 0; i < 20; i++)
            {
                deviceList.Add(new BluetoothDevice() 
                {
                    Name = "Device "+i,
                    ID = i.ToString(),
                });
            }

            return deviceList;
        }

        private ObservableCollection<BluetoothDeviceService> GetServices()
        {
            ObservableCollection<BluetoothDeviceService> services = new ObservableCollection<BluetoothDeviceService>();
            for (int i = 0; i < 10; i++)
            {
                services.Add(new BluetoothDeviceService() 
                {
                    Name = "Service_"+i,
                });
            }
            return services;
        }

        private ObservableCollection<BluetoothServiceCharateristics> GetCharacteristics()
        {
            ObservableCollection<BluetoothServiceCharateristics> characteristics = new ObservableCollection<BluetoothServiceCharateristics>();
            for (int i = 0; i < 20; i++)
            {
                characteristics.Add(new BluetoothServiceCharateristics()
                {
                    Name = "Characteristic_" + i,
                });
            }
            return characteristics;
        }


    }
}
