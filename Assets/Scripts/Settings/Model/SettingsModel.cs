using System;
using UniRx;

public class SettingsModel : IDisposable
{
    public ReactiveProperty<bool> IsOpen { get; } = new(false);
    public ReactiveProperty<float> MasterVolume { get; } = new(1f);
    public ReactiveProperty<float> MusicVolume { get; } = new(1f);
    public ReactiveProperty<float> SfxVolume { get; } = new(1f);
    public ReactiveProperty<int> QualityLevel { get; } = new(0);
    public ReactiveProperty<string> LocaleCode { get; } = new(string.Empty);

    public void Load(SettingsData data)
    {
        SetMasterVolume(data.MasterVolume);
        SetMusicVolume(data.MusicVolume);
        SetSfxVolume(data.SfxVolume);
        SetQualityLevel(data.QualityLevel);
        SetLocaleCode(data.LocaleCode);
        Close();
    }

    public void Open()
    {
        IsOpen.Value = true;
    }

    public void Close()
    {
        IsOpen.Value = false;
    }

    public void Toggle()
    {
        IsOpen.Value = !IsOpen.Value;
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume.Value = Clamp01(value);
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume.Value = Clamp01(value);
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume.Value = Clamp01(value);
    }

    public void SetQualityLevel(int value)
    {
        QualityLevel.Value = value;
    }

    public void SetLocaleCode(string value)
    {
        LocaleCode.Value = value ?? string.Empty;
    }

    public void Dispose()
    {
        IsOpen.Dispose();
        MasterVolume.Dispose();
        MusicVolume.Dispose();
        SfxVolume.Dispose();
        QualityLevel.Dispose();
        LocaleCode.Dispose();
    }

    private float Clamp01(float value)
    {
        if (value < 0f)
        {
            return 0f;
        }

        if (value > 1f)
        {
            return 1f;
        }

        return value;
    }
}
