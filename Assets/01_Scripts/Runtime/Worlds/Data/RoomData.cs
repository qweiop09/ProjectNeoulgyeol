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

        // --- 확장 필드 ---
        // [Header("Enemies")]
        // public EnemySpawnData[] enemySpawns;

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
