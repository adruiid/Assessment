using System;
using UniRx;

public class MainMenuModel : IDisposable
{
    private readonly ReactiveProperty<bool> isSettingsOpen = new(false);

    public IReadOnlyReactiveProperty<bool> IsSettingsOpen => isSettingsOpen;

    public void SetSettingsOpen(bool value)
    {
        isSettingsOpen.Value = value;
    }

    public void ToggleSettings()
    {
        isSettingsOpen.Value = !isSettingsOpen.Value;
    }

    public void Dispose()
    {
        isSettingsOpen.Dispose();
    }
}
