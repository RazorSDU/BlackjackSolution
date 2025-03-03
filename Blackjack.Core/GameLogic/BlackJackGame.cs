using System;
using Blackjack.Core.Models;

namespace Blackjack.Core.GameLogic
{
    public class BlackjackGame
    {
        private Deck _deck;
        private Player _player;
        private Player _dealer;
        private bool _keepPlaying = true;

        public BlackjackGame(string playerName, int startingMoney)
        {
            _deck = new Deck();
            _deck.Shuffle();

            _player = new Player(playerName, startingMoney);
            _dealer = new Player("Dealer");
        }

        /// <summary>
        /// Main loop for multiple rounds until the player is out of money or chooses to exit.
        /// </summary>
        public void RunMultipleRounds()
        {
            while (_keepPlaying && _player.Money > 0)
            {
                Console.WriteLine($"\nYou have {_player.Money} coins.");

                // Ask player if they want to place a bet or exit
                if (!PromptForBetOrExit())
                {
                    break; // Player chose to exit
                }

                // Check for possible insurance scenario if Dealer's up-card is an Ace
                bool dealerHasBlackjack = false;
                if (IsDealerAceUp())
                {
                    // Offer insurance
                    OfferInsurance();

                    // Check dealer's hidden card
                    dealerHasBlackjack = CheckDealerHiddenCardForBlackjack();
                    if (dealerHasBlackjack)
                    {
                        // Dealer has blackjack
                        ResolveDealerBlackjack();
                        // Round ends here, proceed to next round
                        AskToContinue();
                        continue;
                    }
                    else
                    {
                        // Dealer does not have blackjack, player loses insurance if bought
                        if (_player.InsuranceBet > 0)
                        {
                            Console.WriteLine("Dealer does NOT have blackjack. You lose your insurance bet.");
                            _player.LoseInsurance();
                        }
                    }
                }

                // If player has blackjack, skip their turn
                bool playerHasBlackjack = (_player.GetHandValue() == 21 && _player.Hands[0].Cards.Count == 2);
                if (playerHasBlackjack)
                {
                    Console.WriteLine($"{_player.Name} has Blackjack! You automatically stand.");
                }
                else
                {
                    // Proceed with normal player turn
                    PlayerTurn();
                }

                // Dealer turn (only if player hasn't busted, or didn't have an immediate blackjack that ends it)
                if (!_player.IsBust() && !playerHasBlackjack)
                {
                    DealerTurn();
                }

                // Determine outcome if not already decided
                DetermineOutcome(playerHasBlackjack);

                // Ask if they want to continue
                AskToContinue();
            }

            Console.WriteLine("Thank you for playing!");
        }

        /// <summary>
        /// Prompts the player to place a bet or press X to exit. Returns false if exiting.
        /// </summary>
        private bool PromptForBetOrExit()
        {
            while (true)
            {
                Console.Write($"Enter your bet amount, or press X to leave the table: ");
                string input = Console.ReadLine()?.Trim().ToLower();

                if (input == "x")
                {
                    _keepPlaying = false;
                    return false;
                }

                if (int.TryParse(input, out int betAmount))
                {
                    // We'll pass betAmount to StartRound(betAmount)
                    // but do NOT place the bet here. We'll do it in StartRound. 
                    StartRound(betAmount);
                    return true;
                }
                else
                {
                    Console.WriteLine("Please enter a valid number or X.");
                }
            }
        }


