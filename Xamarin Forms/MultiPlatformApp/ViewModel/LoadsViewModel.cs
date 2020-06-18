using Acr.UserDialogs;
using MultiPlatformApp.Model;
using Microsoft.AppCenter.Crashes;
using MvvmHelpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MultiPlatformApp.ViewModel
{
    public class LoadsViewModel : ViewModelBase
    {
        public ObservableRangeCollection<Shippingorder_Mobilesearch> Loads { get; }

        public ICommand RefreshCommand { get; }
        public ICommand ForceRefreshCommand { get; }
        public ICommand LogoutCommand { get; }

        public PageService PageService { get; private set; }

        public string UserEmail
        {
            get
            {
                return _userEmail;
            }
        }

        public string UserPhoneNumber
        {
            get
            {
                return _userPhoneNumber;
            }
        }

        private string _userEmail;
        private string _userPhoneNumber;

        public LoadsViewModel(IUserDialogs dialogs, string userSignInValue, bool emailSignIn) : base(dialogs)
        {
            if (emailSignIn)
                _userEmail = userSignInValue;
            else
                _userPhoneNumber = userSignInValue;
            PageService = new PageService();
            Loads = new ObservableRangeCollection<Shippingorder_Mobilesearch>();
            RefreshCommand = new Command(async () =>
                {
                    using (UserDialogs.Loading("Loading List..."))
                    {
                        await Task.Delay(10);
                        await ExecuteRefreshCommand(false);
                        await Task.Delay(10);
                    }
                });
            ForceRefreshCommand = new Command(async () =>
                {
                    using (UserDialogs.Loading("Loading List..."))
                    {
                        await Task.Delay(10);
                        await ExecuteRefreshCommand(false);
                        await Task.Delay(10);
                    }
                });
            LogoutCommand = new Command(async () =>
                {
                    using (UserDialogs.Loading("Logging Out..."))
                    {
                        await Task.Delay(10);
                        await ExecuteLogoutCommand();
                        await Task.Delay(10);
                    }
                });
        }

        async Task ExecuteRefreshCommand(bool forceRefresh)
        {
            if (IsBusy)
                return;

            if (!await CheckConnectivityAsync())
                return;

            IsBusy = true;

            try
            {
                await Task.Run(async() =>
                {
                    string userSignInString = _userEmail;
                    if (String.IsNullOrWhiteSpace(_userEmail))
                        userSignInString = _userPhoneNumber;
                    IQueryable<Shippingorder_Mobilesearch> loads = App.soapService.GetAll(userSignInString).ShippingOrder_MobileSearch.AsQueryable();

                    if (loads != null && loads.Count() > 0)
                        Loads.ReplaceRange(loads.AsEnumerable());
                    else if (loads != null && loads.Count() == 0)
                    {
                        await PageService.DisplayAlert("No Loads", "There are no loads assigned to or viewable by '" + userSignInString + "'.  Please contact Citation Logistics about your loads.", "OK");
                    }
                    else
                    {
                        await PageService.DisplayAlert("Error Getting Loads", "There was an error getting the loads.", "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                System.Diagnostics.Debug.WriteLine($"*** ERROR: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task ExecuteLogoutCommand()
        {
            if (IsBusy)
                return;

            if (!await CheckConnectivityAsync())
            {
                await PageService.DisplayAlert("Logout Error", "Unable to logout: Not connected", "Dismiss");
                return;
            }

            try
            {
                IsBusy = true;
                try
                {
                    Device.BeginInvokeOnMainThread(() => App.GoLogout());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }
            finally
            {
                await App.soapService.StopListening();
                IsBusy = false;
            }
        }
    }
}
