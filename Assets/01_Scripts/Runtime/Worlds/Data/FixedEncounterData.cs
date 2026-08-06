using System;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    [Serializable]
    public class FixedEncounterEnemyEntry
    {
        public EnemyData enemyData;
        [Min(1)] public int count = 1;
    }

    // 미리 정해둔 적 편성 프리셋 — 보스전처럼 랜덤이 아니라 항상 같은 구성으로 시작해야 하는 전투에 사용.
    [CreateAssetMenu(menuName = "World/Fixed Encounter", fileName = "New Fixed Encounter")]
    public class FixedEncounterData : ScriptableObject
    {
        public string displayName;
        public FixedEncounterEnemyEntry[] roster;
    }
}
