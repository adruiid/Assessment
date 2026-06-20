using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    private const float HiddenScale = 0f;

    [Header("Modal")]
    [SerializeField] private Button closeButton;
    [SerializeField] private float transitionSeconds = 0.25f;

    [Header("Slots")]
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private Image[] slotBackgroundImages;
    [SerializeField] private Image[] slotIconImages;
    [SerializeField] private Color occupiedSlotColor = Color.white;
    [SerializeField] private Color emptySlotColor = new(0.25f, 0.25f, 0.25f, 0.45f);

    [Header("Detail Panel")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailIconImage;
    [SerializeField] private TMP_Text detailNameLabel;
    [SerializeField] private TMP_Text detailDescriptionLabel;
    [SerializeField] private Image detailStatFillImage;

    private Tween activeVisibilityTween;
    private Vector3 visibleScale;

    public CancellationToken DestroyCancellationToken => this.GetCancellationTokenOnDestroy();
    public int SlotCount => Math.Min(slotButtons.Length, Math.Min(slotBackgroundImages.Length, slotIconImages.Length));
    public IObservable<Unit> CloseClicked => closeButton.OnClickAsObservable();

    private void OnDestroy()
    {
        activeVisibilityTween?.Kill();
    }

    public IObservable<Unit> GetSlotClicked(int index)
    {
        return slotButtons[index].OnClickAsObservable();
    }

    public void SetVisibleInstant(bool isVisible)
    {
        EnsureVisibleScale();

        activeVisibilityTween?.Kill();
        gameObject.SetActive(isVisible);
        transform.localScale = isVisible
            ? visibleScale
            : new Vector3(visibleScale.x, HiddenScale, visibleScale.z);
    }

    public async UniTask SetVisibleAsync(bool isVisible, CancellationToken token)
    {
        EnsureVisibleScale();

        activeVisibilityTween?.Kill();
        gameObject.SetActive(true);

        var targetScale = isVisible
            ? visibleScale
            : new Vector3(visibleScale.x, HiddenScale, visibleScale.z);

        activeVisibilityTween = transform
            .DOScale(targetScale, transitionSeconds)
            .SetEase(isVisible ? Ease.OutCubic : Ease.InCubic);

        var tween = activeVisibilityTween;
        await WaitForTweenAsync(tween, token);

        if (activeVisibilityTween != tween)
        {
            return;
        }

        if (!isVisible)
        {
            gameObject.SetActive(false);
        }
    }

    public void RenderSlot(int index, Sprite icon, bool hasItem)
    {
        if (!IsValidSlotIndex(index))
        {
            return;
        }

        slotButtons[index].interactable = hasItem;
        slotBackgroundImages[index].color = hasItem ? occupiedSlotColor : emptySlotColor;
        slotIconImages[index].gameObject.SetActive(hasItem);
        slotIconImages[index].sprite = hasItem ? icon : null;
    }

    public void HideDetail()
    {
        detailPanel.SetActive(false);
    }

    public void RenderDetail(
        Sprite icon,
        string displayName,
        string description,
        float statFillNormalized)
    {
        detailPanel.SetActive(true);
        detailIconImage.sprite = icon;
        detailNameLabel.text = displayName;
        detailDescriptionLabel.text = description;
        detailStatFillImage.fillAmount = Mathf.Clamp01(statFillNormalized);
    }

    private void EnsureVisibleScale()
    {
        if (visibleScale != Vector3.zero)
        {
            return;
        }

        visibleScale = transform.localScale;
    }

    private bool IsValidSlotIndex(int index)
    {
        return index >= 0 && index < SlotCount;
    }

    private async UniTask WaitForTweenAsync(Tween tween, CancellationToken token)
    {
        using (token.Register(() =>
               {
                   if (tween != null && tween.IsActive())
                   {
                       tween.Kill();
                   }
               }))
        {
            await tween.AsyncWaitForCompletion();
            token.ThrowIfCancellationRequested();
        }
    }
}
