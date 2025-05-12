using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Blackjack.Core.GameLogic;
using System.Collections.ObjectModel;
using Blackjack.Core.Models;
using Blackjack.MAUI.Helpers;

namespace Blackjack.MAUI.ViewModels
{
    public class GamePageViewModel : INotifyPropertyChanged
    {
        public BlackjackGameAPI _game;

        // Basic fields
        private string _playerName = "Player";
        private int _startingMoney = 100;

        private string _dealerHandDisplay;
        private string _playerHandDisplay;
        private string _statusMessage;

        private bool _canHit;
        private bool _canStand;
        private bool _canDouble;
        private bool _canSplit;

        private string _betAmount = "0";
        private int _playerMoney;

        // ------------------ New fields for UI logic ------------------
        private bool _isBetting = true;       // Bet UI visible initially
        private bool _isDealerTurn = false;   // Show hidden dealer card only on dealer turn
        private string _dealerUpCard = "";    // The 1st (face-up) card
        private string _dealerHiddenCard = ""; // The 2nd card + total, displayed only if IsDealerTurn == true
        private string _currentHandLabel = "Current Hand...";

        private bool _isAutoPlayOn;
        public bool IsAutoPlayOn
        {
            get => _isAutoPlayOn;
            set => SetProperty(ref _isAutoPlayOn, value);
        }

        private string _autoPlayLastAction = "Auto-Play is idle.";
        public string AutoPlayLastAction
        {
            get => _autoPlayLastAction;
            set => SetProperty(ref _autoPlayLastAction, value);
        }

        private int _currentHandIndex = 0;
        public int CurrentHandIndex
        {
            get => _currentHandIndex;
            set
            {
                if (SetProperty(ref _currentHandIndex, value))
                    UpdateHandHighlights();
            }
        }

        private void UpdateHandHighlights()
        {
            Hand1Opacity = CurrentHandIndex == 0 ? 1.0 : 0.4;
            Hand2Opacity = CurrentHandIndex == 1 ? 1.0 : 0.4;
        }

        private double _hand1Opacity = 1.0;
        private double _hand2Opacity = 0.4;

        public double Hand1Opacity
        {
            get => _hand1Opacity;
            set => SetProperty(ref _hand1Opacity, value);
        }

        public double Hand2Opacity
        {
            get => _hand2Opacity;
            set => SetProperty(ref _hand2Opacity, value);
        }


        // Commands
        public ICommand StartRoundCommand { get; }
        public ICommand HitCommand { get; }
        public ICommand StandCommand { get; }
        public ICommand DoubleCommand { get; }
        public ICommand SplitCommand { get; }
        public ICommand ToggleAutoPlayCommand { get; }

        public GamePageViewModel()
        {
            // Initialize the API game instance
            _game = new BlackjackGameAPI(_playerName, _startingMoney);

            // Sync local money property
            PlayerMoney = _game.PlayerMoney;

            // Initialize commands
            StartRoundCommand = new Command(OnStartRound);
            HitCommand = new Command(OnHit, () => CanHit);
            StandCommand = new Command(OnStand, () => CanStand);
            DoubleCommand = new Command(OnDouble, () => CanDouble);
            SplitCommand = new Command(OnSplit, () => CanSplit);

            UpdateUIState("Place a bet and press Start Round.");
        }

        // ------------------ Properties for Binding ------------------
        public string CurrentHandLabel
        {
            get => _currentHandLabel;
            set => SetProperty(ref _currentHandLabel, value);
        }

        public string PlayerName
        {
            get => _playerName;
            set => SetProperty(ref _playerName, value);
        }

        public int PlayerMoney
        {
            get => _playerMoney;
            set => SetProperty(ref _playerMoney, value);
        }

        public string DealerHandDisplay
        {
            get => _dealerHandDisplay;
            set => SetProperty(ref _dealerHandDisplay, value);
        }

