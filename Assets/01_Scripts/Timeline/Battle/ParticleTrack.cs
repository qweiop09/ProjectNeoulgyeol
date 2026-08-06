using UnityEngine;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline.Battle
{
[TrackColor(0.3f, 0.8f, 0.5f)]
[TrackClipType(typeof(ParticleClip))]
// TrackBindingType 불필요 — MotionTrack과 마찬가지로 anchor(캐스터/타겟)를 클립 안에서 Resolver로 직접 찾아오므로
// 캐스터 위치에서 터지는 이펙트와 타겟 위치에서 터지는 이펙트를 트랙 하나에 자유롭게 섞어 배치할 수 있다.
public class ParticleTrack : TrackAsset { }
}
