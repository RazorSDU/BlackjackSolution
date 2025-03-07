using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Blackjack.MAUI.ViewModels
{
    public class EndScreenViewModel
    {
        public string EndMessage { get; set; } = "You Lose or You Win (placeholder)";

        public ICommand ReturnToTitleCommand { get; }

        public EndScreenViewModel()
        {
            ReturnToTitleCommand = new Command(OnReturnToTitle);
        }

        private async void OnReturnToTitle()
        {
            // Clear navigation stack, then go to TitlePage
            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}
