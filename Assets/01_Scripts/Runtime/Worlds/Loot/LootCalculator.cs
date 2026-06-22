using System.Collections.Generic;
using _01_Scripts.DTO.Item;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds.Loot
{
    public static class LootCalculator
    {
        public static LootResult Calculate(EnemyData[] enemies)
        {
            var result = new LootResult();
            if (enemies == null) return result;

            // 같은 아이템은 합산
            var tally = new Dictionary<Item, int>();

            foreach (var enemy in enemies)
            {
                if (enemy.dropTable == null) continue;

                foreach (var entry in enemy.dropTable)
                {
                    if (entry.item == null) continue;
                    if (Random.value > entry.dropRate) continue;

                    int qty = Random.Range(entry.minQuantity, entry.maxQuantity + 1);
                    if (tally.ContainsKey(entry.item))
                        tally[entry.item] += qty;
                    else
                        tally[entry.item] = qty;
                }
            }

            foreach (var (item, qty) in tally)
                result.Drops.Add(new LootDrop { Item = item, Quantity = qty });

            if (result.HasLoot)
            {
                var sb = new System.Text.StringBuilder("[Loot] 드랍 아이템:\n");
                foreach (var drop in result.Drops)
                    sb.AppendLine($"  - {drop.Item.itemName} x{drop.Quantity}");
                Debug.Log(sb.ToString());
            }
            else
            {
                Debug.Log("[Loot] 드랍 없음");
            }

            return result;
        }
    }
}
