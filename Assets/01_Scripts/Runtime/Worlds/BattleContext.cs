using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles;
using _01_Scripts.Runtime.Worlds.Loot;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    public class BattleResult
    {
        public BattleOutcome Outcome;
        public CharacterStatus[] SurvivedParty;
        public LootResult Loot;
    }

    public static class BattleContext
    {
        // 월드 → 배틀 (인카운터 시 저장)
        public static CharacterStatus[] PlayerParty { get; private set; }
        public static CharacterStatus[] EnemyParty { get; private set; }
        public static EnemyData[] EnemyDatas { get; private set; }   // 드랍 계산용
        public static string WorldSceneName { get; private set; }
        public static Vector3 PlayerWorldPosition { get; private set; }
        public static RoomData CurrentRoomData { get; private set; }

        // 배틀 → 월드 (전투 종료 시 저장)
        public static BattleResult Result { get; private set; }

        public static void SetEncounter(
            CharacterStatus[] playerParty,
            CharacterStatus[] enemyParty,
            EnemyData[] enemyDatas,
            string worldSceneName,
            Vector3 playerWorldPosition,
            RoomData currentRoomData)
        {
            PlayerParty = playerParty;
            EnemyParty = enemyParty;
            EnemyDatas = enemyDatas;
            WorldSceneName = worldSceneName;
            PlayerWorldPosition = playerWorldPosition;
            CurrentRoomData = currentRoomData;
        }

        public static void SetResult(BattleResult result) => Result = result;

        public static void ClearEncounter()
        {
            PlayerParty = null;
            EnemyParty = null;
            EnemyDatas = null;
        }

        public static void ClearResult() => Result = null;
    }
}
