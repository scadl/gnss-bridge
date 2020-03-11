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

        public MainPage()
        {
            this.InitializeComponent();

            myGeolocator = new Geolocator() { DesiredAccuracy = PositionAccuracy.Default };
            myGeolocator.StatusChanged += GetGeoPosUpdated;

            var geoposAsync = myGeolocator.GetGeopositionAsync();
            geoposAsync.Completed = GetGeoPosCompleted;            

        }

        private async void GetGeoPosUpdated(Geolocator sender, StatusChangedEventArgs changeEvent)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (changeEvent.Status)
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
            
            //throw new NotImplementedException();
        }

        public async void showResult(String resultStr)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txtLineRes.Text = resultStr;
            });            
        }

        private void GetGeoPosCompleted(IAsyncOperation<Geoposition> asyncInfo, AsyncStatus asyncStatus)
        {
            switch(asyncStatus)
            {
                case AsyncStatus.Completed:
                    var res = asyncInfo.GetResults();
                    var pos = res.Coordinate.Point.Position;                    
                    showResult (String.Format("Latitude: {0} degrees \nLongitude: {1} degrees \nAccuracy: {2} meters", 
                        pos.Latitude, pos.Longitude, res.Coordinate.Accuracy));
                    break;
                default:
                    showResult (myGeolocator.LocationStatus.ToString());
                    break;
            }
        }

    }
}
