using UnityEngine;
using Zenject;

public class GameplaySceneInstaller : MonoInstaller
{
    [SerializeField] private HudView hudView;
    [SerializeField] private HudConfig hudConfig;
    [SerializeField] private InventoryView inventoryView;
    [SerializeField] private InventoryConfig inventoryConfig;

    public override void InstallBindings()
    {
        Container.Bind<HudConfig>().FromInstance(hudConfig).AsSingle();
        Container.Bind<HudView>().FromInstance(hudView).AsSingle();
        Container.BindInterfacesAndSelfTo<HudModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<HudController>().AsSingle();

        Container.Bind<InventoryConfig>().FromInstance(inventoryConfig).AsSingle();
        Container.Bind<InventoryView>().FromInstance(inventoryView).AsSingle();
        Container.BindInterfacesAndSelfTo<InventoryModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<InventoryController>().AsSingle();
    }
}
