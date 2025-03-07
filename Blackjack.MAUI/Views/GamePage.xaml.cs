using Blackjack.MAUI.ViewModels;
using Blackjack.MAUI.Helpers;

namespace Blackjack.MAUI.Views;

public partial class GamePage : ContentPage
{
    private AutoPlayHelper _autoPlayHelper;

    public GamePage()
    {
        InitializeComponent();

        // Set up ViewModel if not done in XAML
        if (BindingContext is not GamePageViewModel viewModel)
        {
            viewModel = new GamePageViewModel();
            BindingContext = viewModel;
        }

        // Initialize AutoPlayHelper — with all UI elements
        _autoPlayHelper = new AutoPlayHelper(
            page: this,
            viewModel: (GamePageViewModel)this.BindingContext,  // Pass the current ViewModel here
            betEntry: BetEntry,
            startRoundButton: StartRoundButton,
            hitButton: HitButton,
            standButton: StandButton,
            doubleButton: DoubleButton,
            splitButton: SplitButton
        );

    }

    private void OnToggleAutoPlayClicked(object sender, EventArgs e)
    {
        if (_autoPlayHelper.IsRunning)
        {
            _autoPlayHelper.StopAutoPlay();
            ((GamePageViewModel)BindingContext).IsAutoPlayOn = false;
            ((GamePageViewModel)BindingContext).AutoPlayLastAction = "Auto-Play stopped.";
        }
        else
        {
            _autoPlayHelper.StartAutoPlay();
            ((GamePageViewModel)BindingContext).IsAutoPlayOn = true;
            ((GamePageViewModel)BindingContext).AutoPlayLastAction = "Auto-Play started.";
        }
    }

}
