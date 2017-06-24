using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLEServiceBrowser.DataModel
{
    public class BluetoothDevice
    {
        public string Name { get; set; }

        public string ID { get; set; }
    }

    public class BluetoothDeviceService
    {
        public string Name { get; set; }
    }

    public class BluetoothServiceCharateristics
    {
        public string Name { get; set; }
    }

    public class BluetoothServiceCharateristicsDescriptors
    {
        public string Property { get; set; }

        public string Value { get; set; }

        public string Descriptors { get; set; }
    }
}
