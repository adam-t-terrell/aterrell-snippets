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
    public class LoadDetailViewModel : ViewModelBase
    {
        public ObservableRangeCollection<Shippingorder_Mobilesearch> SingleLoad { get; private set; }
        public int OrderNumber { get; set; }
        public string Location { get; set; }
        public string LocationName
        {
            get
            {
                if (!String.IsNullOrEmpty(Location))
                {
                    if (SingleLoad.Count > 0)
                    {
                        string retval = "";
                        switch (Location)
                        {
                            case "1":
                                retval = SingleLoad[0].Location1CompanyName;
                                break;
                            case "2":
                                retval = SingleLoad[0].Location2CompanyName;
                                break;
                            case "3":
                                retval = SingleLoad[0].Location3CompanyName;
                                break;
                            case "4":
                                retval = SingleLoad[0].Location4CompanyName;
                                break;
                            default:
                                break;
                        }
                        return retval;
                    }
                }
                return "";
            }
        }
        public string LoadTitle
        {
            get {
                if (SingleLoad.Count > 0)
                    return "Shipment #" + SingleLoad[0].ShippingOrderID.ToString();
                else return "";
            }
        }
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
        public bool IsDriverLoad
        {
            get
            {
                if (SingleLoad.Count > 0)
                {
                    if (!String.IsNullOrWhiteSpace(_userEmail))
                        return SingleLoad[0].DriverEmail.ToLower() == _userEmail.ToLower();
                    else
                    {
                        string driverCell = (new string(SingleLoad[0].CarrierDriverCell.Where(c => char.IsDigit(c)).ToArray()));
                        if (driverCell.StartsWith("1"))
                            driverCell = "+" + driverCell;
                        else
                            driverCell = "+1" + driverCell;
                        return driverCell == _userPhoneNumber;
                    }
                }
                    
                return false;
            }
        }

        private string _userEmail;
        private string _userPhoneNumber;

        public ICommand RefreshCommand { get; }
        public ICommand ForceRefreshCommand { get; }
        public ICommand CheckInCommand { get; }
        public ICommand UndoCheckInCommand { get; }
        public ICommand PhotoCommand { get; }
        public ICommand LastLocationCommand { get; }
        public ICommand PhotoListCommand { get; }

        public LoadDetailViewModel(IUserDialogs dialogs, int loadId, string userSignInValue, bool emailSignIn) : base(dialogs)
        {
            OrderNumber = loadId;
            if (emailSignIn)
                _userEmail = userSignInValue;
            else
                _userPhoneNumber = userSignInValue;
            SingleLoad = new ObservableRangeCollection<Shippingorder_Mobilesearch>();
            RefreshCommand = new Command(async () => 
                {
                    using (UserDialogs.Loading($"Loading Details of Order # {OrderNumber}..."))
                    {
                        await Task.Delay(10);
                        await ExecuteRefreshCommand(false);
                        await Task.Delay(10);
                    }
                });
            ForceRefreshCommand = new Command(async () =>
                {
                    using (UserDialogs.Loading($"Loading Details of Order # {OrderNumber}..."))
                    {
                        await Task.Delay(10);
                        await ExecuteRefreshCommand(true);
                        await Task.Delay(10);
                    }
                });
            CheckInCommand = new Command(async () =>
                {
                    using (UserDialogs.Loading("Checking in..."))
                    {
                        await Task.Delay(10);
                        await ExecuteCheckInCommand();
                        await Task.Delay(10);
                    }
                });
            UndoCheckInCommand = new Command(async () =>
                {
                    using (UserDialogs.Loading("Undoing checkin..."))
                    {
                        await Task.Delay(10);
                        await ExecuteUndoCheckInCommand();
                        await Task.Delay(10);
                    }
                });
            PhotoCommand = new Command<string>(async (string arg) =>
                {
                    using (UserDialogs.Loading("Please wait..."))
                    {
                        await Task.Delay(10);
                        await ExecutePhotoCommand(arg);
                        await Task.Delay(10);
                    }
                });
            LastLocationCommand = new Command(async () =>
                {
                    using (UserDialogs.Loading("Please wait..."))
                    {
                        await Task.Delay(10);
                        await ExecuteLastLocationCommand();
                        await Task.Delay(10);
                    }
                });
            PhotoListCommand = new Command(async () =>
                {
                    using (UserDialogs.Loading("Please wait..."))
                    {
                        await Task.Delay(10);
                        await ExecutePhotoListCommand();
                        await Task.Delay(10);
                    }
                });
        }

        public LoadDetailViewModel(Shippingorder_Mobilesearch load, IUserDialogs dialogs, int loadId, string userSignInValue, bool emailSignIn) : base(dialogs)
        {
            OrderNumber = loadId;
            if (emailSignIn)
                _userEmail = userSignInValue;
            else
                _userPhoneNumber = userSignInValue;
            SingleLoad = new ObservableRangeCollection<Shippingorder_Mobilesearch>()
            {
                load
            };
            RefreshCommand = new Command(async () =>
            {
                using (UserDialogs.Loading($"Loading Details of Order # {OrderNumber}..."))
                {
                    await Task.Delay(10);
                    await ExecuteRefreshCommand(false);
                    await Task.Delay(10);
                }
            });
            ForceRefreshCommand = new Command(async () =>
            {
                using (UserDialogs.Loading($"Loading Details of Order # {OrderNumber}..."))
                {
                    await Task.Delay(10);
                    await ExecuteRefreshCommand(true);
                    await Task.Delay(10);
                }
            });
            CheckInCommand = new Command(async () =>
            {
                using (UserDialogs.Loading("Checking in..."))
                {
                    await Task.Delay(10);
                    await ExecuteCheckInCommand();
                    await Task.Delay(10);
                }
            });
            UndoCheckInCommand = new Command(async () =>
            {
                using (UserDialogs.Loading("Undoing checkin..."))
                {
                    await Task.Delay(10);
                    await ExecuteUndoCheckInCommand();
                    await Task.Delay(10);
                }
            });
            PhotoCommand = new Command<string>(async (string arg) =>
            {
                using (UserDialogs.Loading("Please wait..."))
                {
                    await Task.Delay(10);
                    await ExecutePhotoCommand(arg);
                    await Task.Delay(10);
                }
            });
            LastLocationCommand = new Command(async () =>
            {
                using (UserDialogs.Loading("Please wait..."))
                {
                    await Task.Delay(10);
                    await ExecuteLastLocationCommand();
                    await Task.Delay(10);
                }
            });
            PhotoListCommand = new Command(async () =>
            {
                using (UserDialogs.Loading("Please wait..."))
                {
                    await Task.Delay(10);
                    await ExecutePhotoListCommand();
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
                await Task.Run(() =>
                {
                    IQueryable<Shippingorder_Mobilesearch> load;
                    if (!String.IsNullOrWhiteSpace(_userEmail))
                        load = App.soapService.GetByOrderNumber(OrderNumber, _userEmail, true).ShippingOrder_MobileSearch.AsQueryable();
                    else 
                        load = App.soapService.GetByOrderNumber(OrderNumber, _userPhoneNumber, false).ShippingOrder_MobileSearch.AsQueryable();

                    if (load != null && load.Count() > 0)
                    {
                        SingleLoad.ReplaceRange(load.AsEnumerable());
                        OnPropertyChanged("LoadTitle");
                        OnPropertyChanged("LocationName");
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

        async Task ExecuteCheckInCommand()
        {
            if (IsBusy)
                return;

            if (!await CheckConnectivityAsync())
                return;

            if (!IsDriverLoad)
                return;

            IsBusy = true;

            try
            {
                Shippingorder_Mobilesearch load;
                if (SingleLoad.Count > 0)
                {
                    load = SingleLoad[0];
                    switch (Location)
                    {
                        case "1":
                            load.a1CompletionDate = DateTime.UtcNow;
                            break;
                        case "2":
                            load.a2CompletionDate = DateTime.UtcNow;
                            break;
                        case "3":
                            load.a3CompletionDate = DateTime.UtcNow;
                            break;
                        case "4":
                            load.a4CompletionDate = DateTime.UtcNow;
                            break;
                        default:
                            break;
                    }
                    App.soapService.UpdateCompletion(load.ShippingOrderID, load.a1CompletionDate, load.a2CompletionDate, load.a3CompletionDate, load.a4CompletionDate);
                    PageService pageService = new PageService();
                    await pageService.DisplayAlert("Checked in", "Successfully Checked in", "OK");
                }
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

        async Task ExecuteUndoCheckInCommand()
        {
            if (IsBusy)
                return;

            if (!await CheckConnectivityAsync())
                return;

            if (!IsDriverLoad)
                return;

            IsBusy = true;

            try
            {
                Shippingorder_Mobilesearch load;
                if (SingleLoad.Count > 0)
                {
                    load = SingleLoad[0];
                    switch (Location)
                    {
                        case "1":
                            load.a1CompletionDate = (DateTime?)null;
                            break;
                        case "2":
                            load.a2CompletionDate = (DateTime?)null;
                            break;
                        case "3":
                            load.a3CompletionDate = (DateTime?)null;
                            break;
                        case "4":
                            load.a4CompletionDate = (DateTime?)null;
                            break;
                        default:
                            break;
                    }
                    App.soapService.UpdateCompletion(load.ShippingOrderID, load.a1CompletionDate, load.a2CompletionDate, load.a3CompletionDate, load.a4CompletionDate);
                    PageService pageService = new PageService();
                    await pageService.DisplayAlert("Checkin undone", "Successful undo", "OK");
                }
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

        async Task ExecutePhotoCommand(string arg)
        {
            if (IsBusy)
                return;

            if (!await CheckConnectivityAsync())
                return;

            IsBusy = true;

            try
            {
                Shippingorder_Mobilesearch load;
                if (SingleLoad.Count > 0)
                {
                    load = SingleLoad[0];
                    PageService pageService = new PageService();
                    string userSignIn;
                    if (!String.IsNullOrWhiteSpace(_userEmail))
                        userSignIn = _userEmail;
                    else
                        userSignIn = _userPhoneNumber;
                    await pageService.PushModalAsync(new View.TakePhoto(userSignIn, load.ShippingOrderID, arg, 2));
                }
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

        async Task ExecuteLastLocationCommand()
        {
            if (IsBusy)
                return;

            if (!await CheckConnectivityAsync())
                return;

            IsBusy = true;

            try
            {
                if (SingleLoad.Count > 0)
                {
                    var url = App.soapService.GetLastLocation(SingleLoad[0].ShippingOrderID);
                    if (!String.IsNullOrWhiteSpace(url))
                    {
                        await Xamarin.Essentials.Browser.OpenAsync(new Uri(url), Xamarin.Essentials.BrowserLaunchMode.SystemPreferred);
                    }
                    else
                    {
                        await new PageService().DisplayAlert("No Location Found", "No Location found for this load.", "OK");
                    }
                }
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

        async Task ExecutePhotoListCommand()
        {
            if (IsBusy)
                return;

            if (!await CheckConnectivityAsync())
                return;

            IsBusy = true;

            try
            {
                Shippingorder_Mobilesearch load;
                if (SingleLoad.Count > 0)
                {
                    load = SingleLoad[0];
                    PageService pageService = new PageService();
                    string userSignIn;
                    if (!String.IsNullOrWhiteSpace(_userEmail))
                        userSignIn = _userEmail;
                    else
                        userSignIn = _userPhoneNumber;
                    await pageService.PushModalAsync(new View.PhotoList(userSignIn, load.ShippingOrderID, OrderNumber));
                }
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
    }
}
