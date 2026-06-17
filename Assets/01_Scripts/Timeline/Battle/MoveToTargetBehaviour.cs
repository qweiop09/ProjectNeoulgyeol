using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle
{
public class MoveToTargetBehaviour : PlayableBehaviour
{
    public Transform targetTransform;
    public AnimationCurve easeCurve;
    public float arrivalDistance;
    public Vector3 distanceAxisWeight = new Vector3(1f, 2f, 1f);
    public bool followTarget = true;
    public bool faceMoveDirection = true;
    
    // 거리(유니티 미터) 1당 걸리는 시간(초)에 곱해지는 배율. 기본값 1 = 1m당 1초
    public float durationMultiplier = 0.3f;
    
    private Vector3 startPosition;
    private Vector3 cachedTargetPosition;
    private Vector3 cachedDestination;
    private float totalMoveDuration;
    private double lastFrameTime;
    private float elapsedMoveTime;
    private bool isInitialized;
    
    // 거리에 따른 이동시간 조절하는 거랑 뒤로 갈 때 방향 조절하는 거 하고있었음
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform actor = playerData as Transform;
        if (actor == null || targetTransform == null) return;

        if (!isInitialized)
        {
            startPosition = actor.position;
            cachedTargetPosition = targetTransform.position;
            cachedDestination = GetArrivalPosition(startPosition, cachedTargetPosition);

            // 거리 기반 총 이동 시간 계산 (1m = 1초 * durationMultiplier)
            float moveDistance = Vector3.Distance(startPosition, cachedDestination);
            totalMoveDuration = moveDistance * durationMultiplier;

            elapsedMoveTime = 0f;
            lastFrameTime = playable.GetTime();
            isInitialized = true;
        }

        // 실제 경과된 시간만큼 누적 (followTarget이어도 시간 흐름 자체는 Timeline 기준)
        double currentTime = playable.GetTime();
        float deltaTime = (float)(currentTime - lastFrameTime);
        if (deltaTime > 0f)
            elapsedMoveTime += deltaTime;
        lastFrameTime = currentTime;

        Vector3 targetPosition = followTarget ? targetTransform.position : cachedTargetPosition;
        Vector3 destination = followTarget ? GetArrivalPosition(startPosition, targetPosition) : cachedDestination;

        float progress = totalMoveDuration > 0f 
            ? Mathf.Clamp01(elapsedMoveTime / totalMoveDuration) 
            : 1f;

        float evaluatedProgress = easeCurve != null ? easeCurve.Evaluate(progress) : progress;
        evaluatedProgress = Mathf.Clamp01(evaluatedProgress);

        Vector3 nextPosition = Vector3.Lerp(startPosition, destination, evaluatedProgress);
        ApplyMoveDirectionFeedback(actor, nextPosition);

        actor.position = nextPosition;
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        isInitialized = false;
    }

    private Vector3 GetArrivalPosition(Vector3 fromPosition, Vector3 targetPosition)
    {
        float safeArrivalDistance = Mathf.Max(0f, arrivalDistance);
        if (safeArrivalDistance <= 0f)
        {
            return targetPosition;
        }

        Vector3 weights = GetSafeDistanceAxisWeight();
        Vector3 scaledStart = Vector3.Scale(fromPosition, weights);
        Vector3 scaledTarget = Vector3.Scale(targetPosition, weights);
        Vector3 scaledDirection = scaledTarget - scaledStart;
        float weightedDistance = scaledDirection.magnitude;

        if (weightedDistance <= Mathf.Epsilon || weightedDistance <= safeArrivalDistance)
        {
            return fromPosition;
        }

        Vector3 scaledDestination = scaledTarget - scaledDirection.normalized * safeArrivalDistance;
        return new Vector3(
            scaledDestination.x / weights.x,
            scaledDestination.y / weights.y,
            scaledDestination.z / weights.z);
    }

    private Vector3 GetSafeDistanceAxisWeight()
    {
        return new Vector3(
            Mathf.Max(Mathf.Abs(distanceAxisWeight.x), Mathf.Epsilon),
            Mathf.Max(Mathf.Abs(distanceAxisWeight.y), Mathf.Epsilon),
            Mathf.Max(Mathf.Abs(distanceAxisWeight.z), Mathf.Epsilon));
    }

    private void ApplyMoveDirectionFeedback(Transform actor, Vector3 nextPosition)
    {
        if (!faceMoveDirection)
        {
            return;
        }
        
        Vector3 moveDirection = nextPosition - actor.position;
        if (moveDirection.sqrMagnitude <= 0.000001f)
        {
            return;
        }
        
        actor.rotation = Quaternion.Euler(0f, nextPosition.x - actor.position.x >= 0? 0 : 180  , 0f);
    }
}
}