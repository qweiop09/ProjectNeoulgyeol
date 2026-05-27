using System;
using _01_Scripts.Timeline.Battle.Marker;
using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle.Receiver
{
public class BattleMarkerReceiver : MonoBehaviour, INotificationReceiver
{
    public event Action<string> OnBattleMarkerReceived;
    
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is BattleMarker marker)
        {
            Debug.Log($"BattleMarker 수신: {marker.message}");
            OnBattleMarkerReceived?.Invoke(marker.message);
        }
        
    }
}
}