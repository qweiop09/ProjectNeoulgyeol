using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    public enum RoomType
    {
        Normal,
        Safe,   // 인카운터 없음
        Boss,
        Shop,
        Rest,
        Event
    }

    [CreateAssetMenu(fileName = "RoomData", menuName = "World/Room Data")]
    public class RoomData : ScriptableObject
    {
        [Header("Room")]
        public GameObject roomPrefab;

        [Header("Doors")]
        // doorId 명명 규칙: 단일 방향 → "N" / "S" / "E" / "W"
        // 같은 방향에 문이 여러 개면 → "N_0" / "N_1" / "S_0" 등
        public DoorConnectionData[] doors;

        [Header("Random Encounter")]
        [Tooltip("이 방에서 발생 가능한 인카운터 목록")]
        public EnemyEncounterEntry[] encounterEntries;

        [Range(0f, 1f)]
        [Tooltip("방 진입 시 인카운터가 발생할 확률 (0 = 없음, 1 = 항상)")]
        public float encounterRate = 0.5f;

        public RoomType roomType = RoomType.Normal;
        public string displayName;

        public DoorConnectionData GetDoorConnection(string doorId)
        {
            foreach (var door in doors)
                if (door.doorId == doorId) return door;
            return null;
        }
    }
}
