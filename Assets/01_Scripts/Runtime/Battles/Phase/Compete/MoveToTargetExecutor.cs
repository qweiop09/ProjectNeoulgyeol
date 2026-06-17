using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Compete
{
public class MoveToTargetExecutor : MonoBehaviour
{
    [Header("이동 설정")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Vector3 distanceAxisWeight = new Vector3(1f, 2f, 1f);
    public bool followTarget = true;
    public bool faceMoveDirection = true;

    /// <summary>거리(m) 1당 걸리는 시간(초)에 곱하는 배율. 기본값 0.3 = 1m당 0.3초</summary>
    public float durationMultiplier = 0.3f;

    /// <summary>
    /// 액터를 타겟 방향으로 arrivalDistance 앞까지 이동시킵니다.
    /// 이동이 완료되면 Task가 완료됩니다.
    /// </summary>
    public Task ExecuteAsync(Transform actor, Transform target, float arrivalDistance)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(MoveCoroutine(actor, target, arrivalDistance, tcs));
        return tcs.Task;
    }

    private IEnumerator MoveCoroutine(
        Transform actor, Transform target, float arrivalDistance,
        TaskCompletionSource<bool> tcs)
    {
        Vector3 startPosition = actor.position;
        Vector3 cachedTargetPosition = target.position;
        Vector3 cachedDestination = GetArrivalPosition(startPosition, cachedTargetPosition, arrivalDistance);

        float moveDistance = Vector3.Distance(startPosition, cachedDestination);
        float totalMoveDuration = moveDistance * durationMultiplier;
        float elapsed = 0f;

        while (elapsed < totalMoveDuration)
        {
            elapsed += Time.deltaTime;

            Vector3 targetPos = followTarget ? target.position : cachedTargetPosition;
            Vector3 destination = followTarget
                ? GetArrivalPosition(startPosition, targetPos, arrivalDistance)
                : cachedDestination;

            float progress = totalMoveDuration > 0f
                ? Mathf.Clamp01(elapsed / totalMoveDuration)
                : 1f;

            float evaluatedProgress = easeCurve != null
                ? Mathf.Clamp01(easeCurve.Evaluate(progress))
                : progress;

            Vector3 nextPosition = Vector3.Lerp(startPosition, destination, evaluatedProgress);
            ApplyFaceDirection(actor, nextPosition);
            actor.position = nextPosition;

            yield return null;
        }

        // 최종 위치 정확히 맞추기
        Vector3 finalTarget = followTarget ? target.position : cachedTargetPosition;
        actor.position = GetArrivalPosition(startPosition, finalTarget, arrivalDistance);

        tcs.SetResult(true);
    }

    private Vector3 GetArrivalPosition(Vector3 fromPosition, Vector3 targetPosition, float arrivalDist)
    {
        float safeArrivalDistance = Mathf.Max(0f, arrivalDist);
        if (safeArrivalDistance <= 0f) return targetPosition;

        Vector3 weights = GetSafeAxisWeight();
        Vector3 scaledStart = Vector3.Scale(fromPosition, weights);
        Vector3 scaledTarget = Vector3.Scale(targetPosition, weights);
        Vector3 scaledDirection = scaledTarget - scaledStart;
        float weightedDistance = scaledDirection.magnitude;

        if (weightedDistance <= Mathf.Epsilon || weightedDistance <= safeArrivalDistance)
            return fromPosition;

        Vector3 scaledDestination = scaledTarget - scaledDirection.normalized * safeArrivalDistance;
        return new Vector3(
            scaledDestination.x / weights.x,
            scaledDestination.y / weights.y,
            scaledDestination.z / weights.z);
    }

    private Vector3 GetSafeAxisWeight()
    {
        return new Vector3(
            Mathf.Max(Mathf.Abs(distanceAxisWeight.x), Mathf.Epsilon),
            Mathf.Max(Mathf.Abs(distanceAxisWeight.y), Mathf.Epsilon),
            Mathf.Max(Mathf.Abs(distanceAxisWeight.z), Mathf.Epsilon));
    }

    private void ApplyFaceDirection(Transform actor, Vector3 nextPosition)
    {
        if (!faceMoveDirection) return;

        Vector3 delta = nextPosition - actor.position;
        if (delta.sqrMagnitude <= 0.000001f) return;

        actor.rotation = Quaternion.Euler(0f, delta.x >= 0f ? 0f : 180f, 0f);
    }
}
}
