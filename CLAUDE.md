# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**digitPark** is a Unity 6000.0.59f2 mobile game project with Firebase backend integration, featuring tournaments, leaderboards, and multi-language support. The project uses Clean Architecture with Domain-Driven Design (DDD) principles and targets primarily Android and iOS platforms.

**Key Technologies:**
- Unity 6 (2D URP)
- Firebase Authentication & Firestore
- C# with IL2CPP backend (Android)
- Unity's New Input System
- Addressables for asset management

## Architecture

The project follows **Clean Architecture** with clear separation of concerns across distinct modules:

### Module Organization (Assets/_Project/Scripts/)

1. **Core/** - Application initialization, configuration, and utilities
   - Foundation layer for app bootstrapping

2. **Domain/** - Business logic and game rules
   - `Gameplay/` - Core game mechanics
   - `Score/` - Scoring system logic

3. **Services/** - External service integration layer
   - `Auth/` - Firebase authentication
   - `Leaderboards/` - Rankings via Firebase Firestore
   - `Storage/` - Cloud and local data persistence
   - `Tournaments/` - Tournament management

4. **UI/** - Presentation layer (all user interface)
   - `RuntimeUI/` - Dynamic UI factory, styling, and transitions
   - `Common/`, `Login/`, `MainMenu/`, `Game/`, `Scores/`, `Settings/`, `Tournaments/`
   - Uses factory pattern (`UIFactory.cs`) for component instantiation

5. **Scenes/** - Scene composition and lifecycle management
   - `SceneDirector.cs` - Manages scene loading/transitions
   - `Builders/` - Scene setup patterns

6. **Localization/** - Multi-language support
   - `ILocalizationService.cs` - Service interface
   - `JsonLocalizationService.cs` - JSON-based implementation

7. **Telemetry/** - Analytics and usage tracking

8. **Test/** - Automated testing
   - `EditMode/` - Unit tests
   - `PlayMode/` - Integration tests

### Key Architectural Patterns

- **Service Locator Pattern**: Services accessed via interfaces (e.g., `ILocalizationService`)
- **Factory Pattern**: UI components created through `UIFactory`
- **Scene-based Architecture**: 7 distinct scenes with Boot scene for initialization
  - Boot → Login → MainMenu → {Game, Scores, Tournaments, Settings}

## Scene Structure

The project uses 7 Unity scenes located in `Assets/_Project/Scenes/`:

1. **Boot.unity** - Application initialization (entry point)
2. **Login.unity** - Firebase authentication
3. **MainMenu.unity** - Main navigation hub
4. **Game.unity** - Core gameplay
5. **Scores.unity** - Leaderboards display
6. **Tournaments.unity** - Tournament management
7. **Settings.unity** - User settings/preferences

## Development Commands

### Building
```bash
# Unity command line builds (from Unity installation directory)
Unity.exe -quit -batchmode -projectPath "C:\Users\josec\digitPark" -buildTarget Android -executeMethod BuildCommand.Build

# Or use Unity Editor: File > Build Settings
```

### Testing
```bash
# Run tests via Unity Test Runner
# Window > General > Test Runner in Unity Editor
# Tests located in Scripts/Test/EditMode/ and Scripts/Test/PlayMode/
```

### Firebase Configuration
- **Android Package**: `com.MatrixSoftware.com`
- **iOS Bundle ID**: `com.MatrixSoftware.com`
- Firebase configuration files (google-services.json, GoogleService-Info.plist) must be placed in appropriate platform directories
- Firebase project: `digitpark-53ad5`

## Key Dependencies

### Unity Packages (Packages/manifest.json)
- `com.unity.inputsystem` (1.14.2) - New Input System
- `com.unity.render-pipelines.universal` (17.0.4) - URP 2D rendering
- `com.unity.test-framework` (1.6.0) - Testing framework
- `com.unity.feature.2d` (2.0.1) - Complete 2D feature set
- `com.unity.timeline` (1.8.9) - Cutscenes/animations
- `com.unity.ugui` (2.0.0) - Canvas UI system

### Third-Party
- **Firebase SDK** (v13.1.0+): Auth, Firestore, Realtime Database
- **External Dependency Manager**: Google Play Services integration
- **Addressables**: Dynamic asset loading system

## Project Settings

- **Company**: DefaultCompany
- **Product Name**: digitPark
- **Android Min SDK**: API 23 (Android 6.0)
- **Target Orientation**: Auto-rotation (all orientations supported)
- **Scripting Backend**: IL2CPP (Android), Mono (Editor)
- **API Compatibility**: .NET Standard 2.1
- **Color Space**: Linear
- **Graphics**: Universal Render Pipeline (2D)

## Code Organization Principles

1. **Namespace per Module**: Each major folder should have corresponding namespaces
   - Example: `namespace DigitPark.Services.Auth { ... }`

2. **Interface-first Design**: Services expose interfaces for testability and loose coupling

3. **Prefab-driven UI**: UI components are instantiated from prefabs in `Assets/_Project/Prefabs/UI/`

4. **ScriptableObjects for Data**: Configuration stored in `Assets/_Project/Data/ScriptableObjects/`

5. **Scene Additivity**: Scenes can be loaded additively for overlay UI (Settings, etc.)

## Development State

The project is in **early development** with:
- ✅ Complete architectural foundation and folder structure
- ✅ Scene setup (7 scenes configured)
- ✅ Firebase integration configured
- ✅ UI organization and prefab structure
- 🚧 Domain logic implementation in progress
- 🚧 Service implementations pending
- 🚧 Core scripts are skeleton classes awaiting implementation

When implementing new features:
1. Follow the established module structure
2. Keep business logic in Domain/ layer
3. External integrations belong in Services/
4. UI logic stays in UI/ layer (never mix with domain logic)
5. Use dependency injection through service interfaces

## Platform-Specific Notes

### Android
- IL2CPP scripting backend enabled for performance
- Minimum SDK 23, no target SDK restriction
- Resizable activity enabled
- ARMv7 architecture support

### iOS
- Minimum iOS version: 13.0
- tvOS minimum: 13.0
- Requires development team configuration for signing
- Metal rendering API

## Assets Organization

- `Art/` - Fonts, icons, UI graphics
- `Audio/` - Music and SFX
- `Data/ScriptableObjects/` - Game configuration data
- `Prefabs/` - Reusable game objects and UI templates
- `Resources/` - Runtime-loaded assets
- `Settings/` - Input and quality settings
- `ThirdParty/` - Firebase and Addressables configurations
