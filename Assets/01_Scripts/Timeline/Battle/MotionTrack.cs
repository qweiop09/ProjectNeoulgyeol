using UnityEngine;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline.Battle
{
[TrackColor(0.9f, 0.5f, 0.2f)]
[TrackClipType(typeof(MotionClip))]
// TrackBindingType 불필요 — 움직일 대상(캐스터/타겟)은 트랙 바인딩이 아니라 클립 안에서
// Resolver로 직접 찾아오므로, 캐스터를 미는 클립과 타겟을 미는 클립을 트랙 하나에 자유롭게 섞어 배치할 수 있다.
public class MotionTrack : TrackAsset { }
}
