using System;
using _01_Scripts.Runtime.Battles;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline.Battle.QTE
{
[Serializable]
public class QTEClip : PlayableAsset, ITimelineClipAsset
{
    [Header("판정 시간 (중심 기준 반경, 초)")]
    public float perfectTime = 0.05f;
    public float goodTime = 0.15f;
    public float badTime = 0.3f; // 클립 길이 = badTime * 2

    [Header("데미지 계수")]
    public float hpDamageCoefficient = 1f;
    public float staminaDamageCoefficient = 1f;

    // 아군이 캐스터면 이 값 그대로, 적군이 캐스터면 1.0 기준 대칭 반전(2.0 - 값)해서 적용
    [Header("QTE 판정별 데미지 배율 (아군 캐스터 기준값)")]
    public QteMultiplierSet multipliers = new QteMultiplierSet(1.5f, 1.15f, 1.0f);

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<QTEBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.perfectTime = perfectTime;
        behaviour.goodTime = goodTime;
        behaviour.badTime = badTime;
        behaviour.hpDamageCoefficient = hpDamageCoefficient;
        behaviour.staminaDamageCoefficient = staminaDamageCoefficient;
        behaviour.multipliers = multipliers;
        return playable;
    }
}
}