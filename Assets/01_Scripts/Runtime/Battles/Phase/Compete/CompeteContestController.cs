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

    private ActData currentActData; // 현재 실행 중인 ActData를 저장하는 변수 (abstract — 직렬화 불가)

    [SerializeField] private int MoveToTargetWaitTime = 500; // milliseconds
    [SerializeField] private int stayStaminaRecovery = 20; // Stay 시 턴 종료 후 회복되는 스테미나 양

    private void OnEnable()
    {
        QTECoordinator.Instance.OnQTEMarkerReceived += ApplyQteResult;
        CharacterStatusCalculator.Instance.isCharacterDead += OnCharacterDead;
    }

    private void OnDisable()
    {
        QTECoordinator.Instance.OnQTEMarkerReceived -= ApplyQteResult;
        CharacterStatusCalculator.Instance.isCharacterDead -= OnCharacterDead;
    }

    private void OnCharacterDead(CharacterStatus status)
    {
        // 새 레지스트리 없이, HP 피격이 이 컨트롤러 안에서만 발생하므로
        // 지금 실행 중인 행동의 타겟 핸들러를 그대로 사용한다.
        if (currentActData.TargetPlayerCharacter.GetCharacterStatus() != status) return;

        CharacterAnimationMonitor.Instance.PlayAnimation(
            currentActData.TargetPlayerCharacter, CharacterAnimationMonitor.CharacterAnimationState.Dead);
    }


    // qte 피드백
    private void ApplyQteResult(QTEResult result)
    {
        Debug.Log("QTE Result: " + result);

        bool isCasterFriendly = currentActData.CastPlayerCharacter.characterType
                                == CharacterHandler.CharacterType.Friendly;

        float damageMultiplier;
        if (isCasterFriendly)
        {
            // 아군 공격 QTE: 성공할수록 데미지 증가
            damageMultiplier = result switch
            {
                QTEResult.Perfect => 1.5f,
                QTEResult.Good    => 1.15f,
                _                 => 1.0f
            };
        }
        else
        {
            // 적군 공격 QTE: 성공할수록 데미지 경감
            damageMultiplier = result switch
            {
                QTEResult.Perfect => 0.5f,
                QTEResult.Good    => 0.75f,
                _                 => 1.0f
            };
        }

        int finalDamage = (int)(currentActData.CastPlayerCharacter.GetCharacterStatus().GetAttack()
                                * damageMultiplier);

        DamageTextSpawner.Instance.SpawnDamageText(
            currentActData.TargetPlayerCharacter.transform.position,
            finalDamage,
            result);

        CharacterStatusCalculator.Instance.ApplyHpModify(
            currentActData.TargetPlayerCharacter.GetCharacterStatus(),
            -finalDamage);
    }

    // 한 캐릭터의 모든 행동을 실행
    public async Task StartCompeteCycle(ActData[] actDatas)
    {
        Debug.Log("Compete Cycle Started with " + actDatas.Length + " actions.");

        for (int i = 0; i < actDatas.Length; i++)
        {
            ActData actData = actDatas[i];
            if (actData == null) continue;
            if (actData.CastPlayerCharacter.GetCharacterStatus().currentState == CharacterState.Dead
                || actData.CastPlayerCharacter.GetCharacterStatus().currentState == CharacterState.Staggered) continue;

            // Stay는 카메라/행동 모두 스킵 — 턴 종료 후 스테미나 회복은 루프 외부에서 처리
            if (actData is StayActData) continue;

            if (actData.TargetPlayerCharacter.GetCharacterStatus().currentState == CharacterState.Dead) continue;

            CameraHandler.Instance.SetFollowTransform(actData.CastPlayerCharacter.transform, 1.5f);
            currentActData = actData;

            if (actData is SkillActData skillActData)
            {
                CharacterAnimationMonitor.Instance.PlayAnimation(actData.CastPlayerCharacter, CharacterAnimationMonitor.CharacterAnimationState.Run);
                await PlayMoveToTarget(actData);
                Debug.Log("PlayCompete Start");
                CharacterAnimationMonitor.Instance.PlayAnimation(actData.CastPlayerCharacter, CharacterAnimationMonitor.CharacterAnimationState.Idle);
                CharacterStatusCalculator.Instance.UseSkill(actData.CastPlayerCharacter.GetCharacterStatus(), skillActData.UseSkill);
                await PlayCompete(skillActData);
            }
            else if (actData is ItemActData itemActData)
            {
                // itemActData.UseItem.Use(actData.TargetPlayerCharacter.GetCharacterStatus());

                if (itemActData.UseItem.isConsumable)
                    actData.CastPlayerCharacter.GetCharacterStatus().inventory
                        .Remove(itemActData.UseItem);
            }

            CameraHandler.Instance.UnsetFollowTransform();
        }

        // Stay 행동 처리: 모든 행동 종료 후 스테미나 회복
        foreach (ActData actData in actDatas)
        {
            if (actData is StayActData &&
                actData.CastPlayerCharacter.GetCharacterStatus().currentState != CharacterState.Dead)
            {
                CharacterStatusCalculator.Instance.ApplyStaminaModify(
                    actData.CastPlayerCharacter.GetCharacterStatus(), stayStaminaRecovery);
            }
        }

        Debug.Log("Compete Cycle Completed.");
    }
    
    private Task PlayMoveToTarget(ActData actData)
    {
        Debug.Log("actData is null? " + (actData == null));

        var skillAct = actData as SkillActData;
        float arrivalDistance = skillAct?.UseSkill.skillStartDistance ?? 0f;

        return moveToTargetExecutor.ExecuteAsync(
            actData.CastPlayerCharacter.transform,
            actData.TargetPlayerCharacter.transform,
            arrivalDistance);
    }

    private async Task PlayCompete(SkillActData actData)
    {
        Debug.Log("PlayCompete: " + actData.UseSkill?.skillName);

        CharacterHandler caster = actData.CastPlayerCharacter;
        CharacterSkill skill = actData.UseSkill;

        await Wait(MoveToTargetWaitTime / 1000f);

        if (skill == null) return;
        await caster.timelineDirector
            .PlayAsync(caster, skill.skillTimelineAsset, skill.timelineBinder, actData);
    }
        
    private Task Wait(float seconds)
    {
        return Task.Delay((int)(seconds * 1000));
    }
}
}