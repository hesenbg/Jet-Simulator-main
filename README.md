# Multiplayer Jet Simulator

A multiplayer jet simulator built in Unity with Photon Fusion 2 (Shared Mode). Players create or join sessions and fly together in the same scene. Jets can lock onto enemy jets or dummy targets and fire tracking missiles.

## Features

- Online multiplayer using Photon Fusion 2 in Shared Mode
- Create a new session or join an existing one
- Multiple players in the same scene
- Target locking on enemy jets and dummy targets
- Missiles that track the locked target

## Controls

| Input | Action |
|-------|--------|
| W | Increase thrust |
| S | Decrease thrust |
| A | Yaw left |
| D | Yaw right |
| Mouse Y (up / down) | Pitch up / down |
| Mouse X (left / right) | Roll left / right |
| Left click | Launch missile at locked target |

## Requirements

- Unity `[version]`
- Photon Fusion 2
- A Photon Fusion App ID

## Setup

1. Clone the repository:
   ```
   git clone [repository-url]
   ```
2. Open the project in Unity.
3. Open the Fusion Hub (`Tools > Fusion > Fusion Hub`) and enter your Photon Fusion App ID.
4. Open the main scene: `[path/to/scene]`.
5. Press Play.

## Multiplayer

1. Start the game.
2. Enter a session name and create the session, or join an existing session by its name.
3. Each additional client that joins the same session spawns into the same scene.

To test locally, build the project and run one instance from the build while running another in the Unity Editor.

## Tech Stack

- Unity
- Photon Fusion 2 (Shared Mode)
- C#
