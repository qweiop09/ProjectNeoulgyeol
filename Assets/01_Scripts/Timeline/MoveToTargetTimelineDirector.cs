using System.Linq;
using _01_Scripts.Interfacese;
using _01_Scripts.Timeline.Battle;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline
{
public class MoveToTargetTimelineDirector : Singleton<MoveToTargetTimelineDirector>
{
    public void PlayMoveToTargetClip(PlayableDirector director,TimelineAsset timelineAsset ,Transform movePoint, Transform targetPoint)
    {
        TrackAsset moveToTargetTrack = FindObjectOfType<MoveToTargetTrack>();
        
        // 여기에 MoveToTargetClip을 재생하는 로직을 구현
        MoveToTargetTrack targetTrack = timelineAsset.GetOutputTracks().FirstOrDefault(t => t.name == "MoveToTargetTrack");
        director.SetGenericBinding(targetTrack, gameObject);
        
        targetPoint.GetComponent<MoveToTargetTrack>().targetEnemy = movePoint; // 타겟 포인트에 이동할 위치 설정
        
        director.Play(timelineAsset);
    }
}
}
