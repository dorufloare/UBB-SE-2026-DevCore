using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevCoreHospital.Services
{
    public sealed class DialogService : IDialogService
    {
        private XamlRoot? xamlRoot;

        public void SetXamlRoot(XamlRoot xamlRoot) => this.xamlRoot = xamlRoot;

        public async Task ShowMessageAsync(string title, string message)
        {
            if (xamlRoot == null)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = xamlRoot,
            };

            await dialog.ShowAsync();
        }
    }
}