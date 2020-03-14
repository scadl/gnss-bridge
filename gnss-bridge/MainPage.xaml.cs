using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.IO.Ports;
using Windows.Devices.Geolocation;
using Windows.UI.Core;

namespace gnss_bridge
{
    /// <summary>
    /// a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        Geolocator myGeolocator;
        int iterator = 0;
        uint scanInterval = 3000;
        SerialPort outCOM;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void GetCOM()
        {            
            outCOM = new SerialPort(tbPort.Text);
            outCOM.Open();
        }

        private async void getAccStatus()
        {
            var accesStatus = await Geolocator.RequestAccessAsync();

            switch (accesStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    myGeolocator = new Geolocator() { ReportInterval = scanInterval };
                    myGeolocator.StatusChanged += getGeo_StatusChange;
                    myGeolocator.PositionChanged += getGeo_PositionChange;
                    txtLineState.Text = "Waiting for update...";
                    break;
                case GeolocationAccessStatus.Denied:
                    txtLineState.Text = "Access to location is denied.";
                    bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));
                    break;
                case GeolocationAccessStatus.Unspecified:
                    txtLineState.Text = "Unspecificed error!";
                    break;
            }         
        }

        private static string getChecksum(string sentence)
        {
            //Start with first Item
            int checksum = Convert.ToByte(sentence[sentence.IndexOf('$') + 1]);
            // Loop through all chars to get a checksum
            for (int i = sentence.IndexOf('$') + 2; i < sentence.IndexOf('*'); i++)
            {
                // No. XOR the checksum with this character's value
                checksum ^= Convert.ToByte(sentence[i]);
            }
            // Return the checksum formatted as a two-character hexadecimal
            return checksum.ToString("X2");
        }

        private async void getGeo_PositionChange(Geolocator sender, PositionChangedEventArgs evtPostition)
        {
            String stateData = "";

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txtLineState.Text = "Location updated.";
                iterator += 1;                

                txtLineRes1.Text = evtPostition.Position.Coordinate.Point.Position.Latitude.ToString() + " degrees.";
                txtLineRes2.Text = evtPostition.Position.Coordinate.Point.Position.Longitude.ToString() + " degrees.";
                txtLineRes3.Text = evtPostition.Position.Coordinate.Accuracy.ToString() + " meters.";
                txtLineRes4.Text = iterator.ToString() + " times.";

                Thickness ZeroThin = new Thickness(0);
                ListViewItem Item = new ListViewItem();
                TextBlock myText = new TextBlock();
                //String stateData = String.Format("Latitude: {0} degrees. \nLongitude: {1} degrees. \nAccuracy: {2} meters.",
                //        evtPostition.Position.Coordinate.Point.Position.Latitude, evtPostition.Position.Coordinate.Point.Position.Longitude, evtPostition.Position.Coordinate.Accuracy);
                                             
                double lat = evtPostition.Position.Coordinate.Point.Position.Latitude;
                double lon = evtPostition.Position.Coordinate.Point.Position.Longitude;
                                
                string latDir = (lat >= 0 ? "N" : "S");                
                lat = Math.Abs(lat);
                var latMin = (lat % Math.Truncate(lat)) * 60;                

                string lonDir = (lon >= 0 ? "E" : "W");                
                lon = Math.Abs(lon);
                var lonMin = (lon % Math.Truncate(lon)) * 60;                

                var timeStamp = DateTime.UtcNow;
                stateData = String.Format("$GPRMC,{0},A,{2}{3},{4},{5}{6},{7},,,{1},,,",
                        timeStamp.ToString("hhmmss.sss"), timeStamp.ToString("ddMMyy"),
                        Math.Truncate(lat).ToString().PadLeft(2,'0'), latMin.ToString().PadLeft(2,'0'), latDir,
                        Math.Truncate(lon).ToString().PadLeft(3,'0'), lonMin.ToString().PadLeft(2,'0'), lonDir);

                myText.Text = stateData;
                Item.Content = myText;
                cbView.Items.Add(Item);

                // What's available in classes?
                //evtPostition.Position.Coordinate.Coordinate.Point.Position.Altitude;
                //evtPostition.Position.Coordinate.Heading;
                //evtPostition.Position.Coordinate.Speed
            });

            // Example of NMEA 0183 String:
            // $GPRMC,125504.049,A,5542.2389,N,03741.6063,E,0.06,25.82,200906,,,*17
            // https://ru.wikipedia.org/wiki/NMEA_0183#RMC-%D1%81%D1%82%D1%80%D0%BE%D0%BA%D0%B0_(%D1%87%D0%B0%D1%81%D1%82%D0%BD%D1%8B%D0%B9_%D0%BF%D1%80%D0%B8%D0%BC%D0%B5%D1%80)

            //evtPostition.Position.Coordinate.Point.AltitudeReferenceSystem = new AltitudeReferenceSystem();
                        
            outCOM.WriteLine(stateData);


        }

        private async void getGeo_StatusChange(Geolocator sender, StatusChangedEventArgs evtStatus)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (evtStatus.Status)
                {
                    case PositionStatus.Ready:
                        txtLineState.Text = "Location platform is ready.";
                        break;
                    case PositionStatus.Initializing:
                        txtLineState.Text = "Location platform is attempting to obtain a position.";
                        break;
                    case PositionStatus.NoData:
                        txtLineState.Text = "Not able to determine the location.";
                        break;
                    case PositionStatus.Disabled:
                        txtLineState.Text = "Access to location is denied.";                        
                        break;
                    case PositionStatus.NotInitialized:
                        txtLineState.Text = "No request for location is made yet.";
                        break;
                    case PositionStatus.NotAvailable:
                        txtLineState.Text = "Location is not available on this version of the OS.";
                        break;
                    default:
                        txtLineState.Text = "Unknown";
                        break;
                }
            });           
          
        }

        private void btnControl_Click(object sender, RoutedEventArgs e)
        {
            
            if (slInterval.IsEnabled)
            {
                slInterval.IsEnabled = false;
                btnControl.Content = "Stop Scan";
                getAccStatus();
                GetCOM();
            } else
            {
                outCOM.Close();
                slInterval.IsEnabled = true;
                btnControl.Content = "Run Scan";
                myGeolocator.StatusChanged -= getGeo_StatusChange;
                myGeolocator.PositionChanged -= getGeo_PositionChange;                
            }
        }

        private void slInterval_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            try { 
                lblInterval.Text = slInterval.Value.ToString() + " sec";
            } catch { }
            scanInterval = Convert.ToUInt32(slInterval.Value * 1000);
        }
    }
}
