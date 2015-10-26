namespace Codecamp.UWP.Views
{
    using ViewModels;

    public sealed partial class HomeView
    {
        public HomeView()
        {
            InitializeComponent();
        }

        public HomeViewModel ViewModel => DataContext as HomeViewModel;
    }
}