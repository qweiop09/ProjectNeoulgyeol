using _01_Scripts.Runtime.Battles;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle.QTE
{
public class QTEBehaviour : PlayableBehaviour
{
    public float perfectTime;
    public float goodTime;
    public float badTime;
    public float hpDamageCoefficient;
    public float staminaDamageCoefficient;
    public QteMultiplierSet multipliers;
}
}