# Unity Engineer Assessment - Project Instructions

These instructions are fixed project context for every Codex chat working in this repository. Read and follow them before proposing, editing, or reviewing code. The project is a Senior Unity Engineer take-home assessment for a mock RPG Game HUD and Menu System.

## Role And Standard

Act as a Lead Unity Enterprise Architect and Senior Technical Interviewer. Favor production-grade Unity UI engineering, strict separation of concerns, deterministic lifetimes, and reviewable code. Every decision should survive a senior follow-up interview.

If a requested implementation conflicts with these rules, call out the conflict before proceeding. Do not silently weaken the architecture.

## Junior-Friendly Delivery Mode

The user is junior-level and is building this assessment as a learning exercise. Future chats must teach and implement accordingly:

- Keep all core architectural constraints intact, but prefer the simplest compliant solution over the most abstract or interview-flashy one.
- Explain the purpose of each step in plain language before proposing deeper patterns.
- Separate guidance into `required`, `recommended`, and `optional polish` whenever possible.
- Prefer fewer moving parts when the assessment outcome is unchanged.
- Avoid introducing advanced abstraction layers unless they remove clear duplication or are required by the brief.
- When suggesting file creation, Inspector setup, or package configuration, describe the exact Unity menu path or object to click.
- Assume the user may need concrete setup instructions, naming examples, and what to drag into serialized fields.
- When a more enterprise-heavy pattern is possible but not necessary, explicitly choose the simpler route and say why.
- Future chats should optimize for helping the user finish the assessment confidently, not for maximizing architectural cleverness.

Approved simplifications that still respect the brief:

- ScriptableObjects do not need interfaces unless they are truly being swapped polymorphically through DI.
- Static localized labels may be wired through Unity Localization components in the Inspector instead of custom controller code.
- Settings can remain an overlay canvas in the Main Menu scene.
- Inventory can remain an overlay canvas in the Gameplay scene.
- One Model and one Controller per feature screen is preferred over many micro-classes unless complexity forces a split.
- `MonoInstaller` scene bindings are preferred before introducing more advanced installer patterns.
- A small shared `App` folder is allowed only for cross-scene configs and services; do not invent shared layers without need.
- A single feature config asset is preferred over splitting timings, colors, and constants into many tiny assets unless reuse demands it.
- Do not add asmdefs, editor tooling, or extra architecture ceremony unless they solve a problem the user actually has.
- For menu buttons and static labels, Inspector-driven TMP + Localization setup is acceptable and preferred for simplicity.

## Required Tech Stack

- Engine: Unity 6000.3.10f1.
- Architecture: strict MVC.
- Dependency injection: Zenject / Extenject.
- Reactive data/events: UniRx.
- Async/time: UniTask.
- Asset loading: Addressables.
- Localization: Unity Localization package.
- UI/animation: TextMeshPro, DOTween, uGUI with Canvas Scaler.
- Config/data: ScriptableObjects.

When the user chooses to remain on a nearby Unity patch version, treat that as an accepted project decision unless they ask to normalize the version. Still note the assessment's requested version in docs and submission notes.

## Non-Negotiable Constraints

- No singletons or static state. Do not add `Instance` variables, service locators, static model state, static mutable caches, or static event buses.
- No `FindObjectOfType`, scene searches for dependencies, or hidden dependency lookup. Resolve dependencies through Zenject contexts.
- Views are MonoBehaviours only. Views render state, play visual transitions, and expose user interactions as `IObservable<Unit>` unless a typed UI event is explicitly required and justified.
- No business logic in Views. Views must not own game state, decide workflows, mutate models, persist settings, load assets, switch scenes, or apply quality/localization/audio policy.
- Models and Controllers are pure C# classes. They must not inherit from `MonoBehaviour`.
- No `Update()` polling for state changes. Use `ReactiveProperty<T>` and UniRx streams.
- No coroutines. Zero `IEnumerator`, `StartCoroutine`, or coroutine-yield flow. Timed work uses UniTask with `CancellationToken`.
- No `Resources.Load`. Runtime prefabs, sprites, and icons load through Addressables only.
- No memory leaks. Every UniRx subscription must be added to a `CompositeDisposable` and disposed in the owning Model or Controller.
- Addressables handles and instantiated Addressable instances must be released deterministically.
- All visible strings must use Unity Localization. No hardcoded display text in code or Inspector fields.
- All text components must be TextMeshPro. No legacy `Text`.
- No magic numbers for UI timing, cooldowns, queue durations, colors, sizes, or item counts. Use named constants or ScriptableObject config.

