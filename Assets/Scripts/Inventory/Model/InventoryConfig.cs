using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "InventoryConfig",
    menuName = "Assessment/Inventory/Inventory Config")]
public class InventoryConfig : ScriptableObject
{
    [SerializeField] private int slotCount = 12;
    [SerializeField] private List<InventoryItemData> startingItems = new();

    public int SlotCount => slotCount;
    public IReadOnlyList<InventoryItemData> StartingItems => startingItems;
}
