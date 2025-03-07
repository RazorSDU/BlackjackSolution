using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using Blackjack.Core.Models;
using Blackjack.MAUI.ViewModels;
using Blackjack.MAUI.Views;

namespace Blackjack.MAUI.Helpers
{
    public class AutoPlayHelper
    {
        private readonly GamePage _page;
        private readonly Entry _betEntry;
        private readonly Button _startRoundButton;
        private readonly Button _hitButton;
        private readonly Button _standButton;
        private readonly Button _doubleButton;
        private readonly Button _splitButton;
        private readonly GamePageViewModel _viewModel;

        private CancellationTokenSource _cts;

        public bool IsRunning => _cts != null && !_cts.IsCancellationRequested;

        public AutoPlayHelper(
            GamePage page,
            GamePageViewModel viewModel,
            Entry betEntry,
            Button startRoundButton,
            Button hitButton,
            Button standButton,
            Button doubleButton,
            Button splitButton)
        {
            _page = page;
            _viewModel = viewModel;
            _betEntry = betEntry;
            _startRoundButton = startRoundButton;
            _hitButton = hitButton;
            _standButton = standButton;
            _doubleButton = doubleButton;
            _splitButton = splitButton;
        }

        public void StartAutoPlay()
        {
            if (IsRunning) return;
            _cts = new CancellationTokenSource();
            Task.Run(() => AutoPlayLoop(_cts.Token));
        }

        public void StopAutoPlay()
        {
            if (!IsRunning) return;
            _cts.Cancel();
            _cts = null;
        }

        private async Task AutoPlayLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var vm = (GamePageViewModel)_page.BindingContext;
                if (vm == null) break;

                if (vm.IsBetting)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _betEntry.Text = "1";
                        _startRoundButton.SendClicked();
                        _viewModel.AutoPlayLastAction = "Auto-Play: Bet 1 and started round.";
                    });

                    await Task.Delay(TimeSpan.FromSeconds(4), token);  // Full 5 seconds even for betting
                    continue;
                }

                int playerTotal = vm._game.GetPlayer().Hands[0].CalculateValue();
                bool dealerUpCard6OrUnder = DealerUpCardIsSixOrUnder(vm);

                if (!vm.HasSplitThisRound && vm.CanSplit)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _splitButton.SendClicked();
                        _viewModel.AutoPlayLastAction = "Auto-Play: Split Hand";
                    });
                }
                else if (playerTotal <= 11 && dealerUpCard6OrUnder && vm.CanDouble)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _doubleButton.SendClicked();
                        _viewModel.AutoPlayLastAction = "Auto-Play: Double";
                    });
                }
                else if (playerTotal <= 13 && vm.CanHit)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _hitButton.SendClicked();
                        _viewModel.AutoPlayLastAction = "Auto-Play: Hit";
                    });
                }
                else if (vm.CanStand)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _standButton.SendClicked();
                        _viewModel.AutoPlayLastAction = "Auto-Play: Stand";
                    });
                }

                // Unified 5-second delay for every action, including split/double/hit/stand
                await Task.Delay(TimeSpan.FromSeconds(4), token);
            }
        }

        private bool DealerUpCardIsSixOrUnder(GamePageViewModel vm)
        {
            var upCard = vm._game.GetDealer().Hands[0].Cards[0];
            return upCard.Rank == Rank.Two
                || upCard.Rank == Rank.Three
                || upCard.Rank == Rank.Four
                || upCard.Rank == Rank.Five
                || upCard.Rank == Rank.Six;
        }
    }
}
