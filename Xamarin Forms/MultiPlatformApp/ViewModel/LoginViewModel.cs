using MultiPlatformApp.Helpers;
using MultiPlatformApp.View;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using Xamarin.Forms;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Newtonsoft.Json.Linq;
using Acr.UserDialogs;

namespace MultiPlatformApp.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        public ICommand LoginCommand { get; }
        public ICommand OnAppearingLoginCommand { get; }

        public PageService PageService { get; private set; }

        public LoginViewModel(IUserDialogs dialogs) : base(dialogs)
        {
            PageService = new PageService();
            LoginCommand = new Command<Page>(async loginPage =>
                {
                    await Task.Delay(10);
                    await ExecuteLoginCommand(loginPage);
                    await Task.Delay(10);
                });
            OnAppearingLoginCommand = new Command<Page>(async loginPage => await ExecuteOnAppearingLogin(loginPage));
        }

        async Task ExecuteOnAppearingLogin(Page loginPage)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                using (UserDialogs.Loading("Checking connection..."))
                {
                    //Check Location Services first
                    var check = await App.CheckLocationServices();
                    if (check != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (check != Plugin.Permissions.Abstractions.PermissionStatus.Unknown)
                                App.LocationServicesFailure();
                            App.GoLogout();
                        });
                        IsBusy = false;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Device.BeginInvokeOnMainThread(() =>
                {
                    App.LocationServicesFailure();
                });
                IsBusy = false;
                return;
            }

            try
            {
                _ = Task.Run(async () =>
                {
                    // Look for existing account
                    IEnumerable<IAccount> accounts = await App.AuthenticationClient.GetAccountsAsync();

                    if (accounts.Count() > 0)
                    {
                        using (UserDialogs.Loading("Logging in automatically..."))
                        {
                            AuthenticationResult result = await App.AuthenticationClient
                            .AcquireTokenSilent(CommonConstants.Scopes, accounts.FirstOrDefault())
                            .ExecuteAsync();

                            string signInValue = "";
                            var token = new JwtSecurityToken(result.IdToken);
                            bool emailSignIn = true;
                            foreach (var claim in token.Claims)
                            {
                                if (claim.Type == "emails" ||
                                    claim.Type == "signInNames.emailAddress")
                                {
                                    signInValue = claim.Value;
                                }
                                else if (claim.Type == "signInNames.phoneNumber")
                                {
                                    signInValue = claim.Value;
                                    emailSignIn = false;
                                }
                            }
                            if (!String.IsNullOrEmpty(signInValue))
                            {
                                Console.WriteLine("User '" + signInValue + "' already logged in, showing LOADS page");
                                await GotoList(signInValue, loginPage, emailSignIn);
                                await App.soapService.StartListening();
                            }
                            else
                            {
                                Console.WriteLine("User not logged in, showing Login page");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("User not logged in, showing Login page");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("User not logged in, showing Login page");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task ExecuteLoginCommand(Page loginPage)
        {
            if (IsBusy)
                return;

            if (!await CheckConnectivityAsync())
                return;

            var check = await App.CheckLocationServices();

            try
            {
                IsBusy = true;

                if (check != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    if (check != Plugin.Permissions.Abstractions.PermissionStatus.Unknown)
                        Device.BeginInvokeOnMainThread(() => App.LocationServicesFailure());
                    IsBusy = false;
                    return;
                }

                Analytics.TrackEvent("Driver-Login");
                AuthenticationResult result;
                try
                {
                    result = await App.AuthenticationClient
                        .AcquireTokenInteractive(CommonConstants.Scopes)
                        .WithPrompt(Prompt.SelectAccount)
                        .WithParentActivityOrWindow(App.UIParent)
                        .WithUseEmbeddedWebView(true)
                        .ExecuteAsync();

                    using (UserDialogs.Loading("Logging In..."))
                    {

                        string signInValue = "";
                        var token = new JwtSecurityToken(result.IdToken);
                        bool emailSignIn = true;
                        foreach (var claim in token.Claims)
                        {
                            if (claim.Type == "emails" ||
                                claim.Type == "signInNames.emailAddress")
                            {
                                signInValue = claim.Value;
                            }
                            else if (claim.Type == "signInNames.phoneNumber")
                            {
                                signInValue = claim.Value;
                                emailSignIn = false;
                            }
                        }

                        await GotoList(signInValue, loginPage, emailSignIn);
                    }
                }
                catch (MsalException ex)
                {
                    if (ex.Message != null && ex.Message.Contains("AADB2C90118"))
                    {
                        result = await ForgotPassword();
                        await App.GoLogin();
                    }
                    else if (ex.Message != null && ex.Message.Contains("AADB2C90091"))
                    {
                        await PageService.DisplayAlert("User Cancelled", "Sign in was cancelled", "Dismiss");
                    }
                    else if (ex.ErrorCode != "authentication_canceled")
                    {
                        await PageService.DisplayAlert("An error has occurred", "Exception message: " + ex.Message, "Dismiss");
                    }
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }
            finally
            {
                if (check == Plugin.Permissions.Abstractions.PermissionStatus.Granted) 
                    await App.soapService.StartListening();
                IsBusy = false;
            }
        }

        async Task<AuthenticationResult> ForgotPassword()
        {
            if (!await CheckConnectivityAsync())
                return null;

            try
            {
                return await App.AuthenticationClient
                    .AcquireTokenInteractive(CommonConstants.Scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .WithParentActivityOrWindow(App.UIParent)
                    .WithB2CAuthority(CommonConstants.AuthorityPasswordReset)
                    .ExecuteAsync();
            }
            catch (MsalException ex)
            {
                Console.WriteLine(ex.Message);
                // Do nothing - ErrorCode will be displayed in ExecuteLoginCommand
                return null;
            }
        }

        private async Task GotoList(string userSignInName, Page loginPage, bool emailSignIn)
        {
            using (UserDialogs.Loading("Loading List..."))
            {
                await Task.Delay(10);
                await Task.Run(async () =>
                {
                    await PageService.PopToRootAsync();
                    await PageService.PushAsync(new LoadsPage(UserDialogs, userSignInName, emailSignIn));
                });
            }
        }
    }
}
