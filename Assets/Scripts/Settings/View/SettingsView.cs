using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Dropdown graphicsDropdown;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Button sfxTestButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private float panelOffsetY = 60f;
    [SerializeField] private float panelAnimationSeconds = 0.25f;

    private Tween activeVisibilityTween;
    private Vector2 shownPosition;
    private bool isCached;

    public IObservable<float> MasterVolumeChanged => masterVolumeSlider.OnValueChangedAsObservable();
    public IObservable<float> MusicVolumeChanged => musicVolumeSlider.OnValueChangedAsObservable();
    public IObservable<float> SfxVolumeChanged => sfxVolumeSlider.OnValueChangedAsObservable();
    public IObservable<int> GraphicsChanged => graphicsDropdown.onValueChanged.AsObservable();
    public IObservable<int> LanguageChanged => languageDropdown.onValueChanged.AsObservable();
    public IObservable<Unit> SfxTestClicked => sfxTestButton.OnClickAsObservable();
    public IObservable<Unit> CloseClicked => closeButton.OnClickAsObservable();
    public CancellationToken DestroyCancellationToken => this.GetCancellationTokenOnDestroy();

    private void Awake()
    {
        CacheReferences();
    }

    private void OnDestroy()
    {
        activeVisibilityTween?.Kill();
    }

    public void SetMasterVolume(float value)
    {
        masterVolumeSlider.SetValueWithoutNotify(value);
    }

    public void SetMusicVolume(float value)
    {
        musicVolumeSlider.SetValueWithoutNotify(value);
    }

    public void SetSfxVolume(float value)
    {
        sfxVolumeSlider.SetValueWithoutNotify(value);
    }

    public void SetGraphicsOptions(IReadOnlyList<string> options)
    {
        SetDropdownOptions(graphicsDropdown, options);
    }

    public void SetGraphicsIndex(int index)
    {
        SetDropdownIndex(graphicsDropdown, index);
    }

    public void SetLanguageOptions(IReadOnlyList<string> options)
    {
        SetDropdownOptions(languageDropdown, options);
    }

    public void SetLanguageIndex(int index)
    {
        SetDropdownIndex(languageDropdown, index);
    }

    public void SetVisibleInstant(bool isVisible)
    {
        CacheReferences();
        activeVisibilityTween?.Kill();

        gameObject.SetActive(isVisible);

        if (panelRoot != null)
        {
            panelRoot.anchoredPosition = shownPosition;
        }

        SetCanvasState(isVisible ? 1f : 0f, isVisible);
    }

    public async UniTask SetVisibleAsync(bool isVisible, CancellationToken token)
    {
        CacheReferences();
        activeVisibilityTween?.Kill();

        if (isVisible)
        {
            gameObject.SetActive(true);
            SetCanvasState(0f, false);

            if (panelRoot != null)
            {
                panelRoot.anchoredPosition = shownPosition + new Vector2(0f, panelOffsetY);
            }
        }
        else
        {
            SetCanvasState(1f, false);
        }

        var targetAlpha = isVisible ? 1f : 0f;
        var targetPosition = isVisible
            ? shownPosition
            : shownPosition + new Vector2(0f, panelOffsetY);

        var sequence = DOTween.Sequence();

        if (canvasGroup != null)
        {
            sequence.Join(canvasGroup.DOFade(targetAlpha, panelAnimationSeconds).SetEase(Ease.OutCubic));
        }

        if (panelRoot != null)
        {
            sequence.Join(panelRoot.DOAnchorPos(targetPosition, panelAnimationSeconds).SetEase(Ease.OutCubic));
        }

        activeVisibilityTween = sequence;

        using (token.Register(() =>
               {
                   if (sequence.IsActive())
                   {
                       sequence.Kill();
                   }
               }))
        {
            await sequence.AsyncWaitForCompletion();
            token.ThrowIfCancellationRequested();
        }

        if (!isVisible)
        {
            gameObject.SetActive(false);
        }

        SetCanvasState(targetAlpha, isVisible);
    }

    private void CacheReferences()
    {
        if (isCached)
        {
            return;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (panelRoot == null)
        {
            panelRoot = (RectTransform)transform;
        }

        if (panelRoot != null)
        {
            shownPosition = panelRoot.anchoredPosition;
        }

        isCached = true;
    }

    private void SetCanvasState(float alpha, bool isInteractive)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = alpha;
        canvasGroup.interactable = isInteractive;
        canvasGroup.blocksRaycasts = isInteractive;
    }

    private void SetDropdownOptions(TMP_Dropdown dropdown, IReadOnlyList<string> options)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(options));
        dropdown.interactable = options.Count > 0;
        dropdown.RefreshShownValue();
    }

    private void SetDropdownIndex(TMP_Dropdown dropdown, int index)
    {
        if (dropdown.options.Count == 0)
        {
            return;
        }

        var safeIndex = index;

        if (safeIndex < 0)
        {
            safeIndex = 0;
        }

        if (safeIndex >= dropdown.options.Count)
        {
            safeIndex = dropdown.options.Count - 1;
        }

        dropdown.SetValueWithoutNotify(safeIndex);
        dropdown.RefreshShownValue();
    }
}
