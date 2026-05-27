using UnityEngine;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline.Battle.TrackAssets
{
[TrackColor(0.8f, 0.8f, 0.9f)] // 트랙 색상 지정
// [TrackBindingType(typeof(Transform))] // 이 트랙에는 Transform을 바인딩함
// [TrackClipType(typeof(MoveToTargetClip))] // 이 트랙에는 MoveToTargetClip만 올림
public class ActFeedbackTrack : TrackAsset { }
}
