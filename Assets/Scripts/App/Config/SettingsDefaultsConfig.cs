using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(
    fileName = "SettingsDefaultsConfig",
    menuName = "Assessment/App/Settings Defaults Config")]
public class SettingsDefaultsConfig : ScriptableObject
{
    [SerializeField]
    [Range(0f, 1f)]
    private float defaultMasterVolume = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    private float defaultMusicVolume = 0.8f;

    [SerializeField]
    [Range(0f, 1f)]
    private float defaultSfxVolume = 0.8f;

    [SerializeField]
    private int defaultQualityLevel = 2;

    [SerializeField]
    private string defaultLocaleCode = "en";

    [SerializeField]
    private LocalizedString[] qualityOptionLabels = new LocalizedString[0];

    public float DefaultMasterVolume => defaultMasterVolume;
    public float DefaultMusicVolume => defaultMusicVolume;
    public float DefaultSfxVolume => defaultSfxVolume;
    public int DefaultQualityLevel => defaultQualityLevel;
    public string DefaultLocaleCode => defaultLocaleCode;
    public IReadOnlyList<LocalizedString> QualityOptionLabels => qualityOptionLabels;
}
