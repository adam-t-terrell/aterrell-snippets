using System;
using Android.App;
using Com.Clover.Connector.Sdk.V3;
using Com.Clover.Sdk.Util;
using Com.Clover.Sdk.V3.Base;
using Com.Clover.Sdk.V3.Remotepay;
using CloverApp.Models;

[assembly: Xamarin.Forms.Dependency(typeof(CloverApp.Droid.CardReader))]

namespace CloverApp.Droid
{
    public class CardReader : ICardReader 
    {
        private CardListener cardListener = null;
        private PaymentConnector paymentConnector = null;
        private bool _alreadyInitialized;

        private CardData cardData;

        public event EventHandler ReadCardDataResponse;

        protected virtual void OnReadCardDataResponseEvent(EventArgs e)
        {
            cardData = cardListener.ReadCard;
            ReadCardDataResponse?.Invoke(this, e);
        }

        /// <summary>
        /// Necessary to start connecting to the card reader
        /// </summary>
        public void InitializeConnection()
        {
            if (_alreadyInitialized)
                return;
            var account = CloverAccount.GetAccount(Application.Context);
            cardListener = new CardListener();
            paymentConnector = new PaymentConnector(Application.Context, account, cardListener);
            paymentConnector.InitializeConnection();
            cardListener.ReadCardDataResponse += OnReadCardDataResponse;
            _alreadyInitialized = true;
        }

        /// <summary>
        /// Prompts use of the card reader
        /// </summary>
        public void OpenReader()
        {
            ReadCardDataRequest r = new ReadCardDataRequest();
            r.SetCardEntryMethods((Java.Lang.Integer)CardEntryMethods.CardEntryMethodMagStripe);
            paymentConnector.ReadCardData(r);
        }

        /// <summary>
        /// Gets raw Track1 string from the CardData 
        /// </summary>
        /// <returns>Raw Track1 data string</returns>
        public string GetTrack1FromCardData()
        {
            if (cardData == null)
                return "";
            return cardData.Track1;
        }

        /// <summary>
        /// Gets raw Track2 string from the CardData 
        /// </summary>
        /// <returns>Raw Track2 data string</returns>
        public string GetTrack2FromCardData()
        {
            if (cardData == null)
                return "";
            return cardData.Track2;
        }

        /// <summary>
        /// Gets "masked" Track1 string from the CardData 
        /// </summary>
        /// <returns>"Masked" Track1 data string</returns>
        public string GetMaskedTrack1FromCardData()
        {
            if (cardData == null)
                return "";
            return cardData.MaskedTrack1;
        }

        /// <summary>
        /// Gets "masked" Track2 string from the CardData 
        /// </summary>
        /// <returns>"Masked" Track2 data string</returns>
        public string GetMaskedTrack2FromCardData()
        {
            if (cardData == null)
                return "";
            return cardData.MaskedTrack2;
        }

        /// <summary>
        /// Gets Track1 and Track2 data from the CardData and puts it into a <c>DriverLicenseData</c> object
        /// </summary>
        /// <returns><c>DriverLicenseData</c></returns>
        public DriverLicenseData GetDriverLicenseDataFromCardData()
        {
            if (cardData == null)
                return new DriverLicenseData();
            return new DriverLicenseData()
            {
                Track1 = cardData.Track1,
                Track2 = cardData.Track2
            };
        }

        private void OnReadCardDataResponse(object sender, EventArgs e)
        {
            OnReadCardDataResponseEvent(e);
        }
    }
}