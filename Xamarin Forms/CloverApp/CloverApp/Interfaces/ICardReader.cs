using CloverApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloverApp
{
    public interface ICardReader
    {
        event EventHandler ReadCardDataResponse;
        void InitializeConnection();
        void OpenReader();
        string GetTrack1FromCardData();
        string GetTrack2FromCardData();
        string GetMaskedTrack1FromCardData();
        string GetMaskedTrack2FromCardData();
        DriverLicenseData GetDriverLicenseDataFromCardData();
    }
}
