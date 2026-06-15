// using System;
// using _01_Scripts.Timeline.Battle.Marker;
// using UnityEngine;
// using UnityEngine.Playables;
//
// namespace _01_Scripts.Timeline.Battle.Receiver
// {
// public class AnimationMarkerReceiver : MonoBehaviour, INotificationReceiver
// {
//     public event Action<string> OnAnimationMarkerReceived;
//     
//     public void OnNotify(Playable origin, INotification notification, object context)
//     {
//         if (notification is BattleMarker marker)
//         {
//             Debug.Log($"BattleMarker 수신: {marker.message}");
//             OnAnimationMarkerReceived?.Invoke(marker.message);
//         }
//         
//     }
// }
// }