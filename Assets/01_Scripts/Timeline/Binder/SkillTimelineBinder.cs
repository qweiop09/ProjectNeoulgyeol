using System.Linq;
using _01_Scripts.Runtime.Battles;
using _01_Scripts.Timeline.Battle;
using _01_Scripts.Timeline.Battle.QTE;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline.Binder
{
// 스킬 전용으로 만들어졌지만 CastPlayerCharacter(애니메이터/QTE리스너)만 참조하므로 아이템 타임라인에도 그대로 재사용된다.
[CreateAssetMenu(menuName = "ProjectNeoulgyeol/Binder/Skill", fileName = "SkillTimelineBinder")]
public class SkillTimelineBinder : ITimelineBinder
{
    [Header("바인딩할 Track 이름")]
    [SerializeField] private string animationTrackName = "AnimationTrack";
    [SerializeField] private string casterAudioTrackName = "CasterAudioTrack";
    [SerializeField] private string targetAudioTrackName = "TargetAudioTrack";

    public override void Bind(PlayableDirector director, ActData data)
    {
        Debug.Log("SkillTimelineBinder: 바인딩 시작");
    
        if (director == null || director.playableAsset == null || data == null
            || data.CastPlayerCharacter == null)
        {
            Debug.LogError("SkillTimelineBinder: 바인딩에 필요한 값이 비어있습니다.");
            return;
        }

        var timeline = director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogError("SkillTimelineBinder: PlayableAsset이 TimelineAsset이 아닙니다.");
            return;
        }

        // 애니메이션 트랙 바인딩
        TrackAsset animationTrack = timeline.GetOutputTracks()
            .FirstOrDefault(t => t is AnimationTrack && t.name == animationTrackName);
        
        if (animationTrack != null)
        {
            director.SetGenericBinding(animationTrack, data.CastPlayerCharacter.animator);
            Debug.Log("SkillTimelineBinder: 애니메이션 트랙 바인딩 완료");
        }
        else
        {
            Debug.LogWarning($"SkillTimelineBinder: '{animationTrackName}' 트랙을 찾을 수 없습니다.");
        }

        // QTE 트랙 바인딩
        TrackAsset qteTrack = timeline.GetOutputTracks()
            .FirstOrDefault(t => t is QTETrack);

        if (qteTrack != null)
        {
            director.SetGenericBinding(qteTrack, data.CastPlayerCharacter.qteListner);
            
            
            Debug.Log("SkillTimelineBinder: QTE 트랙 바인딩 완료");
        }
        else
        {
            Debug.LogWarning("SkillTimelineBinder: QTETrack을 찾을 수 없습니다.");
        }

        // MotionClip/ParticleClip이 "그때그때의" 캐스터/대상을 찾아 쓸 수 있도록 참조값을 세팅
        // (타겟은 MoveToTargetClip이 이미 쓰는 고정 ID를 그대로 재사용)
        director.SetReferenceValue(MotionClip.CasterId, data.CastPlayerCharacter.transform);
        if (data.TargetPlayerCharacter != null)
            director.SetReferenceValue(MoveToTargetClip.TargetId, data.TargetPlayerCharacter.transform);

        // 다중 타겟(AdditionalTargets)도 인덱스별 이름표로 등록 — MotionClip/ParticleClip이 applyToAllTargets일 때 이걸로 훑는다
        if (data.AdditionalTargets != null)
        {
            for (int i = 0; i < data.AdditionalTargets.Length; i++)
            {
                if (data.AdditionalTargets[i] != null)
                    director.SetReferenceValue(MotionClip.GetAdditionalTargetId(i), data.AdditionalTargets[i].transform);
            }
        }

        // 사운드 — 유니티 기본 AudioTrack을 이름으로 찾아서 각자의 스피커에 바인딩만 해준다 (클립은 디자이너가 AudioClip을 직접 올림)
        BindAudioTrack(director, timeline, casterAudioTrackName, data.CastPlayerCharacter.sfxSource);
        if (data.TargetPlayerCharacter != null)
            BindAudioTrack(director, timeline, targetAudioTrackName, data.TargetPlayerCharacter.sfxSource);
    }

    private void BindAudioTrack(PlayableDirector director, TimelineAsset timeline, string trackName, AudioSource sfxSource)
    {
        TrackAsset audioTrack = timeline.GetOutputTracks()
            .FirstOrDefault(t => t is AudioTrack && t.name == trackName);

        if (audioTrack == null) return; // 사운드 트랙은 선택사항이라 없어도 경고 없이 넘어감

        if (sfxSource == null)
        {
            Debug.LogWarning($"SkillTimelineBinder: '{trackName}' 트랙은 있는데 캐릭터에 sfxSource가 없습니다.");
            return;
        }

        director.SetGenericBinding(audioTrack, sfxSource);
        Debug.Log($"SkillTimelineBinder: '{trackName}' 바인딩 완료");
    }
}
}