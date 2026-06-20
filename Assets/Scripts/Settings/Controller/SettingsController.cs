using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using Zenject;

public class SettingsController : IInitializable, IDisposable
{
    private readonly SettingsModel model;
    private readonly SettingsView view;
    private readonly ISettingsPersistence settingsPersistence;
    private readonly IAudioMixerService audioMixerService;
    private readonly IQualitySettingsService qualitySettingsService;
    private readonly ILocalizationService localizationService;
    private readonly SettingsDefaultsConfig settingsDefaultsConfig;
    private readonly CompositeDisposable disposables = new();

    private string[] localeCodes = Array.Empty<string>();

    public SettingsController(
        SettingsModel model,
        SettingsView view,
        ISettingsPersistence settingsPersistence,
        IAudioMixerService audioMixerService,
        IQualitySettingsService qualitySettingsService,
        ILocalizationService localizationService,
        SettingsDefaultsConfig settingsDefaultsConfig)
    {
        this.model = model;
        this.view = view;
        this.settingsPersistence = settingsPersistence;
        this.audioMixerService = audioMixerService;
        this.qualitySettingsService = qualitySettingsService;
        this.localizationService = localizationService;
        this.settingsDefaultsConfig = settingsDefaultsConfig;
    }

    public void Initialize()
    {
        model.Load(settingsPersistence.Load());

        view.SetVisibleInstant(false);
        view.SetMasterVolume(model.MasterVolume.Value);
        view.SetMusicVolume(model.MusicVolume.Value);
        view.SetSfxVolume(model.SfxVolume.Value);

        ApplyAudioVolumes();
        ApplyQualityLevel(model.QualityLevel.Value);

        BindViewEvents();
        BindModelChanges();
        InitializeLocalizationAsync(view.DestroyCancellationToken).Forget();
    }

    public void Open()
    {
        model.Open();
    }

    public void Close()
    {
        model.Close();
    }

    public void Toggle()
    {
        model.Toggle();
    }

    public void Dispose()
    {
        disposables.Dispose();
    }

    private void BindViewEvents()
    {
        view.MasterVolumeChanged
            .Skip(1)
            .Subscribe(model.SetMasterVolume)
            .AddTo(disposables);

        view.MusicVolumeChanged
            .Skip(1)
            .Subscribe(model.SetMusicVolume)
            .AddTo(disposables);

        view.SfxVolumeChanged
            .Skip(1)
            .Subscribe(model.SetSfxVolume)
            .AddTo(disposables);

        view.GraphicsChanged
            .Subscribe(model.SetQualityLevel)
            .AddTo(disposables);

        view.LanguageChanged
            .Subscribe(SetLocaleFromDropdownIndex)
            .AddTo(disposables);

        view.CloseClicked
            .Subscribe(_ => model.Close())
            .AddTo(disposables);
    }

    private void BindModelChanges()
    {
        model.IsOpen
            .SkipLatestValueOnSubscribe()
            .DistinctUntilChanged()
            .Subscribe(AnimateVisibility)
            .AddTo(disposables);

        model.MasterVolume
            .SkipLatestValueOnSubscribe()
            .DistinctUntilChanged()
            .Subscribe(value =>
            {
                view.SetMasterVolume(value);
                audioMixerService.SetMasterVolume(value);
                SaveCurrentSettings();
            })
            .AddTo(disposables);

        model.MusicVolume
            .SkipLatestValueOnSubscribe()
            .DistinctUntilChanged()
            .Subscribe(value =>
            {
                view.SetMusicVolume(value);
                audioMixerService.SetMusicVolume(value);
                SaveCurrentSettings();
            })
            .AddTo(disposables);

        model.SfxVolume
            .SkipLatestValueOnSubscribe()
            .DistinctUntilChanged()
            .Subscribe(value =>
            {
                view.SetSfxVolume(value);
                audioMixerService.SetSfxVolume(value);
                SaveCurrentSettings();
            })
            .AddTo(disposables);

        model.QualityLevel
            .SkipLatestValueOnSubscribe()
            .DistinctUntilChanged()
            .Subscribe(value =>
            {
                view.SetGraphicsIndex(value);
                ApplyQualityLevel(value);
                SaveCurrentSettings();
            })
            .AddTo(disposables);

        model.LocaleCode
            .SkipLatestValueOnSubscribe()
            .DistinctUntilChanged()
            .Subscribe(value => ApplyLocaleAsync(value, view.DestroyCancellationToken).Forget())
            .AddTo(disposables);
    }

