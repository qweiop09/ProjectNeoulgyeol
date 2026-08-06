using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle
{
public enum EffectAnchor
{
    Caster,
    Target,
}

// 파티클 프리팹을 참조해서, 재생 시점에 캐스터/타겟 위치에 Instantiate하는 클립.
// 캐스터/타겟 참조는 MotionClip과 같은 고정 ID(CasterId/MoveToTargetClip.TargetId)를 재사용한다.
public class ParticleClip : PlayableAsset
{
    [Tooltip("재생할 파티클 프리팹")]
    public ParticleSystem particlePrefab;

    [Tooltip("어디서 재생할지")]
    public EffectAnchor anchor;

    [Tooltip("anchor 기준 로컬 오프셋 (예: 타겟 발밑, 캐스터 손끝)")]
    public Vector3 localOffset;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<ParticleBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        var resolver = graph.GetResolver();
        if (resolver != null)
        {
            Transform casterTransform = resolver.GetReferenceValue(MotionClip.CasterId, out _) as Transform;
            Transform targetTransform = resolver.GetReferenceValue(MoveToTargetClip.TargetId, out _) as Transform;

            behaviour.anchorTransform = anchor == EffectAnchor.Caster ? casterTransform : targetTransform;
        }

        behaviour.particlePrefab = particlePrefab;
        behaviour.localOffset = localOffset;

        return playable;
    }
}
}
