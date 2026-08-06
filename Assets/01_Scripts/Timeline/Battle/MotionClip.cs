using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle
{
public enum MotionRole
{
    Caster,
    Target,
}

public enum MotionSpace
{
    Self,         // 배우 자신의 좌우 방향 기준 (좌우 반전 시 자동 반영)
    TowardTarget, // +X = 항상 상대방 쪽, +Y = 항상 월드 위쪽
}

// 넉백/돌진처럼 정해진 타입을 코드로 만드는 대신, 디자이너가 경로 점을 찍어서
// 임의의 움직임(직선이 아닌 아치형/지그재그 등)을 직접 그릴 수 있게 하는 범용 이동 클립.
// "경로 모양"(pathPoints)과 "진행 타이밍"(progressCurve)을 분리해서 재사용성을 높인다.
public class MotionClip : PlayableAsset
{
    // 캐스터 참조용 고정 ID. 메인 타겟은 MoveToTargetClip.TargetId를 그대로 재사용한다.
    public static readonly PropertyName CasterId = new PropertyName("casterActor");

    // 추가 타겟(다중 타겟 아이템의 AdditionalTargets)용 인덱스 참조 ID.
    // ExposedReference/Resolver는 "이름표 하나 = 오브젝트 하나" 구조라 가변 개수 리스트를 한 번에 못 담으므로,
    // 인덱스별로 이름표를 등록해두고 0번부터 순서대로 못 찾을 때까지 훑어서 리스트를 구성한다.
    public static PropertyName GetAdditionalTargetId(int index) => new PropertyName($"additionalTarget_{index}");

    [Tooltip("이 클립이 누구를 움직일지")]
    public MotionRole role;

    [Tooltip("경로 좌표를 어느 축 기준으로 해석할지")]
    public MotionSpace space;

    [Tooltip("role이 Target일 때만 의미 있음 — 켜면 메인 타겟뿐 아니라 추가 타겟(다중 타겟 아이템) 전원에게 동시에 같은 움직임이 적용된다.")]
    public bool applyToAllTargets;

    [Tooltip("이동 경로를 이루는 점들(클립 시작 위치 기준 상대좌표). 3개 이상이면 Catmull-Rom 스플라인으로 부드럽게 이어진다.")]
    public List<Vector2> pathPoints = new();

    [Tooltip("시간에 따라 경로를 얼마나 진행했는지(0~1). 경로 '모양'과 '타이밍'을 분리해서, 같은 모양을 다른 속도감으로 재사용할 수 있다.")]
    public AnimationCurve progressCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<MotionBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        var resolver = graph.GetResolver();
        if (resolver != null)
        {
            Transform casterTransform = resolver.GetReferenceValue(CasterId, out _) as Transform;
            Transform mainTargetTransform = resolver.GetReferenceValue(MoveToTargetClip.TargetId, out _) as Transform;

            if (role == MotionRole.Caster)
            {
                behaviour.actorTransforms = new List<Transform> { casterTransform };
                behaviour.otherTransform = mainTargetTransform;
            }
            else
            {
                List<Transform> targets = new List<Transform>();
                if (mainTargetTransform != null)
                    targets.Add(mainTargetTransform);

                if (applyToAllTargets)
                    AppendAdditionalTargets(resolver, targets);

                behaviour.actorTransforms = targets;
                behaviour.otherTransform = casterTransform;
            }
        }

        behaviour.space = space;
        behaviour.pathPoints = pathPoints;
        behaviour.progressCurve = progressCurve;

        return playable;
    }

    private static void AppendAdditionalTargets(IExposedPropertyTable resolver, List<Transform> targets)
    {
        int i = 0;
        while (true)
        {
            Transform additional = resolver.GetReferenceValue(GetAdditionalTargetId(i), out bool valid) as Transform;
            if (!valid || additional == null) break;

            targets.Add(additional);
            i++;
        }
    }
}
}
