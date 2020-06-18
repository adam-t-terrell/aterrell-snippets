using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CloverApp
{
	public interface IPageService
	{
        Task PopAsync();
        Task PopModalAsync();
        Task PopToRootAsync();
        Task PushAsync(Page page);
        Task PushModalAsync(Page page);
        Task DisplayAlert(string title, string message, string ok);
        Task<bool> DisplayAlert(string title, string message, string ok, string cancel);
        Task RemovePage(Page page);
        bool IsModal(Page page);
    }
}
