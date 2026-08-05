using System.Linq;
using _01_Scripts.Runtime.Battles;
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
    }
}
}