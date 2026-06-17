using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

[CreateAssetMenu(
    fileName = "InventoryItemData",
    menuName = "Assessment/Inventory/Inventory Item Data")]
public class InventoryItemData : ScriptableObject
{
    [SerializeField] private string itemId = "item_01";
    [SerializeField] private AssetReferenceSprite iconReference;
    [SerializeField] private LocalizedString displayName;
    [SerializeField] private LocalizedString description;

    [SerializeField]
    [Range(0f, 1f)]
    private float statFillNormalized = 0.5f;

    public string ItemId => itemId;
    public AssetReferenceSprite IconReference => iconReference;
    public LocalizedString DisplayName => displayName;
    public LocalizedString Description => description;
    public float StatFillNormalized => statFillNormalized;
}
