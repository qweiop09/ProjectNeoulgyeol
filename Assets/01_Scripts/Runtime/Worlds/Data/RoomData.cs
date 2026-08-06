using System;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    [Serializable]
    public class FixedEncounterOption
    {
        public FixedEncounterData encounter;
        [Range(0f, 1f)] public float triggerProbability = 0.1f;
    }

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

        [Tooltip("랜덤 인카운터가 발생했을 때, encounterEntries 개별 롤링 대신 여기서 먼저 고정 편성 하나가 뽑힐 수 있다. " +
                 "위에서부터 각자의 triggerProbability로 시도하고, 하나라도 걸리면 그걸로 확정하고 아래 개별 롤링은 건너뛴다.")]
        public FixedEncounterOption[] randomFixedEncounters;

        public RoomType roomType = RoomType.Normal;
        public string displayName;

        [Header("도망")]
        [Tooltip("이 방에서 발생하는 전투에서 도망이 가능한지. 꺼두면 도망 선택 시 아래 설명만 뜨고 실제 도망은 시도되지 않는다.")]
        public bool canEscape = true;

        [Tooltip("도망이 불가능할 때(canEscape=false) 도망을 선택하면 보여줄 설명")]
        public string nonEscapableReason = "이 전투에서는 도망칠 수 없습니다.";

        public DoorConnectionData GetDoorConnection(string doorId)
        {
            foreach (var door in doors)
                if (door.doorId == doorId) return door;
            return null;
        }
    }
}
