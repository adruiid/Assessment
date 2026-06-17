using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(
    fileName = "HudConfig",
    menuName = "Assessment/HUD/HUD Config")]
public class HudConfig : ScriptableObject
{
    [Header("Demo State")]
    [SerializeField] private int maxPlayerHealth = 100;
    [SerializeField] private int startingPlayerHealth = 75;
    [SerializeField] private int maxBossHealth = 250;
    [SerializeField] private int startingBossHealth = 250;
    [SerializeField] private int startingCurrency = 1250;

    [Header("Animation Timings")]
    [SerializeField] private float currencyAnimationDurationSeconds = 0.4f;
    [SerializeField] private float toastVisibleSeconds = 2.5f;

    [Header("Skills")]
    [SerializeField] private List<SkillHudData> skills = new();

    public int MaxPlayerHealth => maxPlayerHealth;
    public int StartingPlayerHealth => startingPlayerHealth;
    public int MaxBossHealth => maxBossHealth;
    public int StartingBossHealth => startingBossHealth;
    public int StartingCurrency => startingCurrency;
    public float CurrencyAnimationDurationSeconds => currencyAnimationDurationSeconds;
    public float ToastVisibleSeconds => toastVisibleSeconds;
    public IReadOnlyList<SkillHudData> Skills => skills;
}

[Serializable]
public class SkillHudData
{
    [SerializeField] private string skillId = "skill_01";
    [SerializeField] private AssetReferenceSprite iconReference;
    [SerializeField] private float cooldownSeconds = 4f;

    public string SkillId => skillId;
    public AssetReferenceSprite IconReference => iconReference;
    public float CooldownSeconds => cooldownSeconds;
}
