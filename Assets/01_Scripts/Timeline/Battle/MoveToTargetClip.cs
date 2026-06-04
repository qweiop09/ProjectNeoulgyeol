using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle
{
public class MoveToTargetClip : PlayableAsset
{
    // [핵심 1] 매니저와 통일할 고정 ID를 아예 static으로 선언해둬!
    public static readonly PropertyName TargetId = new PropertyName("targetEnemy");

    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float arrivalDistance;
    public Vector3 distanceAxisWeight = new Vector3(1f, 2f, 1f);
    public bool followTarget = true;
    public bool faceMoveDirection = true;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<MoveToTargetBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        // [핵심 2] ExposedReference 대신, Resolver를 통해 고정된 ID의 값을 바로 꺼내옴
        var resolver = graph.GetResolver();
        if (resolver != null)
        {
            // "targetEnemy"라는 이름표가 붙은 Transform을 찾아와!
            behaviour.targetTransform = resolver.GetReferenceValue(TargetId, out bool isValid) as Transform;
        }

        behaviour.easeCurve = easeCurve;
        behaviour.arrivalDistance = arrivalDistance;
        behaviour.distanceAxisWeight = distanceAxisWeight;
        behaviour.followTarget = followTarget;
        behaviour.faceMoveDirection = faceMoveDirection;

        return playable;
    }
}
}
