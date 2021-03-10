using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Distancer.ExtraUtils;
using Xamarin.Essentials;
using static Xamarin.Essentials.Permissions;

namespace Distancer
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ILocationListener
    {
        LocationManager locMgr;
        Button butSearch;
        EditText txtCurrentLocation;
        EditText txtSearchResults;
        EditText txtSearchLocation;
        LatLngSimple coordinatesFound;

        string bestLocationProvider = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            InitLocationService();
            CheckPermissionAndStartLocationUpdates();
            SetupViews();
        }

        void InitLocationService()
        {
            locMgr = (LocationManager)GetSystemService(Context.LocationService);

            Criteria locationCriteria = new Criteria();
            locationCriteria.Accuracy = Accuracy.Coarse;
            locationCriteria.PowerRequirement = Power.Medium;

            bestLocationProvider = locMgr.GetBestProvider(locationCriteria, true);
        }

        void SetupViews()
        {
            butSearch = FindViewById<Button>(Resource.Id.buttSearch);
            txtCurrentLocation = FindViewById<EditText>(Resource.Id.txtCurrentLocation);
            txtSearchResults = FindViewById<EditText>(Resource.Id.txtLocationResults);
            txtSearchLocation = FindViewById<EditText>(Resource.Id.txtTypeLocation);

            butSearch.Click += delegate
            {
                SetButtSearchAction();
            };

            txtSearchLocation.EditorAction += TxtSearchLocation_EditorAction;
        }

        private void TxtSearchLocation_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Done && butSearch.Enabled)
            {
                SetButtSearchAction();
            }
        }

        void ShowErrorAlert(string message)
        {
            Show_Dialog msg = new Show_Dialog(this);
            msg.ShowDialog("Error", message);
        }

        async void ShowQuestionAlert()
        {
            Show_Dialog msg1 = new Show_Dialog(this);
            var result = await msg1.ShowDialog("Error", "Message", true, false, Show_Dialog.MessageResult.YES, Show_Dialog.MessageResult.NO);
            //if (await msg1.ShowDialog("Error", "Message", true, false, Show_Dialog.MessageResult.YES, Show_Dialog.MessageResult.NO) == Show_Dialog.MessageResult.YES)
            if (result == Show_Dialog.MessageResult.YES)
            {
                //do anything
            }
        }

        void SetButtSearchAction()
        {
            DismissKeyboard();
            CheckPermissionAndStartLocationUpdates();
        }

        void DismissKeyboard()
        {
            if (this.CurrentFocus != null)
            {
                InputMethodManager inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
                inputManager.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
            }
        }

        async void CheckPermissionAndStartLocationUpdates()
        {
            var status = await CheckAndRequestPermissionAsync(new Permissions.LocationWhenInUse());
            if (status != PermissionStatus.Granted)
            {
                Toast.MakeText(this, "The Network Provider does not exist or is not enabled!", ToastLength.Long).Show();

                Show_Dialog msg1 = new Show_Dialog(this);
                if (await msg1.ShowDialog("Needs Location", "You would need to activate the Location service in order for this app to get the correct distance to a location.", true, false, Show_Dialog.MessageResult.YES, Show_Dialog.MessageResult.NO) == Show_Dialog.MessageResult.YES)
                {
                    CheckPermissionAndStartLocationUpdates();
                }
            }
            else 
            {
                if (bestLocationProvider != null)
                {
                    locMgr.RequestLocationUpdates(bestLocationProvider, 2000, 1, this);
                }
                else
                {
                    locMgr.RequestLocationUpdates(LocationManager.GpsProvider, 2000, 1, this);       //Simulator works ok with this
                }
            }
        }

        void DoSearchOfNextLocation()
        {
            string sSearch = txtSearchLocation.Text;

            GeoLocGetter jGeo = new GeoLocGetter(this);
            jGeo.coordsCompareTo = coordinatesFound;

            LatLngSimple nCoords = jGeo.GetCoordsFromLocation(sSearch);
            string sPlaceResults;
            if (nCoords == null)
            {
                sPlaceResults = "Location Not Found";
            }
            else
            {
                sPlaceResults = jGeo.GetGeoLocation(nCoords.Latitude, nCoords.Longitude, true);
            }

            DEBUG("J -> Search Address = " + sPlaceResults);

            txtSearchResults.Text = sPlaceResults;
        }

        void DEBUG(string sMsg)
        {
            Log.Debug("com.jon.distancer", sMsg);
        }

        #region Native App State Handler

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnResume()
        {
            base.OnResume();

            InitLocationService();
        }

        protected override void OnPause()
        {
            base.OnPause();

            locMgr.RemoveUpdates(this);
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        #endregion

        #region Location Handler Implementations

        public void OnLocationChanged(Android.Locations.Location location)
        {
            DEBUG("Location changed");

            string sResult = "";

            GeoLocGetter jGeo = new GeoLocGetter(this);

            coordinatesFound = new LatLngSimple(location.Latitude, location.Longitude);

            string sAddr = jGeo.GetGeoLocation(location.Latitude, location.Longitude, false);

            sResult += "Coordinates => " + location.Latitude.ToString() + ", " + location.Longitude.ToString() + "\n" + sAddr; // + " -> Provider = " + location.Provider.ToString()

            txtCurrentLocation.Text = sResult;

            //Stop Search after first find...???
            locMgr.RemoveUpdates(this);

            if (butSearch.Enabled == true)
            {
                DoSearchOfNextLocation();
            } else
            {
                butSearch.Enabled = true;
            }
        }

        public void OnProviderDisabled(string provider)
        {
            ShowErrorAlert($"Provider disabled: {provider}");
            locMgr.RemoveUpdates(this);
        }

        public void OnProviderEnabled(string provider)
        {
            DEBUG(provider + " enabled by user");
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            DEBUG(provider + " availability has changed to " + status.ToString());
        }

        #endregion

        #region Permission Handling

        public async Task<PermissionStatus> CheckAndRequestPermissionAsync<T>(T permission)
                    where T : BasePermission
        {
            var status = await permission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await permission.RequestAsync();
            }

            return status;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        #endregion
    }
}
