using UnityEngine;

public class TargetTimeLineMovementManager : Singleton<TargetTimeLineMovementManager>
{
        public void PlayMoveToTargetClip( Transform movePoint, Transform targetPoint)
        {
            MoveToTargetTimelineDirector.Instance.PlayMoveToTargetClip(director, timelineAsset, movePoint, targetPoint);
        }
}
