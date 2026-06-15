using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline.Binder
{
[CreateAssetMenu(menuName = "ProjectNeoulgyeol/Binder/Skill", fileName = "SkillTimelineBinder")]
public class SkillTimelineBinder : ITimelineBinder
{
    [Header("바인딩할 Track 이름")]
    [SerializeField] private string animationTrackName = "AnimationTrack";
    [SerializeField] private string particleTrackName = "ParticleTrack";

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

        // // 파티클 트랙 바인딩 (스킬에 파티클이 없으면 트랙 자체가 없을 수 있음)
        // TrackAsset particleTrack = timeline.GetOutputTracks()
        //     .FirstOrDefault(t => t.name == particleTrackName);
        //
        // if (particleTrack != null)
        // {
        //     director.SetGenericBinding(particleTrack, data.CastPlayerCharacter.skillEffectRoot);
        // }
    }
}
}