using System;
using System.Collections.Generic;

namespace Blackjack.Core.Models
{
    public class Deck
    {
        private List<Card> _cards;
        private Random _rng;

        public Deck()
        {
            _cards = new List<Card>();
            _rng = new Random();

            // Create a standard 52-card deck
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    _cards.Add(new Card(suit, rank));
                }
            }
        }

        public void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int swapIndex = _rng.Next(i + 1);
                Card temp = _cards[i];
                _cards[i] = _cards[swapIndex];
                _cards[swapIndex] = temp;
            }
        }

        public Card DealCard()
        {
            if (_cards.Count == 0)
                throw new InvalidOperationException("No cards left in the deck.");

            Card dealtCard = _cards[0];
            _cards.RemoveAt(0); // Remove from the "top" of the deck
            return dealtCard;
        }

        public int CardsRemaining => _cards.Count;
    }
}
