using UnityEngine;
using Zenject;

public class GameplaySceneInstaller : MonoInstaller
{
    [SerializeField] private HudView hudView;
    [SerializeField] private HudConfig hudConfig;

    public override void InstallBindings()
    {
        Container.Bind<HudConfig>().FromInstance(hudConfig).AsSingle();
        Container.Bind<HudView>().FromInstance(hudView).AsSingle();
        Container.BindInterfacesAndSelfTo<HudModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<HudController>().AsSingle();
    }
}
