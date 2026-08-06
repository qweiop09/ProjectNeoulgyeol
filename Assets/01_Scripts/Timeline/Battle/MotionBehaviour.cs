using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle
{
public class MotionBehaviour : PlayableBehaviour
{
    public List<Transform> actorTransforms; // 이 클립이 움직이는 대상들 (메인 타겟 하나, 또는 메인+추가 타겟 전원)
    public Transform otherTransform;        // TowardTarget 축 계산용 상대방 (Self 모드에서는 안 씀)
    public MotionSpace space;
    public List<Vector2> pathPoints;
    public AnimationCurve progressCurve;

    private readonly Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();
    private bool isInitialized;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (actorTransforms == null || actorTransforms.Count == 0) return;

        if (!isInitialized)
        {
            foreach (Transform actor in actorTransforms)
                if (actor != null)
                    startPositions[actor] = actor.position;
            isInitialized = true;
        }

        float normalizedTime = GetNormalizedTime(playable);
        float progress = progressCurve != null
            ? Mathf.Clamp01(progressCurve.Evaluate(normalizedTime))
            : normalizedTime;

        // 모든 액터가 같은 진행도/경로 모양을 공유하되, 각자의 시작 위치·기준축으로 계산한다
        // (프레임레이트와 무관하게 항상 같은 모양이 나오도록 매 프레임 절대 위치로 직접 계산).
        Vector2 localOffset = EvaluateCatmullRom(pathPoints, progress);

        foreach (Transform actor in actorTransforms)
        {
            if (actor == null || !startPositions.TryGetValue(actor, out Vector3 start)) continue;

            Vector3 worldOffset = ToWorldOffset(actor, start, localOffset);
            actor.position = start + worldOffset;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        isInitialized = false;
        startPositions.Clear();
    }

    private static float GetNormalizedTime(Playable playable)
    {
        double duration = playable.GetDuration();
        if (duration <= 0) return 1f;
        return Mathf.Clamp01((float)(playable.GetTime() / duration));
    }

    private Vector3 ToWorldOffset(Transform actor, Vector3 actorStart, Vector2 localOffset)
    {
        if (space == MotionSpace.Self)
            return actor.right * localOffset.x + actor.up * localOffset.y;

        // TowardTarget: +X는 항상 상대방 쪽, +Y는 항상 월드 위쪽. 액터마다 자기 시작 위치 기준으로 개별 계산해서
        // 대형이 흐트러져 있어도(여러 명이 다중 타겟으로 동시에 움직여도) 각자 방향이 자연스럽게 맞는다.
        Vector3 axisX = otherTransform != null
            ? (otherTransform.position - actorStart)
            : actor.right;

        if (axisX.sqrMagnitude < 0.0001f)
            axisX = actor.right;

        return axisX.normalized * localOffset.x + Vector3.up * localOffset.y;
    }

    // points가 시작 위치 기준 상대좌표라고 가정 — t(0~1)에 해당하는 지점을 부드럽게 보간해 반환한다.
    private static Vector2 EvaluateCatmullRom(List<Vector2> points, float t)
    {
        if (points == null || points.Count == 0) return Vector2.zero;
        if (points.Count == 1) return points[0];

        t = Mathf.Clamp01(t);
        int segmentCount = points.Count - 1;
        float scaledT = t * segmentCount;
        int segment = Mathf.Clamp(Mathf.FloorToInt(scaledT), 0, segmentCount - 1);
        float localT = scaledT - segment;

        Vector2 p0 = points[Mathf.Max(segment - 1, 0)];
        Vector2 p1 = points[segment];
        Vector2 p2 = points[Mathf.Min(segment + 1, points.Count - 1)];
        Vector2 p3 = points[Mathf.Min(segment + 2, points.Count - 1)];

        return CatmullRom(p0, p1, p2, p3, localT);
    }

    private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
}
}
