using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles.CameraControlle;
using _01_Scripts.Timeline;

namespace _01_Scripts.Runtime.Battles.Compete
{using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Timeline;

namespace _01_Scripts.Runtime.Battles.Compete
{
public class CompeteContestController : MonoBehaviour
{
    [SerializeField] private TimelineAsset moveToTargetTimelineAsset;
    [SerializeField] private MoveToTargetBinder moveToTargetBinder;
    
    [SerializeField] private int MoveToTargetWaitTime = 500; // milliseconds

    // 한 캐릭터의 모든 행동을 실행
    public async Task StartCompeteCycle(ActData[] actDatas)
    {
        Debug.Log("Compete Cycle Started with " + actDatas.Length + " actions.");
        
        for (int i = 0; i < actDatas.Length; i++)
        {
            if (actDatas[i] == null) continue;
            
            CameraHandler.Instance.SetFollowTransform(actDatas[i].CastPlayerCharacter.transform, 1.5f);
            
            await PlayMoveToTarget(actDatas[i]);
            Debug.Log("PlayCompete Start");
            await PlayCompete(actDatas[i]);
            
            CameraHandler.Instance.UnsetFollowTransform();
        }
        
        Debug.Log("Compete Cycle Completed.");
    }

    private Task PlayMoveToTarget(ActData actData)
    {
        Debug.Log("actData is null? " + (actData == null));
        
        return actData.CastPlayerCharacter.timelineDirector
            .PlayAsync(actData.CastPlayerCharacter , moveToTargetTimelineAsset, moveToTargetBinder, actData);
    }

    private async Task PlayCompete(ActData actData)
    {
        Debug.Log("PlayCompete: " + actData.CastPlayerCharacter.characterBattleData.TargetingData[actData.UseSlot].UseSkill.skillName);
        
        CharacterHandler caster = actData.CastPlayerCharacter;
        CharacterSkill skill = caster.characterBattleData.TargetingData[actData.UseSlot].UseSkill; 
        
        await Wait(MoveToTargetWaitTime / 1000f); // MoveToTarget이 끝난 후 잠시 대기

        await caster.timelineDirector
            .PlayAsync(caster, skill.skillTimelineAsset, skill.timelineBinder, actData);
    }
        
    private Task Wait(float seconds)
    {
        return Task.Delay((int)(seconds * 1000));
    }
}
}
}
