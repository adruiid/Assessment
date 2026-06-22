using UnityEngine;
using Zenject;

public class MainMenuSceneInstaller : MonoInstaller
{
    [SerializeField] private MainMenuView mainMenuView;
    [SerializeField] private SettingsView settingsView;
    [SerializeField] private SfxPlayerView sfxPlayerView;

    public override void InstallBindings()
    {
        Container.Bind<MainMenuView>().FromInstance(mainMenuView).AsSingle();
        Container.BindInterfacesAndSelfTo<MainMenuModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<MainMenuController>().AsSingle();

        Container.Bind<SettingsView>().FromInstance(settingsView).AsSingle();
        Container.Bind<SfxPlayerView>().FromInstance(sfxPlayerView).AsSingle();
        Container.BindInterfacesAndSelfTo<SettingsModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<SettingsController>().AsSingle();
    }
}
