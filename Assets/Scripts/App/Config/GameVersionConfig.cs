using UnityEngine;

[CreateAssetMenu(
    fileName = "GameVersionConfig",
    menuName = "Assessment/App/Game Version Config")]
public class GameVersionConfig : ScriptableObject
{
    [SerializeField] private string versionText = "0.1.0";

    public string VersionText => versionText;
}
