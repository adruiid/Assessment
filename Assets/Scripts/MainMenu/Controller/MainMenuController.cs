using System;
using Cysharp.Threading.Tasks;
using UniRx;
using Zenject;

public class MainMenuController : IInitializable, IDisposable
{
    private readonly MainMenuModel model;
    private readonly MainMenuView view;
    private readonly SettingsController settingsController;
    private readonly ISceneNavigator sceneNavigator;
    private readonly IApplicationService applicationService;
    private readonly GameVersionConfig gameVersionConfig;
    private readonly CompositeDisposable disposables = new();

    public MainMenuController(
        MainMenuModel model,
        MainMenuView view,
        SettingsController settingsController,
        ISceneNavigator sceneNavigator,
        IApplicationService applicationService,
        GameVersionConfig gameVersionConfig)
    {
        this.model = model;
        this.view = view;
        this.settingsController = settingsController;
        this.sceneNavigator = sceneNavigator;
        this.applicationService = applicationService;
        this.gameVersionConfig = gameVersionConfig;
    }

    public void Initialize()
    {
        model.SetNavigatingToGameplay(false);
        view.SetVersionValue(gameVersionConfig.VersionText);

        model.IsNavigatingToGameplay
            .DistinctUntilChanged()
            .Subscribe(isNavigating => view.SetMainButtonsInteractable(!isNavigating))
            .AddTo(disposables);

        view.PlayClicked
            .Subscribe(_ => HandlePlayClicked().Forget())
            .AddTo(disposables);

        view.SettingsClicked
            .Subscribe(_ => settingsController.Open())
            .AddTo(disposables);

        view.QuitClicked
            .Subscribe(_ => applicationService.Quit())
            .AddTo(disposables);

        view.PlayEntranceAsync(view.DestroyCancellationToken).Forget();
    }

    public void Dispose()
    {
        disposables.Dispose();
    }

    private async UniTaskVoid HandlePlayClicked()
    {
        if (model.IsNavigationInProgress)
        {
            return;
        }

        model.SetNavigatingToGameplay(true);
        settingsController.Close();

        try
        {
            await sceneNavigator.LoadGameplayAsync(view.DestroyCancellationToken);
        }
        catch (OperationCanceledException)
        {
            model.SetNavigatingToGameplay(false);
        }
    }
}
