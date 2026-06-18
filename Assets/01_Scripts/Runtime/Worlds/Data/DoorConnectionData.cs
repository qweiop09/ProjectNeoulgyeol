using System;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    [Serializable]
    public class DoorConnectionData
    {
        [Tooltip("이 문의 ID (RoomDoor의 doorId와 일치해야 함)")]
        public string doorId;

        [Tooltip("이 문이 연결된 목적지 방")]
        public RoomData targetRoom;

        [Tooltip("목적지 방에서 나올 문의 ID")]
        public string exitDoorId;
    }
}
