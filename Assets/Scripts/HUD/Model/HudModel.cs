using System;
using UniRx;

public class HudModel : IDisposable
{
    public ReactiveProperty<int> CurrentHealth { get; } = new(0);
    public ReactiveProperty<int> MaxHealth { get; } = new(1);
    public ReactiveProperty<int> Currency { get; } = new(0);

    public void Load(int maxHealth, int currentHealth, int currency)
    {
        MaxHealth.Value = Math.Max(1, maxHealth);
        SetCurrentHealth(currentHealth);
        SetCurrency(currency);
    }

    public void SetCurrentHealth(int value)
    {
        CurrentHealth.Value = Clamp(value, 0, MaxHealth.Value);
    }

    public void SetCurrency(int value)
    {
        Currency.Value = Math.Max(0, value);
    }

    public void Dispose()
    {
        CurrentHealth.Dispose();
        MaxHealth.Dispose();
        Currency.Dispose();
    }

    private int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }
}
