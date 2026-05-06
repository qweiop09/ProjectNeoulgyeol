using UnityEngine;
using UnityEngine.Playables;

public class MoveToTargetClip : PlayableAsset
{
    public ExposedReference<Transform> targetEnemy; // 씬의 적 위치
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 기본 EaseInOut

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<MoveToTargetBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        // 씬 오브젝트를 찾아서 로직에 전달
        behaviour.targetTransform = targetEnemy.Resolve(graph.GetResolver());
        behaviour.easeCurve = easeCurve;

        return playable;
    }
} 