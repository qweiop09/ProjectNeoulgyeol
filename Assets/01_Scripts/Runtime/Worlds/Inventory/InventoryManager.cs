using System;
using System.Collections.Generic;
using System.Linq;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using _01_Scripts.Interfacese;
using _01_Scripts.Runtime.Battles;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds.Inventory
{
public enum InventoryResult
{
    Success,
    CapacityExceeded,
    InvalidItem
}

[Serializable]
public struct CategoryCapacity
{
    public ItemCategory category;
    public int maxSlots;
}

// 파티/캐릭터 구분 없이 공유하는 인벤토리 관리 클래스. 월드/전투 씬을 오가도 유지된다(Singleton -> DontDestroyOnLoad).
public class InventoryManager : Singleton<InventoryManager>
{
    [SerializeField]
    private List<CategoryCapacity> categoryCapacities = new()
    {
        new CategoryCapacity { category = ItemCategory.Equipment, maxSlots = 100 },
        new CategoryCapacity { category = ItemCategory.Consumable, maxSlots = 100 },
        new CategoryCapacity { category = ItemCategory.Material, maxSlots = 100 },
        new CategoryCapacity { category = ItemCategory.Currency, maxSlots = 100 },
        new CategoryCapacity { category = ItemCategory.EventItem, maxSlots = 100 },
    };

    [Header("디버그")]
    [SerializeField] private Item debugTestItem;
    [SerializeField] private int debugTestQuantity = 1;

    private readonly List<InventorySlot> slots = new();

    public event Action onInventoryChanged;
    public event Action<Item, InventoryResult> onItemRejected;

    public IReadOnlyList<InventorySlot> Slots => slots;

    public IEnumerable<InventorySlot> GetSlots(ItemCategory category) =>
        slots.Where(s => s.item != null && s.item.category == category);

    // 부작용 없이 "지금 이 개수만큼 넣을 수 있는지"만 확인
    public bool CanAddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        int maxSlots = GetMaxSlots(item.category);

        if (item.IsStackable)
        {
            InventorySlot existing = FindStackableSlot(item);
            int remainingInExisting = existing != null ? item.maxStack - existing.quantity : 0;
            int overflow = quantity - remainingInExisting;
            if (overflow <= 0) return true;

            int newSlotsNeeded = Mathf.CeilToInt(overflow / (float)item.maxStack);
            return CountSlotsInCategory(item.category) + newSlotsNeeded <= maxSlots;
        }

        return CountSlotsInCategory(item.category) + quantity <= maxSlots;
    }

    // 전량 수용 불가능하면 아무것도 바꾸지 않고 실패 처리 (부분 추가 없음)
    public InventoryResult AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
            return Reject(item, InventoryResult.InvalidItem);

        if (!CanAddItem(item, quantity))
            return Reject(item, InventoryResult.CapacityExceeded);

        if (item.IsStackable)
        {
            int remaining = quantity;

            InventorySlot existing = FindStackableSlot(item);
            if (existing != null)
            {
                int space = item.maxStack - existing.quantity;
                int add = Mathf.Min(space, remaining);
                existing.quantity += add;
                remaining -= add;
            }

            while (remaining > 0)
            {
                int add = Mathf.Min(item.maxStack, remaining);
                slots.Add(new InventorySlot { item = item, quantity = add });
                remaining -= add;
            }
        }
        else
        {
            for (int i = 0; i < quantity; i++)
                slots.Add(new InventorySlot { item = item, quantity = 1 });
        }

        onInventoryChanged?.Invoke();
        return InventoryResult.Success;
    }

    public InventoryResult RemoveItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return InventoryResult.InvalidItem;

        if (item.IsStackable)
        {
            // 오버플로우로 같은 아이템이 슬롯 여러 개에 나뉘어 있을 수 있으므로 전체 슬롯에서 합산 차감한다.
            List<InventorySlot> matching = slots.Where(s => s.item == item).ToList();
            int total = matching.Sum(s => s.quantity);
            if (total < quantity) return InventoryResult.InvalidItem;

            int remaining = quantity;
            foreach (InventorySlot s in matching)
            {
                if (remaining <= 0) break;

                int take = Mathf.Min(s.quantity, remaining);
                s.quantity -= take;
                remaining -= take;

                if (s.quantity <= 0) slots.Remove(s);
            }

            onInventoryChanged?.Invoke();
            return InventoryResult.Success;
        }
        else
        {
            // 비스택: quantity개만큼 슬롯 제거. 장착 중인 슬롯은 대상에서 제외(해제부터 해야 함).
            List<InventorySlot> removable = slots
                .Where(s => s.item == item && s.equippedBy == null)
                .Take(quantity)
                .ToList();
            if (removable.Count < quantity) return InventoryResult.InvalidItem;

            foreach (InventorySlot s in removable) slots.Remove(s);

            onInventoryChanged?.Invoke();
            return InventoryResult.Success;
        }
    }

    // 비스택 아이템의 특정 인스턴스 하나를 콕 집어 제거 (예: 이 검 팔기)
    public bool RemoveSlot(InventorySlot slot)
    {
        if (slot == null || slot.equippedBy != null) return false;

        bool removed = slots.Remove(slot);
        if (removed) onInventoryChanged?.Invoke();
        return removed;
    }

    public bool EquipItem(CharacterHandler character, InventorySlot equipmentSlot)
    {
        if (character == null || equipmentSlot == null) return false;
        if (equipmentSlot.item is not EquipmentItem equipment) return false;
        if (equipmentSlot.equippedBy != null) return false; // 이미 누군가 장착 중
        if (!slots.Contains(equipmentSlot)) return false;

        UnequipItem(character, equipment.slotType); // 같은 부위에 이미 장착 중이던 게 있으면 먼저 해제

        equipmentSlot.equippedBy = character;
        character.GetCharacterStatus().SetEquipment(equipment.slotType, equipment);

        onInventoryChanged?.Invoke();
        return true;
    }

    public bool UnequipItem(CharacterHandler character, EquipmentSlotType slotType)
    {
        if (character == null) return false;

        CharacterStatus status = character.GetCharacterStatus();
        if (status.GetEquipment(slotType) == null) return false;

        InventorySlot equipped = slots.Find(s =>
            s.equippedBy == character && s.item is EquipmentItem eq && eq.slotType == slotType);
        if (equipped != null) equipped.equippedBy = null;

        status.SetEquipment(slotType, null);

        onInventoryChanged?.Invoke();
        return true;
    }

    private InventorySlot FindStackableSlot(Item item) =>
        slots.Find(s => s.item == item && s.quantity < item.maxStack);

    private int CountSlotsInCategory(ItemCategory category) =>
        slots.Count(s => s.item != null && s.item.category == category);

    private int GetMaxSlots(ItemCategory category)
    {
        foreach (CategoryCapacity c in categoryCapacities)
            if (c.category == category) return c.maxSlots;
        return 0;
    }

    private InventoryResult Reject(Item item, InventoryResult result)
    {
        onItemRejected?.Invoke(item, result);
        return result;
    }

    [ContextMenu("Debug: Add Test Item")]
    private void DebugAddTestItem()
    {
        if (debugTestItem == null)
        {
            Debug.LogWarning("[InventoryManager] debugTestItem이 비어있습니다.");
            return;
        }

        InventoryResult result = AddItem(debugTestItem, debugTestQuantity);
        Debug.Log($"[InventoryManager] DebugAddTestItem: {debugTestItem.itemName} x{debugTestQuantity} -> {result}");
    }
}
}