        /// <summary>
        /// Resets the deck, deals two cards to player and dealer.
        /// </summary>
        private void StartRound(int betAmount)
        {
            // We assume you've already placed the bet in PromptForBetOrExit.
            // Just rewriting: 
            _deck = new Deck();
            _deck.Shuffle();

            _player.ResetHand();
            _dealer.ResetHand();

            // Place the bet on the first hand
            _player.PlaceBet(betAmount, 0);

            // Initial deal
            _player.Hit(_deck.DealCard(), 0);
            _player.Hit(_deck.DealCard(), 0);

            _dealer.Hit(_deck.DealCard(), 0);
            _dealer.Hit(_deck.DealCard(), 0);

            DisplayInitialHands();

            // Right after the initial deal, check if the player can split
            if (_player.CanSplit(0))
            {
                Console.WriteLine("You have a pair! Would you like to split? (Y/N)");
                string splitInput = Console.ReadLine()?.Trim().ToLower();
                if (splitInput == "y")
                {
                    _player.Split(0);

                    // After splitting, each hand now has 1 card, so we deal one more card to each split hand
                    // per normal Blackjack rules: each new hand must have 2 starting cards
                    _player.Hit(_deck.DealCard(), 0); // second card for first hand
                    _player.Hit(_deck.DealCard(), 1); // second card for second hand

                    Console.WriteLine("\n--- After splitting ---");
                    for (int i = 0; i < _player.Hands.Count; i++)
                    {
                        Console.WriteLine($"Hand {i + 1}: {DescribeHand(_player, i)} (Bet: {_player.GetBet(i)})");
                    }
                }
            }
        }

        private void DisplayInitialHands()
        {
            Console.WriteLine($"\n--- Round Start ---");
            Console.WriteLine($"{_player.Name}'s first hand: {DescribeHand(_player, 0)}");
            var dealerFirstCard = _dealer.Hands[0].Cards[0];
            Console.WriteLine($"Dealer shows: {dealerFirstCard}, and a hidden card.");
        }

        private bool IsDealerAceUp()
        {
            // Dealer's up-card is dealer.Hands[0].Cards[0] in this example
            var upCard = _dealer.Hands[0].Cards[0];
            return (upCard.Rank == Rank.Ace);
        }

