using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public interface ILocalizationService
{
    UniTask InitializeAsync(CancellationToken token);
    UniTask SetLocaleAsync(string localeCode, CancellationToken token);
    UniTask<string> GetLocalizedStringAsync(LocalizedString localizedString, CancellationToken token);
    string GetCurrentLocaleCode();
    string[] GetAvailableLocaleCodes();
    string[] GetAvailableLocaleDisplayNames();
}

public class LocalizationService : ILocalizationService
{
    public async UniTask InitializeAsync(CancellationToken token)
    {
        await LocalizationSettings.InitializationOperation.ToUniTask(cancellationToken: token);
    }

    public async UniTask SetLocaleAsync(string localeCode, CancellationToken token)
    {
        await InitializeAsync(token);

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == localeCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                return;
            }
        }
    }

    public async UniTask<string> GetLocalizedStringAsync(LocalizedString localizedString, CancellationToken token)
    {
        await InitializeAsync(token);

        return await localizedString
            .GetLocalizedStringAsync()
            .ToUniTask(cancellationToken: token);
    }

    public string GetCurrentLocaleCode()
    {
        if (LocalizationSettings.SelectedLocale == null)
        {
            return string.Empty;
        }

        return LocalizationSettings.SelectedLocale.Identifier.Code;
    }

    public string[] GetAvailableLocaleCodes()
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        var codes = new List<string>();

        foreach (var locale in locales)
        {
            codes.Add(locale.Identifier.Code);
        }

        return codes.ToArray();
    }

    public string[] GetAvailableLocaleDisplayNames()
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        var displayNames = new List<string>();

        foreach (var locale in locales)
        {
            var cultureInfo = locale.Identifier.CultureInfo;
            displayNames.Add(cultureInfo != null ? cultureInfo.NativeName : locale.LocaleName);
        }

        return displayNames.ToArray();
    }
}
