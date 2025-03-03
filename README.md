# Blackjack Game

## Project Overview

This project is a **Blackjack game** developed in **C#** using .NET, with plans for future expansion to a graphical user interface (GUI). The game supports:

- ✅ Single player vs. dealer.
- ✅ Full betting system with starting money.
- ✅ Insurance mechanic when the dealer shows an Ace.
- ✅ Splitting pairs into two hands.
- ✅ Doubling down with correct bet adjustment.
- ✅ Multiple hands after splitting, each independently played.
- ✅ Automatic Blackjack detection and payouts (2.5x for natural Blackjack).

The project was developed as part of an **educational exercise** to build a structured and extensible application using good object-oriented practices.

---

## Features

- 🃏 Full deck handling (shuffling and dealing).
- 💰 Betting system with split-hand bet tracking.
- 🔥 Multi-round support — game continues until the player runs out of money or quits.
- 🃏 Blackjack (21) auto-detect and automatic win.
- ✂️ Pair splitting with independent hand management.
- 🛡️ Insurance bets when the dealer shows an Ace.
- ✖️ Dealer logic follows standard rules — hits until 17+.

---

## Installation & Running

### Prerequisites

- Visual Studio 2022 (or newer)
- .NET 6 or 7 SDK (depending on your target version)

### Running the Game

1. Clone the repository:
    ```bash
    git clone <your-repo-url>
    ```
2. Open the project in **Visual Studio 2022**.
3. Set the **Blackjack.ConsoleApp** project as the startup project.
4. Run the project (F5 or `Ctrl+F5`).

---

## Controls & Gameplay

- Place a bet each round.
- Choose to **Hit**, **Stand**, **Double Down**, or **Split** (when allowed).
- Blackjack pays 2.5x.
- Split hands are played independently.
- Insurance is offered if the dealer shows an Ace.

---

## Future Plans

- 💻 Graphical user interface (planned in future phase).
- 🤖 Optional AI players.
- 📊 Game statistics across sessions.

---

## Folder Structure

