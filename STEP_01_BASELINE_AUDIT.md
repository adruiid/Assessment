# Step 1 - Baseline Audit

Date: 2026-06-16

Purpose: verify the current Unity project state before architecture work begins. This audit is measured against `AGENTS.md` and the assessment PDF requirements.

## Executive Summary

Step 1 audit is complete. The project is a very early Unity UI project with some required packages already present, but it is not yet ready for implementation work without cleanup and setup.

Highest-priority findings:

1. Unity version mismatch: the project records `6000.4.10f1`, while the assessment requires `6000.3.10f1`.
2. UniTask is missing.
3. Required MVC folders are incomplete and one folder name uses `Main Menu` instead of assessment-friendly `MainMenu`.
4. `Assets/AdressableAssets` is misspelled; required folder is `Assets/AddressableAssets`.
5. Addressables and Localization packages are installed, but their project data/assets are not initialized.
6. The Gameplay/In Game scene is not included in Build Settings.
7. The current Main Menu scene Canvas Scaler uses Constant Pixel Size with `800x600`, not Scale With Screen Size at `1920x1080`.
8. Imported UniRx and Zenject sample/test/plugin folders contain code patterns that will pollute strict banned-pattern audits unless we exclude, remove, or document third-party package code.

## Unity Project State

Project file:

- `ProjectSettings/ProjectVersion.txt`

Recorded version:

```text
m_EditorVersion: 6000.4.10f1
m_EditorVersionWithRevision: 6000.4.10f1 (feeafc12a938)
```

Assessment-required version:

```text
6000.3.10f1
```

Decision needed:

- Preferred: open and save the project with Unity `6000.3.10f1` before final submission.
- Acceptable only if forced: keep `6000.4.10f1` and explicitly document the deviation in `README.md`.

Risk:

- Reviewers may treat version mismatch as avoidable non-compliance.

## Package And Dependency State

Installed through `Packages/manifest.json`:

- `com.unity.addressables` = `2.9.1`
- `com.unity.localization` = `1.5.12`
- `com.unity.ugui` = `2.0.0`
- `com.unity.inputsystem` = `1.19.0`
- `com.unity.render-pipelines.universal` = `17.4.0`
- `com.unity.test-framework` = `1.6.0`

Vendored under `Assets/Plugins`:

- DOTween: present under `Assets/Plugins/Demigiant/DOTween`
- UniRx: present under `Assets/Plugins/UniRx`, ReadMe reports `6.2.2`
- Zenject: present under `Assets/Plugins/Zenject`, `Version.txt` reports `9.2.0`

Missing or not detected:

- UniTask / Cysharp.Threading.Tasks
- TextMeshPro package/assets were not detected by text scan outside Unity internals

Important note:

- DOTween has `Assets/Resources/DOTweenSettings.asset`. This is a normal DOTween setup artifact, but the assessment has a strict no-`Resources.Load` rule. Our app code must never use `Resources.Load`, and we should document or leave plugin-owned DOTween settings alone unless the final audit requires stricter cleanup.

## Folder Structure Audit

Current top-level relevant folders:

```text
Assets/
  AdressableAssets/
  Localization/
  Plugins/
  Prefabs/
  Resources/
  Scenes/
  ScriptableObjects/
  Scripts/
  Settings/
```

Assessment-required folders:

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

Current `Assets/Scripts` state:

```text
Assets/Scripts/
  HUD/
    Controller/
    Installer/
    Model/
    View/
  Inventory/
  Main Menu/
    Controller/
    Installer/
    Model/
    View/
  Settings/
```

Gaps:

- Missing `Assets/Scripts/App`.
- Missing `Events` folders for `HUD` and `Main Menu`.
- `Inventory` is missing `Model`, `View`, `Controller`, `Events`, `Installer`.
- `Settings` is missing `Model`, `View`, `Controller`, `Events`, `Installer`.
- `Main Menu` should become `MainMenu` to match conventional C# namespace/folder naming and the assessment language.
- `Assets/AdressableAssets` should become `Assets/AddressableAssets`.

Recommendation for Step 2:

- Normalize folders before adding scripts.
- Preserve `.meta` files where possible if moving through Unity; if moving by filesystem, verify Unity meta behavior after opening.

## Scene And Build Settings Audit

Scenes present:

```text
Assets/Scenes/Main Menu.unity
Assets/Scenes/In Game.unity
```

Build settings:

```text
Enabled:
  Assets/Scenes/Main Menu.unity

Missing:
  Assets/Scenes/In Game.unity
```

Scene setup detected:

- `Main Menu.unity` contains `MainMenu UI`.
- `Main Menu.unity` contains a `CanvasScaler`.
- `CanvasScaler` currently has:

```text
m_UiScaleMode: 0
m_ReferenceResolution: {x: 800, y: 600}
```

Assessment requires:

```text
Scale With Screen Size
Reference Resolution 1920x1080
```

No detected scene references to:

- `SceneContext`
- `ProjectContext`
- `MonoInstaller`
- TextMeshPro components

Recommendation:

- Step 5 should rebuild scene foundations intentionally instead of patching ad hoc objects.
- Add both Main Menu and Gameplay scenes to Build Settings.
- Rename scene files eventually to `MainMenu.unity` and `Gameplay.unity` if we want clean naming, but do that only when ready to update build settings/references.

## Addressables And Localization Audit

