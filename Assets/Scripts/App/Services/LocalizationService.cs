using Cysharp.Threading.Tasks;
using UnityEngine.Localization.Settings;

public interface ILocalizationService
{
    UniTask SetLocaleAsync(string localeCode);
    string GetCurrentLocaleCode();
}

public class LocalizationService : ILocalizationService
{
    public async UniTask SetLocaleAsync(string localeCode)
    {
        await LocalizationSettings.InitializationOperation.Task;

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == localeCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                return;
            }
        }
    }

    public string GetCurrentLocaleCode()
    {
        if (LocalizationSettings.SelectedLocale == null)
        {
            return string.Empty;
        }

        return LocalizationSettings.SelectedLocale.Identifier.Code;
    }
}
