using System.Threading.Tasks;
using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles.CameraControlle;
using _01_Scripts.Runtime.Battles.Characters;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Compete
{
public class CompeteContestController : MonoBehaviour
{
    [SerializeField] private MoveToTargetExecutor moveToTargetExecutor;

    [SerializeField] private ActData currentActData; // 현재 실행 중인 ActData를 저장하는 변수

    [SerializeField] private int MoveToTargetWaitTime = 500; // milliseconds

    public void OnEnable()
    {
        QTECoordinator.Instance.OnQTEMarkerReceived += ApplyQteResult;

        CharacterStatusCalculator.Instance.isCharacterDead +=
            CH => { CharacterAnimationMonitor.Instance.PlayAnimation(CH, CharacterAnimationMonitor.CharacterAnimationState.Dead); };
        
        // CharacterStatusCalculator.Instance.isCharacterStagger +=
        //     CH => { CharacterAnimationMonitor.Instance.PlayAnimation(CH, CharacterAnimationMonitor.CharacterAnimationState.Idle); };
    }
    
    public void OnDisable()
    {
        QTECoordinator.Instance.OnQTEMarkerReceived -= ApplyQteResult;
        
        CharacterStatusCalculator.Instance.isCharacterDead -=
            CH => { CharacterAnimationMonitor.Instance.PlayAnimation(CH, CharacterAnimationMonitor.CharacterAnimationState.Dead); };
        
        // CharacterStatusCalculator.Instance.isCharacterStagger -=
        //     CH => { CharacterAnimationMonitor.Instance.PlayAnimation(CH, CharacterAnimationMonitor.CharacterAnimationState.Idle); };

    }
    
    // qte 피드백
    private void ApplyQteResult(QTEResult result)
    {
        Debug.LogError("QTE Result: " + result);
        
        float damageMultiplier = (result == QTEResult.Perfect) ? 1.5f : (result == QTEResult.Good) ? 1.15f : 1f;
        
        DamageTextSpawner.Instance.SpawnDamageText(currentActData.TargetPlayerCharacter.transform.position,
            (int)(currentActData.CastPlayerCharacter.characterBattleData.CharacterData.attack * damageMultiplier)
            , result);
        
        // 데미지 반영
        CharacterStatusCalculator.Instance.ApplyHpModify(currentActData.TargetPlayerCharacter,
            -(int)(currentActData.CastPlayerCharacter.characterBattleData.CharacterData.attack * damageMultiplier));                     
    }

    // 한 캐릭터의 모든 행동을 실행
    public async Task StartCompeteCycle(ActData[] actDatas)
    {
        Debug.Log("Compete Cycle Started with " + actDatas.Length + " actions.");
        
        for (int i = 0; i < actDatas.Length; i++)
        {
            if (actDatas[i] == null) continue;
            if (actDatas[i].CastPlayerCharacter.characterBattleData.currentState == CharacterState.Dead
                || actDatas[i].CastPlayerCharacter.characterBattleData.currentState == CharacterState.Staggered) continue;
            if (actDatas[i].TargetPlayerCharacter.characterBattleData.currentState == CharacterState.Dead) continue;
            
            CameraHandler.Instance.SetFollowTransform(actDatas[i].CastPlayerCharacter.transform, 1.5f);
            
            currentActData = actDatas[i]; // 현재 실행 중인 ActData 업데이트
            
            CharacterAnimationMonitor.Instance.PlayAnimation(actDatas[i].CastPlayerCharacter, CharacterAnimationMonitor.CharacterAnimationState.Run);
            
            await PlayMoveToTarget(actDatas[i]);
            Debug.Log("PlayCompete Start");
            
            CharacterAnimationMonitor.Instance.PlayAnimation(actDatas[i].CastPlayerCharacter, CharacterAnimationMonitor.CharacterAnimationState.Idle);
            
            CharacterStatusCalculator.Instance.UseSkill(actDatas[i].CastPlayerCharacter,actDatas[i].UseSkill);
            await PlayCompete(actDatas[i]);
            
            CameraHandler.Instance.UnsetFollowTransform();
        }
        
        Debug.Log("Compete Cycle Completed.");
    }
    
    private Task PlayMoveToTarget(ActData actData)
    {
        Debug.Log("actData is null? " + (actData == null));

        return moveToTargetExecutor.ExecuteAsync(
            actData.CastPlayerCharacter.transform,
            actData.TargetPlayerCharacter.transform,
            actData.UseSkill.skillStartDistance);
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