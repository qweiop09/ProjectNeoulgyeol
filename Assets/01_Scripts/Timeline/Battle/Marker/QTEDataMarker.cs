
using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle.Marker
{
public class QTEDataMarker : UnityEngine.Timeline.Marker, INotification
{
    public PropertyName id => new PropertyName("QTEDataMarker");

    public float qtePerfectTime = 0.05f; // 퍼펙트 타이밍
    public float qteGoodTime = 0.1f; // 굿 타이밍
    public float qteBadTime = 0.2f; // 배드 타이밍

    public float hpDamageCoefficient; // 스킬 데미지 계수
    public float staminaDamageCoefficient; // 스킬 스태미너 데미지 계수
    
    // public 추가적인 부가효과 (예: 상태이상, 버프 등) 를 위한 변수
    
}
}