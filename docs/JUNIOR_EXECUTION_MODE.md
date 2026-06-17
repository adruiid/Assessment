# Junior Execution Mode

This file is the practical companion to `AGENTS.md`.

Use it in future chats when giving step-by-step guidance for this assessment. The goal is to keep the project architecture valid while reducing unnecessary complexity for a junior-level implementation.

## Ground Rule

We do **not** relax the core assessment constraints:

- strict MVC
- Zenject / Extenject DI
- UniRx reactive state
- UniTask instead of coroutines
- Addressables instead of `Resources.Load`
- no singletons
- no `FindObjectOfType`
- no `Update()` polling
- no business logic in Views

What we **do** relax is the amount of abstraction and ceremony used to satisfy those constraints.

## How To Explain Each Step

For every future step, structure the explanation like this:

1. What this step is for
2. What must exist when the step is done
3. What to do in the Unity Editor
4. What code files to create
5. What can wait until later
6. Common mistakes to avoid

Keep instructions concrete. Prefer:

- exact folder names
- exact component names
- exact serialized field examples
- simple class counts

Avoid:

- speculative abstractions
- extra interface layers that are not needed yet
- splitting one obvious class into several tiny classes too early

## Approved Simplifications By Area

### ScriptableObjects

Use plain ScriptableObject classes unless polymorphism is genuinely needed.

Good junior-friendly examples:

- `GameVersionConfig`
- `SceneConfig`
- `SettingsDefaultsConfig`
- `HudConfig`
- `InventoryItemData`

Avoid creating interfaces for these unless the data must be swapped behind DI.

### Localization

For static labels:

- use `Localize String Event` in the Inspector
- bind directly to TMP labels

For dynamic labels controlled by runtime state:

- let the Controller decide what key or value to show
- let the View only render the final string/value

### Installers

Prefer:

- one `ProjectContext` installer for shared services/config
- one scene installer per scene
- optional feature sub-installers only if the scene gets crowded

Avoid advanced installer nesting unless the scene has become hard to manage.

### Models And Controllers

Prefer one main Model and one main Controller per feature at first:

- `MainMenuModel` / `MainMenuController`
- `SettingsModel` / `SettingsController`
- `HudModel` / `HudController`
- `InventoryModel` / `InventoryController`

Only split when one class becomes hard to read.

### Shared Services

Keep `App` small. It is only for cross-scene needs such as:

- scene navigation
- settings persistence
- localization switching
- addressables loading helper

Do not create generic frameworks or base-class hierarchies unless repeated code clearly justifies it.

## Fixed Project Step Numbers

Use these step numbers in future chats. If the user says "go to step 6," they mean this list:

- Step 0 - Project rules and constraints
- Step 1 - Baseline audit
- Step 2 - Folder/project normalization
- Step 3 - Simple config/data layer
- Step 4 - Shared App services and ProjectContext
- Step 5 - SceneContext and scene-level foundations
- Step 6 - Main Menu MVC
- Step 7 - Settings MVC
- Step 8 - Gameplay HUD core MVC
- Step 9 - HUD advanced features
- Step 10 - Inventory MVC
- Step 11 - Polish and responsive pass
- Step 12 - Audit and submission

## Step-By-Step Simplified Path

## Step 1 - Project Baseline

Minimum outcome:

- packages installed
- folder structure valid
- scenes in build settings
- localization initialized
- addressables initialized
- canvas scaler corrected

Do not block the user on optional cleanup unless it risks the assessment.

## Step 2 - Folder And Project Normalization

Minimum outcome:

- `App` folders exist
- required feature folders exist
- scene names and build settings are sane
- no broken or misleading folder names remain

What this step usually includes:

- creating the `App` shared folders
- creating the required MVC folders for each feature
- correcting naming issues like spaces or misspellings when needed
- checking scenes are present in build settings

Do not start service binding or ProjectContext setup here unless the user explicitly merges steps.

## Step 3 - Simple Config/Data Layer

Minimum outcome:

- create only the config assets needed to unblock UI work

Recommended minimal assets:

- `GameVersionConfig`
- `SceneConfig`
- `SettingsDefaultsConfig`
- `HudConfig`
- `InventoryItemData` assets for at least 6 items

Optional:

- `InventoryConfig` if slot count/layout values should live outside code

Do not add interfaces to these assets unless a later step proves the need.

## Step 4 - Shared App Services And ProjectContext

Minimum outcome:

- `ProjectContext` exists
- only the truly needed shared services/configs are bound
- no scene Views are bound globally

Use a small list of shared items:

- `SceneConfig`
- `GameVersionConfig`
- `SettingsDefaultsConfig`
- `ISettingsPersistence`
- `ISceneNavigator`
- `ILocalizationService`
- `IAddressableAssetService`

Skip theme systems, advanced config hierarchies, and extra service layers unless needed later.

## Step 5 - SceneContext And Scene-Level Foundations

Minimum outcome:

- Main Menu scene has a `SceneContext`
- Gameplay scene has a `SceneContext`
- scene-local Views can be bound in the correct scene
- no global scene object lookups are needed

Keep scene installers small and obvious.

## Step 6 - Main Menu MVC

Minimum outcome:

- main menu canvas present
- localized labels visible
- play/settings/quit button events exposed by View
- version label pulled from config
- entrance animation triggered by Controller

Keep it simple:

- `MainMenuView` references buttons/panel/version TMP
- `MainMenuModel` stores whether settings is open
- `MainMenuController` wires clicks to scene navigation and panel visibility

## Step 7 - Settings MVC

Minimum outcome:

- settings panel opens/closes
- sliders restore saved values
- slider changes persist
- graphics dropdown works
- language toggle changes live

Keep the View dumb:

- expose slider/dropdown/toggle/button observables
- provide render methods like `SetMasterVolume`, `SetLanguageState`, `SetVisible`

## Step 8 - Gameplay HUD Core MVC

Minimum outcome:

- player health bar reacts
- currency label animates
- boss bar can show/hide separately
- minimap placeholder is assigned

Start with demo data if gameplay systems do not exist yet.

## Step 9 - HUD Advanced Pieces

Minimum outcome:

- 3 skill buttons
- cooldown fill overlay
- disabled state during cooldown
- sequential toast queue

Keep cooldown data in `HudConfig` or Model state. Do not create a full skill framework unless needed.

## Step 10 - Inventory MVC

Minimum outcome:

- 12 visible slots
- 6+ item data entries
- empty slots render differently
- click shows details
- icons loaded via Addressables

Keep inventory data flat and readable. A simple list is enough.

## Step 11 - Polish And Responsive Pass

Minimum outcome:

- safe-area plan documented or implemented
- major aspect ratios checked
- button states verified
- transitions feel intentional

This is where visual cleanup happens. Do not do it too early.

## Step 12 - Audit And Submission

Minimum outcome:

- banned-pattern search run
- leaked subscriptions checked
- addressables release points checked
- README updated
- version mismatch noted honestly if still present

## What Future Chats Should Avoid

- suggesting a refactor just because it is "cleaner"
- replacing simple Inspector setup with code-only setups for no gain
- insisting on interfaces for every data object
- introducing factories, signals, or generic presenters before the project actually needs them
- turning one feature step into a large framework step

## Good Default Tone

Future chats should act like a patient technical mentor:

- explain why
- keep steps small
- say what is enough for now
- mark extra polish as optional
- help the user finish without feeling buried
