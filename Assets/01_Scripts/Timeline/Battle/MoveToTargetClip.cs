using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle
{
public class MoveToTargetClip : PlayableAsset
{
    // [핵심 1] 매니저와 통일할 고정 ID를 아예 static으로 선언해둬!
    public static readonly PropertyName TargetId = new PropertyName("targetEnemy"); 

    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

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
}