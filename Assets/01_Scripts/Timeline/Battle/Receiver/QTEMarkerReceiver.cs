using System;
using _01_Scripts.Timeline.Battle.Marker;
using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle.Receiver
{
public class QTEMarkerReceiver : MonoBehaviour, INotificationReceiver
{
    public event Action<QTEDataMarker> OnQTEMarkerReceived;
    
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is QTEDataMarker marker)
        {
            Debug.Log($" QTEMarker 수신: {marker.time} 초, Perfect: {marker.qtePerfectTime}, Good: {marker.qteGoodTime}, Bad: {marker.qteBadTime}");
            OnQTEMarkerReceived?.Invoke(marker);
        }
        
    }
}
}