using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text versionValueLabel;
    [SerializeField] private float entranceOffsetY = 80f;
    [SerializeField] private float entranceDurationSeconds = 0.45f;
    [SerializeField] private float entranceStaggerSeconds = 0.08f;

    private Sequence activeEntranceSequence;
    private RectTransform[] entranceTargets;
    private Vector2[] entranceTargetPositions;

    public IObservable<Unit> PlayClicked => playButton.OnClickAsObservable();
    public IObservable<Unit> SettingsClicked => settingsButton.OnClickAsObservable();
    public IObservable<Unit> QuitClicked => quitButton.OnClickAsObservable();
    public CancellationToken DestroyCancellationToken => this.GetCancellationTokenOnDestroy();

    private void Awake()
    {
        CacheEntranceTargets();
    }

    private void OnDestroy()
    {
        activeEntranceSequence?.Kill();
    }

    public void SetVersionValue(string value)
    {
        versionValueLabel.text = value;
    }

    public void SetMainButtonsInteractable(bool isInteractable)
    {
        playButton.interactable = isInteractable;
        settingsButton.interactable = isInteractable;
        quitButton.interactable = isInteractable;
    }

    public async UniTask PlayEntranceAsync(CancellationToken token)
    {
        CacheEntranceTargets();

        activeEntranceSequence?.Kill();
        ResetEntranceTargetsToStartOffset();

        activeEntranceSequence = DOTween.Sequence();

        for (var i = 0; i < entranceTargets.Length; i++)
        {
            activeEntranceSequence.Insert(
                i * entranceStaggerSeconds,
                entranceTargets[i]
                    .DOAnchorPos(entranceTargetPositions[i], entranceDurationSeconds)
                    .SetEase(Ease.OutCubic));
        }

        using (token.Register(() =>
               {
                   if (activeEntranceSequence != null && activeEntranceSequence.IsActive())
                   {
                       activeEntranceSequence.Kill();
                   }
               }))
        {
            await activeEntranceSequence.AsyncWaitForCompletion();
            token.ThrowIfCancellationRequested();
        }
    }

    private void CacheEntranceTargets()
    {
        if (entranceTargets != null)
        {
            return;
        }

        entranceTargets = new[]
        {
            titleLabel.rectTransform,
            playButton.GetComponent<RectTransform>(),
            settingsButton.GetComponent<RectTransform>(),
            quitButton.GetComponent<RectTransform>(),
            versionValueLabel.rectTransform
        };

        entranceTargetPositions = new Vector2[entranceTargets.Length];

        for (var i = 0; i < entranceTargets.Length; i++)
        {
            entranceTargetPositions[i] = entranceTargets[i].anchoredPosition;
        }
    }

    private void ResetEntranceTargetsToStartOffset()
    {
        for (var i = 0; i < entranceTargets.Length; i++)
        {
            entranceTargets[i].anchoredPosition = entranceTargetPositions[i] + new Vector2(0f, entranceOffsetY);
        }
    }
}
