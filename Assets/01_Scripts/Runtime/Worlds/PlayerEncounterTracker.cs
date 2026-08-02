using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    public class PlayerEncounterTracker : MonoBehaviour
    {
        [SerializeField] private EncounterManager _encounterManager;
        [SerializeField] private float _stepDistance = 3f;
        [SerializeField] private PlayerCharacterMove _playerMove;

        private Vector3 _lastPosition;
        private float _accumulatedDistance;

        private void Start()
        {
            _lastPosition = transform.position;
        }

        private void Update()
        {
            // 이동이 막혀 있는 동안(방 전환 등)은 위치 추적과 누적 거리를 초기화
            if (_playerMove != null && !_playerMove.enabled)
            {
                _lastPosition = transform.position;
                _accumulatedDistance = 0f;
                return;
            }

            float moved = Vector3.Distance(transform.position, _lastPosition);
            if (moved == 0f) return;

            _accumulatedDistance += moved;
            _lastPosition = transform.position;

            if (_accumulatedDistance >= _stepDistance)
            {
                _accumulatedDistance = 0f;
                TryEncounter();
            }
        }

        private void TryEncounter()
        {
            var roomData = RoomManager.Instance.CurrentRoomData;
            if (roomData == null) return;

            _encounterManager.TryEncounter(roomData);
        }
    }
}
