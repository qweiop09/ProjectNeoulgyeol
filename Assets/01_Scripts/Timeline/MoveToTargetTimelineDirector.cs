using System.Linq;
using _01_Scripts.Timeline.Battle;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline
{
public class MoveToTargetTimelineDirector : MonoBehaviour
{
    public void PlayMoveToTargetClip(PlayableDirector director,TimelineAsset timelineAsset ,Transform movePoint, Transform targetPoint)
    {
        if (director == null || timelineAsset == null || movePoint == null || targetPoint == null)
        {
            Debug.LogError("MoveToTargetTimelineDirector: 타임라인 실행에 필요한 값이 비어있습니다.");
            return;
        }

        TrackAsset targetTrack = timelineAsset.GetOutputTracks()
            .FirstOrDefault(t => t is MoveToTargetTrack || t.name == "MoveToTargetTrack");

        if (targetTrack == null)
        {
            Debug.LogError("MoveToTargetTimelineDirector: MoveToTargetTrack을 찾을 수 없습니다.");
            return;
        }

        director.playableAsset = timelineAsset;
        director.SetGenericBinding(targetTrack, movePoint);
        director.SetReferenceValue(MoveToTargetClip.TargetId, targetPoint);
        director.Play(timelineAsset);
    }
}
}
