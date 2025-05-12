using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using Blackjack.Core.Models;
using Blackjack.MAUI.ViewModels;
using Blackjack.MAUI.Views;

namespace Blackjack.MAUI.Helpers;

/// <summary>
/// Automated basic-strategy player for the MAUI blackjack UI.
/// </summary>
public class AutoPlayHelper
{
    private readonly GamePage _page;
    private readonly GamePageViewModel _vm;
    private readonly Entry _betEntry;
    private readonly Button _startRoundButton;
    private readonly Button _hitButton;
    private readonly Button _standButton;
    private readonly Button _doubleButton;
    private readonly Button _splitButton;

    private const int Delay = 200;        // ms between actions
    private CancellationTokenSource? _cts;

    public bool IsRunning => _cts is { IsCancellationRequested: false };

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
        _vm = viewModel;
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
        Task.Run(() => Loop(_cts.Token));
    }

    public void StopAutoPlay()
    {
        if (!IsRunning) return;
        _cts!.Cancel();
        _cts = null;
    }

    // ------------------------------------------------------------
    // core loop
    // ------------------------------------------------------------
    private async Task Loop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // 1) place a bet and start the round
            if (_vm.IsBetting)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _betEntry.Text = "1";
                    _vm.CurrentHandIndex = 0;          // <-- important reset
                    _startRoundButton.SendClicked();
                    _vm.AutoPlayLastAction = "Auto-Play: bet 1, new round";
                });
                await Task.Delay(Delay, token);
                continue;
            }

            // 2) look at the *current* hand (0 or 1)
            int handIdx = _vm.CurrentHandIndex;
            var hand = _vm._game.GetPlayer().Hands[handIdx];
            int playerTotal = hand.CalculateValue();
            bool isSoft = hand.Cards.Any(c => c.Rank == Rank.Ace);

            var dealerUp = _vm._game.GetDealer().Hands[0].Cards[0];
            int dealerValue = DealerUpValue(dealerUp);

            // 3) split?
            if (!_vm.HasSplitThisRound && _vm.CanSplit && handIdx == 0 &&
                ShouldSplit(hand, dealerValue))
            {
                await ClickAsync(_splitButton, "Split");
                continue;
            }

            // 4) double?
            if (_vm.CanDouble &&
                ((isSoft && ShouldDoubleSoft(playerTotal, dealerValue)) ||
                 (!isSoft && ShouldDoubleHard(playerTotal, dealerValue))))
            {
                await ClickAsync(_doubleButton, isSoft ? "Double (soft)" : "Double (hard)");
                continue;
            }

            // 5) hit?
            if (_vm.CanHit &&
                ((isSoft && ShouldHitSoft(playerTotal, dealerValue)) ||
                 (!isSoft && ShouldHitHard(playerTotal, dealerValue))))
            {
                await ClickAsync(_hitButton, isSoft ? "Hit (soft)" : "Hit (hard)");
                continue;
            }

            // 6) otherwise stand
            await ClickAsync(_standButton, "Stand");
        }
    }

    // ------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------
    private async Task ClickAsync(Button btn, string action)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            btn.SendClicked();
            _vm.AutoPlayLastAction = $"Auto-Play: {action}";
        });
        await Task.Delay(Delay);
    }

    private static int DealerUpValue(Card c) =>
        c.Rank switch
        {
            Rank.Ace => 11,
            Rank.King or Rank.Queen
                or Rank.Jack or Rank.Ten => 10,
            _ => (int)c.Rank
        };

    private static bool ShouldSplit(Hand h, int dealer)
    {
        var r = h.Cards[0].Rank;   // both cards same rank by definition
        return r switch
        {
            Rank.Ace => true,
            Rank.Eight => true,
            Rank.Nine => dealer is < 10 and not 7,
            Rank.Seven => dealer <= 7,
            Rank.Six => dealer <= 6,
            Rank.Four => dealer is 5 or 6,
            Rank.Two or Rank.Three => dealer <= 7,
            _ => false
        };
    }

    private static bool ShouldDoubleHard(int total, int dealer) =>
        (total == 9 && dealer is >= 3 and <= 6) ||
        (total == 10 && dealer <= 9) ||
        (total == 11);

    private static bool ShouldHitHard(int total, int dealer)
    {
        if (total <= 8) return true;
        if (total == 9 && dealer is < 3 or > 6) return true;
        if (total == 10 && dealer >= 10) return true;
        if (total == 11 && dealer == 11) return true;
        if (total == 12 && (dealer < 4 || dealer > 6)) return true;
        if (total is >= 13 and <= 16 && dealer >= 7) return true;
        return false;
    }

    private static bool ShouldDoubleSoft(int total, int dealer) =>
        (total is 13 or 14) && dealer is 5 or 6 ||
        (total is 15 or 16) && dealer is 4 or 5 or 6 ||
        (total == 17) && dealer is >= 3 and <= 6;

    private static bool ShouldHitSoft(int total, int dealer) =>
        total <= 17 ||
        (total == 18 && dealer is 9 or 10 or 11);
}
