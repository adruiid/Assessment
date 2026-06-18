using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class HudController : IInitializable, IDisposable
{
    private readonly HudModel model;
    private readonly HudView view;
    private readonly HudConfig hudConfig;
    private readonly IAddressableAssetService addressableAssetService;
    private readonly CompositeDisposable disposables = new();
    private readonly List<AsyncOperationHandle<Sprite>> skillIconHandles = new();
    private readonly CancellationTokenSource disposeTokenSource = new();

    private int lastCurrencyValue;

    public HudController(
        HudModel model,
        HudView view,
        HudConfig hudConfig,
        IAddressableAssetService addressableAssetService)
    {
        this.model = model;
        this.view = view;
        this.hudConfig = hudConfig;
        this.addressableAssetService = addressableAssetService;
    }

    public void Initialize()
    {
        var skillCount = Math.Min(hudConfig.Skills.Count, view.SkillSlotCount);

        model.Load(
            hudConfig.MaxPlayerHealth,
            hudConfig.StartingPlayerHealth,
            hudConfig.StartingCurrency,
            skillCount);

        lastCurrencyValue = model.Currency.Value;
        view.SetCurrencyInstant(lastCurrencyValue);
        view.SetMinimapVisible(true);
        InitializeSkillSlots(skillCount);

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

        BindSkillSlots(skillCount);
        LoadSkillIconsAsync(skillCount).Forget();
    }

    public void Dispose()
    {
        disposeTokenSource.Cancel();
        disposables.Dispose();

        foreach (var handle in skillIconHandles)
        {
            addressableAssetService.Release(handle);
        }

        skillIconHandles.Clear();
        disposeTokenSource.Dispose();
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

    private void InitializeSkillSlots(int skillCount)
    {
        for (var i = 0; i < skillCount; i++)
        {
            view.SetSkillCooldownState(i, false);
        }
    }

    private void BindSkillSlots(int skillCount)
    {
        for (var i = 0; i < skillCount; i++)
        {
            var skillIndex = i;

            view.GetSkillClicked(skillIndex)
                .Subscribe(_ => UseSkillAsync(skillIndex).Forget())
                .AddTo(disposables);

            model.Skills[skillIndex].IsCoolingDown
                .DistinctUntilChanged()
                .Subscribe(isCoolingDown => view.SetSkillCooldownState(skillIndex, isCoolingDown))
                .AddTo(disposables);
        }
    }

    private async UniTaskVoid UseSkillAsync(int skillIndex)
    {
        if (model.Skills[skillIndex].IsCoolingDown.Value)
        {
            return;
        }

        model.Skills[skillIndex].StartCooldown();

        try
        {
            await view.PlaySkillCooldownFillAsync(
                skillIndex,
                hudConfig.Skills[skillIndex].CooldownSeconds,
                disposeTokenSource.Token);

            model.Skills[skillIndex].CompleteCooldown();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTaskVoid LoadSkillIconsAsync(int skillCount)
    {
        try
        {
            for (var i = 0; i < skillCount; i++)
            {
                var handle = await addressableAssetService.LoadAssetAsync(
                    hudConfig.Skills[i].IconReference,
                    disposeTokenSource.Token);

                skillIconHandles.Add(handle);
                view.SetSkillIcon(i, handle.Result);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
