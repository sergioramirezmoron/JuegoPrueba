# Project Structure

This project is organized to be easy to understand for someone new to Unity and scalable enough to keep growing without turning into a mess.

## Top-Level Rule

- `Assets/Game`: everything that belongs to the game itself.
- `Assets/ThirdParty`: everything imported from external packages or vendors.

If you create something new and it is part of your game, it goes in `Assets/Game`.

## Current Layout

```text
Assets/
  Game/
    Art/
    Audio/
    Materials/
    Prefabs/
    Scenes/
    Scripts/
      Core/
      Gameplay/
        Enemies/
        Interactions/
          Doors/
        Player/
          Camera/
        Weapons/
      UI/
    Settings/
      Input/
      Rendering/
    Tests/
      EditMode/
    UI/
  ThirdParty/
    FreeWoodDoorPack/
    TextMeshPro/
    UnityTutorialInfo/
```

## What Goes Where

### `Assets/Game/Scenes`

- Unity scenes used by the game.
- The main playable scene lives here.

### `Assets/Game/Scripts`

- All game code written for this project.
- `Core`: reusable foundations, utilities, managers, interfaces, shared helpers.
- `Gameplay`: systems related to actual game behavior.
- `UI`: scripts that only control menus, HUD, prompts, and interface behavior.

### `Assets/Game/Scripts/Gameplay/Player`

- Movement, camera, player input handling, and player-specific features.

### `Assets/Game/Scripts/Gameplay/Interactions`

- Interactive world systems.
- Doors live in `Interactions/Doors`.

### `Assets/Game/Settings`

- Project-owned runtime configuration assets.
- `Input`: input action assets.
- `Rendering`: URP renderers, render pipeline assets, and volume profiles.

### `Assets/Game/Art`, `Audio`, `Materials`, `Prefabs`, `UI`

- Game-owned assets separated by content type.
- This keeps the project easy to scan even before feature-specific asset pipelines exist.

### `Assets/Game/Tests`

- Automated tests for game code.
- `EditMode`: logic and setup tests that do not require full gameplay runtime.

### `Assets/ThirdParty`

- Imported content that should stay isolated from game code.
- Do not mix custom gameplay scripts into these folders.
- If a third-party asset needs custom behavior, add your wrapper or integration code in `Assets/Game/Scripts`.

## Practical Rules For Future Work

1. New gameplay systems go under `Assets/Game/Scripts/Gameplay/<SystemName>`.
2. New player features stay inside `Assets/Game/Scripts/Gameplay/Player`.
3. New interactables go inside `Assets/Game/Scripts/Gameplay/Interactions`.
4. Third-party packs never go into `Assets/Game`.
5. Avoid putting new folders directly in the root of `Assets`.
6. Prefer extending game code from `Assets/Game` instead of editing vendor code inside `Assets/ThirdParty`.

## Why This Layout Works

- A beginner can answer the first question fast: `Is this mine or from a package?`
- A programmer can answer the second question fast: `Is this core, gameplay, UI, settings, or content?`
- The project can grow by systems without losing the simplicity of content-type folders.
