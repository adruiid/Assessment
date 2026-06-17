using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private GameVersionConfig gameVersionConfig;
    [SerializeField] private SceneConfig sceneConfig;
    [SerializeField] private SettingsDefaultsConfig settingsDefaultsConfig;

    public override void InstallBindings()
    {
        Container.Bind<GameVersionConfig>().FromInstance(gameVersionConfig).AsSingle();
        Container.Bind<SceneConfig>().FromInstance(sceneConfig).AsSingle();
        Container.Bind<SettingsDefaultsConfig>().FromInstance(settingsDefaultsConfig).AsSingle();

        Container.Bind<ISettingsPersistence>().To<PlayerPrefsSettingsPersistence>().AsSingle();
        Container.Bind<ISceneNavigator>().To<SceneNavigator>().AsSingle();
        Container.Bind<IApplicationService>().To<ApplicationService>().AsSingle();
        Container.Bind<ILocalizationService>().To<LocalizationService>().AsSingle();
        Container.Bind<IQualitySettingsService>().To<QualitySettingsService>().AsSingle();
        Container.Bind<IAudioMixerService>().To<AudioMixerService>().AsSingle();
    }
}
