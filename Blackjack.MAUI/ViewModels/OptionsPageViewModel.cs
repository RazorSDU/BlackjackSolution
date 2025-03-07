using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Blackjack.MAUI.ViewModels
{
    public class OptionsPageViewModel
    {
        public ICommand BackCommand { get; }

        public OptionsPageViewModel()
        {
            BackCommand = new Command(OnBack);
        }

        private async void OnBack()
        {
            // Navigate back to the previous page (TitlePage)
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}
