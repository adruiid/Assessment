using UnityEngine;

[CreateAssetMenu(
    fileName = "SceneConfig",
    menuName = "Assessment/App/Scene Config")]
public class SceneConfig : ScriptableObject
{
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string gameplaySceneName = "In Game";

    public string MainMenuSceneName => mainMenuSceneName;
    public string GameplaySceneName => gameplaySceneName;
}
