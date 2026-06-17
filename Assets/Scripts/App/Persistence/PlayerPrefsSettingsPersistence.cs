using UnityEngine;

public interface ISettingsPersistence
{
    SettingsData Load();
    void Save(SettingsData data);
}

public class PlayerPrefsSettingsPersistence : ISettingsPersistence
{
    private const string MasterVolumeKey = "settings.masterVolume";
    private const string MusicVolumeKey = "settings.musicVolume";
    private const string SfxVolumeKey = "settings.sfxVolume";
    private const string QualityLevelKey = "settings.qualityLevel";
    private const string LocaleCodeKey = "settings.localeCode";

    private readonly SettingsDefaultsConfig settingsDefaultsConfig;

    public PlayerPrefsSettingsPersistence(SettingsDefaultsConfig settingsDefaultsConfig)
    {
        this.settingsDefaultsConfig = settingsDefaultsConfig;
    }

    public SettingsData Load()
    {
        return new SettingsData
        {
            MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, settingsDefaultsConfig.DefaultMasterVolume),
            MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, settingsDefaultsConfig.DefaultMusicVolume),
            SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, settingsDefaultsConfig.DefaultSfxVolume),
            QualityLevel = PlayerPrefs.GetInt(QualityLevelKey, settingsDefaultsConfig.DefaultQualityLevel),
            LocaleCode = PlayerPrefs.GetString(LocaleCodeKey, settingsDefaultsConfig.DefaultLocaleCode)
        };
    }

    public void Save(SettingsData data)
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, data.MasterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, data.MusicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, data.SfxVolume);
        PlayerPrefs.SetInt(QualityLevelKey, data.QualityLevel);
        PlayerPrefs.SetString(LocaleCodeKey, data.LocaleCode);
        PlayerPrefs.Save();
    }
}
