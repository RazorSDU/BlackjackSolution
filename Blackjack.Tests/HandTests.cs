using Xunit;
using Blackjack.Core.Models;

namespace Blackjack.Tests
{
    public class HandTests
    {
        [Fact]
        public void CalculateValue_WithAceAndTen_ShouldBe21()
        {
            var hand = new Hand();
            hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
            hand.AddCard(new Card(Suit.Spades, Rank.Ten));

            Assert.Equal(21, hand.CalculateValue());
        }

        [Fact]
        public void CalculateValue_WithAceAceAndNine_ShouldTreatOneAceAsOne()
        {
            var hand = new Hand();
            hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
            hand.AddCard(new Card(Suit.Spades, Rank.Ace));
            hand.AddCard(new Card(Suit.Clubs, Rank.Nine));

            Assert.Equal(21, hand.CalculateValue());
        }
    }
}
