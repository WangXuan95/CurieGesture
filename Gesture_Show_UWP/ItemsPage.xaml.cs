using BLEServiceBrowser.DataModel;
using BLEServiceBrowser.Viewmodels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace BLEServiceBrowser
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsPage : BLEServiceBrowser.Common.LayoutAwarePage
    {
        private ItemsPageViewModel viewmodel;

        public ItemsPage()
        {
            this.InitializeComponent();
            this.viewmodel = this.DataContext as ItemsPageViewModel;
        }

        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            this.viewmodel.ScanForDevices();
        }

        private void Devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.viewmodel.Reset();

            ListView view = (ListView)sender;

            BluetoothDevice selected = view.SelectedItem as BluetoothDevice;
            if (selected != null)
            {
                viewmodel.GetSelectedDeviceServices(selected.ID);
            }
        }

        private void Services_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                viewmodel.GetSelectedServicesCharacteristics("");
            }
        }

        private void Characteristics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                viewmodel.GetDescriptors("");
            }
        }
    }
}
