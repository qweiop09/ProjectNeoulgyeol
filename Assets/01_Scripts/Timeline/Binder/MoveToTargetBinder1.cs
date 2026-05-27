// 바인더

using System.Linq;
using _01_Scripts.Timeline.Battle;
using _01_Scripts.Timeline.Battle.Receiver;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline.Binder
{
[CreateAssetMenu(fileName = "MoveToTargetBinder", menuName = "Timeline/Binder/MoveToTarget")]
public class MoveToTargetBinder : ITimelineBinder
{
    public override void Bind(PlayableDirector director, ActData data)
    {
        if (director == null || director.playableAsset == null ||
            data.CastPlayerCharacter.transform == null || data.TargetPlayerCharacter.transform == null)
        {
            Debug.LogError("MoveToTargetBinder: 바인딩에 필요한 값이 비어있습니다.");
            return;
        }

        var timeline = director.playableAsset as TimelineAsset;
        TrackAsset targetTrack = timeline.GetOutputTracks()
            .FirstOrDefault(t => t is MoveToTargetTrack || t.name == "MoveToTargetTrack");

        if (targetTrack == null)
        {
            Debug.LogError("MoveToTargetBinder: MoveToTargetTrack을 찾을 수 없습니다.");
            return;
        }

        director.SetGenericBinding(targetTrack, data.CastPlayerCharacter.transform);
        director.SetReferenceValue(MoveToTargetClip.TargetId, data.TargetPlayerCharacter.transform);
        
        // 마커 수신자 바인딩
        director.SetGenericBinding(
            timeline.markerTrack,
            data.CastPlayerCharacter.GetComponent<BattleMarkerReceiver>()
        );
    }
}
}