using Xunit;
using Blackjack.Core.Models;

namespace Blackjack.Tests
{
    public class DeckTests
    {
        [Fact]
        public void Deck_ShouldContain52Cards_AfterInitialization()
        {
            // Arrange
            var deck = new Deck();

            // Act
            int cardCount = deck.CardsRemaining;

            // Assert
            Assert.Equal(52, cardCount);
        }

        [Fact]
        public void DealCard_ShouldReduceDeckSize()
        {
            // Arrange
            var deck = new Deck();
            deck.Shuffle();

            // Act
            var card = deck.DealCard();

            // Assert
            Assert.NotNull(card);
            Assert.Equal(51, deck.CardsRemaining);
        }
    }
}
