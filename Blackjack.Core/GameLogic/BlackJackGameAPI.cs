using System;
using Blackjack.Core.Models;

namespace Blackjack.Core.GameLogic
{
    /// <summary>
    /// A UI-friendly version of the Blackjack game, 
    /// exposing public methods for step-by-step interaction 
    /// (instead of console I/O).
    /// </summary>
    public class BlackjackGameAPI
    {
        private Deck _deck;
        private Player _player;
        private Player _dealer;

        private bool _keepPlaying = true; // If you want a multi-round approach

        public BlackjackGameAPI(string playerName, int startingMoney)
        {
            // Initialize & shuffle deck right away (or do it each round, up to you)
            _deck = new Deck();
            _deck.Shuffle();

            _player = new Player(playerName, startingMoney);
            _dealer = new Player("Dealer");
        }

        // ---------------------------------------
        // Public Properties for Quick Access
        // ---------------------------------------

        /// <summary>
        /// Whether the game can continue another round 
        /// (player hasn't quit or gone to 0 money).
        /// </summary>
        public bool KeepPlaying => _keepPlaying;

        /// <summary>
        /// The player's current money balance.
        /// </summary>
        public int PlayerMoney => _player.Money;

        /// <summary>
        /// The player's name.
        /// </summary>
        public string PlayerName => _player.Name;

        /// <summary>
        /// Insurance bet placed by the player, if any.
        /// </summary>
        public int InsuranceBet => _player.InsuranceBet;

        // You can also expose references to the Player / Dealer if needed:
        public Player GetPlayer() => _player;
        public Player GetDealer() => _dealer;

        // ---------------------------------------
        // Public: Start & Manage a Round
        // ---------------------------------------

        /// <summary>
        /// Start a new round, placing the bet for the player's first hand.
        /// Resets deck, shuffles, and deals initial cards to player & dealer.
        /// </summary>
        public void StartRound(int betAmount)
        {
            // Recreate and shuffle deck each round
            _deck = new Deck();
            _deck.Shuffle();

            // Reset hands
            _player.ResetHand();
            _dealer.ResetHand();

            // Place bet on first hand
            _player.PlaceBet(betAmount, 0);

            // Initial deal (2 cards each)
            _player.Hit(_deck.DealCard(), 0);
            _player.Hit(_deck.DealCard(), 0);

            _dealer.Hit(_deck.DealCard());
            _dealer.Hit(_deck.DealCard());
        }

        /// <summary>
        /// Check if the dealer is showing an Ace as the up-card.
        /// </summary>
        public bool IsDealerAceUp()
        {
            // The dealer's up-card is the first card in the dealer's hand
            var upCard = _dealer.Hands[0].Cards[0];
            return upCard.Rank == Rank.Ace;
        }

