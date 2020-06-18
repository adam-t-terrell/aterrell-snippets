using Acr.UserDialogs;
using MultiPlatformApp.Helpers;
using MultiPlatformApp.Model;
using MultiPlatformApp.Services;
using Microsoft.Azure.Documents;
using Plugin.Media.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApp.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TakePhoto : ContentPage
    {
        private string _userSignIn;
        int _nShippingOrderFileID;
        int _nShippingOrderID;
        string _picType;
        string _fileName;
        private bool _adjusted;
        private byte[] _imageByte;
        public TakePhoto(string userSignIn, int shippingOrderId, string picType, int shippingorderFileId = 0)
        {
            _userSignIn = userSignIn;
            _nShippingOrderFileID = shippingorderFileId;
            _nShippingOrderID = shippingOrderId;
            _picType = picType;

            NavigationPage.SetHasBackButton(this, false);
            InitializeComponent();
            AssignInitialFileName(shippingOrderId, shippingorderFileId);
            popupFileName.Text = _fileName.Replace(".png", "");
            popupCategory.Text = picType;
            CameraButton.Clicked += CameraButton_Clicked;
            DeleteButton.Clicked += DeleteButton_Clicked;
            BackButton.Clicked += BackButton_Clicked;
            UploadPhotoButton.Clicked += UploadPhotoButton_Clicked;
            CancelUploadPhotoButton.Clicked += CancelUploadPhotoButton_Clicked;
        }

        private async void AssignInitialFileName(int shippingOrderId, int shippingorderFileId)
        {
            if (shippingorderFileId == 0)
            {
                Device.BeginInvokeOnMainThread(() => DeleteButton.IsEnabled = false);
                int num = 0;
                bool exists = true;
                while (exists)
                {
                    num++;
                    _fileName = _nShippingOrderID.ToString() + "_" + _picType + "_" + num.ToString("0000") + ".png";
                    exists = await App.soapService.PictureExists(_fileName);
                }
            }
            else
                _fileName = App.soapService.GetShippingOrderFiles(shippingOrderId, "").SingleOrDefault(x => x.ShippingOrderFileID == shippingorderFileId)?.FileName;
        }

        protected override async void OnAppearing()
        {
            if (IsBusy)
            {
                base.OnAppearing();
                return;
            }
            try
            {
                IsBusy = true;
                if (!_adjusted)
                {
                    await Device.InvokeOnMainThreadAsync(() =>
                    {
                        UniformFormattingHelpers.UniversalAdjustments(this.Content);
                        _adjusted = true;
                    });
                }
                base.OnAppearing();
                var noimageyet = "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";
                ImageSource noImg = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(noimageyet)));

                if (_nShippingOrderFileID > 0)
                {
                    Device.BeginInvokeOnMainThread(() => CameraButton.Text = "Retake Photo");
                    using (UserDialogs.Instance.Loading("Showing existing image...."))
                    {
                        await Task.Delay(100);
                        var existingPic = App.soapService.GetShippingOrderFiles(_nShippingOrderID, _picType).SingleOrDefault(x => x.ShippingOrderFileID == _nShippingOrderFileID);
                        if (existingPic != null)
                        {
                            await Task.Run(async () =>
                            {
                                var str = await App.soapService.DownloadPicture(existingPic.FileName);
                                var imagedata = Convert.FromBase64String(str);
                                var image = ImageSource.FromStream(() => new MemoryStream(imagedata));
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    PhotoImage.Source = image;
                                });
                                await Task.Delay(3000);
                            });
                        }
                        else
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                PhotoImage.Source = noImg;
                            });
                        }
                    }
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        PhotoImage.Source = noImg;
                        await Task.Delay(10);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            IsBusy = false;
        }

        private async void CameraButton_Clicked(object sender, EventArgs e)
        {
            if (IsBusy)
                return;
            IsBusy = true;
            UserDialogs.Instance.ShowLoading("Taking picture...");
            await Task.Delay(10);
            await Plugin.Media.CrossMedia.Current.Initialize();

            if (Debugger.IsAttached && !Plugin.Media.CrossMedia.Current.IsCameraAvailable || !Plugin.Media.CrossMedia.Current.IsTakePhotoSupported)
            {
                OpenPopup(null);
                UserDialogs.Instance.HideLoading();
                IsBusy = false;
                return;
            }

            if (!Plugin.Media.CrossMedia.Current.IsCameraAvailable || !Plugin.Media.CrossMedia.Current.IsTakePhotoSupported)
            {
                PageService pageService = new PageService();
                await pageService.DisplayAlert("No Camera", ":( No camera available.", "OK");
                UserDialogs.Instance.HideLoading();
                IsBusy = false;
                return;
            }

            var photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions()
            {
                Name = _fileName
            });

            UserDialogs.Instance.HideLoading();
            if (photo != null)
            {
                OpenPopup(photo);
                using (UserDialogs.Instance.Loading("Processing photo..."))
                {
                    await Task.Delay(10);
                    var image = ImageSource.FromStream(() => { return photo.GetStream(); });
                    PhotoImage.Source = image;
                    await Task.Delay(1000);
                }
            }
            else
            {
                PageService pageService = new PageService();
                await pageService.DisplayAlert("No Photo", "Photo not taken", "OK");
            }
            await Task.Delay(10);
            IsBusy = false;
        }

        private void OpenPopup(MediaFile photo)
        {
            if (photo == null)
            {
                popupImageMetadata.IsVisible = true;
                return;
            }
            using (UserDialogs.Instance.Loading("Waiting for Photo Data..."))
            {
                IsBusy = true;
                var imageStream = photo.GetStreamWithImageRotatedForExternalStorage();
                BinaryReader br = new BinaryReader(imageStream);
                _imageByte = br.ReadBytes((int)imageStream.Length);
                popupImageMetadata.IsVisible = true;
                IsBusy = false;
            }
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

        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            if (IsBusy)
                return;
            IsBusy = true;
            using (UserDialogs.Instance.Loading("Deleting Photo..."))
            {
                await App.soapService.ShippingOrderFileDelete(_nShippingOrderFileID, _fileName);
                await Task.Delay(10);
            }
            await new PageService().PopModalAsync();
            await Task.Delay(1000);
            IsBusy = false;
        }

        private async void UploadPhotoButton_Clicked(object sender, EventArgs e)
        {
            if (IsBusy)
                return;

            if (String.IsNullOrWhiteSpace(popupFileName.Text))
            {
                await new PageService().DisplayAlert("No Name", "You must enter a Name", "OK");
                return;
            }
            
            if (_imageByte == null || _imageByte.Length <= 0)
            {
                popupImageMetadata.IsVisible = false;
                await new PageService().DisplayAlert("No Image", "There appears to be no image to upload.", "OK");
                return;
            }

            IsBusy = true;
            string newFileName = popupFileName.Text.Trim() + ".png";
            if (newFileName != _fileName)
            {
                bool fileExists = await App.soapService.PictureExists(newFileName);
                if (fileExists)
                {
                    await new PageService().DisplayAlert("Picture Exists", "The name you have chosen: '" + popupFileName.Text.Trim() + "' already exists on the server.  Please choose a different name.", "OK");
                    IsBusy = false;
                    return;
                }
            }

            using (UserDialogs.Instance.Loading("Uploading photo..."))
            {
                var pic = new ShippingOrderFile()
                {
                    ShippingOrderFileID = _nShippingOrderFileID,
                    ShippingOrderID = _nShippingOrderID,
                    FileName = popupFileName.Text.Trim() + ".png",
                    UploadedBy = _userSignIn,
                    UploadedTimeStamp = DateTime.Now,
                    DocumentType = _picType
                };

                if (!String.IsNullOrWhiteSpace(_fileName)
                    && _fileName != pic.FileName)
                {
                    await App.soapService.ShippingOrderFileDelete(pic.ShippingOrderFileID, _fileName);
                }
                await App.soapService.UploadPicture(_imageByte, pic.FileName);
                string thumbFileName = pic.FileName.Replace(".png", "_thumb.png");
                var newthumbnail = App.imageService.GenerateThumbnail(_imageByte, 200);
                await App.soapService.UploadPicture(newthumbnail, thumbFileName);
                if (_nShippingOrderFileID == 0 ||
                        (!String.IsNullOrWhiteSpace(_fileName)
                        && _fileName != pic.FileName)
                    )
                {
                    await App.soapService.ShippingOrderFileInsert(pic);
                    var newfile = App.soapService.GetShippingOrderFiles(pic.ShippingOrderID, pic.DocumentType).SingleOrDefault(x => x.FileName == pic.FileName);
                    if (newfile != null)
                    {
                        _nShippingOrderFileID = newfile.ShippingOrderFileID;
                        Device.BeginInvokeOnMainThread(() => DeleteButton.IsEnabled = true);
                    }
                }
                _imageByte = null;
                popupImageMetadata.IsVisible = false;
            }
            IsBusy = false;
        }

        private async void CancelUploadPhotoButton_Clicked(object sender, EventArgs e)
        {
            if (IsBusy)
                return;
            IsBusy = true;
            _imageByte = null;
            popupImageMetadata.IsVisible = false;
            IsBusy = false;
            this.OnAppearing();
            IsBusy = true;
            await Task.Delay(100);
            IsBusy = false;
        }
    }
}