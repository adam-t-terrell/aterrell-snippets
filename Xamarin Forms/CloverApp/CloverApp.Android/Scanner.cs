using System;
using Android.App;
using Com.Clover.Sdk.V1;
using Com.Clover.Sdk.V3.Scanner;

//Dependency Service export of ICardReader implementation
[assembly: Xamarin.Forms.Dependency(typeof(CloverApp.Droid.Scanner))]

namespace CloverApp.Droid
{
    /// <summary>
    /// Implements IScanner interface, which the Xamarin Dependency Service
    /// uses to access the scanner
    /// </summary>
    public class Scanner : IScanner 
    {
        private BarcodeScanner scanner = null;
        private BarcodeReceiver receiver = null;
        private bool _alreadyInitialized;
        public event EventHandler ScannerResponse;
        public string Barcode { get; private set; }

        public void InitializeConnection()
        {
            if (_alreadyInitialized)
                return;
            scanner = new BarcodeScanner(Application.Context);
            receiver = new BarcodeReceiver();
            receiver.ScannerResponse += Receiver_ScannerResponse;
            new Android.Content.ContextWrapper(Application.Context).RegisterReceiver(receiver, new Android.Content.IntentFilter(BarcodeResult.IntentAction));
            _alreadyInitialized = true;
        }

        protected virtual void OnScannerResponseEvent(EventArgs e)
        {
            Barcode = receiver.Barcode;
            ScannerResponse?.Invoke(this, e);
        }

        private void Receiver_ScannerResponse(object sender, EventArgs e)
        {
            OnScannerResponseEvent(e);
        }

        /// <summary>
        /// Prompts use of the scanner
        /// </summary>
        public void OpenScanner()
        {
            Android.OS.Bundle bundle = new Android.OS.Bundle();
            bundle.PutBoolean(Intents.ExtraScanQrCode, true);
            scanner.StartScan(bundle);
        }

        public string GetBarcode()
        {
            return Barcode;
        }

        ~Scanner()
        {
            if (receiver != null)
                new Android.Content.ContextWrapper(Application.Context).UnregisterReceiver(receiver);
        }
    }
}