        /// <summary>
        /// Offer insurance to the player if the dealer shows an Ace.
        /// The UI decides whether to actually call this or not.
        /// </summary>
        public bool PlaceInsurance()
        {
            // Returns true if insurance was successfully placed, false otherwise
            try
            {
                _player.PlaceInsurance();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Peek the dealer's hidden card to check if it's 10/J/Q/K => Blackjack.
        /// </summary>
        public bool CheckDealerHiddenCardForBlackjack()
        {
            var hiddenCard = _dealer.Hands[0].Cards[1];
            bool isTenValued = hiddenCard.Rank == Rank.Ten
                            || hiddenCard.Rank == Rank.Jack
                            || hiddenCard.Rank == Rank.Queen
                            || hiddenCard.Rank == Rank.King;
            return isTenValued;
        }

        /// <summary>
        /// Resolves the scenario where the dealer actually has Blackjack.
        /// Returns a message describing outcome for UI to display.
        /// </summary>
        public string ResolveDealerBlackjack()
        {
            // If the dealer has Blackjack, the hidden card is shown
            // Process insurance bets, etc.

            string result = "Dealer has BLACKJACK!\n";

            // If player had insurance
            if (_player.InsuranceBet > 0)
            {
                _player.WinInsurance();
                result += "Insurance bet pays off!\n";
            }
            else
            {
                result += "No insurance. Player cannot recover bet.\n";
            }

            // Process each hand individually
            for (int handIndex = 0; handIndex < _player.Hands.Count; handIndex++)
            {
                var hand = _player.Hands[handIndex];
                int handValue = hand.CalculateValue();

                bool isOriginalBlackjack = (handIndex == 0 && hand.Cards.Count == 2 && handValue == 21);

                if (isOriginalBlackjack)
                {
                    // Typically, two blackjacks => push
                    _player.PushBet(handIndex);
                    result += $"Hand {handIndex + 1}: Player also has Blackjack => push.\n";
                }
                else
                {
                    _player.LoseBet(handIndex);
                    result += $"Hand {handIndex + 1} loses to dealer's Blackjack.\n";
                }
            }

            return result;
        }

        // ---------------------------------------
        // Public: Player Actions
        // ---------------------------------------

        /// <summary>
        /// Player hits on a specific hand index (draws one card).
        /// </summary>
        public void PlayerHit(int handIndex = 0)
        {
            var newCard = _deck.DealCard();
            _player.Hit(newCard, handIndex);
        }

        /// <summary>
        /// Checks if the player's specified hand is bust (>21).
        /// </summary>
        public bool IsPlayerBust(int handIndex = 0)
        {
            return _player.IsBust(handIndex);
        }

        /// <summary>
        /// Attempt to double the bet for the given hand (if enough money).
        /// Then automatically deal one card to that hand.
        /// </summary>
        /// <returns>
        /// True if successfully doubled, false if not enough money.
        /// </returns>
        public bool PlayerDouble(int handIndex = 0)
        {
            int currentBet = _player.GetBet(handIndex);
            if (_player.Money >= currentBet)
            {
                // Deduct from player's money
                _player.Money -= currentBet;
                // Actually double the bet
                _player.DoubleBet(handIndex);

                // Deal one card
                var newCard = _deck.DealCard();
                _player.Hit(newCard, handIndex);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the player can split the specified hand.
        /// </summary>
        public bool CanSplit(int handIndex = 0)
        {
            return _player.CanSplit(handIndex);
        }

        /// <summary>
        /// Splits the player's hand at handIndex into two, 
        /// deals one additional card to each new hand.
        /// </summary>
        public void SplitHand(int handIndex = 0)
        {
            _player.Split(handIndex);
            // After splitting, each sub-hand has 1 card,
            // so give each sub-hand one more card to form 2-card hands
            _player.Hit(_deck.DealCard(), handIndex);
            _player.Hit(_deck.DealCard(), _player.Hands.Count - 1);
        }

        // ---------------------------------------
        // Public: Dealer Actions
        // ---------------------------------------

        /// <summary>
        /// Dealer draws until at least 17 or busts.
        /// </summary>
        public void DealerTurn()
        {
            while (_dealer.GetHandValue(0) < 17)
            {
                var newCard = _deck.DealCard();
                _dealer.Hit(newCard);
                if (_dealer.IsBust())
                {
                    // Dealer busts => stop
                    break;
                }
            }
        }

        /// <summary>
        /// Checks if the dealer is bust.
        /// </summary>
        public bool IsDealerBust()
        {
            return _dealer.IsBust();
        }

        // ---------------------------------------
        // Public: Determine Round Outcome
        // ---------------------------------------

        /// <summary>
        /// Compare each player's hand vs. dealer, 
        /// handle bets, and return a summary string.
        /// If player had a natural Blackjack, pass playerHadBlackjack = true for special payout.
        /// </summary>
        public string DetermineOutcome(bool playerHadBlackjack)
        {
            var result = "\n--- Round Results ---\n";

            int dealerValue = _dealer.GetHandValue(0);
            bool dealerBust = _dealer.IsBust(0);

            for (int handIndex = 0; handIndex < _player.Hands.Count; handIndex++)
            {
                int playerValue = _player.GetHandValue(handIndex);
                int bet = _player.GetBet(handIndex);
                if (bet <= 0) continue; // Already resolved earlier
                var handStr = DescribeHand(_player, handIndex);

                result += $"\nHand {handIndex + 1}: {handStr} (Bet: {bet})\n";
                result += $"Dealer's total: {dealerValue}\n";

                if (_player.IsBust(handIndex))
                {
                    _player.LoseBet(handIndex);
                    result += $"Hand {handIndex + 1} busted => you lose.\n";
                }
                else if (dealerBust)
                {
                    _player.WinBet(handIndex);
                    result += "Dealer busted => you win!\n";
                }
                else
                {
                    // Compare totals
                    if (playerValue > dealerValue)
                    {
                        bool isOriginalFirstHand = (handIndex == 0 && _player.Hands[handIndex].Cards.Count == 2);
                        bool splitted = (_player.Hands.Count > 1);

                        // If it was a natural blackjack on the first hand
                        if (isOriginalFirstHand && playerHadBlackjack && !splitted)
                        {
                            _player.WinBlackjack(handIndex);
                            result += "Blackjack on first hand => 2.5x payout!\n";
                        }
                        else if (_player.Hands[handIndex].Cards.Count == 2 && playerValue == 21 && splitted)
                        {
                            // 21 from a split is usually a normal 2x
                            _player.Win21(handIndex);
                            result += "21 from a split => normal 2x payout.\n";
                        }
                        else
                        {
                            _player.WinBet(handIndex);
                            result += "You win!\n";
                        }
                    }
                    else if (playerValue < dealerValue)
                    {
                        _player.LoseBet(handIndex);
                        result += "Dealer wins this hand.\n";
                    }
                    else
                    {
                        _player.PushBet(handIndex);
                        result += "Push (tie). Bet returned.\n";
                    }
                }
            }

            result += $"\nYou now have {_player.Money} coins.\n";
            return result;
        }

        // ---------------------------------------
        // Public: Describe Hands (for UI)
        // ---------------------------------------

        /// <summary>
        /// Returns a string describing all cards in the dealer's first hand.
        /// (Optionally hide second card if you want partial info.)
        /// </summary>
        public string DescribeDealerHand()
        {
            return DescribeHand(_dealer, 0);
        }

        /// <summary>
        /// Returns a string describing the player's first hand (or specified index).
        /// </summary>
        public string DescribePlayerHand(int handIndex = 0)
        {
            return DescribeHand(_player, handIndex);
        }

        // If you want a partial display (show only the first card for the dealer), 
        // you can create another helper method.

        // ---------------------------------------
        // Public: Multi-round Approach
        // ---------------------------------------

        /// <summary>
        /// If you want to mimic multi-round logic: 
        /// after each round, the UI calls this to see if 
        /// the player wants to continue or has money > 0.
        /// If the user wants to exit or money <= 0, set _keepPlaying = false.
        /// </summary>
        public void EndRoundCheck(bool userChoseExit)
        {
            if (userChoseExit)
            {
                _keepPlaying = false;
                return;
            }

            if (_player.Money <= 0)
            {
                _keepPlaying = false;
            }
        }

        // ---------------------------------------
        // Private/Helper
        // ---------------------------------------

        private string DescribeHand(Player p, int handIndex = 0)
        {
            var hand = p.Hands[handIndex];
            string cardsJoined = string.Join(", ", hand.Cards);
            return $"{cardsJoined} (Value: {hand.CalculateValue()})";
        }
    }
}
