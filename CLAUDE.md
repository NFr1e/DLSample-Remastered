# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

DLSample is a Unity 2022.3 LTS project inspired by the game "Dancing Line" (跳舞的线). It includes runtime gameplay, plus editor tooling for level creation, beatmap import, and path authoring. The codebase was recently refactored (2026-05-13, see `REFACTOR_SUMMARY.md`) for formatting, naming conventions, and XML documentation — but business logic and public API signatures were preserved.

## Build & Development

- **Recommended Unity version**: 2022.3 LTS (minimum: 2019.4)
- Open `DLSample-Rematered.sln` in an IDE, or open the project folder in Unity
- No CI or test suite exists yet

## Assembly Structure

Two custom assemblies defined via `.asmdef`:

| Assembly | Scope | References |
|----------|-------|------------|
| `DLSample` | Runtime + Shared (all platforms) | UniTask, DOTween, Odin Inspector, AnimationSequencer |
| `DLSample.Editor` | Editor-only tools | `DLSample` |

## Architecture

### App Bootstrap

`AppEntry` (`Runtime/App/AppEntry.cs`) is the global entry point, initialized via `[RuntimeInitializeOnLoadMethod(BeforeSplashScreen)]`. It creates and owns singleton instances of `EventBus`, `AsyncEventBus`, `GameInput`, `InputManager`, `UIElementManager`, and `ScenesManager`. These are accessible as static properties.

### Gameplay Scene Lifecycle

Each gameplay scene has a `GameplayEntry` MonoBehaviour (lazy singleton). It creates its own `EventBus`, `AsyncEventBus`, `ServiceLocator`, and `ModulesManager` scoped to that scene. It also manages a list of `GameplayObject` components (registered via `RegisterGameplayObject`).

### Module System

`IModule` (`Runtime/Framework/IModule.cs`) defines `Priority`, `OnInit()`, `OnUpdate(float)`, `OnShutdown()`. Modules are registered with `ModulesManager`, which sorts by priority (lower = earlier init), then auto-injects dependencies via `IModuleRequire<T>` interface scanning with reflection.

Gameplay module priorities are defined in `DLSampleConsts.Gameplay` (e.g., `BacktrackablesHandler = 0`, `PlayerController = 1`, `Initializer = 10`, `StairController = 11`).

### Gameplay State Machine

`GameplayFSM` (`Runtime/Gameplay/GameplayFSM.cs`) drives game phase transitions via `GameplayStateBase` subclasses defined in `GameplayStates`:

```
Waiting → Preparing → Gaming → Over
                            → Pause → Gaming
                            → Respawn → Preparing
                            → Exiting
```

`GameplayStateHandler` bridges the FSM and EventBus — it subscribes to gameplay event types and calls `_fsm.SetCurrentState<T>()` in response.

### Event Bus

`EventBus` is a generic, type-safe synchronous pub/sub keyed by `IEventArg` type. `AsyncEventBus` follows the same pattern for async event types. Both exist at two scopes: global (`AppEntry`) and per-gameplay-scene (`GameplayEntry`).

### Service Locator

`ServiceLocator` (`Runtime/Facility/ServiceLocator.cs`) allows modules to register and retrieve services by type. Supports `WhenServicesReady(callback, types...)` for late-binding scenarios.

### Beat-Driven Path System (Core Workflow)

This is the central data pipeline:

1. **`BeatmapDataScriptable`** (`Shared/BeatmapDataScriptable.cs`) — a list of `Beat` structs, each holding a `TimeSecond`. Can be populated manually or via the ChartReader editor tool (osu! beatmap import).

2. **`PathGrapherAsset`** (`Shared/PathGrapher/PathGrapherAsset.cs`) — ScriptableObject holding source config (start position, speed, gravity, directions, beatmap reference) plus `PathData` (global events, generated waypoints, generated segments).

3. **`PathSimulator.Simulate()`** (`Shared/PathGrapher/PathSimulator.cs`) — the algorithmic core. Takes a `PathGrapherAsset`, steps through beatmap time points, applies physics simulation (gravity, jumps, teleports), and populates `generatedWaypoints` and `generatedSegments`. Called from `PathGrapherBehaviour.RequestRebuild()` in the editor.

4. **`PathGrapherEventsSyncer`** (`Shared/PathGrapher/PathGrapherEventsSyncer.cs`) — at runtime, reads `globalEvents` from the `PathGrapherAsset` and registers them as tick events on `GameplayTimer`.

### Path Events

`IPathEvent` (`Shared/PathGrapher/PathEvent.cs`) has two base types:
- `PointPathEvent` — instantaneous: `ForceTurnEvent`, `SpeedChangeEvent`, `GravityChangeEvent`, `DirectionChangeEvent`
- `SegmentPathEvent` — duration-based: `TeleportEvent`, `JumpEvent`

`PathEventResolver.ResolveToGameplayEvent()` converts path events to `PlayerEvents` (from `Runtime/Gameplay/PlayerEvents.cs`) which implement `IGameplayEvent` for runtime processing.

### Data Structures

- `Waypoint` — position, rotation, time, beatIndex
- `PathSegment` — start/end waypoints, contained events, list of `PathSection`
- `PathSection` — start/end time, point array, up direction, isJump/isTeleport flags
- `LevelDataScriptable` — references scene, level name, soundtrack info, gem count, level length

## Editor Tools

All under `Assets/DLSample/Scripts/Editor/`, accessible via the `DLSample` menu:

- **LevelCreator** — one-click creation of a full level: scene + `LevelData` + `BeatmapData` + `PathGrapherAsset` + directory structure
- **PathBuilder** — visual path editing tool
- **PathGrapher** — `PathGrapherBehaviour` (ExecuteInEditMode) visualizes the path in Scene view; `PathGrapherDrawer` handles rendering
- **ChartReader** — imports osu! beatmap files into `BeatmapDataScriptable`

## Third-Party Dependencies

- **UniTask** — zero-allocation async/await for Unity
- **DOTween** — tweening library
- **Odin Inspector** — editor serialization enhancements (guarded by `#if ODIN_INSPECTOR`)
- **BrunoMikoski.AnimationSequencer** — animation sequencing
- Unity packages: InputSystem, UI Toolkit, Timeline, TextMeshPro

## Code Conventions

- 4-space indentation, Allman braces
- Private fields: `_camelCase`
- Constants: `UPPER_SNAKE_CASE`
- XML documentation comments in Chinese (added during the refactor)
- CSS class name constants in editor controllers match `.uss` files exactly — do not rename them
- UTF-8 encoding for all source files (`.editorconfig` recommended but not yet present)
