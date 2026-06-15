using System;
using _01_Scripts.Timeline.Battle.Marker;
using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle.Receiver
{
public class QTEMarkerReceiver : MonoBehaviour, INotificationReceiver
{
    public event Action<string> OnQTEMarkerReceived;
    
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is BattleMarker marker)
        {
            Debug.Log($" QTEMarker 수신: {marker.message}");
            OnQTEMarkerReceived?.Invoke(marker.message);
        }
        
    }
}
}