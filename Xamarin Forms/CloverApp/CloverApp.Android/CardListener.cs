using System;
using System.Threading.Tasks;
using Com.Clover.Sdk.V3.Base;
using Com.Clover.Sdk.V3.Connector;
using Com.Clover.Sdk.V3.Remotepay;

[assembly: Xamarin.Forms.Dependency(typeof(CloverApp.Droid.CardListener))]
namespace CloverApp.Droid
{
    /// <summary>
    /// Implementation of Clover's IPaymentConnectorListener, which exposes different 
    /// events that occur when using the card reader
    /// </summary>
    public class CardListener : Java.Lang.Object, IPaymentConnectorListener
    {
        /// <summary>
        /// Storage for data obtained from the card
        /// </summary>
        public CardData ReadCard { get; private set; }

        /// <summary>
        /// EventHandler for .NET that should be raised when the card is read
        /// </summary>
        public event EventHandler ReadCardDataResponse;

        public void OnAuthResponse(AuthResponse p0) { }

        public void OnCapturePreAuthResponse(CapturePreAuthResponse p0) { }

        public void OnCloseoutResponse(CloseoutResponse p0) { }

        public void OnConfirmPaymentRequest(ConfirmPaymentRequest p0) { }

        public void OnDeviceConnected()
        {
            Task.Delay(2000); //for some reason there needs to be a delay instituted in order for the card swipe prompt to appear
        }

        public void OnDeviceDisconnected() { }

        public void OnManualRefundResponse(ManualRefundResponse p0) { }

        public void OnPreAuthResponse(PreAuthResponse p0) { }

        public void OnReadCardDataResponse(ReadCardDataResponse response)
        {
            if (response.HasSuccess)
            {
                ReadCard = response.CardData; //Dump card data to ReadCard property
                OnReadCardDataResponseEvent(new EventArgs()); //raise event to indicate card has been read
            }
        }

        public void OnRefundPaymentResponse(RefundPaymentResponse p0) { }

        public void OnRetrievePaymentResponse(RetrievePaymentResponse p0) { }

        public void OnRetrievePendingPaymentsResponse(RetrievePendingPaymentsResponse p0) { }

        public void OnSaleResponse(SaleResponse p0) { }

        public void OnTipAdded(TipAdded p0) { }

        public void OnTipAdjustAuthResponse(TipAdjustAuthResponse p0) { }

        public void OnVaultCardResponse(VaultCardResponse p0) { }

        public void OnVerifySignatureRequest(VerifySignatureRequest p0) { }

        public void OnVoidPaymentRefundResponse(VoidPaymentRefundResponse p0) { }

        public void OnVoidPaymentResponse(VoidPaymentResponse p0) { }

        protected virtual void OnReadCardDataResponseEvent(EventArgs e)
        {
            ReadCardDataResponse?.Invoke(this, e);
        }
    }
}