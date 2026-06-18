using System.Collections.Generic;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    public class RoomInstance : MonoBehaviour
    {
        public RoomData data;

        private Dictionary<string, RoomDoor> _doorMap;

        private void Awake()
        {
            _doorMap = new Dictionary<string, RoomDoor>();
            foreach (var door in GetComponentsInChildren<RoomDoor>())
                _doorMap[door.doorId] = door;
        }

        public RoomDoor GetDoor(string doorId)
        {
            _doorMap.TryGetValue(doorId, out var door);
            return door;
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
