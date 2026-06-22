using System;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    [Serializable]
    public class EnemyDropEntry
    {
        public Item item;
        [Range(0f, 1f)] public float dropRate = 0.5f;
        [Min(1)] public int minQuantity = 1;
        [Min(1)] public int maxQuantity = 1;
    }

    [CreateAssetMenu(fileName = "EnemyData", menuName = "World/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public CharacterData characterData;

        [Header("Drop Table")]
        public EnemyDropEntry[] dropTable;
    }
}
