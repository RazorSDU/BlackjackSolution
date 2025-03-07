
# 📝 Teacher Code Review Focus Points

### 1️⃣ Code Architecture
- 🔹 **Is the modular design easy enough to understand for someone who didn’t write the code?**
- 🔹 **Is the code sufficiently divided into multiple files and projects where necessary? Should any areas be further split to improve clarity?**

### 2️⃣ ChatGPT Usage in the Codebase
- 🔹 **Many parts of the code were written with the help of ChatGPT (this is also noted in comments).**  
- 🔹 **I plan to keep using AI tools in the future — are there any patterns, pitfalls, or concerns you notice that I should be aware of when using ChatGPT in actual production work?**

### 3️⃣ General Feedback
- 🔹 **If you spot any areas where the code could be improved (performance, readability, better design), I’d love to hear your feedback.**

---

# ♠️ Blackjack Game

## 🏗️ Project Overview
Welcome to my **Blackjack game project**, written in **C# with .NET**! The game logic is fully separated into a **core library (`Blackjack.Core`)**, allowing reuse across different frontends like:

- ✅ **Console Application** (`Blackjack.ConsoleApp`)
- ✅ **Cross-platform MAUI GUI** (`Blackjack.MAUI`)

This modular design makes the game logic testable, extensible, and adaptable for future expansions, such as AI players or a web interface.

---

## ✨ Features

- 🃏 **Single Player vs. Dealer Gameplay**
- 💰 **Betting System** — Start with a set amount of money
- 🛡️ **Insurance Bets** — Available if the dealer’s up-card is an Ace
- ✂️ **Splitting Pairs** — If your first two cards match, split into two hands
- 🔥 **Double Down** — Double your bet and draw exactly **one** more card
- 🃏 **Blackjack Detection** — Natural Blackjack (Ace + 10) pays **2.5x**
- 🔄 **Multi-Round Play** — Continue until you run out of money or quit
- 🏦 **Dealer Logic** — Dealer hits until at least 17

---

## 🛠️ Installation & Running

### Prerequisites
- 📥 **Visual Studio 2022** (or newer)
- 🔗 **.NET 6 or 7 SDK** (depending on your environment)

### 1️⃣ Clone the Repository
```bash
git clone <your-repo-url>
```

### 2️⃣ Run the Console Version
1. Open the solution in **Visual Studio**.
2. Set **`Blackjack.ConsoleApp`** as the startup project.
3. Press **F5** (or **Ctrl+F5**) to run.

### 3️⃣ Run the MAUI Version
1. Set **`Blackjack.MAUI`** as the startup project.
2. Select your platform target (Windows, Android, etc.).
3. Press **F5** to launch the graphical UI.

---

## 🎮 How to Play

1. 🪙 **Place Your Bet** — Enter a bet amount and start the round.
2. 🃏 **Initial Deal** — You and the dealer receive 2 cards each.
3. 🎯 **Choose Your Actions**:
    - Hit – Draw another card.
    - Stand – End your turn with your current total.
    - Double Down – Double your bet and draw exactly **one** more card.
    - Split – If your initial two cards are a pair, split them into two hands.
4. 🔍 **Optional Insurance** — If the dealer shows an Ace, you may place an insurance bet.
5. 🏁 **Dealer’s Turn** — Dealer reveals their hand and plays until they reach at least 17.
6. 🏆 **Outcome & Payout** — Compare hands, resolve bets, and move to the next round.

---

## 🧰 Code Structure

### 📂 `Blackjack.Core`
- 🎯 **Models** — `Player`, `Hand`, `Deck`, `Card`, and supporting enums.
- 🧠 **Game Logic** — `BlackjackGameAPI`, a clean API exposing methods like:
    - `StartRound`, `PlayerHit`, `DealerTurn`, `DetermineOutcome`
    - This layer is completely **UI-independent** for maximum flexibility.

### 📂 `Blackjack.ConsoleApp`
- 🖥️ Console interface using `BlackjackGameAPI`.
- Supports multi-round play until player runs out of money or quits.

### 📂 `Blackjack.MAUI`
- 🌐 Cross-platform graphical interface using **.NET MAUI**.
- 🧑‍🎨 XAML pages for layout, bound to a **`GamePageViewModel`**, which interacts with the core game logic.
- 🃏 Card images, dynamic buttons, split-hand display, and more!
