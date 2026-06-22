using System;
using System.Collections.Generic;
using UniRx;

public class HudModel : IDisposable
{
    private readonly List<SkillCooldownModel> skills = new();
    private readonly Subject<string> toastMessages = new();

    public ReactiveProperty<int> CurrentHealth { get; } = new(0);
    public ReactiveProperty<int> MaxHealth { get; } = new(1);
    public ReactiveProperty<int> CurrentBossHealth { get; } = new(0);
    public ReactiveProperty<int> MaxBossHealth { get; } = new(1);
    public ReactiveProperty<bool> IsBossVisible { get; } = new(false);
    public ReactiveProperty<int> Currency { get; } = new(0);
    public IReadOnlyList<SkillCooldownModel> Skills => skills;
    public IObservable<string> ToastMessages => toastMessages;

    public void Load(
        int maxHealth,
        int currentHealth,
        int maxBossHealth,
        int currentBossHealth,
        int currency,
        int skillCount)
    {
        MaxHealth.Value = Math.Max(1, maxHealth);
        MaxBossHealth.Value = Math.Max(1, maxBossHealth);

        SetCurrentHealth(currentHealth);
        SetBossHealth(currentBossHealth);
        SetCurrency(currency);
        SetBossVisible(false);
        SetupSkills(skillCount);
    }

    public void SetCurrentHealth(int value)
    {
        CurrentHealth.Value = Clamp(value, 0, MaxHealth.Value);
    }

    public void SetCurrency(int value)
    {
        Currency.Value = Math.Max(0, value);
    }

    public void AddCurrency(int amount)
    {
        SetCurrency(Currency.Value + amount);
    }

    public void SetBossHealth(int value)
    {
        CurrentBossHealth.Value = Clamp(value, 0, MaxBossHealth.Value);
    }

    public void SetBossVisible(bool isVisible)
    {
        IsBossVisible.Value = isVisible;
    }

    public void EnqueueToast(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        toastMessages.OnNext(message);
    }

    public void Dispose()
    {
        CurrentHealth.Dispose();
        MaxHealth.Dispose();
        CurrentBossHealth.Dispose();
        MaxBossHealth.Dispose();
        IsBossVisible.Dispose();
        Currency.Dispose();
        toastMessages.OnCompleted();
        toastMessages.Dispose();

        foreach (var skill in skills)
        {
            skill.Dispose();
        }
    }

    private void SetupSkills(int skillCount)
    {
        foreach (var skill in skills)
        {
            skill.Dispose();
        }

        skills.Clear();

        for (var i = 0; i < skillCount; i++)
        {
            skills.Add(new SkillCooldownModel());
        }
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

public class SkillCooldownModel : IDisposable
{
    public ReactiveProperty<bool> IsCoolingDown { get; } = new(false);

    public void StartCooldown()
    {
        IsCoolingDown.Value = true;
    }

    public void CompleteCooldown()
    {
        IsCoolingDown.Value = false;
    }

    public void Dispose()
    {
        IsCoolingDown.Dispose();
    }
}
