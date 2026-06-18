using System.Collections.Generic;
using _01_Scripts.DTO;
using UnityEngine;

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

            var playerParty = BuildPlayerParty();
            BattleContext.Set(playerParty, enemies.ToArray());

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

        private CharacterBattleData[] BuildPlayerParty()
        {
            return WorldPartyManager.Instance.BuildBattleParty();
        }
    }
}
