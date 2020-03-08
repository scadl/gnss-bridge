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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace gnss_bridge
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        Geolocator myGeolocator;

        public MainPage()
        {
            this.InitializeComponent();

            myGeolocator = new Geolocator() { DesiredAccuracy = PositionAccuracy.Default };
            var geoposAsync = myGeolocator.GetGeopositionAsync();
            geoposAsync.Completed = GetGeoPosCompleted;

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
