using System;
using System.Collections.Generic;
using UniRx;

public class InventoryModel : IDisposable
{
    private readonly List<InventoryItemData> slots = new();

    public ReactiveProperty<bool> IsOpen { get; } = new(false);
    public ReactiveProperty<int> SelectedSlotIndex { get; } = new(-1);
    public IReadOnlyList<InventoryItemData> Slots => slots;

    public void Load(InventoryConfig config)
    {
        slots.Clear();

        for (var i = 0; i < config.SlotCount; i++)
        {
            var item = i < config.StartingItems.Count
                ? config.StartingItems[i]
                : null;

            slots.Add(item);
        }

        SelectedSlotIndex.Value = -1;
        IsOpen.Value = false;
    }

    public void Open()
    {
        IsOpen.Value = true;
    }

    public void Close()
    {
        IsOpen.Value = false;
        SelectedSlotIndex.Value = -1;
    }

    public void SelectSlot(int index)
    {
        if (SelectedSlotIndex.Value == index)
        {
            SelectedSlotIndex.Value = -1;
            return;
        }

        if (index < 0 || index >= slots.Count || slots[index] == null)
        {
            SelectedSlotIndex.Value = -1;
            return;
        }

        SelectedSlotIndex.Value = index;
    }

    public InventoryItemData GetItemAt(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            return null;
        }

        return slots[index];
    }

    public void Dispose()
    {
        IsOpen.Dispose();
        SelectedSlotIndex.Dispose();
    }
}
