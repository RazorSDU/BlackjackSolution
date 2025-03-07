using Microsoft.Maui.Controls;

namespace Blackjack.MAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Start the app on the TitlePage, wrapped in a NavigationPage
            MainPage = new NavigationPage(new Views.TitlePage());
        }
    }
}
