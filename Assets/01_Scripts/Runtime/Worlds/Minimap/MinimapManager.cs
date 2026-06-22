using System;
using System.Collections.Generic;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds.Minimap
{
    public class MinimapRoomState
    {
        public bool IsVisited;
    }

    public class MinimapManager : MonoBehaviour
    {
        public static MinimapManager Instance { get; private set; }

        private readonly Dictionary<RoomData, MinimapRoomState> _states = new();
        public RoomData CurrentRoom { get; private set; }

        public event Action OnUpdated;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (RoomManager.Instance == null) return;

            RoomManager.Instance.OnRoomActivated += HandleRoomActivated;

            // RoomManager.Start()가 먼저 실행됐을 경우 현재 방을 직접 처리
            if (RoomManager.Instance.CurrentRoomData != null)
                HandleRoomActivated(RoomManager.Instance.CurrentRoomData);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (RoomManager.Instance != null)
                RoomManager.Instance.OnRoomActivated -= HandleRoomActivated;
        }

        public void RegisterRooms(IEnumerable<RoomData> rooms)
        {
            foreach (var room in rooms)
                if (!_states.ContainsKey(room))
                    _states[room] = new MinimapRoomState();
        }

        private void HandleRoomActivated(RoomData room)
        {
            CurrentRoom = room;
            EnsureState(room).IsVisited = true;
            OnUpdated?.Invoke();
        }

        public bool TryGetState(RoomData room, out MinimapRoomState state)
            => _states.TryGetValue(room, out state);

        private MinimapRoomState EnsureState(RoomData room)
        {
            if (!_states.TryGetValue(room, out var state))
            {
                state = new MinimapRoomState();
                _states[room] = state;
            }
            return state;
        }
    }
}
