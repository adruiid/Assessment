using UnityEngine;

public interface IQualitySettingsService
{
    string[] GetQualityNames();
    int GetCurrentQualityLevel();
    void SetQualityLevel(int level);
}

public class QualitySettingsService : IQualitySettingsService
{
    public string[] GetQualityNames()
    {
        return QualitySettings.names;
    }

    public int GetCurrentQualityLevel()
    {
        return QualitySettings.GetQualityLevel();
    }

    public void SetQualityLevel(int level)
    {
        QualitySettings.SetQualityLevel(level);
    }
}
