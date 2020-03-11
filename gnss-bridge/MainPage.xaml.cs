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

        public MainPage()
        {
            this.InitializeComponent();
            getAccStatus();
        }

        private async void getAccStatus()
        {
            var accesStatus = await Geolocator.RequestAccessAsync();

            switch (accesStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    myGeolocator = new Geolocator() { ReportInterval = 3000 };
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

        private async void getGeo_PositionChange(Geolocator sender, PositionChangedEventArgs evtPostition)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txtLineState.Text = "Location updated.";
                iterator += 1;                

                txtLineRes1.Text = evtPostition.Position.Coordinate.Latitude.ToString();
                txtLineRes2.Text = evtPostition.Position.Coordinate.Longitude.ToString();
                txtLineRes3.Text = evtPostition.Position.Coordinate.Accuracy.ToString();
                txtLineRes4.Text = iterator.ToString();                

                ListViewItem Item = new ListViewItem();
                TextBlock myText = new TextBlock();
                String stateData = String.Format("Latitude: {0} degrees. \nLongitude: {1} degrees. \nAccuracy: {2} meters.",
                        evtPostition.Position.Coordinate.Latitude, evtPostition.Position.Coordinate.Longitude, evtPostition.Position.Coordinate.Accuracy);

                myText.Text = stateData;
                Item.Content = myText;
                cbView.Items.Add(Item);
            });
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

    }
}
