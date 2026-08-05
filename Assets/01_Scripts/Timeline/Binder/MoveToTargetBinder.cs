// MoveToTarget 전용 바인더
using System.Linq;
using _01_Scripts.Timeline.Battle;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline
{

[CreateAssetMenu(menuName = "ProjectNeoulgyeol/Binder/MoveToTarget", fileName = "MoveToTargetBinder")]
public class MoveToTargetBinder : ITimelineBinder
{
    public override void Bind(PlayableDirector director, ActData data)
    {
        if (director == null || director.playableAsset == null || data == null ||
            data.CastPlayerCharacter == null || data.TargetPlayerCharacter == null ||
            data.CastPlayerCharacter.transform == null || data.TargetPlayerCharacter.transform == null)
        {
            Debug.LogError("MoveToTargetBinder: 바인딩에 필요한 값이 비어있습니다.");
            return;
        }

        // 스킬이 아니면(예: 아이템) 이동 시작 거리 개념이 없으므로 0으로 처리
        float arrivalDistance = (data as SkillActData)?.UseSkill?.skillStartDistance ?? 0f;

        var timeline = director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogError("MoveToTargetBinder: PlayableAsset이 TimelineAsset이 아닙니다.");
            return;
        }

        TrackAsset targetTrack = timeline.GetOutputTracks()
            .FirstOrDefault(t => t is MoveToTargetTrack || t.name == "MoveToTargetTrack");

        if (targetTrack == null)
        {
            Debug.LogError("MoveToTargetBinder: MoveToTargetTrack을 찾을 수 없습니다.");
            return;
        }

        foreach (TimelineClip clip in targetTrack.GetClips())
        {
            if (clip.asset is MoveToTargetClip moveToTargetClip)
                moveToTargetClip.arrivalDistance = arrivalDistance;
        }

        director.SetGenericBinding(targetTrack, data.CastPlayerCharacter.transform);
        director.SetReferenceValue(MoveToTargetClip.TargetId, data.TargetPlayerCharacter.transform);
    }
}
}
