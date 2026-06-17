using System;
using Cysharp.Threading.Tasks;
using UniRx;
using Zenject;

public class HudController : IInitializable, IDisposable
{
    private readonly HudModel model;
    private readonly HudView view;
    private readonly HudConfig hudConfig;
    private readonly CompositeDisposable disposables = new();

    private int lastCurrencyValue;

    public HudController(HudModel model, HudView view, HudConfig hudConfig)
    {
        this.model = model;
        this.view = view;
        this.hudConfig = hudConfig;
    }

    public void Initialize()
    {
        model.Load(
            hudConfig.MaxPlayerHealth,
            hudConfig.StartingPlayerHealth,
            hudConfig.StartingCurrency);

        lastCurrencyValue = model.Currency.Value;
        view.SetCurrencyInstant(lastCurrencyValue);
        view.SetMinimapVisible(true);

        model.CurrentHealth
            .CombineLatest(model.MaxHealth, (current, max) => new HealthSnapshot(current, max))
            .DistinctUntilChanged()
            .Subscribe(snapshot => view.SetHealth(snapshot.Current, snapshot.Max))
            .AddTo(disposables);

        model.Currency
            .SkipLatestValueOnSubscribe()
            .DistinctUntilChanged()
            .Subscribe(AnimateCurrency)
            .AddTo(disposables);
    }

    public void Dispose()
    {
        disposables.Dispose();
    }

    private void AnimateCurrency(int value)
    {
        AnimateCurrencyAsync(value).Forget();
    }

    private async UniTaskVoid AnimateCurrencyAsync(int value)
    {
        try
        {
            var previousValue = lastCurrencyValue;
            lastCurrencyValue = value;

            await view.AnimateCurrencyAsync(
                previousValue,
                value,
                hudConfig.CurrencyAnimationDurationSeconds,
                view.DestroyCancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
