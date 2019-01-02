using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PrintTesting
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StreamSocket dataSocket = null;
        private DataWriter dataWriter = null;
        private RfcommDeviceService deviceService = null;
        private DeviceInformationCollection dataServiceCollection = null;

        public MainPage()
        {
            this.InitializeComponent();
            panelDisconnect.Visibility = Visibility.Collapsed;
            panelSelectFile.Visibility = Visibility.Collapsed;
            panelStatus.Visibility = Visibility.Collapsed;
        }

        private async void btnListPrinters_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DeviceList_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ComboboxFiles_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        public void start()
        {

        }

        private void Disconnect(string disconnectReason)
        {

        }

        public void NotifyUser(string strMessage, NotifyType type)
        {

        }

        public enum NotifyType
        {

        };

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            
        }

        private async void SendFile()
        {

        }

        private string toHex(byte[] buffer)
        {

        }

        private async void receiveStringLoop(DataReader dataReader)
        {

        }
    }
}