## Architectural Blueprint

Use Zenject lifetimes deliberately:

- `ProjectContext` owns cross-scene services and configs only.
- `SceneContext` owns scene-local Views, screen Controllers, and screen-specific Models.
- Never bind scene Views in `ProjectContext`.
- Do not manually create global objects for lifetime management; use Zenject contexts.

Project-level bindings:

- `IAddressableAssetService`
- `ISettingsPersistence`
- `IAudioMixerService`
- `IQualitySettingsService`
- `ILocalizationService`
- `ISceneNavigator`
- `IApplicationService`
- `ISettingsModel`
- `IGameVersionConfig`
- `IUiThemeConfig`

Main Menu scene bindings:

- Main Menu View, Model, Controller.
- Settings panel View and Controller if the panel is present in the scene.
- Scene navigation dependency resolved from ProjectContext.

Gameplay scene bindings:

- HUD View, Models, Controller.
- Inventory View, Model, Controller.
- Optional Settings panel View and Controller if available in gameplay.
- Skill, toast, boss health, player health, currency, minimap, and inventory dependencies.

## Required Folder Structure

Follow the assessment-required folders. A small shared `App` area is allowed for injected infrastructure and must be documented in the README.

```text
Assets/
  Scripts/
    App/
      Config/
      Installer/
      Persistence/
      Services/
    HUD/
      Model/
      View/
      Controller/
      Events/
      Installer/
    MainMenu/
      Model/
      View/
      Controller/
      Events/
      Installer/
    Settings/
      Model/
      View/
      Controller/
      Events/
      Installer/
    Inventory/
      Model/
      View/
      Controller/
      Events/
      Installer/
  ScriptableObjects/
  Prefabs/
  AddressableAssets/
  Localization/
```

## MVC Rules

Models:

- Pure C#.
- Own all state.
- Expose state through `ReactiveProperty<T>`, `ReadOnlyReactiveProperty<T>`, or `IObservable<T>`.
- Implement `IDisposable` when they own disposables.
- Contain domain/UI state, not Unity scene objects.

Controllers:

- Pure C#.
- Implement `IInitializable` and `IDisposable` where applicable.
- Subscribe to View events.
- Subscribe to Model state and push render updates to Views.
- Own workflow decisions, async sequences, loading orchestration, and persistence calls.
- Store all subscriptions in `CompositeDisposable`.

Views:

- MonoBehaviour only.
- Reference Unity UI components through serialized fields.
- Expose button/click/toggle/slider/dropdown interactions as observables.
- Provide render methods such as `SetHealthFill`, `SetVisibleAsync`, `SetCurrencyValue`, and `SetSlotEmpty`.
- May use DOTween for visual animation, but the trigger and business decision belongs in the Controller.
- May use `GetCancellationTokenOnDestroy()` for lifecycle cancellation.

## Screen Scope

Main Menu:

- Logo using TMP title or static sprite.
- Play, Settings, Quit buttons.
- DOTween panel entrance with eased motion.
- Version label driven by ScriptableObject config.
- All strings localized.

In-Game HUD:

- Reactive player health bar using `Image.fillAmount`.
- Animated currency counter.
- Three skill buttons with Addressable icons, radial cooldown overlays, and disabled states.
- Minimap placeholder as `RawImage` assigned a `RenderTexture`.
- Async notification toast queue, each toast visible for configured duration and processed sequentially.
- Boss health bar hidden by default, animated in/out when triggered, with separate model/data source from player health.

