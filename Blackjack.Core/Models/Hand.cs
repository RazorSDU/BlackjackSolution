using System.Collections.Generic;

namespace Blackjack.Core.Models
{
    public class Hand
    {
        private List<Card> _cards = new List<Card>();

        public IReadOnlyList<Card> Cards => _cards.AsReadOnly();

        public void AddCard(Card card)
        {
            _cards.Add(card);
        }

        public int CalculateValue()
        {
            int total = 0;
            int aceCount = 0;

            // First pass: treat all Aces as 11
            foreach (var card in _cards)
            {
                switch (card.Rank)
                {
                    case Rank.Ace:
                        aceCount++;
                        total += 11;
                        break;
                    case Rank.Jack:
                    case Rank.Queen:
                    case Rank.King:
                        total += 10;
                        break;
                    default:
                        total += (int)card.Rank; // For 2-10
                        break;
                }
            }

            // If we're over 21, convert Aces (11) down to 1
            while (total > 21 && aceCount > 0)
            {
                total -= 10;
                aceCount--;
            }

            return total;
        }

        public bool IsBust => CalculateValue() > 21;

        public Card RemoveCardAt(int index)
        {
            if (index < 0 || index >= _cards.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid card index.");

            Card removedCard = _cards[index];
            _cards.RemoveAt(index);
            return removedCard;
        }

    }
}
