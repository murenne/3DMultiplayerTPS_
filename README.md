# 3D Multiplayer TPS

[English](./README.md)| [中文](./README_cn.md) | [日本語](./README_jp.md)

**A Multiplayer 3D Third-Person Shooter Project developed based on the Photon Quantum 3.0 Deterministic Engine.**

Based on actual project experience, the core gameplay systems have been ported and refactored.

* [Getting Started](#getting-started)
* [Control](#control)
* [Software Architecture](#software-architecture)
    * [Quantum ECS](#quantum-ecs)
    * [AssetObject](#assetobject)
* [Core Systems](#core-systems)
    * [Game System](#game-system)
    * [Spawn System](#spawn-system)
    * [Player Movement System](#player-movement-system)
    * [Action System](#action-system)
    * [Data Processing](#data-processing)
    * [Status and Damage System](#status-and-damage-system)
    * [Animation System](#animation-system)
    * [Camera System](#camera-system)
* [Statement](#statement)
## Getting Started

This project is developed based on **Unity 2022.3.36f1**. Please ensure your Unity version is **2022.3.36f1** or higher.

1.  Ensure **Photon Quantum SDK 3.0** is installed and the **AppID** is correctly configured.
2.  Clone the project from GitHub to your local folder and open it with Unity.
3.  Click the **Play** button to run the project.

## Control

This project supports both **Keyboard/Mouse** and **Gamepad** modes.

| Key / Input | Action Function |
| :--- | :--- |
| **WASD** / **Left Stick** | Player Movement |
| **Left Click** / **RT** | Shoot / Attack |
| **Space** / **A (South)** | Jump |
| **Shift** / **X (West)** | Sprint / Dash |
| **Tab** / **RB** | Toggle Target Lock |

## Software Architecture

This project utilizes the **Photon Quantum 3.0** deterministic network framework, adopting an architecture that strictly separates logic from the view.

### Quantum ECS
Abandoning the traditional MonoBehaviour Singleton pattern, the core logic adopts the **ECS (Entity Component System)** architecture:

* **Entity:** Objects in the game world (Players, Bullets, etc.).
* **Component:** Stores the data state of entities.
* **System:** Handles game logic and data updates.
* **Simulation:** The deterministic simulation core, ensuring synchronization across all clients.

### AssetObject
The project uses Quantum's **AssetObject** system to manage static configuration data.

* **Player Action Data:** Configuration for player actions (Jump, Dash, Shoot, etc.).
* **Player Parts Data:** Configuration for player customization and parts data.

All configuration data is managed via AssetObject, facilitating easy adjustments and version control.

## Core Systems

The project has implemented the following core game systems:

### Game System
Responsible for managing the game lifecycle.
* Countdown display before the game starts.
* Game state switching (Ready, In-Progress, Ended).
* Countdown animation display.
* Game flow control.

### Spawn System
Handles player generation logic.
* Randomly assigns initial positions to each player at the start of the game.
* Allocates positions based on pre-defined spawn points in the map.
* Ensures player spawn positions do not overlap.
* Supports respawn mechanics.

### Movement System
Controls player movement and orientation.
* Supports movement control via Keyboard/Gamepad.
* Automatically faces the locked target.
* Manually switches attack targets.
* Smooth movement and rotation interpolation.

### Action System
Unified management of all player action behaviors.
* **Action Types:** Jump, Dash, Shoot.
* **Cooldown Management:** Handles action cooldowns.
* **Duration Control:** Manages action duration.
* **Input Buffer:** Mechanism to queue inputs for smoother gameplay.
* **Priority:** Logic for action priority and interruption.

### Data Processing
Framework for equipment data injection.
* Automatically reads equipment configuration data.
* Dynamically injects data into the Quantum simulation system.
* Supports attribute differences across different equipment.
* Allows for hot-updating equipment data.

### Status and Damage System
Handles player status and damage calculations.
* **HP Management:** Tracks health points.
* **Damage:** Calculation and application of damage.
* **Hit Feedback:** Visual/Logic feedback upon taking damage.
* **Death:** Death judgment and processing.

### Animation System
Handles view-layer animation presentation.
* Connects logic and view based on an **Event** mechanism.
* Plays corresponding animations based on player state.
* Supports action blending and transitions.
* Synchronizes network state to local presentation.

### Camera
Camera control and lens management.
* Activates the correct team camera upon game initialization.
* Automatically adjusts camera distance to include all players.
* Dynamically follows the target player.
* Smooth camera transition effects.

## Statement

This project is developed based on the **Photon Quantum 3.0** deterministic network engine.
The current version has completed the porting and refactoring of the core gameplay systems.