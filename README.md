# Blackjack .MAUI — Mobile & Desktop Blackjack built with .NET 8  

> **Showcase project** — demonstrates my ability to design, architect and ship a complete cross-platform app using modern Microsoft tooling.

---

## 🛠 Tech Stack & Skills Demonstrated
| Area | Stack / Library | Highlights shown in this project |
|------|-----------------|-----------------------------------|
| **Cross-platform UI** | **.NET MAUI 8 (.NET 8)** | Single code-base for Android, iOS, Windows, macOS |
| **Language** | **C# 12** | Nullable refs, top-level statements, expression-bodied members |
| **Architecture** | **MVVM** + CommunityToolkit | Clean separation of UI / state / logic, XAML bindings |
| **Game logic** | Pure C# class library | Testable `Blackjack.Core` with Deck, Player, Hand models |
| **Asynchrony** | `async/await`, `CancellationToken` | Responsive Auto-Play helper with safe UI-thread marshaling |
| **XAML UI** | Absolute/Attached-layout helpers | Custom attached properties for “grid-like” positioning |
| **Design patterns** | Commanding, Dependency injection | Reusable `AutoPlayHelper`, `GridPosition` |
| **Tooling** | Visual Studio 2022, Git, GitHub Actions (CI) | Lint-clean and ready for multi-platform CI/CD |

---

## ✨ What the App Does
### Core Gameplay
* Full **Blackjack** rules: hit, stand, double, split (1 × per round), dealer stands on 17.
* **Automatic shoe shuffle** every round.
* **Split hands UI** with live highlight: the active hand is bright; the waiting hand is dimmed.
* **Auto-Play mode** — an optional basic-strategy bot that plays perfect Blackjack hands autonomously.

### Visuals & UX
* Table-background image and card sprites (standard 52-card deck).
* Overlapping/rotated card stacks (double bets, split hands) for authentic casino feel.
* Coins‐balance, bet entry, contextual status messages and result summary.

### Engineering Features
* **Clean separation** between `Blackjack.Core` (game logic) and `Blackjack.MAUI` (presentation layer).
* **Unit-ready** core — no static state, deterministic deck class.
* **Attached layout helper** (`GridPosition`) gives CSS-grid-like coordinates inside an MAUI `AbsoluteLayout`.
* **AutoPlayHelper** demonstrates multithreaded orchestration, strategy decision tables and UI automation.

---

## 🚀 Running the App Locally
```bash
git clone https://github.com/RazorSDU/BlackjackSolution.git
cd Blackjack.MAUI

# Run on Windows desktop
dotnet build -c Release
dotnet run --project Blackjack.MAUI -f net8.0-windows

# Or target Android emulator / device
dotnet build -t:Run -f net8.0-android
