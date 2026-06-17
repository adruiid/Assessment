using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudView : MonoBehaviour
{
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TMP_Text healthValueLabel;
    [SerializeField] private TMP_Text currencyValueLabel;
    [SerializeField] private RawImage minimapImage;

    private Tween activeCurrencyTween;

    public CancellationToken DestroyCancellationToken => this.GetCancellationTokenOnDestroy();

    private void OnDestroy()
    {
        activeCurrencyTween?.Kill();
    }

    public void SetHealth(int current, int max)
    {
        var safeMax = Math.Max(1, max);
        var safeCurrent = Mathf.Clamp(current, 0, safeMax);

        healthFillImage.fillAmount = safeCurrent / (float)safeMax;
        healthValueLabel.text = $"{safeCurrent}/{safeMax}";
    }

    public void SetCurrencyInstant(int value)
    {
        currencyValueLabel.text = value.ToString();
    }

    public async UniTask AnimateCurrencyAsync(
        int fromValue,
        int toValue,
        float durationSeconds,
        CancellationToken token)
    {
        activeCurrencyTween?.Kill();

        var displayedValue = fromValue;
        activeCurrencyTween = DOTween
            .To(() => displayedValue, value =>
            {
                displayedValue = value;
                SetCurrencyInstant(displayedValue);
            }, toValue, durationSeconds)
            .SetEase(Ease.OutCubic);

        using (token.Register(() =>
               {
                   if (activeCurrencyTween != null && activeCurrencyTween.IsActive())
                   {
                       activeCurrencyTween.Kill();
                   }
               }))
        {
            await activeCurrencyTween.AsyncWaitForCompletion();
            token.ThrowIfCancellationRequested();
        }
    }

    public void SetMinimapVisible(bool isVisible)
    {
        minimapImage.gameObject.SetActive(isVisible);
    }
}
