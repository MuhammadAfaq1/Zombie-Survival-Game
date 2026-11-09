
# 🧟 Zombie Apocalypse Survival Game

A 3D First-Person Shooter (FPS) survival game developed using **Unity and C#**, where players must survive waves of increasingly challenging zombies across multiple environments. The game features weapon switching, realistic shooting mechanics, AI-driven zombie behavior, health and ammunition management, and immersive visual effects.

---

## 📌 Project Overview

The Zombie Apocalypse Survival Game is a wave-based FPS project focused on combining first-person gameplay mechanics with AI-driven enemy behavior and interactive game systems.

Players must navigate different environments, eliminate incoming zombies, manage ammunition and health, and survive progressively difficult waves. The project incorporates several interconnected gameplay systems, including weapon management, shooting mechanics, zombie navigation, player damage, and game progression.

## 🎮 Gameplay Features

### Player Mechanics
- First-person player movement and camera control.
- Mouse-based aiming and shooting.
- Jumping and movement controls.
- Player health and damage system.
- Death screen and game-over functionality.

### Weapons and Combat
- Multiple selectable weapons.
- Weapon switching using number keys.
- Raycast-based shooting mechanics.
- Automatic and semi-automatic firing modes.
- Ammunition tracking and reloading.
- Weapon-specific shooting and reload animations.
- Muzzle flash and shooting sound effects.
- Sniper scope with adjustable zoom.
- Aim Down Sights (ADS) functionality.
- Blood impact effects when zombies are hit.

### Zombie AI
- AI-controlled zombie enemies.
- NavMesh-based navigation and movement.
- Zombie patrol and player-chasing behavior.
- Detection and attack range systems.
- Zombie health and damage handling.
- Death and damage animations.

### Wave-Based Gameplay
- Progressive zombie wave system.
- Increasing difficulty as waves advance.
- Zombie spawning and wave management.
- Configurable time between waves.
- Multiple playable maps.

### User Interface
- Main menu and gameplay interface.
- Health and ammunition indicators.
- Wave and kill tracking.
- Game-over screen.
- Configurable gameplay settings.
- Player preferences and saved settings.

---

## 🛠️ Technologies and Tools

| Technology | Purpose |
|---|---|
| Unity | Game engine and development environment |
| C# | Gameplay programming and game logic |
| Universal Render Pipeline (URP) | Graphics rendering |
| Unity NavMesh | Zombie navigation and AI movement |
| Animator | Character and weapon animations |
| TextMeshPro | UI text rendering |
| Unity Physics | Raycasting and collision detection |
| Git & GitHub | Version control and source management |

---

## 🚀 Getting Started

Follow these steps to run the project locally.

### Prerequisites

- Unity Hub.
- Unity **6000.6.0f1** (Unity 6).
- Git (optional, for cloning the repository).

### Installation

**1. Clone the repository**

```bash
git clone https://github.com/MuhammadAfaq1/Zombie-Survival-Game.git
```

**2. Open Unity Hub**

- Launch Unity Hub.
- Select **Add → Add project from disk**.
- Navigate to the cloned project directory.
- Select the project folder.

**3. Open the project**

- Open the project using Unity **6000.6.0f1**.
- Allow Unity to import and process the project assets.
- The configured build scenes include the main menu at
  `Assets/SlimUI/Modern Menu 1/Scenes/Demos/MainMenu.unity` and gameplay maps
  at `Assets/_Recovery/Map1.unity` and `Assets/Old Sea Port/scenes/Map2.unity`.

**4. Run the game**

Press the **Play** button in the Unity Editor to start playing.

---

## ⌨️ Controls

| Key / Input | Action |
|---|---|
| W | Move Forward |
| A | Move Left |
| S | Move Backward |
| D | Move Right |
| Mouse | Look Around |
| Left Mouse Button | Shoot |
| Right Mouse Button | Aim / ADS |
| R | Reload |
| Space | Jump |
| 1–4 | Switch Weapons |
| Escape | Return to Main Menu |

*Controls may depend on the active scene and weapon configuration.*

---


## 🔧 Development Highlights

Some of the key technical aspects of this project include:

- Implementing modular gameplay systems using C# scripts.
- Integrating Unity NavMesh for enemy navigation and player tracking.
- Developing raycast-based shooting and hit-detection mechanics.
- Managing multiple weapons through centralized weapon-switching logic.
- Implementing ammunition, reloading, health, and damage systems.
- Connecting animations, audio, and visual effects with gameplay events.
- Developing wave-based enemy spawning and difficulty progression.
- Building interactive UI elements and persistent player settings.

---

**Developed with Unity and C#**