Packages:

- Addressables installed.
- Localization installed.

Project data/assets:

- No `Assets/AddressableAssetsData` folder detected.
- `Assets/AdressableAssets` exists but is misspelled and empty.
- `Assets/Localization` exists but appears empty.

Recommendation:

- Initialize Addressables settings in Unity before building inventory icons and prefab loading.
- Create the correctly spelled `Assets/AddressableAssets` folder.
- Create Localization settings, locales, string tables, and localized asset references before implementing screen Views.

## Zenject State

Zenject is present as a vendored plugin:

```text
Assets/Plugins/Zenject
Version: 9.2.0
```

Concerns:

- Optional Extras, Integration Tests, Unit Tests, Samples, and Reflection Baking folders are present.
- These folders add noise and may trigger banned-pattern scans.
- Three Zenject `.csproj.meta` files are currently deleted in git status, likely from Unity regenerating solution files.

Recommendation:

- Do not use Zenject optional samples/tests for app implementation.
- Later cleanup should consider removing unnecessary Zenject OptionalExtras and UniRx Examples if this does not break package functionality.
- Keep app installers in `Assets/Scripts/**/Installer`, not inside plugin folders.

## UniRx State

UniRx is present as vendored source:

```text
Assets/Plugins/UniRx
Version: 6.2.2
```

Concerns:

- `Assets/Plugins/UniRx/Examples` contains sample scripts with coroutine-related code.
- UniRx internals contain coroutine bridge code by design.

Recommendation:

- Do not write app code using UniRx coroutine bridge APIs.
- Prefer `ReactiveProperty<T>`, `ReadOnlyReactiveProperty<T>`, `Subject<T>` where appropriate, and `CompositeDisposable`.
- Consider removing `Examples` before final submission.

## DOTween State

DOTween is present:

```text
Assets/Plugins/Demigiant/DOTween
Assets/Resources/DOTweenSettings.asset
```

Project settings include `DOTWEEN` scripting define symbols.

Recommendation:

- Use DOTween only inside Views for visual animation execution.
- Trigger animation from Controllers.
- Convert tweens to UniTask with cancellation once UniTask is installed.

## Banned-Pattern Audit

Project-owned code scan excluding `Assets/Plugins` found no current hits for:

- `FindObjectOfType`
- `.Instance`
- `Resources.Load`
- `IEnumerator`
- `StartCoroutine`
- `Update(`
- `UnityEngine.UI.Text`
- `Subscribe(`

Reason:

- There is essentially no project-owned implementation code yet.

Important caveat:

- A full `Assets` scan would produce many hits from imported third-party packages, examples, tests, generated docs, and plugin internals. Final audit should separate:
  1. app code, which must be clean;
  2. third-party package internals, which should be minimized and documented.

## Git State

Current branch:

```text
main
```

Recent history:

```text
90cb0cb Initial commit
```

Current status:

```text
D  Assets/Plugins/Zenject/OptionalExtras/ReflectionBaking/Zenject-ReflectionBaking.csproj.meta
D  Assets/Plugins/Zenject/OptionalExtras/Signals/Zenject-Signals.csproj.meta
D  Assets/Plugins/Zenject/Source/Zenject.csproj.meta
M  ProjectSettings/EditorBuildSettings.asset
M  ProjectSettings/ProjectSettings.asset
M  README.md
?? AGENTS.md
?? STEP_01_BASELINE_AUDIT.md
```

Notes:

- `AGENTS.md` and `STEP_01_BASELINE_AUDIT.md` are intentional Codex/project documentation.
- `README.md` was intentionally updated to point to `AGENTS.md`.
- ProjectSettings files appear dirty in status, but no content diff was reported in this audit; likely line ending or Unity metadata touch.
- Deleted Zenject `.csproj.meta` files appear unrelated to the architecture work and should not be restored or removed without a deliberate cleanup decision.

## Logs

Project `Logs/` scan found no current text hits for:

- error
- exception
- failed
- warning
- compile

Limitations:

- This was a file/log audit, not a fresh Unity batchmode compile.
- A future milestone should open the project in Unity and verify the Console is clean after package installation and folder normalization.

## Step 1 Exit Criteria

Completed:

- Fixed project rules read from `AGENTS.md`.
- Unity version checked.
- Package manifest checked.
- Vendored plugin presence checked.
- Scene/build settings inspected.
- Folder structure compared to assessment requirements.
- Banned-pattern baseline run against project-owned code.
- Git state inspected.
- Logs inspected.

Blocked or deferred:

- Fresh Unity compile was not run from the command line.
- UniTask installation is deferred to setup work.
- Folder normalization is deferred to Step 2.
- Addressables/Localization initialization is deferred to later setup work in Unity.

## Recommended Next Action

Proceed to Step 2: normalize the required project folder structure before adding any code.

Step 2 should do the following:

1. Create `Assets/Scripts/App/Config`, `Installer`, `Persistence`, and `Services`.
2. Add missing `Events` folders.
3. Complete `Inventory` and `Settings` MVC subfolders.
4. Rename or recreate `Assets/Scripts/Main Menu` as `Assets/Scripts/MainMenu`.
5. Create correctly spelled `Assets/AddressableAssets`.
6. Decide what to do with misspelled `Assets/AdressableAssets`.
7. Leave third-party plugin cleanup as a deliberate later cleanup task unless it blocks folder work.

