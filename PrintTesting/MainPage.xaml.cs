﻿using System;
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
        private RfcommDeviceService dataService = null;
        private DeviceInformationCollection dataServiceDeviceCollection = null;

        public MainPage()
        {
            this.InitializeComponent();
            panelDisconnect.Visibility = Visibility.Collapsed;
            panelSelectFile.Visibility = Visibility.Collapsed;
            panelStatus.Visibility = Visibility.Collapsed;
        }

        private async void btnListPrinters_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            // Disable the button while we do async operations so the user can't Run twice.
            button.IsEnabled = false;

            // Clear any previous messages
            NotifyUser("", NotifyType.StatusMessage);

            // Find all paired instances of the Rfcomm service and display them in a list
            dataServiceDeviceCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

            if(dataServiceDeviceCollection.Contains > 0)
            {
                DeviceList.Items.Clear();
                foreach (var dataServiceDevice in dataServiceDeviceCollection)
                {
                    DeviceList.Items.Add(dataServiceDevice.Name);
                }
                DeviceList.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else {
                NotifyUser("No SPP services were found.  Please pair with a device that is advertising the SPP service.", NotifyType.ErrorMessage);
            }
            button.IsEnabled = true;
        }

        private async void DeviceList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            btnListPrinters.IsEnabled = false;
            DeviceList.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            var dataServiceDevice = dataServiceDeviceCollection[DeviceList.SelectedIndex];
            dataService = await RfcommDeviceService.FromIdAsync(dataServiceDevice.Id);

            if(dataService == null)
            {
                NotifyUser("Access to the device is denied because the application was not granted access", NotifyType.StatusMessage);
                return;
            }
            txtDeviceName.Text = dataServiceDevice.Name;

            lock (this)
            {
                dataSocket = new StreamSocket();
            }

            try
            {
                await dataSocket.ConnectAsync(dataService.ConnectionHostName, dataService.ConnectionServiceName);

                dataWriter = new DataWriter(dataSocket.OutputStream);

                panel_SelectPrinter.Visibility = Visibility.Collapsed;
                panelSelectFile.Visibility = Visibility.Visible;
                panelDisconnect.Visibility = Visibility.Visible;

                DataReader dataReader = new DataReader(dataSocket.InputStream);
                receiveStringLoop(dataReader);
            }
            catch (Exception ex)
            {
                switch ((uint)ex.HResult)
                {
                    case (0x80070490): //ERROR_ELEMENT_NOT_FOUND
                        NotifyUser("Please verify that the device is using SPP.", NotifyType.ErrorMessage);
                        btnListPrinters.IsEnabled = true;
                        break;
                    case (0x80070103): //not connected, possible switched off
                        NotifyUser("Please verify that the device is switched ON.", NotifyType.ErrorMessage);
                        btnListPrinters.IsEnabled = true;
                        break;
                    default:
                        throw;
                }
            }
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect("Disconnected");
        }

        private async void ComboboxFiles_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboboxFiles.Items.Clear();
                // fill the list with the files
                var folder = await StorageFolder.GetFolderFromPathAsync(Windows.ApplicationModel.Package.Current.InstalledLocation.Path + @"\files");
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                foreach (StorageFile f in files)
                {
                    ComboboxFiles.Items.Add(f.Name);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ListFiles: " + ex.Message);
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            SendFile();
        }

        public void start()
        {
            dataSocket = null;
            dataWriter = null;
            dataService = null;
            dataServiceDeviceCollection = null;
            panelDisconnect.Visibility = Visibility.Collapsed;
            panelSelectFile.Visibility = Visibility.Collapsed;
            panelStatus.Visibility = Visibility.Collapsed;
        }

        private void Disconnect(string disconnectReason)
        {
            if(dataWriter != null)
            {
                dataWriter.DetachStream();
                dataWriter = null;
            }

            if(dataService != null)
            {
                dataService.Dispose();
                dataService = null;
            }
            lock (this)
            {
                if(dataSocket != null)
                {
                    dataSocket.Dispose();
                    dataSocket = null;
                }
            }

            NotifyUser(disconnectReason, NotifyType.StatusMessage);
            btnListPrinters.IsEnabled = true;

            panel_SelectPrinter.Visibility = Visibility.Visible;
            panelSelectFile.Visibility = Visibility.Collapsed;
            panelDisconnect.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Used  to display messages to the user
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    borderStatus.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    borderStatus.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }
            textBlockStatus.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve realestate.
            borderStatus.Visibility = (textBlockStatus.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
            if(textBlockStatus.Text != String.Empty)
            {
                borderStatus.Visibility = Visibility.Visible;
                panelStatus.Visibility = Visibility.Visible;
            }
            else
            {
                borderStatus.Visibility = Visibility.Collapsed;
                panelStatus.Visibility = Visibility.Collapsed;
            }
        }

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Disconnect("deactivated");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Disconnect("");
        }

        private async void SendFile()
        {
            try
            {
                if (ComboboxFiles.SelectedIndex == -1)
                    return;
                string filename = ComboboxFiles.SelectedItem.ToString();
                // fp3macklabel.prn
                // Open file in application package
                // needs to be marked as Content and Copy Always
                var fileToRead = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///files/" + filename, UriKind.Absolute));
                byte[] buffer = new byte[1024];
                int readcount = 0;
                using (BinaryReader fileReader = new BinaryReader(await fileToRead.OpenStreamForReadAsync()))
                {
                    int read = fileReader.Read(buffer, 0, buffer.Length);
                    while (read > 0)
                    {
                        readcount += read;
                        Stream streamWrite = dataSocket.OutputStream.AsStreamForWrite();
                        streamWrite.Write(buffer, 0, read);
                        streamWrite.Flush();
                        // the following does corrupt the byte stream!!!!!
                        //byte[] buf = new byte[read];
                        //Array.Copy(buffer, buf, read);
                        //chatWriter.WriteBytes(buf);
                        //await chatWriter.FlushAsync();
                        //fileWriter.Write(buffer, 0, read);
                        read = fileReader.Read(buffer, 0, buffer.Length);
                    }
                }
                NotifyUser("snedFile " + readcount.ToString(), NotifyType.StatusMessage);
                await dataWriter.StoreAsync();
            }
            // catch hresult = 0x8000000e
            catch (NullReferenceException ex)
            {

                NotifyUser("Error: " + ex.HResult.ToString() + " - " + ex.Message, NotifyType.StatusMessage);
            }

            catch (IOException ex)
            {
                NotifyUser("Error: " + ex.HResult.ToString() + " - " + ex.Message, NotifyType.StatusMessage);
            }

            catch(Exception ex)
            {
                // TODO: Catch disconnect - HResult = 0x80072745 - catch this (remote device disconnect) ex = {"An established connnection was aborted by teh software in your host machine. (Exception from HRESULT: 0x80072745)"}
                NotifyUser("Error: " + ex.HResult.ToString() + " - " + ex.Message, NotifyType.StatusMessage);
            }
        }

        private string toHex(byte[] buffer)
        {
            string s = "";
            foreach (byte b in buffer)
                if (b > 32)
                    s += "<" + b.ToString("x") + ">"
                else
                    s += System.Text.Encoding.UTF8.GetString(new byte[] { b });
            return s;
        }

        private async void receiveStringLoop(DataReader dataReader)
        {
            try
            {
                uint bufLen = dataReader.UnconsumedBufferLength;
                if(bufLen > 0)
                {
                    byte[] buffer = new byte[bufLen];
                    uint size = await dataReader.LoadAsync(bufLen);
                    dataReader.ReadBytes(buffer);
                    string s = toHex(buffer);
                    listboxConversation.Items.Add(s);
                    //uint size = await chatReader.LoadAsync(sizeof(uint));
                    //if(size < sizeof(uint))
                    // {
                    //      Disconnect("Remote device terminated connection");
                    //      return;
                    //}

                    //uint stringLenth = chatReader.ReadUInt32();
                    //uint actualStringLength = await chatReader.LoadAsync(stringLength);
                    //if(actualStringLength != stringLength)
                    //{
                    //      //The underlying socket was closed before we were able to read the whole data
                    //      return;
                    //}

                    //listboxConversation.Items.Add("Received: " + chatReader.ReadString(stringLength));
                    receiveStringLoop(dataReader);
                }
            }
            catch ( Exception ex)
            {
                lock (this)
                {
                    if(dataSocket == null)
                    {
                        // Do not print anything here - the user closed the socket.
                        // HResult = 0x80072745 - catch this (remote device disconnect) ex = {"An established connection was aborted by the software in your host machine.  (Exception from HRESULT: 0x80072745)"}
                    }
                    else
                    {
                        Disconnect("Read stream failed with error: " + ex.Message);
                    }
                }
            }
        }
    }
}
