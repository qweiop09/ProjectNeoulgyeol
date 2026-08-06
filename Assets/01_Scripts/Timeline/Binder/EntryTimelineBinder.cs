using System.Linq;
using _01_Scripts.Timeline.Battle;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline.Binder
{
// 전투 진입 연출 전용 — SkillTimelineBinder보다 가볍다. 타겟/QTE 개념이 없고(EntryActData는 항상 자기 자신이
// 대상), 애니메이터 + 자기 자신 기준 이동(MotionClip.CasterId) + 캐스터 오디오만 바인딩한다.
[CreateAssetMenu(menuName = "ProjectNeoulgyeol/Binder/Entry", fileName = "EntryTimelineBinder")]
public class EntryTimelineBinder : ITimelineBinder
{
    [Header("바인딩할 Track 이름")]
    [SerializeField] private string animationTrackName = "AnimationTrack";
    [SerializeField] private string casterAudioTrackName = "CasterAudioTrack";

    public override void Bind(PlayableDirector director, ActData data)
    {
        if (director == null || director.playableAsset == null || data == null
            || data.CastPlayerCharacter == null)
        {
            Debug.LogError("EntryTimelineBinder: 바인딩에 필요한 값이 비어있습니다.");
            return;
        }

        var timeline = director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogError("EntryTimelineBinder: PlayableAsset이 TimelineAsset이 아닙니다.");
            return;
        }

        TrackAsset animationTrack = timeline.GetOutputTracks()
            .FirstOrDefault(t => t is AnimationTrack && t.name == animationTrackName);

        if (animationTrack != null)
            director.SetGenericBinding(animationTrack, data.CastPlayerCharacter.animator);
        else
            Debug.LogWarning($"EntryTimelineBinder: '{animationTrackName}' 트랙을 찾을 수 없습니다.");

        // MotionClip이 Self 공간 이동에 쓸 수 있도록 자기 자신을 캐스터로 등록
        director.SetReferenceValue(MotionClip.CasterId, data.CastPlayerCharacter.transform);

        TrackAsset audioTrack = timeline.GetOutputTracks()
            .FirstOrDefault(t => t is AudioTrack && t.name == casterAudioTrackName);

        if (audioTrack == null) return; // 사운드 트랙은 선택사항이라 없어도 경고 없이 넘어감

        if (data.CastPlayerCharacter.sfxSource == null)
        {
            Debug.LogWarning($"EntryTimelineBinder: '{casterAudioTrackName}' 트랙은 있는데 캐릭터에 sfxSource가 없습니다.");
            return;
        }

        director.SetGenericBinding(audioTrack, data.CastPlayerCharacter.sfxSource);
    }
}
}
