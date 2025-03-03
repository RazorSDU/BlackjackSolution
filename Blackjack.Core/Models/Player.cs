using System;
using System.Collections.Generic;

namespace Blackjack.Core.Models
{
    public class Player
    {
        public string Name { get; set; }

        // Currency
        public int Money { get; set; }

        // A list of bets, each bet corresponding to a hand.
        // For example, Bets[0] is the bet for Hands[0].
        private List<int> _bets = new List<int>();

        // For insurance
        public int InsuranceBet { get; private set; }

        private List<Hand> _hands = new List<Hand>();
        public IReadOnlyList<Hand> Hands => _hands.AsReadOnly();

        public Player(string name, int startingMoney = 100)
        {
            Name = name;
            Money = startingMoney;

            // Initialize with one hand and a 0 bet
            _hands.Add(new Hand());
            _bets.Add(0);
        }

        /// <summary>
        /// Places a bet on the first hand by default. Deducts from Money immediately.
        /// </summary>
        public void PlaceBet(int amount, int handIndex = 0)
        {
            if (amount <= 0)
                throw new ArgumentException("Bet amount must be positive.");
            if (amount > Money)
                throw new ArgumentException("Insufficient funds for this bet.");

            _bets[handIndex] = amount;
            Money -= amount;
        }

        /// <summary>
        /// The amount currently bet on a given hand.
        /// </summary>
        public int GetBet(int handIndex = 0)
        {
            return _bets[handIndex];
        }

        /// <summary>
        /// Doubles the current bet on the specified hand.
        /// </summary>
        public void DoubleBet(int handIndex = 0)
        {
            _bets[handIndex] *= 2;  // Double the current bet
        }

        /// <summary>
        /// Places an insurance bet equal to half of the first hand's main bet.
        /// </summary>
        public void PlaceInsurance()
        {
            // For simplicity, always tie insurance to hand 0
            int baseBet = _bets[0];
            InsuranceBet = baseBet / 2;

            if (InsuranceBet <= 0)
                throw new ArgumentException("Insurance bet must be greater than zero.");
            if (InsuranceBet > Money)
                throw new ArgumentException("Insufficient funds for insurance.");

            Money -= InsuranceBet;
        }

        /// <summary>
        /// Add a card to the specified hand.
        /// </summary>
        public void Hit(Card card, int handIndex = 0)
        {
            _hands[handIndex].AddCard(card);
        }

        public bool IsBust(int handIndex = 0) => _hands[handIndex].IsBust;

        public int GetHandValue(int handIndex = 0) => _hands[handIndex].CalculateValue();

        /// <summary>
        /// Checks if the player can split the first hand:
        /// 1) The hand must have exactly 2 cards.
        /// 2) They must be the same rank (for simplicity).
        /// 3) The player must still have enough money to double the initial bet.
        /// 4) There's only 1 hand currently (no multiple splits).
        /// </summary>
        public bool CanSplit(int handIndex = 0)
        {
            if (_hands.Count > 1) return false; // Only allow one split total
            if (_hands[handIndex].Cards.Count != 2) return false;

            var c1 = _hands[handIndex].Cards[0];
            var c2 = _hands[handIndex].Cards[1];
            bool sameRank = (c1.Rank == c2.Rank);

            // Some Blackjack variants let you split on same "value" (e.g. 10 + King),
            // but we follow "exact same rank" here. Adjust if desired.

            if (!sameRank) return false;

            // Need enough money to match the bet
            int currentBet = _bets[handIndex];
            if (Money < currentBet) return false;

            return true;
        }

        /// <summary>
        /// Splits the hand at handIndex into two separate hands, each with its own bet.
        /// Deducts the matching bet from player's money again.
        /// </summary>
        public void Split(int handIndex = 0)
        {
            // We'll create a second hand
            var newHand = new Hand();

            // Move the second card from the original hand to the new hand
            Card secondCard = _hands[handIndex].RemoveCardAt(1);
            newHand.AddCard(secondCard);

            // Deduct the same bet again
            int betForSplit = _bets[handIndex];
            Money -= betForSplit;

            // Now add the new hand and new bet to our lists
            _hands.Add(newHand);
            _bets.Add(betForSplit);
        }

        // --------------------- Win/Lose/Push for each hand --------------------- //

        /// <summary>
        /// The player wins the bet for the specified hand: + 2x bet (net +bet).
        /// </summary>
        public void WinBet(int handIndex)
        {
            Money += (_bets[handIndex] * 2);
            _bets[handIndex] = 0;
        }

        /// <summary>
        /// For a normal 21 or if you want to treat a splitted Ace + 10 as normal 21 (not Blackjack).
        /// This can be used if you want a special payout anyway.
        /// </summary>
        public void Win21(int handIndex)
        {
            // This is effectively the same as WinBet in typical casinos.
            // But if you want a different ratio, adjust here.
            Money += (_bets[handIndex] * 2);
            _bets[handIndex] = 0;
        }

        /// <summary>
        /// Player wins with an original (non-split) Blackjack for 2.5x. 
        /// (You can skip this for split hands or call it anyway if you want!)
        /// </summary>
        public void WinBlackjack(int handIndex)
        {
            // +2.5 × the bet
            Money += (int)(_bets[handIndex] * 2.5);
            _bets[handIndex] = 0;
        }

        public void LoseBet(int handIndex)
        {
            // Already deducted
            _bets[handIndex] = 0;
        }

        public void PushBet(int handIndex)
        {
            // Return the bet
            Money += _bets[handIndex];
            _bets[handIndex] = 0;
        }

        /// <summary>
        /// Insurance resolved if dealer has blackjack.
        /// </summary>
        public void WinInsurance()
        {
            Money += InsuranceBet * 2;
            InsuranceBet = 0;
        }

        public void LoseInsurance()
        {
            // Already deducted from money
            InsuranceBet = 0;
        }

        // --------------------- Round Reset --------------------- //

        public void ResetHand()
        {
            _hands.Clear();
            _bets.Clear();
            InsuranceBet = 0;

            _hands.Add(new Hand());
            _bets.Add(0);
        }
    }
}
