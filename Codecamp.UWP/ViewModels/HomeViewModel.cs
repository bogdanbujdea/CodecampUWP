using System;
using Windows.UI.Popups;

namespace Codecamp.UWP.ViewModels
{
    public class HomeViewModel
    {
        public async void Click()
        {
            await new MessageDialog("Hello Codecamp").ShowAsync();
        }
    }
}
