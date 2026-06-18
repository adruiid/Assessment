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
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TMP_Text healthValueLabel;
    [SerializeField] private TMP_Text currencyValueLabel;
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private Button[] skillButtons;
    [SerializeField] private Image[] skillIconImages;
    [SerializeField] private Image[] skillCooldownFillImages;
    [SerializeField] private Color cooldownSkillIconColor = new(0.7f, 0.7f, 0.7f, 1f);

    private Tween activeCurrencyTween;
    private Tween[] activeSkillCooldownTweens;
    private Color[] skillIconDefaultColors;

    public CancellationToken DestroyCancellationToken => this.GetCancellationTokenOnDestroy();
    public int SkillSlotCount => Mathf.Min(skillButtons.Length, skillIconImages.Length, skillCooldownFillImages.Length);

    private void Awake()
    {
        CacheSkillVisualState();
    }

    private void OnDestroy()
    {
        activeCurrencyTween?.Kill();

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
