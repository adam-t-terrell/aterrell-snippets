using System;
using System.Collections.Generic;
using System.Text;

namespace CloverApp
{
    /// <summary>
    /// The Xamarin Dependency Service uses this interface to access the scanner
    /// </summary>
    public interface IScanner
    {
        event EventHandler ScannerResponse;
        void InitializeConnection();
        void OpenScanner();
        string GetBarcode();
    }
}
