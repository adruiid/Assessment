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
        var names = QualitySettings.names;

        if (names.Length == 0)
        {
            return;
        }

        var safeLevel = level;

        if (safeLevel < 0)
        {
            safeLevel = 0;
        }

        if (safeLevel >= names.Length)
        {
            safeLevel = names.Length - 1;
        }

        QualitySettings.SetQualityLevel(safeLevel, true);
    }
}