        public string PlayerHandDisplay
        {
            get => _playerHandDisplay;
            set => SetProperty(ref _playerHandDisplay, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string BetAmount
        {
            get => _betAmount;
            set => SetProperty(ref _betAmount, value);
        }

        // Button Enables
        public bool CanHit
        {
            get => _canHit;
            set
            {
                if (SetProperty(ref _canHit, value))
                    (HitCommand as Command)?.ChangeCanExecute();
            }
        }

        public bool CanStand
        {
            get => _canStand;
            set
            {
                if (SetProperty(ref _canStand, value))
                    (StandCommand as Command)?.ChangeCanExecute();
            }
        }

        public bool CanDouble
        {
            get => _canDouble;
            set
            {
                if (SetProperty(ref _canDouble, value))
                    (DoubleCommand as Command)?.ChangeCanExecute();
            }
        }

        public bool CanSplit
        {
            get => _canSplit;
            set
            {
                if (SetProperty(ref _canSplit, value))
                    (SplitCommand as Command)?.ChangeCanExecute();
            }
        }

        // ------------------ New UI Control Properties ------------------

        private bool _hasSplitThisRound;
        public bool HasSplitThisRound
        {
            get => _hasSplitThisRound;
            set => SetProperty(ref _hasSplitThisRound, value);
        }

        private bool _isSplitHand1Active;
        public bool IsSplitHand1Active
        {
            get => _isSplitHand1Active;
            set => SetProperty(ref _isSplitHand1Active, value);
        }

        private bool _isSplitHand2Active;
        public bool IsSplitHand2Active
        {
            get => _isSplitHand2Active;
            set => SetProperty(ref _isSplitHand2Active, value);
        }

        private ObservableCollection<string> _splitHand1Images = new();
        public ObservableCollection<string> SplitHand1Images
        {
            get => _splitHand1Images;
            set => SetProperty(ref _splitHand1Images, value);
        }

        private ObservableCollection<string> _splitHand2Images = new();
        public ObservableCollection<string> SplitHand2Images
        {
            get => _splitHand2Images;
            set => SetProperty(ref _splitHand2Images, value);
        }

        public bool IsBetting
        {
            get => _isBetting;
            set => SetProperty(ref _isBetting, value);
        }

        public bool IsDealerTurn
        {
            get => _isDealerTurn;
            set => SetProperty(ref _isDealerTurn, value);
        }

        public string DealerUpCard
        {
            get => _dealerUpCard;
            set => SetProperty(ref _dealerUpCard, value);
        }

        public string DealerHiddenCard
        {
            get => _dealerHiddenCard;
            set => SetProperty(ref _dealerHiddenCard, value);
        }

        // ------------------ Card Image Collections ------------------



        private ObservableCollection<string> _dealerCardImages = new ObservableCollection<string>();
        public ObservableCollection<string> DealerCardImages
        {
            get => _dealerCardImages;
            set => SetProperty(ref _dealerCardImages, value);
        }

        private ObservableCollection<string> _playerCardImages = new ObservableCollection<string>();
        public ObservableCollection<string> PlayerCardImages
        {
            get => _playerCardImages;
            set => SetProperty(ref _playerCardImages, value);
        }

        private ObservableCollection<string> _doubleCardImages = new ObservableCollection<string>();
        public ObservableCollection<string> DoubleCardImages
        {
            get => _doubleCardImages;
            set => SetProperty(ref _doubleCardImages, value);
        }


        // ------------------ Command Implementations ------------------

        private void OnStartRound()
        {
            if (!int.TryParse(BetAmount, out int betValue) || betValue <= 0)
            {
                UpdateUIState("Invalid bet amount.");
                return;
            }

            if (betValue > _game.PlayerMoney)
            {
                UpdateUIState($"Insufficient funds. You only have {_game.PlayerMoney} coins.");
                return;
            }

            // Hide the bet UI now
            IsBetting = false;

            // Reset the Split check
            HasSplitThisRound = false;

            // 1) Start a new round
            _game.StartRound(betValue);

            // 2) Clear old images
            DealerCardImages.Clear();
            PlayerCardImages.Clear();

            SplitHand1Images.Clear();
            SplitHand2Images.Clear();

            // Also turn off their visibility if you're using flags:
            IsSplitHand1Active = false;
            IsSplitHand2Active = false;

            // 3) Read the newly dealt cards from _game
            // By default, StartRound(betValue) deals 2 cards each to player & dealer.

            var dealerHand = _game.GetDealer().Hands[0];
            var playerHand = _game.GetPlayer().Hands[0];

            var dealerFirstCard = dealerHand.Cards[0];
            var dealerSecondCard = dealerHand.Cards[1];

            var playerCard1 = playerHand.Cards[0];
            var playerCard2 = playerHand.Cards[1];

            // 4) Add the dealer's first card with real image, second card as backface
            DealerCardImages.Add(GetCardImagePath(dealerFirstCard));
            DealerCardImages.Add("blue_cardback.WEBP");

            // 5) Add player's first two cards
            PlayerCardImages.Add(GetCardImagePath(playerCard1));
            PlayerCardImages.Add(GetCardImagePath(playerCard2));

            // Reset states
            IsDealerTurn = false;
            DealerUpCard = $"Up Card: {dealerFirstCard}";
            DealerHiddenCard = ""; // not revealed yet

            // Update text-based displays if you still want them
            DealerHandDisplay = DealerUpCard;            // show partial info
            PlayerHandDisplay = _game.DescribePlayerHand(0);

            PlayerMoney = _game.PlayerMoney;

            // Usually player can always hit or stand after the initial deal
            CanHit = true;
            CanStand = true;
            CanDouble = true; // if there's enough money
            CanSplit = _game.CanSplit(0);

            // Set the label to something that indicates we're on Hand #1
            CurrentHandLabel = "Playing Hand #1";
            UpdateUIState("Round started. Good luck!");
        }

        private void OnHit()
        {
            _game.PlayerHit(CurrentHandIndex);

            // We need to add a card image to PlayerCardImages as well
            var playerHand = _game.GetPlayer().Hands[CurrentHandIndex];
            var newCard = playerHand.Cards[^1]; // last card
            if (!HasSplitThisRound)
                PlayerCardImages.Add(GetCardImagePath(newCard));
            else if (CurrentHandIndex == 0)
                SplitHand1Images.Add(GetCardImagePath(newCard));
            else
                SplitHand2Images.Add(GetCardImagePath(newCard));

            // Refresh UI
            PlayerHandDisplay = _game.DescribePlayerHand(CurrentHandIndex);
            PlayerMoney = _game.PlayerMoney;

            if (_game.IsPlayerBust(CurrentHandIndex))
            {
                UpdateUIState("You busted!");
                RevealDealerHandAndFinish();
            }
            else
            {
                UpdateUIState("You drew a card. Hit or stand?");
            }
        }

        private void OnStand()
        {
            if (HasSplitThisRound && CurrentHandIndex == 0)
            {
                // move to second hand
                CurrentHandIndex = 1;
                CurrentHandLabel = "Playing Hand #2";

                CanHit = true;
                CanStand = true;
                CanDouble = true;
                CanSplit = false;     // no re-split in this simple version

                UpdateUIState("Hand #2: Hit, Stand or Double.");
            }
            else
            {
                // either single hand, or just finished hand #2
                _game.DealerTurn();
                RevealDealerHandAndFinish();
            }
        }

        private void OnDouble()
        {
            bool success = _game.PlayerDouble(0);
            if (!success)
            {
                UpdateUIState("Not enough money to double.");
                return;
            }

            // The player gets exactly one more card
            var playerHand = _game.GetPlayer().Hands[0];
            var newCard = playerHand.Cards[^1];
            PlayerCardImages.Add(GetCardImagePath(newCard));

            PlayerHandDisplay = _game.DescribePlayerHand(0);
            PlayerMoney = _game.PlayerMoney;

            if (_game.IsPlayerBust(0))
            {
                UpdateUIState("Busted after doubling!");
                RevealDealerHandAndFinish();
            }
            else
            {
                _game.DealerTurn();
                RevealDealerHandAndFinish("Double done.");
            }
        }

        private void OnSplit()
        {
            if (_hasSplitThisRound)
            {
                UpdateUIState("You already split once this round.");
                return;
            }

            if (!_game.CanSplit(0))
            {
                UpdateUIState("Cannot split right now.");
                return;
            }

            _game.SplitHand(0);
            HasSplitThisRound = true;
            CurrentHandIndex = 0;          // start on first split hand
            UpdateHandHighlights();

            PlayerCardImages.Clear();
            SplitHand1Images.Clear();
            SplitHand2Images.Clear();

            foreach (var card in _game.GetPlayer().Hands[0].Cards)
                SplitHand1Images.Add(GetCardImagePath(card));

            foreach (var card in _game.GetPlayer().Hands[1].Cards)
                SplitHand2Images.Add(GetCardImagePath(card));

            IsSplitHand1Active = true;
            IsSplitHand2Active = true;

            PlayerMoney = _game.PlayerMoney;
            PlayerHandDisplay = _game.DescribePlayerHand(0) + " | " +
                                 _game.DescribePlayerHand(1);

            CurrentHandLabel = "Playing Hand #1";
            CanSplit = false;     // prevent second split
            UpdateUIState("You split your hand. Now play Hand #1.");
        }



        // ------------------ Helpers ------------------

        private string GetCardImagePath(Card c)
        {
            // suit: hearts=1, clubs=2, diamonds=3, spades=4
            // rank: 2=1, 3=2, ... 10=9, J=10, Q=11, K=12, A=13
            int suitNumber = c.Suit switch
            {
                Suit.Hearts => 1,
                Suit.Clubs => 2,
                Suit.Diamonds => 3,
                Suit.Spades => 4,
                _ => 1
            };

            int rankNumber = c.Rank switch
            {
                Rank.Two => 1,
                Rank.Three => 2,
                Rank.Four => 3,
                Rank.Five => 4,
                Rank.Six => 5,
                Rank.Seven => 6,
                Rank.Eight => 7,
                Rank.Nine => 8,
                Rank.Ten => 9,
                Rank.Jack => 10,
                Rank.Queen => 11,
                Rank.King => 12,
                Rank.Ace => 13,
                _ => 1
            };

            return $"row_{suitNumber}_column_{rankNumber}.PNG";
        }

        /// <summary>
        /// Reveal the dealer's hidden card, run outcome, and end round.
        /// </summary>
        private void RevealDealerHandAndFinish(string extraMessage = "")
        {
            IsDealerTurn = true;

            // Clear all dealer images (we're about to refresh them)
            DealerCardImages.Clear();

            // Rebuild the full list of images for all cards the dealer holds
            var dealerHand = _game.GetDealer().Hands[0];
            foreach (var card in dealerHand.Cards)
            {
                DealerCardImages.Add(GetCardImagePath(card));
            }

            // Update the text display if you still want that
            DealerHandDisplay = _game.DescribeDealerHand();

            // Determine outcome and update the UI
            bool playerHadBlackjack = false; // (This could be tracked per hand if you support multiple hands)
            string outcome = _game.DetermineOutcome(playerHadBlackjack);

            PlayerMoney = _game.PlayerMoney;
            UpdateUIState($"{extraMessage}\n{outcome}");

            EndRoundActions();
        }


        private void EndRoundActions()
        {
            CanHit = false;
            CanStand = false;
            CanDouble = false;
            CanSplit = false;
            IsBetting = true;

            // (We don't reset HasSplitThisRound here, because a new round 
            // will call OnStartRound() and do that.)
        }


        private void UpdateUIState(string message)
        {
            StatusMessage = message;
        }

        // ------------------ INotifyPropertyChanged ------------------

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }
    }
}
