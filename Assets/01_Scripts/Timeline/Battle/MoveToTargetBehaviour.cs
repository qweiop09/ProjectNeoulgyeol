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

    private Vector3 startPosition;
    private Vector3 cachedTargetPosition;
    private bool isInitialized;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform actor = playerData as Transform;
        if (actor == null || targetTransform == null) return;

        if (!isInitialized)
        {
            startPosition = actor.position;
            cachedTargetPosition = targetTransform.position;
            isInitialized = true;
        }

        Vector3 targetPosition = followTarget ? targetTransform.position : cachedTargetPosition;
        Vector3 destination = GetArrivalPosition(startPosition, targetPosition);

        double duration = playable.GetDuration();
        float progress = duration > 0 ? (float)(playable.GetTime() / duration) : 1f;
        progress = Mathf.Clamp01(progress);

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

        // actor.rotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up);
    }
}
}
