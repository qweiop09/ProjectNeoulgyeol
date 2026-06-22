using System;
using System.Collections;
using System.Collections.Generic;
using _01_Scripts.Runtime.Worlds.UI;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        [Header("Transition")]
        [SerializeField] private RoomTransition _transition;

        [Header("World")]
        [SerializeField] private RoomData _startRoom;

        [Header("Player")]
        [SerializeField] private Transform _player;

        [Header("UI")]
        [SerializeField] private LootUI _lootUI;

        public event Action<RoomData> OnRoomActivated;

        public RoomData CurrentRoomData => _currentRoom?.data;
        public Vector3 PlayerPosition => _player != null ? _player.position : Vector3.zero;

        public IEnumerable<RoomData> GetAllRoomDatas() => _rooms.Keys;

        private readonly Dictionary<RoomData, RoomInstance> _rooms = new();
        private RoomInstance _currentRoom;
        private bool _isTransitioning;
        private PlayerCharacterMove _playerMove;

        private void Awake()
        {
            Instance = this;
            if (_player != null)
                _playerMove = _player.GetComponent<PlayerCharacterMove>();
            InitializeRooms(_startRoom);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (BattleContext.Result != null)
            {
                HandleBattleReturn();
                return;
            }

            if (_currentRoom == null && _rooms.TryGetValue(_startRoom, out var room))
                ActivateRoom(room, null);
        }

        public void SetPlayerMovement(bool enabled)
        {
            if (_playerMove != null)
                _playerMove.enabled = enabled;
        }

        // 배틀 씬에서 돌아왔을 때 처리
        private void HandleBattleReturn()
        {
            var result = BattleContext.Result;
            var returnRoomData = BattleContext.CurrentRoomData;
            var returnPosition = BattleContext.PlayerWorldPosition;

            BattleContext.ClearResult();
            BattleContext.ClearEncounter();

            if (result.IsWin)
            {
                WorldPartyManager.Instance.ApplyBattleResults(result.SurvivedParty);

                if (returnRoomData != null && _rooms.TryGetValue(returnRoomData, out var returnRoom))
                    ActivateRoom(returnRoom, null);
                else if (_rooms.TryGetValue(_startRoom, out var fallback))
                    ActivateRoom(fallback, null);

                if (_player != null)
                    _player.position = returnPosition;

                // 전리품이 있으면 UI 표시 후 이동 허용, 없으면 즉시 허용
                if (_lootUI != null && result.Loot != null && result.Loot.HasLoot)
                    _lootUI.Show(result.Loot, () => SetPlayerMovement(true));
                else
                    SetPlayerMovement(true);
            }
            else
            {
                WorldPartyManager.Instance.ResetParty();

                if (_rooms.TryGetValue(_startRoom, out var startRoom))
                    ActivateRoom(startRoom, null);

                SetPlayerMovement(true);
            }
        }

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
            room.MarkVisited();
            _currentRoom = room;

            if (_player != null && exitDoorId != null)
            {
                var exitDoor = room.GetDoor(exitDoorId);
                if (exitDoor != null)
                    _player.position = exitDoor.transform.position;
                else
                    Debug.LogWarning($"[RoomManager] 출구 문 '{exitDoorId}'을 찾을 수 없습니다.");
            }

            OnRoomActivated?.Invoke(room.data);
        }
    }
}
