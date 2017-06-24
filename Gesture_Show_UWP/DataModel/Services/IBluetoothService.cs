using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BLEServiceBrowser.DataModel.Services
{
    public interface IBluetoothService
    {
        void StartScan(Action<ObservableCollection<BluetoothDevice>> callback);

        void GetServicesScan(string deviceId, Action<ObservableCollection<BluetoothDeviceService>> callback);

        void GetCharacteristicsScan(string serviceId, Action<ObservableCollection<BluetoothServiceCharateristics>> callback);

        void GetDescriptorsScan(string characteristicsId, Action<BluetoothServiceCharateristicsDescriptors> callback);
    }
}
