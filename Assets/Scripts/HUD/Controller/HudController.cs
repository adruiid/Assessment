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
    private readonly ILocalizationService localizationService;
    private readonly CompositeDisposable disposables = new();
    private readonly List<AsyncOperationHandle<Sprite>> skillIconHandles = new();
    private readonly Queue<string> toastQueue = new();
    private readonly CancellationTokenSource disposeTokenSource = new();

    private CancellationTokenSource bossHideDelayTokenSource;
    private int lastCurrencyValue;
    private bool isShowingToast;

    public HudController(
        HudModel model,
        HudView view,
        HudConfig hudConfig,
        IAddressableAssetService addressableAssetService,
        ILocalizationService localizationService)
    {
        this.model = model;
        this.view = view;
        this.hudConfig = hudConfig;
        this.addressableAssetService = addressableAssetService;
        this.localizationService = localizationService;
    }

    public void Initialize()
    {
        var skillCount = Math.Min(hudConfig.Skills.Count, view.SkillSlotCount);

        model.Load(
            hudConfig.MaxPlayerHealth,
            hudConfig.StartingPlayerHealth,
            hudConfig.MaxBossHealth,
            hudConfig.StartingBossHealth,
            hudConfig.StartingCurrency,
            skillCount);

        lastCurrencyValue = model.Currency.Value;
        view.SetCurrencyInstant(lastCurrencyValue);
        view.SetMinimapVisible(true);
        view.SetBossHealthVisibleInstant(false);
        InitializeSkillSlots(skillCount);

        model.CurrentHealth
            .CombineLatest(model.MaxHealth, (current, max) => new HealthSnapshot(current, max))
            .DistinctUntilChanged()
            .Subscribe(snapshot => view.SetHealth(snapshot.Current, snapshot.Max))
            .AddTo(disposables);

        model.CurrentBossHealth
            .CombineLatest(model.MaxBossHealth, (current, max) => new HealthSnapshot(current, max))
            .DistinctUntilChanged()
            .Subscribe(snapshot => view.SetBossHealth(snapshot.Current, snapshot.Max))
            .AddTo(disposables);

        model.IsBossVisible
            .SkipLatestValueOnSubscribe()
            .DistinctUntilChanged()
            .Subscribe(AnimateBossVisibility)
            .AddTo(disposables);

        model.Currency
            .SkipLatestValueOnSubscribe()
            .DistinctUntilChanged()
            .Subscribe(AnimateCurrency)
            .AddTo(disposables);

        model.ToastMessages
            .Subscribe(QueueToast)
            .AddTo(disposables);

        BindSkillSlots(skillCount);
        LoadSkillIconsAsync(skillCount).Forget();
    }

    public void Dispose()
    {
        disposeTokenSource.Cancel();
        bossHideDelayTokenSource?.Cancel();
        bossHideDelayTokenSource?.Dispose();
        disposables.Dispose();
        toastQueue.Clear();

        foreach (var handle in skillIconHandles)
        {
            addressableAssetService.Release(handle);
        }

        skillIconHandles.Clear();
        disposeTokenSource.Dispose();
    }

    private void AnimateBossVisibility(bool isVisible)
    {
        view.SetBossHealthVisibleAsync(
            isVisible,
            hudConfig.BossBarTransitionSeconds,
            disposeTokenSource.Token).Forget();
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
        model.SetBossVisible(true);
        model.SetBossHealth(model.CurrentBossHealth.Value - hudConfig.Skills[skillIndex].BossDamage);
        RestartBossHideTimer();
        QueueSkillToastAsync(skillIndex).Forget();

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

    private void RestartBossHideTimer()
    {
        bossHideDelayTokenSource?.Cancel();
        bossHideDelayTokenSource?.Dispose();
        bossHideDelayTokenSource = CancellationTokenSource.CreateLinkedTokenSource(disposeTokenSource.Token);

        HideBossAfterDelayAsync(bossHideDelayTokenSource.Token).Forget();
    }

    private async UniTaskVoid HideBossAfterDelayAsync(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(hudConfig.BossBarHideDelaySeconds),
                cancellationToken: token);

            model.SetBossVisible(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTaskVoid QueueSkillToastAsync(int skillIndex)
    {
        try
        {
            var message = await localizationService.GetLocalizedStringAsync(
                hudConfig.Skills[skillIndex].ToastMessage,
                disposeTokenSource.Token);

            model.EnqueueToast(message);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void QueueToast(string message)
    {
        toastQueue.Enqueue(message);

        if (!isShowingToast)
        {
            ProcessToastQueueAsync().Forget();
        }
    }

    private async UniTaskVoid ProcessToastQueueAsync()
    {
        if (isShowingToast)
        {
            return;
        }

        isShowingToast = true;

        try
        {
            while (toastQueue.Count > 0)
            {
                var message = toastQueue.Dequeue();

                await view.ShowToastAsync(
                    message,
                    hudConfig.ToastVisibleSeconds,
                    hudConfig.ToastTransitionSeconds,
                    disposeTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            isShowingToast = false;
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
