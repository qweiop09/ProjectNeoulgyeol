using System.Collections.Generic;
using _01_Scripts.DTO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01_Scripts.Runtime.Worlds
{
    public class EncounterManager : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string _battleSceneName = "Battle";

        public void TryEncounter(RoomData roomData)
        {
            if (roomData.encounterEntries == null || roomData.encounterEntries.Length == 0) return;
            if (Random.value > roomData.encounterRate) return;

            var enemies = RollEnemies(roomData);
            if (enemies.Count == 0) return;

            Debug.Log($"[Encounter] 발생! 적 종류: {enemies.Count}");
            foreach (var data in enemies)
                Debug.Log($"[Encounter] {data.CharacterData.name}");

            RoomManager.Instance.SetPlayerMovement(false);

            BattleContext.SetEncounter(
                playerParty: WorldPartyManager.Instance.GetBattleParty(),
                enemyParty: enemies.ToArray(),
                worldSceneName: SceneManager.GetActiveScene().name,
                playerWorldPosition: RoomManager.Instance.PlayerPosition,
                currentRoomData: RoomManager.Instance.CurrentRoomData
            );

            SceneLoader.Instance.LoadScene(_battleSceneName);
        }

        private List<CharacterBattleData> RollEnemies(RoomData roomData)
        {
            var result = new List<CharacterBattleData>();

            foreach (var entry in roomData.encounterEntries)
            {
                if (entry.enemyData?.characterData == null) continue;
                if (Random.value > entry.spawnProbability) continue;

                int count = Random.Range(entry.minCount, entry.maxCount + 1);
                for (int i = 0; i < count; i++)
                    result.Add(new CharacterBattleData(entry.enemyData.characterData));
            }

            return result;
        }
    }
}
