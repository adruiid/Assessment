using UnityEngine;
using Zenject;

public class MainMenuSceneInstaller : MonoInstaller
{
    [SerializeField] private MainMenuView mainMenuView;

    public override void InstallBindings()
    {
        Container.Bind<MainMenuView>().FromInstance(mainMenuView).AsSingle();
        Container.BindInterfacesAndSelfTo<MainMenuModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<MainMenuController>().AsSingle();
    }
}
