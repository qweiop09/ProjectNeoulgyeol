using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    [CreateAssetMenu(fileName = "RoomData", menuName = "World/Room Data")]
    public class RoomData : ScriptableObject
    {
        [Header("Room")]
        public GameObject roomPrefab;

        [Header("Doors")]
        public DoorConnectionData[] doors;

        [Header("Random Encounter")]
        [Tooltip("이 방에서 발생 가능한 인카운터 목록")]
        public EnemyEncounterEntry[] encounterEntries;

        [Range(0f, 1f)]
        [Tooltip("방 진입 시 인카운터가 발생할 확률 (0 = 없음, 1 = 항상)")]
        public float encounterRate = 0.5f;

        // [Header("Events")]
        // public RoomEventData[] events;

        public DoorConnectionData GetDoorConnection(string doorId)
        {
            foreach (var door in doors)
                if (door.doorId == doorId) return door;
            return null;
        }
    }
}
