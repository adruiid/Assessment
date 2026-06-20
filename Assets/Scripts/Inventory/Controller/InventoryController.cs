using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class InventoryController : IInitializable, IDisposable
{
    private readonly InventoryModel model;
    private readonly InventoryView view;
    private readonly InventoryConfig config;
    private readonly HudView hudView;
    private readonly IAddressableAssetService addressableAssetService;
    private readonly ILocalizationService localizationService;
    private readonly CompositeDisposable disposables = new();
    private readonly List<AsyncOperationHandle<Sprite>> iconHandles = new();
    private readonly CancellationTokenSource disposeTokenSource = new();

    private Sprite[] loadedSlotIcons;
    private CancellationTokenSource detailTokenSource;

    public InventoryController(
        InventoryModel model,
        InventoryView view,
        InventoryConfig config,
        HudView hudView,
        IAddressableAssetService addressableAssetService,
        ILocalizationService localizationService)
    {
        this.model = model;
        this.view = view;
        this.config = config;
        this.hudView = hudView;
        this.addressableAssetService = addressableAssetService;
        this.localizationService = localizationService;
    }

    public void Initialize()
    {
        model.Load(config);
        loadedSlotIcons = new Sprite[view.SlotCount];

        view.SetVisibleInstant(false);
        view.HideDetail();

        hudView.InventoryClicked
            .Subscribe(_ => model.Open())
            .AddTo(disposables);

        view.CloseClicked
            .Subscribe(_ => model.Close())
            .AddTo(disposables);

        model.IsOpen
            .SkipLatestValueOnSubscribe()
            .DistinctUntilChanged()
            .Subscribe(AnimateVisibility)
            .AddTo(disposables);

        model.SelectedSlotIndex
            .SkipLatestValueOnSubscribe()
            .DistinctUntilChanged()
            .Subscribe(RenderSelectedSlot)
            .AddTo(disposables);

        BindSlotClicks();
        RenderSlotsAsync().Forget();
    }

    public void Dispose()
    {
        disposeTokenSource.Cancel();
        detailTokenSource?.Cancel();
        detailTokenSource?.Dispose();
        disposables.Dispose();

        foreach (var handle in iconHandles)
        {
            addressableAssetService.Release(handle);
        }

        iconHandles.Clear();
        disposeTokenSource.Dispose();
    }

    private void BindSlotClicks()
    {
        for (var i = 0; i < view.SlotCount; i++)
        {
            var slotIndex = i;

            view.GetSlotClicked(slotIndex)
                .Subscribe(_ => model.SelectSlot(slotIndex))
                .AddTo(disposables);
        }
    }

    private void AnimateVisibility(bool isOpen)
    {
        view.SetVisibleAsync(isOpen, disposeTokenSource.Token).Forget();
    }

    private async UniTaskVoid RenderSlotsAsync()
    {
        try
        {
            for (var i = 0; i < view.SlotCount; i++)
            {
                var item = model.GetItemAt(i);

                if (item == null)
                {
                    view.RenderSlot(i, null, false);
                    continue;
                }

                var handle = await addressableAssetService.LoadAssetAsync(
                    item.IconReference,
                    disposeTokenSource.Token);

                iconHandles.Add(handle);
                loadedSlotIcons[i] = handle.Result;
                view.RenderSlot(i, handle.Result, true);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void RenderSelectedSlot(int slotIndex)
    {
        detailTokenSource?.Cancel();
        detailTokenSource?.Dispose();
        detailTokenSource = CancellationTokenSource.CreateLinkedTokenSource(disposeTokenSource.Token);

        RenderSelectedSlotAsync(slotIndex, detailTokenSource.Token).Forget();
    }

    private async UniTaskVoid RenderSelectedSlotAsync(int slotIndex, CancellationToken token)
    {
        try
        {
            var item = model.GetItemAt(slotIndex);

            if (item == null)
            {
                view.HideDetail();
                return;
            }

            var displayName = await localizationService.GetLocalizedStringAsync(
                item.DisplayName,
                token);

            var description = await localizationService.GetLocalizedStringAsync(
                item.Description,
                token);

            view.RenderDetail(
                loadedSlotIcons[slotIndex],
                displayName,
                description,
                item.StatFillNormalized);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
