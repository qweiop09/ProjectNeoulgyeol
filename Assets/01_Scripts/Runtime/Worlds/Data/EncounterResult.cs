using System.Collections.Generic;

namespace _01_Scripts.Runtime.Worlds
{
    public class EncounterResult
    {
        public bool hasEncounter;

        // 등장하는 적 목록: (EnemyData, 마릿수) 쌍
        public List<(EnemyData enemy, int count)> enemies = new();
    }
}
