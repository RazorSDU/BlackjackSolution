using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Blackjack.MAUI.ViewModels
{
    public class TitlePageViewModel
    {
        public ICommand PlayCommand { get; }
        public ICommand OptionsCommand { get; }
        public ICommand QuitCommand { get; }

        public TitlePageViewModel()
        {
            PlayCommand = new Command(OnPlayClicked);
            OptionsCommand = new Command(OnOptionsClicked);
            QuitCommand = new Command(OnQuitClicked);
        }

        private async void OnPlayClicked()
        {
            // Navigate to GamePage (placeholder version for now)
            await Application.Current.MainPage.Navigation.PushAsync(new Views.GamePage());
        }

        private async void OnOptionsClicked()
        {
            // Navigate to OptionsPage
            await Application.Current.MainPage.Navigation.PushAsync(new Views.OptionsPage());
        }

        private void OnQuitClicked()
        {
            // Quit the application
            // This approach might vary by platform
#if ANDROID || IOS
            // For mobile, might not be recommended to kill the app directly,
            // but we can do:
            System.Diagnostics.Process.GetCurrentProcess().Kill();
#else
            Application.Current.Quit();
#endif
        }
    }
}
