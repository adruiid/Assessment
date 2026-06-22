using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HudView : MonoBehaviour
{
    private const float HiddenScale = 0f;

    [SerializeField] private Image healthFillImage;
    [SerializeField] private TMP_Text healthValueLabel;
    [SerializeField] private TMP_Text currencyValueLabel;
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private GameObject bossHealthRoot;
    [SerializeField] private Image bossHealthFillImage;
    [SerializeField] private GameObject toastRoot;
    [SerializeField] private TMP_Text toastLabel;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button addGoldButton;
    [SerializeField] private Button[] skillButtons;
    [SerializeField] private Image[] skillIconImages;
    [SerializeField] private Image[] skillCooldownFillImages;
    [SerializeField] private Color cooldownSkillIconColor = new(0.7f, 0.7f, 0.7f, 1f);

    private Tween activeCurrencyTween;
    private Tween activeBossTween;
    private Tween activeToastTween;
    private Tween[] activeSkillCooldownTweens;
    private Color[] skillIconDefaultColors;
    private Vector3 bossHealthVisibleScale;
    private Vector3 toastVisibleScale;

    public CancellationToken DestroyCancellationToken => this.GetCancellationTokenOnDestroy();
    public int SkillSlotCount => Mathf.Min(skillButtons.Length, skillIconImages.Length, skillCooldownFillImages.Length);
    public IObservable<Unit> InventoryClicked => inventoryButton.OnClickAsObservable();
    public IObservable<Unit> HomeClicked => homeButton.OnClickAsObservable();
    public IObservable<Unit> AddGoldClicked => addGoldButton.OnClickAsObservable();

    private void Awake()
    {
        bossHealthVisibleScale = bossHealthRoot.transform.localScale;
        toastVisibleScale = toastRoot.transform.localScale;

        CacheSkillVisualState();
        SetBossHealthVisibleInstant(false);
        SetToastVisibleInstant(false);
    }

    private void OnDestroy()
    {
        activeCurrencyTween?.Kill();
        activeBossTween?.Kill();
        activeToastTween?.Kill();

        if (activeSkillCooldownTweens == null)
        {
            return;
        }

        foreach (var tween in activeSkillCooldownTweens)
        {
            tween?.Kill();
        }
    }

    public IObservable<Unit> GetSkillClicked(int index)
    {
        return skillButtons[index].OnClickAsObservable();
    }

    public void SetHealth(int current, int max)
    {
        var safeMax = Math.Max(1, max);
        var safeCurrent = Mathf.Clamp(current, 0, safeMax);

        healthFillImage.fillAmount = safeCurrent / (float)safeMax;
        healthValueLabel.text = $"{safeCurrent}/{safeMax}";
    }

    public void SetBossHealth(int current, int max)
    {
        var safeMax = Math.Max(1, max);
        var safeCurrent = Mathf.Clamp(current, 0, safeMax);

        bossHealthFillImage.fillAmount = safeCurrent / (float)safeMax;
    }

    public void SetBossHealthVisibleInstant(bool isVisible)
    {
        activeBossTween?.Kill();
        bossHealthRoot.SetActive(isVisible);
        bossHealthRoot.transform.localScale = isVisible
            ? bossHealthVisibleScale
            : new Vector3(
                bossHealthVisibleScale.x,
                HiddenScale,
                bossHealthVisibleScale.z);
    }

    public async UniTask SetBossHealthVisibleAsync(
        bool isVisible,
        float durationSeconds,
        CancellationToken token)
    {
        activeBossTween?.Kill();
        bossHealthRoot.SetActive(true);

        var targetScale = isVisible
            ? bossHealthVisibleScale
            : new Vector3(
                bossHealthVisibleScale.x,
                HiddenScale,
                bossHealthVisibleScale.z);

        activeBossTween = bossHealthRoot.transform
            .DOScale(targetScale, durationSeconds)
            .SetEase(isVisible ? Ease.OutCubic : Ease.InCubic);

        var tween = activeBossTween;
        await WaitForTweenAsync(tween, token);

        if (activeBossTween != tween)
        {
            return;
        }

        if (!isVisible)
        {
            bossHealthRoot.SetActive(false);
        }
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

    public async UniTask ShowToastAsync(
        string message,
        float visibleSeconds,
        float transitionSeconds,
        CancellationToken token)
    {
        activeToastTween?.Kill();
        toastLabel.text = message;
        toastRoot.SetActive(true);
        toastRoot.transform.localScale = new Vector3(
            toastVisibleScale.x,
            HiddenScale,
            toastVisibleScale.z);

        activeToastTween = toastRoot.transform
            .DOScale(toastVisibleScale, transitionSeconds)
            .SetEase(Ease.OutCubic);

        await WaitForTweenAsync(activeToastTween, token);

        await UniTask.Delay(
            TimeSpan.FromSeconds(visibleSeconds),
            cancellationToken: token);

        activeToastTween = toastRoot.transform
            .DOScale(new Vector3(
                toastVisibleScale.x,
                HiddenScale,
                toastVisibleScale.z),
                transitionSeconds)
            .SetEase(Ease.InCubic);

        await WaitForTweenAsync(activeToastTween, token);
        toastRoot.SetActive(false);
        toastRoot.transform.localScale = toastVisibleScale;
    }

    public void SetMinimapVisible(bool isVisible)
    {
        minimapImage.gameObject.SetActive(isVisible);
    }

    public void SetSkillIcon(int index, Sprite sprite)
    {
        if (!IsValidSkillIndex(index))
        {
            return;
        }

        skillIconImages[index].sprite = sprite;
    }

    public void SetSkillCooldownState(int index, bool isCoolingDown)
    {
        if (!IsValidSkillIndex(index))
        {
            return;
        }

        CacheSkillVisualState();

        skillButtons[index].interactable = !isCoolingDown;
        skillIconImages[index].color = isCoolingDown
            ? cooldownSkillIconColor
            : skillIconDefaultColors[index];

        skillCooldownFillImages[index].gameObject.SetActive(isCoolingDown);

        if (isCoolingDown)
        {
            skillCooldownFillImages[index].fillAmount = 1f;
        }
        else
        {
            activeSkillCooldownTweens[index]?.Kill();
            skillCooldownFillImages[index].fillAmount = 0f;
        }
    }

    public async UniTask PlaySkillCooldownFillAsync(
        int index,
        float durationSeconds,
        CancellationToken token)
    {
        if (!IsValidSkillIndex(index))
        {
            return;
        }

        activeSkillCooldownTweens[index]?.Kill();
        skillCooldownFillImages[index].gameObject.SetActive(true);
        skillCooldownFillImages[index].fillAmount = 1f;

        activeSkillCooldownTweens[index] = skillCooldownFillImages[index]
            .DOFillAmount(0f, durationSeconds)
            .SetEase(Ease.Linear);

        using (token.Register(() =>
               {
                   if (activeSkillCooldownTweens[index] != null &&
                       activeSkillCooldownTweens[index].IsActive())
                   {
                       activeSkillCooldownTweens[index].Kill();
                   }
               }))
        {
            await activeSkillCooldownTweens[index].AsyncWaitForCompletion();
            token.ThrowIfCancellationRequested();
        }
    }

    private void SetToastVisibleInstant(bool isVisible)
    {
        activeToastTween?.Kill();
        toastRoot.SetActive(isVisible);
        toastRoot.transform.localScale = isVisible
            ? toastVisibleScale
            : new Vector3(
                toastVisibleScale.x,
                HiddenScale,
                toastVisibleScale.z);
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

    private void CacheSkillVisualState()
    {
        if (skillIconDefaultColors != null)
        {
            return;
        }

        skillIconDefaultColors = new Color[skillIconImages.Length];
        activeSkillCooldownTweens = new Tween[skillCooldownFillImages.Length];

        for (var i = 0; i < skillButtons.Length; i++)
        {
            KeepSkillButtonVisibleWhenDisabled(skillButtons[i]);
        }

        for (var i = 0; i < skillIconImages.Length; i++)
        {
            skillIconDefaultColors[i] = skillIconImages[i].color;
        }
    }

    private void KeepSkillButtonVisibleWhenDisabled(Button button)
    {
        var colors = button.colors;
        colors.disabledColor = Color.white;
        button.colors = colors;
    }

    private bool IsValidSkillIndex(int index)
    {
        return index >= 0 && index < SkillSlotCount;
    }
}