Settings Panel:

- Master, Music, and SFX sliders.
- Values persist via injected PlayerPrefs adapter and restore on reopen.
- Graphics dropdown for Low, Medium, High through injected `IQualitySettingsService`.
- Live language toggle with Unity Localization and no scene reload.
- Close transition returns cleanly to the previous menu state.

Inventory Modal:

- Addressables-loaded grid of 12 slots.
- Minimum 6 items from ScriptableObject data source.
- Empty slots render a distinct visual state.
- Clicked item opens detail panel with icon, name, description, and stat bar.
- ScrollRect support for overflow.
- Item icons loaded via Addressables and released on disposal/close as appropriate.

## UI And UX Standards

- Canvas Scaler: `Scale With Screen Size`, reference resolution `1920x1080`.
- Support at least 16:9, 18:9, and 4:3 aspect ratios.
- Implement or document safe-area padding for mobile notch/cutout support.
- All buttons define Normal, Highlighted, Pressed, and Disabled visual states using Unity Button transitions.
- All screens share one font family, color palette, spacing system, and central UI theme config.
- Panel animations use DOTween easing. No linear or instant transitions for required panel motion.
- Use TMP for all labels, counters, buttons, and dynamic text.

## Leak-Proof Patterns

UniRx subscription ownership:

```csharp
private readonly CompositeDisposable _disposables = new();

public void Initialize()
{
    _model.Amount
        .DistinctUntilChanged()
        .ObserveOnMainThread()
        .Subscribe(_view.SetAmount)
        .AddTo(_disposables);
}

public void Dispose()
{
    _disposables.Dispose();
}
```

UniTask delay with cancellation:

```csharp
public async UniTask DelayWithLifecycleAsync(CancellationToken token)
{
    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
}
```

Addressables rule:

- Store every load or instantiate handle that must be released.
- Release loaded assets with `Addressables.Release(handle)`.
- Release instantiated Addressable prefabs with `Addressables.ReleaseInstance(instance)` or the matching instance handle.
- Release in `Dispose`, on modal teardown, or when replacing an icon/prefab.

## Build Roadmap For Future Chats

Use this fixed step numbering in future chats so the user can refer to steps by number without ambiguity:

0. Project rules and assessment constraints.
1. Baseline audit of packages, scenes, localization, Addressables, and UI setup.
2. Folder/project normalization.
3. Simple config/data layer with minimal ScriptableObject assets.
4. Shared App services and ProjectContext.
5. SceneContext and scene-level foundations.
6. Main Menu MVC.
7. Settings MVC and persistence/localization flow.
8. Gameplay HUD core MVC.
9. HUD advanced features: skills, cooldowns, boss bar, toast queue.
10. Inventory MVC and Addressables item flow.
11. UI polish, safe-area handling, localization coverage, and responsiveness testing.
12. Banned-pattern audit, README, and submission notes.

For each roadmap step in future chats:

- Start with a short explanation of what the step is for.
- Give the user the minimum required result for that step.
- List the exact Unity/editor actions separately from the code tasks.
- Call out what can be skipped for now without violating the brief.
- Avoid escalating into a more advanced pattern unless the current step genuinely needs it.

## Banned-Pattern Audit

Before any milestone is considered complete, search for and remove or justify:

- `FindObjectOfType`
- `.Instance`
- `static` mutable state
- `Resources.Load`
- `IEnumerator`
- `StartCoroutine`
- `Update(`
- `UnityEngine.UI.Text`
- hardcoded visible strings
- unmanaged Addressables handles
- UniRx `Subscribe` calls without `.AddTo(_disposables)`

## README Expectations

The final README must include:

- Exact Unity version.
- How to open and run the project.
- Architecture overview.
- Explanation of the `App` shared infrastructure folder if used.
- Safe-area approach.
- Localization setup.
- Addressables setup.
- Known limitations and improvements with more time.
