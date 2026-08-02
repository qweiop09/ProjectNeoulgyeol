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

            RollEnemies(roomData, out var battleDatas, out var sourceDatas);
            if (battleDatas.Count == 0) return;

            Debug.Log($"[Encounter] 발생! 적 수: {battleDatas.Count}");

            RoomManager.Instance.SetPlayerMovement(false);

            BattleContext.SetEncounter(
                playerParty: WorldPartyManager.Instance.GetBattleParty(),
                enemyParty: battleDatas.ToArray(),
                enemyDatas: sourceDatas.ToArray(),
                worldSceneName: SceneManager.GetActiveScene().name,
                playerWorldPosition: RoomManager.Instance.PlayerPosition,
                currentRoomData: RoomManager.Instance.CurrentRoomData
            );

            SceneLoader.Instance.LoadScene(_battleSceneName);
        }

        private void RollEnemies(RoomData roomData,
            out List<CharacterStatus> battleDatas,
            out List<EnemyData> sourceDatas)
        {
            battleDatas = new List<CharacterStatus>();
            sourceDatas = new List<EnemyData>();

            foreach (var entry in roomData.encounterEntries)
            {
                if (entry.enemyData?.characterData == null) continue;
                if (Random.value > entry.spawnProbability) continue;

                int count = Random.Range(entry.minCount, entry.maxCount + 1);
                for (int i = 0; i < count; i++)
                {
                    battleDatas.Add(new CharacterStatus(entry.enemyData.characterData));
                    sourceDatas.Add(entry.enemyData);
                }
            }
        }
    }
}
