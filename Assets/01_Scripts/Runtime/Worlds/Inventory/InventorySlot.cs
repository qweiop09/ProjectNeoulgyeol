using System;
using _01_Scripts.DTO.Item;
using _01_Scripts.Runtime.Battles;

namespace _01_Scripts.Runtime.Worlds.Inventory
{
[Serializable]
public class InventorySlot
{
    public Item item;
    public int quantity;

    // Equipment 카테고리에서만 의미 있음 (현재 이 슬롯의 장비를 장착 중인 캐릭터). 그 외 카테고리는 항상 null.
    public CharacterHandler equippedBy;

    public bool IsStackable => item != null && item.IsStackable;
}
}
