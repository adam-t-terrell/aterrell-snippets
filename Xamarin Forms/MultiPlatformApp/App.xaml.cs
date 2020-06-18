using MultiPlatformApp.Helpers;
using MultiPlatformApp.Resources;
using MultiPlatformApp.Services;
using MultiPlatformApp.View;
using Device = Xamarin.Forms.Device;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Microsoft.AppCenter;
using Microsoft.Identity.Client;
using Plugin.Multilingual;
using Xamarin.Forms;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Acr.UserDialogs;
using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;
using Plugin.Permissions;

namespace MultiPlatformApp
{
    public partial class App : Application
    {
        public static IPublicClientApplication AuthenticationClient { get; private set; }

        public static object UIParent { get; set; } = null;
        public static MultiPlatformApp.Services.SOAPService soapService;
        public static MultiPlatformApp.Services.IImageService imageService;

        public App()
        {
            InitializeComponent();

            var culture = CrossMultilingual.Current.DeviceCultureInfo;
            AppResources.Culture = culture;

            DependencyService.Register<IDialogs, Dialogs>();
            DependencyService.Register<IPageService, PageService>();
            AuthenticationClient = PublicClientApplicationBuilder.Create(CommonConstants.ClientId)
                .WithIosKeychainSecurityGroup(CommonConstants.IosKeychainSecurityGroups)
                .WithB2CAuthority(CommonConstants.AuthoritySignin)
                .WithRedirectUri($"msal{CommonConstants.ClientId}://auth")
                .Build();

            MonkeyCache.FileStore.Barrel.ApplicationId = "MultiPlatformApp";

            try
            {
                soapService = new MultiPlatformApp.Services.SOAPService();
                imageService = DependencyService.Get<IImageService>();
                MainPage = new NavigationPage(new LoginPage(UserDialogs.Instance, true));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                UserDialogs.Instance.Alert("Error: " + ex.Message, "Error", "Exit");
                return;
            }
        }

        public static async void LocationServicesFailure()
        {
            string message = "You must enable location services to use this app. On the next screen, please select \"Always\" for Location for this App.";
            if (Device.RuntimePlatform == Device.Android)
                message = "You must enable location services to use this app. On the next screen, please select \"Permissions\", and then make sure the \"Location\" permission is Enabled." + Environment.NewLine + Environment.NewLine + "If the Location Permission is Enabled already, please make sure you have location services turned on on your device." + Environment.NewLine + Environment.NewLine + "If this message keeps appearing, you have not enabled both permissions for the app and Location Services.";
            Console.WriteLine("Location Services Failure");
            var alertconfig = new AlertConfig()
            {
                OkText = "Open App Settings",
                Title = "Location Services",
                Message = message
            };

            var tokenSource = new System.Threading.CancellationTokenSource();
            var token = tokenSource.Token;

            await UserDialogs.Instance.AlertAsync(alertconfig, token);
            OpenSettings();
            tokenSource.Cancel();
            GoLogout();
        }

        private static void OpenSettings()
        {
            CrossPermissions.Current.OpenAppSettings();
        }

        public static async Task<Plugin.Permissions.Abstractions.PermissionStatus> CheckLocationServices()
        {
            try
            {
                Plugin.Permissions.Abstractions.PermissionStatus status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                Plugin.Permissions.Abstractions.PermissionStatus status2 = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationWhenInUse);
                Plugin.Permissions.Abstractions.PermissionStatus status3 = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationAlways);
                Console.WriteLine("Location Permissions: Location: " + status.ToString() + ", When In Use: " + status2.ToString() + ", Always: " + status3.ToString());
                if (status3 != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            ILocationManager manager = DependencyService.Get<ILocationManager>();
                            manager.RequestBackgroundLocation();
                        }
                    });
                }
                else
                {
                    try
                    {
                        var request = new GeolocationRequest(GeolocationAccuracy.Lowest, new TimeSpan(0,0,0,0,100));
                        var location = await Geolocation.GetLocationAsync(request);

                        if (location != null)
                        {
                            Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                        }
                    }
                    catch (FeatureNotSupportedException)
                    {
                        return Plugin.Permissions.Abstractions.PermissionStatus.Denied;
                    }
                    catch (FeatureNotEnabledException)
                    {
                        return Plugin.Permissions.Abstractions.PermissionStatus.Denied;
                    }
                }
                return status3;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return Plugin.Permissions.Abstractions.PermissionStatus.Denied;
            }
        }

        public static async Task GoLogin(bool autoLogin = true)
        {
            await new PageService().PopToRootAsync();
        }

        public static async void GoLogout()
        {
            Console.WriteLine("Logging out....");
            var accounts = await AuthenticationClient.GetAccountsAsync();
            foreach (var account in accounts)
            {
                Console.WriteLine(account.Username + " logging out");
                await AuthenticationClient.RemoveAsync(account);
            }
            await GoLogin(false);
        }
        
        public static string GetRunningVersion()
        {
            return VersionTracking.CurrentVersion + " (" + VersionTracking.CurrentBuild + ")";
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            if ((Device.RuntimePlatform == Device.Android && CommonConstants.AppCenterAndroid != "AC_ANDROID") ||
                (Device.RuntimePlatform == Device.iOS && CommonConstants.AppCenteriOS != "AC_IOS") ||
                (Device.RuntimePlatform == Device.UWP && CommonConstants.AppCenterUWP != "AC_UWP"))
            {
                if (CommonConstants.ShowLogin == "AC_SHOWLOGIN")
                {

                    AppCenter.Start($"android={CommonConstants.AppCenterAndroid};" +
                           $"uwp={CommonConstants.AppCenterUWP};" +
                           $"ios={CommonConstants.AppCenteriOS}",
                           typeof(Analytics), typeof(Crashes), typeof(Distribute));
                }
                else
                {
                    AppCenter.Start($"android={CommonConstants.AppCenterAndroid};" +
                           $"uwp={CommonConstants.AppCenterUWP};" +
                           $"ios={CommonConstants.AppCenteriOS}",
                           typeof(Analytics), typeof(Crashes));
                }
            }
        }

        protected override void OnSleep()
        {
            
        }

        protected override void OnResume()
        {
            
        }
    }
}
