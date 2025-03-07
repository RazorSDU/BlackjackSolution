using Xunit;
using Blackjack.Core.Models;

namespace Blackjack.Tests
{
    public class PlayerTests
    {
        [Fact]
        public void PlaceBet_ShouldDeductMoney()
        {
            var player = new Player("Test", 100);

            player.PlaceBet(20);

            Assert.Equal(80, player.Money);
            Assert.Equal(20, player.GetBet());
        }

        [Fact]
        public void WinBet_ShouldIncreaseMoney()
        {
            var player = new Player("Test", 100);
            player.PlaceBet(20);
            player.WinBet(0);

            Assert.Equal(120, player.Money);
        }

        [Fact]
        public void DoubleBet_ShouldDoubleCurrentBet()
        {
            var player = new Player("Test", 100);
            player.PlaceBet(20);
            player.DoubleBet();

            Assert.Equal(40, player.GetBet());
        }
    }
}
