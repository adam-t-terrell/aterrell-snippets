using Acr.UserDialogs;
using MultiPlatformApp.Helpers;
using MultiPlatformApp.Model;
using Microsoft.Azure.Documents;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApp.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhotoList : ContentPage
    {
        private string _userSignIn;
        int _nOrderNumber;
        int _nShippingOrderID;
        private bool _adjusted;
        public PhotoList(string userSignIn, int shippingOrderId, int orderNumber)
        {
            _userSignIn = userSignIn;
            _nShippingOrderID = shippingOrderId;
            _nOrderNumber = orderNumber;
            InitializeComponent();
            BackButton.Clicked += BackButton_Clicked;
        }

        private async Task MakeGrid()
        {
            string[] docTypes = {
                "Document",
                "Pickup",
                "Delivery",
                "Other"
            };
            var files = App.soapService.GetShippingOrderFiles(_nShippingOrderID, "").OrderByDescending(f => f.UploadedTimeStamp);

            await Device.InvokeOnMainThreadAsync(() =>
            {
                if (grid.RowDefinitions != null && grid.RowDefinitions.Count > 0)
                    grid.RowDefinitions.Clear();
                if (grid.Children != null && grid.Children.Count > 0)
                    grid.Children.Clear();
                foreach (var col in grid.ColumnDefinitions)
                    col.Width = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width / Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density / 5;
            });

            var colDef = grid.ColumnDefinitions.FirstOrDefault();

            double colWidth = 60.0;
            if (colDef != null)
                colWidth = colDef.Width.Value;

            int columnCount = 0;
            int rowCount = 0;

            await Device.InvokeOnMainThreadAsync(() =>
            {
                mainHeader.Text = "Photos for Order #" + _nOrderNumber.ToString();
                mainHeader.FontSize = 18;
                mainHeader.FontAttributes = FontAttributes.Bold;
                mainHeader.HorizontalOptions = LayoutOptions.CenterAndExpand;
                mainHeader.HeightRequest = 40;
                UniformFormattingHelpers.SetLabelFontAttributes(mainHeader);
            });

            foreach (var type in docTypes)
            {
                var header = new Label() { Text = type + " Photos", HeightRequest = 34, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start, VerticalTextAlignment = TextAlignment.Center };
                var addButton = 
                    new Button()
                    {
                        Text = "Add " + type + " Photo",
                        FontSize = 13,
                        HeightRequest = 32,
                        WidthRequest = 160,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End,
                        Command = new Command(async () =>
                        {
                            if (IsBusy)
                                return;
                            IsBusy = true;
                            ScrollDownPrompt.IsVisible = false;
                            await new PageService().PushModalAsync(new TakePhoto(_userSignIn, _nShippingOrderID, type, 0));
                            await Task.Delay(1000);
                            IsBusy = false;
                        })
                    };
                UniformFormattingHelpers.SetLabelFontAttributes(header);
                await Device.InvokeOnMainThreadAsync(async () =>
                {
                    UniformFormattingHelpers.SetButtonAttributes(addButton);
                    grid.RowDefinitions.Add(new RowDefinition() { Height = header.HeightRequest });
                    double spaceAvailableForScrolling = scrollView.ContentSize.Height - scrollView.Height;
                    if (spaceAvailableForScrolling > 0)
                    {
                        await scrollView.ScrollToAsync(0, spaceAvailableForScrolling, false);
                        await Task.Delay(10);
                    }
                    grid.Children.Add(header, 0, rowCount);
                    grid.Children.Add(addButton, 2, rowCount++);
                    Grid.SetColumnSpan(header, 2);
                    Grid.SetColumnSpan(addButton, 2);
                });

                foreach (var item in files.Where(f => f.DocumentType == type))
                {
                    bool fileExists = await App.soapService.PictureExists(item.FileName);
                    if (fileExists)
                    {
                        string thumbFileName = item.FileName.Replace(".png", "_thumb.png");
                        bool thumbnailExists = await App.soapService.PictureExists(thumbFileName);
                        if (!thumbnailExists)
                        {
                            string bigImageStr = await App.soapService.DownloadPicture(item.FileName);
                            byte[] newthumbnail = null;
                            var bigimagedata = Convert.FromBase64String(bigImageStr);
                            try
                            {
                                newthumbnail = App.imageService.GenerateThumbnail(bigimagedata, 200);
                                await App.soapService.UploadPicture(newthumbnail, thumbFileName);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                            }
                        }
                        string str = await App.soapService.DownloadPicture(thumbFileName);
                        var imagedata = Convert.FromBase64String(str);
                        var resizedThumbnail = imagedata;
                        try
                        {
                            resizedThumbnail = App.imageService.GenerateThumbnail(imagedata, (int)colWidth);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                        }
                        var size = App.imageService.GetSize(resizedThumbnail);
                        var image = new Image()
                        {
                            Source = ImageSource.FromStream(() => new MemoryStream(resizedThumbnail)),
                            GestureRecognizers = {
                                new TapGestureRecognizer()
                                {
                                    Command = new Command(async () =>
                                    {
                                        if (IsBusy)
                                            return;
                                        IsBusy = true;
                                        ScrollDownPrompt.IsVisible = false;
                                        await new PageService().PushModalAsync(new TakePhoto(_userSignIn, _nShippingOrderID, type, item.ShippingOrderFileID));
                                        await Task.Delay(1000);
                                        IsBusy = false;
                                    })
                                }
                            },
                            WidthRequest = size.Width,
                            HeightRequest = size.Height
                        };

                        await Device.InvokeOnMainThreadAsync(async () =>
                        {
                            if (columnCount == 0)
                            {
                                grid.RowDefinitions.Add(new RowDefinition() { Height = colWidth });
                                grid.Children.Add(image, columnCount++, rowCount);
                                await Task.Delay(10);
                                double spaceAvailableForScrolling = scrollView.ContentSize.Height - scrollView.Height;
                                if (spaceAvailableForScrolling > 0)
                                {
                                    await scrollView.ScrollToAsync(0, spaceAvailableForScrolling, false);
                                    await Task.Delay(10);
                                }
                            }
                            else
                            {
                                grid.Children.Add(image, columnCount++, rowCount);
                            }
                            if (columnCount > 3)
                            {
                                columnCount = 0;
                                rowCount++;
                            }
                        });
                    }
                    else
                        Console.WriteLine(item.FileName + " doesn't exist as a blob on Azure");
                }

                if (files.Where(f => f.DocumentType == type).Count() <= 0)
                {
                    var noPhotos = new Label() { Text = "No " + type + " Photos yet", FontSize = 12, HeightRequest = 60, VerticalTextAlignment = TextAlignment.Center, TextColor = Color.FromHex("#666666"), HorizontalTextAlignment = TextAlignment.Center };
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        UniformFormattingHelpers.SetLabelFontAttributes(noPhotos);
                        grid.RowDefinitions.Add(new RowDefinition() { Height = UniformFormattingHelpers.SizeByFontRatio(60) });
                        grid.Children.Add(noPhotos, 0, rowCount);
                        Grid.SetColumnSpan(noPhotos, 4);
                        double spaceAvailableForScrolling = scrollView.ContentSize.Height - scrollView.Height;
                        if (spaceAvailableForScrolling > 0)
                        {
                            await scrollView.ScrollToAsync(0, spaceAvailableForScrolling, false);
                            await Task.Delay(10);
                        }
                    });
                }
                if (columnCount > 0)
                {
                    columnCount = 0;
                    rowCount++;
                }
            }
            var lastRow = new Label() { Text = "" };
            lastRow.VerticalOptions = LayoutOptions.EndAndExpand;
            Grid.SetColumnSpan(lastRow, 4);
            await Device.InvokeOnMainThreadAsync(async () => {
                grid.Children.Add(lastRow, 0, rowCount);
                await scrollView.ScrollToAsync(0, 0, false);
                await Task.Delay(10);
            });
        }

        protected override async void OnAppearing()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                if (!_adjusted)
                {
                    await Device.InvokeOnMainThreadAsync(() => UniformFormattingHelpers.UniversalAdjustments(this.Content));
                    _adjusted = false;
                }
                ScrollDownPrompt.IsVisible = false;
                await Task.Delay(10);
                _ = Task.Run(async () =>
                {
                    await Task.Delay(10);
                    using (UserDialogs.Instance.Loading("Loading Photos..."))
                    {
                        await MakeGrid();
                        _adjusted = true;
                    }
                });
                while (!_adjusted)
                {
                    await Task.Delay(200);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            IsBusy = false;
        }

        private async void BackButton_Clicked(object sender, EventArgs e)
        {
            if (IsBusy)
                return;
            IsBusy = true;
            await new PageService().PopModalAsync();
            await Task.Delay(1000);
            IsBusy = false;
        }

        private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
        {
            double spaceAvailableForScrolling = scrollView.ContentSize.Height - scrollView.Height;
            double buffer = 32;
            ScrollDownPrompt.IsVisible = spaceAvailableForScrolling > e.ScrollY + buffer;
        }
    }
}