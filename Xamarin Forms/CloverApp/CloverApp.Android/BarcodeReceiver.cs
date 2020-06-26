using Android.Content;
using Com.Clover.Sdk.V3.Scanner;
using System;

[BroadcastReceiver(Enabled = true)]
public class BarcodeReceiver : BroadcastReceiver
{
    public event EventHandler ScannerResponse;

    public string Barcode { get; private set; }

    protected virtual void OnScannerResponseEvent(EventArgs e)
    {
        ScannerResponse?.Invoke(this, e);
    }

    public override void OnReceive(Context context, Intent intent)
    {
        BarcodeResult barcodeResult = new BarcodeResult(intent);

        if (barcodeResult.IsBarcodeAction)
        {
            this.Barcode = barcodeResult.Barcode;
            OnScannerResponseEvent(new EventArgs());
        }
    }
}