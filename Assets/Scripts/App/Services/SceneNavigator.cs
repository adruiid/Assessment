using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public interface ISceneNavigator
{
    UniTask LoadMainMenuAsync(CancellationToken token = default);
    UniTask LoadGameplayAsync(CancellationToken token = default);
}

public class SceneNavigator : ISceneNavigator
{
    private readonly SceneConfig sceneConfig;

    public SceneNavigator(SceneConfig sceneConfig)
    {
        this.sceneConfig = sceneConfig;
    }

    public async UniTask LoadMainMenuAsync(CancellationToken token = default)
    {
        await SceneManager.LoadSceneAsync(sceneConfig.MainMenuSceneName)
            .ToUniTask(cancellationToken: token);
    }

    public async UniTask LoadGameplayAsync(CancellationToken token = default)
    {
        await SceneManager.LoadSceneAsync(sceneConfig.GameplaySceneName)
            .ToUniTask(cancellationToken: token);
    }
}
