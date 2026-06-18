using System;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    [Serializable]
    public class EnemyEncounterEntry
    {
        public EnemyData enemyData;

        [Range(0f, 1f)]
        [Tooltip("인카운터 발생 시 이 적이 등장할 확률 (0~1)")]
        public float spawnProbability = 0.5f;

        [Min(1)]
        public int minCount = 1;

        [Min(1)]
        public int maxCount = 3;
    }
}
