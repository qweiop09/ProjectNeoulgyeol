using System;
using System.Collections.Generic;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds.Minimap
{
    [Serializable]
    public class RoomMinimapEntry
    {
        public RoomData room;

        [Tooltip("미니맵 격자 기준점. 시작방=(0,0) 기준")]
        public Vector2Int anchor;

        [Tooltip("방이 차지하는 셀 목록 (anchor 기준 상대 좌표). 기본 1x1 = [(0,0)]")]
        public Vector2Int[] cells = { new(0, 0) };
    }

    [CreateAssetMenu(fileName = "MinimapLayout", menuName = "World/Minimap Layout")]
    public class MinimapLayoutData : ScriptableObject
    {
        public RoomMinimapEntry[] entries;

        private Dictionary<RoomData, RoomMinimapEntry> _lookup;

        public RoomMinimapEntry GetEntry(RoomData room)
        {
            BuildLookupIfNeeded();
            _lookup.TryGetValue(room, out var entry);
            return entry;
        }

        public IEnumerable<RoomMinimapEntry> GetAllEntries() => entries;

        private void BuildLookupIfNeeded()
        {
            if (_lookup != null) return;
            _lookup = new Dictionary<RoomData, RoomMinimapEntry>();
            foreach (var entry in entries)
                if (entry.room != null)
                    _lookup[entry.room] = entry;
        }

        // SO가 에디터에서 변경될 때 캐시 무효화
        private void OnValidate() => _lookup = null;
    }
}
