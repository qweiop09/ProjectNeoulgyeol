using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle.Marker
{
public class BattleMarker : UnityEngine.Timeline.Marker, INotification
{
    public PropertyName id => new PropertyName("BattleMarker");
    
    public string message;
}
}