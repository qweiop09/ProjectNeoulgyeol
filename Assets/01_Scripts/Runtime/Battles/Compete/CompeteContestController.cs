using System.Threading.Tasks;
using _01_Scripts.DTO;
using _01_Scripts.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

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

    public async Task StartCompeteCycle(ActData[] actDatas)
    {
        Debug.Log("Compete Cycle Started with " + actDatas.Length + " actions.");
        Debug.Log(actDatas);

        for (int i = 0; i < actDatas.Length; i++)
        {
            if (actDatas[i] == null) continue;
            
            await PlayMoveToTarget(actDatas[i]);
            Debug.Log("PlayCompete Start");
            // await PlayCompete(actDatas[i]);
        }
    }

    private Task PlayMoveToTarget(ActData actData)
    {
        Debug.Log("actData is null? " + (actData == null));
        
        return actData.CastPlayerCharacter.timelineDirector
            .PlayAsync(moveToTargetTimelineAsset, moveToTargetBinder, actData);
    }

    private Task PlayCompete(ActData actData)
    {
        CharacterHandler caster = actData.CastPlayerCharacter;
        CharacterSkill skill = caster.characterBattleData.TargetingData[actData.UseSlot].UseSkill;
        
        return caster.timelineDirector
            .PlayAsync(skill.skillTimelineAsset, skill.timelineBinder, actData);
    }
}
}
}
