using System.Collections.Generic;
using _01_Scripts.DTO.Item;

namespace _01_Scripts.Runtime.Worlds.Loot
{
    public class LootDrop
    {
        public Item Item;
        public int Quantity;
    }

    public class LootResult
    {
        public List<LootDrop> Drops = new();
        public bool HasLoot => Drops.Count > 0;
    }
}
