using System;
using UniRx;

public class MainMenuModel : IDisposable
{
    private readonly ReactiveProperty<bool> isNavigatingToGameplay = new(false);

    public IReadOnlyReactiveProperty<bool> IsNavigatingToGameplay => isNavigatingToGameplay;
    public bool IsNavigationInProgress => isNavigatingToGameplay.Value;

    public void SetNavigatingToGameplay(bool value)
    {
        isNavigatingToGameplay.Value = value;
    }

    public void Dispose()
    {
        isNavigatingToGameplay.Dispose();
    }
}
