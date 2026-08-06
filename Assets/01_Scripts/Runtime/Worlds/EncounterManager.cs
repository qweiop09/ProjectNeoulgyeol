using System.Collections.Generic;
using _01_Scripts.DTO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01_Scripts.Runtime.Worlds
{
    public class EncounterManager : MonoBehaviour
    {
        public static EncounterManager Instance { get; private set; }

        [Header("Scene")]
        [SerializeField] private string _battleSceneName = "Battle";

        private void Awake()
        {
            Instance = this;
        }

        // 스텝 기반 랜덤 인카운터 (PlayerEncounterTracker가 호출)
        public void TryEncounter(RoomData roomData)
        {
            if (Random.value > roomData.encounterRate) return;

            FixedEncounterData rolledFixed = TryRollFixedOption(roomData.randomFixedEncounters);
            if (rolledFixed != null)
            {
                BuildRosterFromFixedEncounter(rolledFixed, out var fixedBattleDatas, out var fixedSourceDatas);
                StartBattle(roomData, fixedBattleDatas, fixedSourceDatas);
                return;
            }

            if (roomData.encounterEntries == null || roomData.encounterEntries.Length == 0) return;

            RollEnemies(roomData, out var battleDatas, out var sourceDatas);
            StartBattle(roomData, battleDatas, sourceDatas);
        }

        // 트리거 방식과 무관하게 언제든 호출 가능한 범용 진입점 — 필드 인카운터, 스크립트 이벤트 등.
        // contextRoomData를 안 주면 현재 있는 방을 그대로 씀(퇴각/전리품 등 결과 처리에 필요).
        public void StartFixedEncounter(FixedEncounterData encounter, RoomData contextRoomData = null)
        {
            if (encounter == null) return;

            BuildRosterFromFixedEncounter(encounter, out var battleDatas, out var sourceDatas);
            StartBattle(contextRoomData != null ? contextRoomData : RoomManager.Instance.CurrentRoomData, battleDatas, sourceDatas);
        }

        private void StartBattle(RoomData roomData, List<CharacterStatus> battleDatas, List<EnemyData> sourceDatas)
        {
            if (battleDatas.Count == 0) return;

            Debug.Log($"[Encounter] 발생! 적 수: {battleDatas.Count}");

            RoomManager.Instance.SetPlayerMovement(false);

            BattleContext.SetEncounter(
                playerParty: WorldPartyManager.Instance.GetBattleParty(),
                enemyParty: battleDatas.ToArray(),
                enemyDatas: sourceDatas.ToArray(),
                worldSceneName: SceneManager.GetActiveScene().name,
                playerWorldPosition: RoomManager.Instance.PlayerPosition,
                currentRoomData: roomData
            );

            SceneLoader.Instance.LoadScene(_battleSceneName);
        }

        // options를 위에서부터 순서대로 굴려서 처음 걸리는 하나를 반환. 하나도 안 걸리면 null(개별 롤링으로 폴백).
        private FixedEncounterData TryRollFixedOption(FixedEncounterOption[] options)
        {
            if (options == null) return null;

            foreach (var option in options)
            {
                if (option.encounter == null) continue;
                if (Random.value <= option.triggerProbability) return option.encounter;
            }

            return null;
        }

        private void BuildRosterFromFixedEncounter(FixedEncounterData fixedEncounter,
            out List<CharacterStatus> battleDatas,
            out List<EnemyData> sourceDatas)
        {
            battleDatas = new List<CharacterStatus>();
            sourceDatas = new List<EnemyData>();

            if (fixedEncounter.roster == null) return; // roster를 안 채운 에셋 — 빈 전투로 처리(StartBattle이 걸러줌)

            foreach (var entry in fixedEncounter.roster)
            {
                if (entry.enemyData?.characterData == null) continue;

                for (int i = 0; i < entry.count; i++)
                {
                    battleDatas.Add(new CharacterStatus(entry.enemyData.characterData));
                    sourceDatas.Add(entry.enemyData);
                }
            }
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
