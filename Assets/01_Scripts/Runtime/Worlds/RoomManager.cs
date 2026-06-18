using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _01_Scripts.Interfacese;

namespace _01_Scripts.Runtime.Worlds
{
    public class RoomManager : Singleton<RoomManager>
    {
        [Header("Transition")]
        [SerializeField] private RoomTransition _transition;

        [Header("World")]
        [SerializeField] private RoomData _startRoom;

        [Header("Player")]
        [SerializeField] private Transform _player;

        public RoomData CurrentRoomData => _currentRoom?.data;

        private readonly Dictionary<RoomData, RoomInstance> _rooms = new();
        private RoomInstance _currentRoom;
        private bool _isTransitioning;
        private PlayerCharacterMove _playerMove;

        protected override void Awake()
        {
            base.Awake();
            if (_player != null)
                _playerMove = _player.GetComponent<PlayerCharacterMove>();
            InitializeRooms(_startRoom);
        }

        public void SetPlayerMovement(bool enabled)
        {
            if (_playerMove != null)
                _playerMove.enabled = enabled;
        }

        private void Start()
        {
            if (_currentRoom == null && _rooms.TryGetValue(_startRoom, out var room))
                ActivateRoom(room, null);
        }

        // 시작 방부터 연결된 모든 방을 BFS로 순회하며 미리 인스턴스화
        private void InitializeRooms(RoomData root)
        {
            var queue = new Queue<RoomData>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var data = queue.Dequeue();
                if (_rooms.ContainsKey(data)) continue;

                var go = Instantiate(data.roomPrefab);
                var instance = go.GetComponent<RoomInstance>();
                if (instance == null)
                    instance = go.AddComponent<RoomInstance>();

                instance.data = data;
                instance.Hide();
                _rooms[data] = instance;

                foreach (var door in data.doors)
                    if (door.targetRoom != null && !_rooms.ContainsKey(door.targetRoom))
                        queue.Enqueue(door.targetRoom);
            }
        }

        public void RequestTransition(string fromDoorId)
        {
            if (_isTransitioning || _currentRoom == null) return;

            var connection = _currentRoom.data.GetDoorConnection(fromDoorId);
            if (connection == null)
            {
                Debug.LogWarning($"[RoomManager] Door '{fromDoorId}'에 연결 정보가 없습니다.");
                return;
            }

            if (!_rooms.TryGetValue(connection.targetRoom, out var targetRoom))
            {
                Debug.LogWarning($"[RoomManager] 목적지 방 인스턴스를 찾을 수 없습니다: {connection.targetRoom.name}");
                return;
            }

            StartCoroutine(TransitionRoutine(targetRoom, connection.exitDoorId));
        }

        private IEnumerator TransitionRoutine(RoomInstance targetRoom, string exitDoorId)
        {
            _isTransitioning = true;
            SetPlayerMovement(false);

            yield return _transition.Execute(() =>
            {
                _currentRoom.Hide();
                ActivateRoom(targetRoom, exitDoorId);
            });

            SetPlayerMovement(true);
            _isTransitioning = false;
        }

        private void ActivateRoom(RoomInstance room, string exitDoorId)
        {
            room.Show();
            _currentRoom = room;

            if (_player != null && exitDoorId != null)
            {
                var exitDoor = room.GetDoor(exitDoorId);
                if (exitDoor != null)
                    _player.position = exitDoor.transform.position;
                else
                    Debug.LogWarning($"[RoomManager] 출구 문 '{exitDoorId}'을 찾을 수 없습니다.");
            }

        }
    }
}
