using System;
using Blackjack.Core.GameLogic;

namespace Blackjack.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Blackjack with Bets, Insurance, and Blackjack Pays 2.5x!");
            Console.Write("Enter your name: ");
            string playerName = Console.ReadLine();

            Console.Write("Enter your starting money: ");
            int startingMoney;
            if (!int.TryParse(Console.ReadLine(), out startingMoney))
            {
                startingMoney = 100; // default
            }

            var game = new BlackjackGame(playerName, startingMoney);
            game.RunMultipleRounds();

            Console.WriteLine("\nGame Over. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
