using System;
using System.Linq;
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
        private readonly GamePageViewModel _viewModel;
        private readonly Entry _betEntry;
        private readonly Button _startRoundButton;
        private readonly Button _hitButton;
        private readonly Button _standButton;
        private readonly Button _doubleButton;
        private readonly Button _splitButton;
        private readonly int Delay = 1;

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
                    await Task.Delay(TimeSpan.FromMilliseconds(Delay), token);
                    continue;
                }

                var hand = vm._game.GetPlayer().Hands[0];
                int playerTotal = hand.CalculateValue();
                bool isSoft = IsSoftHand(hand);
                var dealerUpCard = vm._game.GetDealer().Hands[0].Cards[0];
                int dealerRank = (int)dealerUpCard.Rank;

                if (!vm.HasSplitThisRound && vm.CanSplit && ShouldSplit(hand, dealerRank))
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _splitButton.SendClicked();
                        _viewModel.AutoPlayLastAction = "Auto-Play: Split";
                    });
                    await Task.Delay(TimeSpan.FromMilliseconds(Delay), token);
                    continue;
                }

                if (isSoft)
                {
                    if (ShouldDoubleSoft(playerTotal, dealerRank) && vm.CanDouble)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            _doubleButton.SendClicked();
                            _viewModel.AutoPlayLastAction = "Auto-Play: Double (Soft)";
                        });
                        await Task.Delay(TimeSpan.FromMilliseconds(Delay), token);
                        continue;
                    }
                    if (ShouldHitSoft(playerTotal, dealerRank) && vm.CanHit)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            _hitButton.SendClicked();
                            _viewModel.AutoPlayLastAction = "Auto-Play: Hit (Soft)";
                        });
                        await Task.Delay(TimeSpan.FromMilliseconds(Delay), token);
                        continue;
                    }
                }
                else
                {
                    if (ShouldDoubleHard(playerTotal, dealerRank) && vm.CanDouble)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            _doubleButton.SendClicked();
                            _viewModel.AutoPlayLastAction = "Auto-Play: Double (Hard)";
                        });
                        await Task.Delay(TimeSpan.FromMilliseconds(Delay), token);
                        continue;
                    }
                    if (ShouldHitHard(playerTotal, dealerRank) && vm.CanHit)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            _hitButton.SendClicked();
                            _viewModel.AutoPlayLastAction = "Auto-Play: Hit (Hard)";
                        });
                        await Task.Delay(TimeSpan.FromMilliseconds(Delay), token);
                        continue;
                    }
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _standButton.SendClicked();
                    _viewModel.AutoPlayLastAction = "Auto-Play: Stand";
                });

                await Task.Delay(TimeSpan.FromMilliseconds(Delay), token);
            }
        }

        private bool IsSoftHand(Hand hand)
        {
            return hand.Cards.Any(c => c.Rank == Rank.Ace);
        }

        private bool ShouldSplit(Hand hand, int dealerRank)
        {
            var card1 = hand.Cards[0];
            var card2 = hand.Cards[1];
            if (card1.Rank != card2.Rank) return false;

            switch (card1.Rank)
            {
                case Rank.Ace: return true;
                case Rank.Ten: return false;
                case Rank.Nine: return dealerRank != 7 && dealerRank < 10;
                case Rank.Eight: return true;
                case Rank.Seven: return dealerRank <= 7;
                case Rank.Six: return dealerRank <= 6;
                case Rank.Five: return false;
                case Rank.Four: return dealerRank == 5 || dealerRank == 6;
                case Rank.Three:
                case Rank.Two: return dealerRank <= 7;
                default: return false;
            }
        }

        private bool ShouldDoubleHard(int total, int dealerRank)
        {
            return (total == 9 && dealerRank >= 3 && dealerRank <= 6) ||
                   (total == 10 && dealerRank <= 9) ||
                   (total == 11);
        }

        private bool ShouldHitHard(int total, int dealerRank)
        {
            if (total <= 8) return true;
            if (total == 9 && !(dealerRank >= 3 && dealerRank <= 6)) return true;
            if (total == 10 && dealerRank >= 10) return true;
            if (total == 11 && dealerRank == 11) return true;
            if (total == 12) return dealerRank < 4 || dealerRank > 6;
            if (total >= 13 && total <= 16) return dealerRank >= 7;
            return false;
        }

        private bool ShouldDoubleSoft(int total, int dealerRank)
        {
            if (total == 13 || total == 14) return dealerRank >= 5 && dealerRank <= 6;
            if (total == 15 || total == 16) return dealerRank >= 4 && dealerRank <= 6;
            if (total == 17) return dealerRank >= 3 && dealerRank <= 6;
            return false;
        }

        private bool ShouldHitSoft(int total, int dealerRank)
        {
            if (total <= 17) return true;
            if (total == 18 && (dealerRank == 9 || dealerRank == 10 || dealerRank == 11)) return true;
            return false;
        }
    }
}
