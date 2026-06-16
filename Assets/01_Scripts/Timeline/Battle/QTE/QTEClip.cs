using System;
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

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<QTEBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.perfectTime = perfectTime;
        behaviour.goodTime = goodTime;
        behaviour.badTime = badTime;
        return playable;
    }
}
}