    private async UniTaskVoid InitializeLocalizationAsync(CancellationToken token)
    {
        try
        {
            await localizationService.InitializeAsync(token);

            localeCodes = localizationService.GetAvailableLocaleCodes();

            await localizationService.SetLocaleAsync(model.LocaleCode.Value, token);
            view.SetLanguageOptions(localizationService.GetAvailableLocaleDisplayNames());
            view.SetLanguageIndex(GetLocaleIndex(model.LocaleCode.Value));
            await RefreshGraphicsOptionsAsync(token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTaskVoid ApplyLocaleAsync(string localeCode, CancellationToken token)
    {
        try
        {
            await localizationService.SetLocaleAsync(localeCode, token);
            await RefreshGraphicsOptionsAsync(token);
            view.SetLanguageIndex(GetLocaleIndex(localeCode));
            SaveCurrentSettings();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void AnimateVisibility(bool isVisible)
    {
        AnimateVisibilityAsync(isVisible, view.DestroyCancellationToken).Forget();
    }

    private async UniTaskVoid AnimateVisibilityAsync(bool isVisible, CancellationToken token)
    {
        try
        {
            await view.SetVisibleAsync(isVisible, token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void SetLocaleFromDropdownIndex(int index)
    {
        if (index < 0 || index >= localeCodes.Length)
        {
            return;
        }

        model.SetLocaleCode(localeCodes[index]);
    }

    private int GetLocaleIndex(string localeCode)
    {
        for (var i = 0; i < localeCodes.Length; i++)
        {
            if (localeCodes[i] == localeCode)
            {
                return i;
            }
        }

        return 0;
    }

    private void ApplyAudioVolumes()
    {
        audioMixerService.SetMasterVolume(model.MasterVolume.Value);
        audioMixerService.SetMusicVolume(model.MusicVolume.Value);
        audioMixerService.SetSfxVolume(model.SfxVolume.Value);
    }

    private void ApplyQualityLevel(int qualityLevel)
    {
        var names = qualitySettingsService.GetQualityNames();

        if (names.Length == 0)
        {
            return;
        }

        var safeLevel = qualityLevel;

        if (safeLevel < 0)
        {
            safeLevel = 0;
        }

        if (safeLevel >= names.Length)
        {
            safeLevel = names.Length - 1;
        }

        qualitySettingsService.SetQualityLevel(safeLevel);
    }

    private async UniTask RefreshGraphicsOptionsAsync(CancellationToken token)
    {
        var options = await GetGraphicsOptionLabelsAsync(token);

        view.SetGraphicsOptions(options);
        view.SetGraphicsIndex(model.QualityLevel.Value);
    }

    private async UniTask<IReadOnlyList<string>> GetGraphicsOptionLabelsAsync(CancellationToken token)
    {
        if (settingsDefaultsConfig.QualityOptionLabels.Count == 0)
        {
            return qualitySettingsService.GetQualityNames();
        }

        var labels = new List<string>();

        foreach (var localizedLabel in settingsDefaultsConfig.QualityOptionLabels)
        {
            var label = await localizationService.GetLocalizedStringAsync(localizedLabel, token);
            labels.Add(label);
        }

        return labels;
    }

    private void SaveCurrentSettings()
    {
        settingsPersistence.Save(new SettingsData
        {
            MasterVolume = model.MasterVolume.Value,
            MusicVolume = model.MusicVolume.Value,
            SfxVolume = model.SfxVolume.Value,
            QualityLevel = model.QualityLevel.Value,
            LocaleCode = model.LocaleCode.Value
        });
    }
}
