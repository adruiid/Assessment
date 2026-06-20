# Unity Engineer Assessment

Mock RPG HUD and Menu System built for a Unity engineering take-home assessment.

The assignment requested Unity `6000.3.10f1`. This project was completed on Unity `6000.4.10f1`, which is a nearby Unity 6 patch version. The implementation follows the required architectural rules: strict MVC, Zenject dependency injection, UniRx reactive state/events, UniTask async flow, Addressables, Unity Localization, TextMeshPro, DOTween, and uGUI.

## How To Open And Run

1. Open the project folder in Unity Hub:

   ```text
   C:/Assessment
   ```

2. Use Unity `6000.4.10f1`.

3. Open the main menu scene:

   ```text
   Assets/Scenes/Main Menu.unity
   ```

4. Press Play.

5. Use the Play button to load the gameplay scene:

   ```text
   Assets/Scenes/In Game.unity
   ```

The scene names are configured through:

```text
Assets/ScriptableObjects/App/SceneConfig.asset
```

## Architecture Overview

The project uses strict MVC:

- Models are pure C# classes that own state through `ReactiveProperty<T>`.
- Views are `MonoBehaviour` classes that only render UI, play visual transitions, and expose UI interactions as `IObservable` streams.
- Controllers are pure C# classes bound through Zenject. They subscribe to view/model streams, make workflow decisions, run async tasks, load Addressables, and dispose subscriptions.

Dependency lifetime is split by Zenject context:

- `ProjectContext` owns cross-scene services and shared config assets.
- `SceneContext` owns scene-local views, models, and controllers.
- Scene views are not bound globally.

The shared `App` folder contains only cross-scene infrastructure:

```text
Assets/Scripts/App/Config
Assets/Scripts/App/Installer
Assets/Scripts/App/Persistence
Assets/Scripts/App/Services
```

This is used for small services such as scene navigation, settings persistence, localization, quality settings, application quit, audio mixer access, and Addressable asset loading.

## Implemented Screens

### Main Menu

- Logo/title area.
- Play, Settings, and Quit buttons.
- DOTween entrance animation with eased motion.
- Version label driven by `GameVersionConfig`.
- Static labels are localized through Unity Localization components in the scene.

### Settings Panel

- Overlay canvas on the Main Menu scene.
- Master, Music, and SFX sliders.
- Settings saved through a PlayerPrefs-backed persistence adapter.
- Graphics dropdown using Low, Medium, and High localized labels.
- Language dropdown updates Unity Localization live without reloading the scene.
- Panel open/close transitions use DOTween and UniTask cancellation.

### Gameplay HUD

- Reactive player health bar.
- Animated currency counter.
- Minimap `RawImage` placeholder with a Render Texture.
- Three skill buttons with Addressable icons.
- Skill cooldown overlays use radial fill animation.
- Boss health bar appears on skill use and hides after a 5 second async delay.
- Toast notifications are processed through a queue so messages show one at a time.

### Inventory Modal

- Overlay canvas on the Gameplay scene.
- 12-slot grid with ScrollRect support.
- Minimum 6 ScriptableObject-backed item assets.
- Item icons load through Addressables.
- Empty slots render with a different visual state.
- Clicking an item shows a shared detail panel with localized name, localized description, icon, and stat fill.
- Addressable handles are released in controller disposal.

## Localization Setup

Localization assets live under:

```text
Assets/Localization
```

Current string tables include:

- `Menu Labels`
- `Settings`
- `HUD`
- `Notification`
- `Inventory`

Visible static labels are intended to be connected in the Inspector using Unity Localization components. Dynamic strings such as item names, item descriptions, toast messages, and graphics option labels are resolved through the injected `ILocalizationService`.

## Addressables Setup

Runtime sprites and icons are loaded through Addressables instead of `Resources.Load`.

Used Addressable flows:

- HUD skill icons are configured in `HudConfig`.
- Inventory item icons are configured in `InventoryItemData` assets.
- Loaded `AsyncOperationHandle<Sprite>` values are stored by controllers and released on dispose.

Addressables service:

```text
Assets/Scripts/App/Services/AddressableAssetService.cs
```

## Async And Memory Safety

The project avoids coroutines and `Update()` polling.

- Timed waits use UniTask with `CancellationToken`.
- DOTween animations are awaited through UniTask-compatible async calls.
- Controllers own `CompositeDisposable` instances and dispose UniRx subscriptions.
- Addressables handles are released deterministically.
- View lifecycle cancellation uses `GetCancellationTokenOnDestroy()`.

## UI And Mobile Notes

Canvas Scaler target:

```text
Scale With Screen Size
Reference Resolution: 1920 x 1080
```

The project was primarily built and tested in desktop Game view, with layout checks intended for:

- 16:9
- 18:9
- 4:3

Mobile safe-area support is handled with a simple no-code Player Settings approach for now:

- Android `Render outside safe area` disabled.
- Android system insets/status/navigation areas kept reserved when mobile safe-area behavior is needed.

With more time, a `Screen.safeArea` layout component could be added to move HUD and modal roots inside the safe rectangle while still allowing fullscreen rendering behind notches.

## Final Audit Commands

These commands were used during the final pass:

```text
dotnet build Assessment.slnx -v minimal
rg -n "FindObjectOfType|\.Instance|static |Resources\.Load|IEnumerator|StartCoroutine|Update\(|UnityEngine\.UI\.Text" Assets/Scripts -g "*.cs"
rg -n "m_text: (New Text|Button|Option A|Option B|Option C)|m_Text: (Option A|Option B|Option C)" Assets/Scenes -g "*.unity"
```

Expected result:

- Build succeeds.
- No banned script patterns are found.
- No default Unity placeholder text remains in scenes.

## Known Limitations And Improvements

- Unity version is `6000.4.10f1` instead of the requested `6000.3.10f1`.
- Mobile safe area is documented and handled through Android Player Settings, not through a custom runtime safe-area component.
- The gameplay systems are UI/demo-focused. Health, boss damage, currency, and item stats are driven by config/model data for assessment demonstration rather than a full combat/inventory backend.
- A full production version would add automated Unity PlayMode tests, stronger UI snapshot testing, and a reusable theme config for all colors/spacing.
