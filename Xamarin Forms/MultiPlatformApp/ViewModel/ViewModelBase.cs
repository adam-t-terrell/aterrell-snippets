using System.Threading.Tasks;
using MultiPlatformApp.Helpers;
using MultiPlatformApp.Resources;
using MultiPlatformApp.Services;
using MvvmHelpers;
using Xamarin.Forms;
using Xamarin.Essentials;
using Acr.UserDialogs;

namespace MultiPlatformApp.ViewModel
{
    public class ViewModelBase : BaseViewModel
    {
        public Settings Settings => Settings.Current;

        IDataService dataService;
        public IDataService DataService => dataService ?? (dataService = DependencyService.Get<IDataService>());
        IDialogs dialogs;
        public IDialogs Dialogs => dialogs ?? (dialogs = DependencyService.Get<IDialogs>());

        public IUserDialogs UserDialogs { get; }

        protected ViewModelBase(IUserDialogs dialogs)
        {
            this.UserDialogs = dialogs;
        }

        string updateMessage;
        public string UpdateMessage
        {
            get => updateMessage;
            set => SetProperty(ref updateMessage, value);
        }

        public async Task<bool> CheckConnectivityAsync()
        {

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                UserDialogs.HideLoading();
                await Dialogs.AlertAsync(null, AppResources.NoInternet, AppResources.OK);
                return false;
            }

            return true;
        }

        public static Task ExecuteGoToSiteExtCommand(string site) =>
            Browser.OpenAsync(site, BrowserLaunchMode.SystemPreferred);
        
    }
}
