using System;
using System.Collections.Generic;
using _01_Scripts.DTO.Item;
using _01_Scripts.Interfacese;

namespace _01_Scripts.Runtime.Worlds
{
    [Serializable]
    public class InventorySlot
    {
        public Item item;
        public int quantity;
    }

    public class PartyInventory : Singleton<PartyInventory>
    {
        private readonly List<InventorySlot> _slots = new();

        public IReadOnlyList<InventorySlot> Slots => _slots;

        public void Add(Item item, int quantity = 1)
        {
            var slot = _slots.Find(s => s.item == item);
            if (slot != null)
                slot.quantity += quantity;
            else
                _slots.Add(new InventorySlot { item = item, quantity = quantity });
        }

        public bool Remove(Item item, int quantity = 1)
        {
            var slot = _slots.Find(s => s.item == item);
            if (slot == null || slot.quantity < quantity) return false;

            slot.quantity -= quantity;
            if (slot.quantity <= 0) _slots.Remove(slot);
            return true;
        }
    }
}
