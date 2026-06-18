using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    public class RoomDoor : MonoBehaviour
    {
        [Tooltip("RoomData.doors의 doorId와 일치해야 함")]
        public string doorId;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            RoomManager.Instance.RequestTransition(doorId);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            RoomManager.Instance.RequestTransition(doorId);
        }
    }
}
