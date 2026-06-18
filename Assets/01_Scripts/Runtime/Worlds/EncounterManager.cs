using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01_Scripts.Runtime.Worlds
{
    public class EncounterManager : MonoBehaviour
    {
        [SerializeField] private string _battleSceneName = "Battle";

        public void TryEncounter(RoomData roomData)
        {
            if (roomData.encounterEntries == null || roomData.encounterEntries.Length == 0) return;
            if (Random.value > roomData.encounterRate) return;

            var result = Roll(roomData);
            if (!result.hasEncounter) return;

            BattleContext.Set(result);

            foreach (var (enemy, count) in result.enemies)
                Debug.Log($"[Encounter] {enemy.characterData.name} x{count}");

            // SceneManager.LoadScene(_battleSceneName);
        }

        private EncounterResult Roll(RoomData roomData)
        {
            var result = new EncounterResult();

            foreach (var entry in roomData.encounterEntries)
            {
                if (entry.enemyData == null) continue;
                if (Random.value > entry.spawnProbability) continue;

                int count = Random.Range(entry.minCount, entry.maxCount + 1);
                result.enemies.Add((entry.enemyData, count));
            }

            result.hasEncounter = result.enemies.Count > 0;
            return result;
        }
    }
}