        /// <summary>
        /// Offer insurance if dealer shows an Ace. Insurance bet = half of current main bet, if the player can afford it.
        /// </summary>
        private void OfferInsurance()
        {
            Console.WriteLine("\nDealer is showing an Ace. Do you want to buy insurance? (Y/N)");
            string input = Console.ReadLine()?.Trim().ToLower();
            if (input == "y")
            {
                try
                {
                    _player.PlaceInsurance();
                    Console.WriteLine($"Insurance placed for {_player.InsuranceBet} coins.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot place insurance: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("You chose not to buy insurance.");
            }
        }

        /// <summary>
        /// Peek dealer's hidden card. If it's a 10/J/Q/K, that's a dealer Blackjack.
        /// </summary>
        private bool CheckDealerHiddenCardForBlackjack()
        {
            var hiddenCard = _dealer.Hands[0].Cards[1];
            // If hidden card is 10, Jack, Queen, or King, that equates to 10 in value
            // (We've assigned numeric ranks for J=11, Q=12, K=13 in your earlier code, but all are worth 10 in Blackjack.)
            bool isTenValued = hiddenCard.Rank == Rank.Ten
                            || hiddenCard.Rank == Rank.Jack
                            || hiddenCard.Rank == Rank.Queen
                            || hiddenCard.Rank == Rank.King;
            return isTenValued;
        }

        /// <summary>
        /// If the dealer does have blackjack, reveal it and resolve insurance bets.
        /// </summary>
        private void ResolveDealerBlackjack()
        {
            Console.WriteLine("\nDealer has BLACKJACK! The hidden card is shown:");
            Console.WriteLine($"Dealer's hand: {DescribeHand(_dealer)}");

            if (_player.InsuranceBet > 0)
            {
                // Player who bought insurance is refunded 2x the insurance bet
                Console.WriteLine("Your insurance bet pays off!");
                _player.WinInsurance();
            }
            else
            {
                Console.WriteLine("No insurance, so you cannot recover your bet.");
            }

            // Process each hand's bet (accounting for potential split hands)
            for (int handIndex = 0; handIndex < _player.Hands.Count; handIndex++)
            {
                var hand = _player.Hands[handIndex];
                int handValue = hand.CalculateValue();

                Console.WriteLine($"\n{_player.Name}'s Hand {handIndex + 1}: {DescribeHand(_player, handIndex)}");

                // Check if this is the original first hand and has blackjack (2 cards = natural blackjack)
                bool isOriginalBlackjack = (handIndex == 0 && hand.Cards.Count == 2 && handValue == 21);

                if (isOriginalBlackjack)
                {
                    // Standard blackjack rules say this should be a push if dealer also has blackjack.
                    Console.WriteLine("You have a natural Blackjack, but dealer also has Blackjack.");
                    Console.WriteLine("This is usually a push, but adjusting per your house rules...");

                    _player.PushBet(handIndex);
                }
                else
                {
                    // All other hands (including split hands) automatically lose.
                    Console.WriteLine($"Hand {handIndex + 1} loses to dealer's Blackjack.");
                    _player.LoseBet(handIndex);
                }
            }
        }


        /// <summary>
        /// Instead of a single player's turn, we iterate over each hand the player has.
        /// The player can Hit, Stand, or Double. Doubling a split hand is sometimes restricted in real Blackjack,
        /// but we'll allow it for simplicity.
        /// </summary>
        private void PlayerTurn()
        {
            for (int handIndex = 0; handIndex < _player.Hands.Count; handIndex++)
            {
                // If the player has a "natural" 21 on a split, treat it as a normal 21, not a blackjack
                // but let's check if it's 21. If so, we skip the sub-turn because standard practice is auto-stand.
                if (_player.GetHandValue(handIndex) == 21 && _player.Hands[handIndex].Cards.Count == 2)
                {
                    // If this is the original first hand, it could be a "true" blackjack,
                    // but the code below typically lumps splits into a normal 21. We'll just auto-stand:
                    Console.WriteLine($"\n{_player.Name}'s Hand {handIndex + 1} is 21! (Auto-stand)");
                    continue;
                }

                while (true)
                {
                    if (_player.IsBust(handIndex))
                    {
                        Console.WriteLine($"Hand {handIndex + 1} is bust!");
                        break;
                    }

                    Console.WriteLine($"\nHand {handIndex + 1}: {DescribeHand(_player, handIndex)}");

                    // Add [P] option for splitting
                    Console.WriteLine("[H]it, [S]tand, [D]ouble");

                    string input = Console.ReadLine()?.Trim().ToLower();

                    if (input == "h")
                    {
                        var newCard = _deck.DealCard();
                        _player.Hit(newCard, handIndex);
                        Console.WriteLine($"You drew: {newCard}");
                    }
                    else if (input == "s")
                    {
                        Console.WriteLine("You chose to stand.");
                        break;
                    }
                    else if (input == "d")
                    {
                        int currentBet = _player.GetBet(handIndex);
                        if (_player.Money >= currentBet)
                        {
                            _player.Money -= currentBet;      // deduct additional bet
                            _player.DoubleBet(handIndex);     // correctly update bet
                            var newCard = _deck.DealCard();
                            _player.Hit(newCard, handIndex);
                            Console.WriteLine($"You doubled down and drew: {newCard}");
                        }
                        else
                        {
                            Console.WriteLine("Not enough money to double down on this hand.");
                        }
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Please enter H, S, or D.");
                    }

                    if (_player.IsBust(handIndex))
                    {
                        Console.WriteLine($"Hand {handIndex + 1} is bust!");
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// Dealer draws until at least 17 or busts.
        /// </summary>
        private void DealerTurn()
        {
            Console.WriteLine("\n--- Dealer's Turn ---");
            Console.WriteLine($"Dealer's starting hand: {DescribeHand(_dealer)}");

            while (_dealer.GetHandValue() < 17)
            {
                var newCard = _deck.DealCard();
                _dealer.Hit(newCard);
                Console.WriteLine($"Dealer draws: {newCard}");
                Console.WriteLine($"Dealer's hand is now: {DescribeHand(_dealer)}");

                if (_dealer.IsBust())
                {
                    Console.WriteLine("Dealer busts!");
                    return;
                }
            }
            Console.WriteLine("Dealer stands.");
        }

        /// <summary>
        /// Determines outcome for each player's hand individually.
        /// If the hand is bust, the bet is lost. 
        /// If dealer busts, each non-bust hand wins 2x that hand's bet.
        /// Otherwise, compare each hand to dealer's total.
        /// 
        /// If original 2-card 21 (and not a split), you might pay 2.5x. 
        /// For splitted 21, just do normal 2x. 
        /// </summary>
        private void DetermineOutcome(bool playerHadBlackjack)
        {
            // If the bets are 0, possibly already resolved, etc. 
            // but let's assume we handle it here.

            int dealerValue = _dealer.GetHandValue(0);
            bool dealerBust = _dealer.IsBust(0);

            Console.WriteLine("\n--- Round Results ---");
            for (int handIndex = 0; handIndex < _player.Hands.Count; handIndex++)
            {
                int playerValue = _player.GetHandValue(handIndex);
                int bet = _player.GetBet(handIndex);
                if (bet <= 0) continue; // Already resolved

                Console.WriteLine($"\n{_player.Name}'s Hand {handIndex + 1}: {DescribeHand(_player, handIndex)} (Bet: {bet})");
                Console.WriteLine($"Dealer's total: {dealerValue}");

                if (_player.IsBust(handIndex))
                {
                    Console.WriteLine($"Hand {handIndex + 1} busted. You lose your bet.");
                    _player.LoseBet(handIndex);
                }
                else if (dealerBust)
                {
                    Console.WriteLine("Dealer busted. You win!");
                    _player.WinBet(handIndex);
                }
                else
                {
                    // Compare values
                    if (playerValue > dealerValue)
                    {
                        // Check if this is the very first hand and an actual Blackjack
                        // (2 cards, 21, no split) => pay 2.5x
                        bool isOriginalFirstHand = (handIndex == 0 && _player.Hands[handIndex].Cards.Count == 2);
                        bool splitted = (_player.Hands.Count > 1);
                        if (isOriginalFirstHand && playerHadBlackjack && !splitted)
                        {
                            Console.WriteLine("Blackjack on first hand! 2.5x payout!");
                            _player.WinBlackjack(handIndex);
                        }
                        else if (_player.Hands[handIndex].Cards.Count == 2 && playerValue == 21 && splitted)
                        {
                            // Typically a 21 from a split is not a "natural Blackjack".
                            // We'll treat it as normal 21 => 2x the bet
                            Console.WriteLine("21 from a split. Normal payout 2x.");
                            _player.Win21(handIndex);
                        }
                        else
                        {
                            Console.WriteLine("You win this hand!");
                            _player.WinBet(handIndex);
                        }
                    }
                    else if (playerValue < dealerValue)
                    {
                        Console.WriteLine("Dealer wins this hand!");
                        _player.LoseBet(handIndex);
                    }
                    else
                    {
                        Console.WriteLine("Push (tie) for this hand. Bet returned.");
                        _player.PushBet(handIndex);
                    }
                }
            }

            Console.WriteLine($"\nYou now have {_player.Money} coins.");
        }

        private void AskToContinue()
        {
            if (_player.Money <= 0)
            {
                Console.WriteLine("You have run out of money!");
                _keepPlaying = false;
                return;
            }

            Console.WriteLine("\nPress Enter to play another round, or X to leave.");
            string input = Console.ReadLine()?.Trim().ToLower();
            if (input == "x")
            {
                _keepPlaying = false;
            }
        }

        private string DescribeHand(Player player, int handIndex = 0)
        {
            var hand = player.Hands[handIndex];
            string cardsJoined = string.Join(", ", hand.Cards);
            return $"{cardsJoined} (Value: {hand.CalculateValue()})";
        }
    }
}
