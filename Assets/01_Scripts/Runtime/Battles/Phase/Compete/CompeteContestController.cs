using System.Collections.Generic;
using System.Threading.Tasks;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using _01_Scripts.Runtime.Battles.CameraControlle;
using _01_Scripts.Runtime.Worlds.Inventory;
using _01_Scripts.Runtime.Battles.Characters;
using UnityEngine;
using UnityEngine.Timeline;

namespace _01_Scripts.Runtime.Battles.Compete
{
public class CompeteContestController : MonoBehaviour
{
    [SerializeField] private MoveToTargetExecutor moveToTargetExecutor;

    private ActData currentActData; // 현재 실행 중인 ActData를 저장하는 변수 (abstract — 직렬화 불가)

    [SerializeField] private int MoveToTargetWaitTime = 500; // milliseconds
    [SerializeField] private int stayStaminaRecovery = 20; // Stay 시 턴 종료 후 회복되는 스테미나 양

    [Header("아이템 공용 QTE (아이템에 전용 타임라인/바인더가 없을 때 사용)")]
    [SerializeField] private TimelineAsset defaultItemTimelineAsset;
    [SerializeField] private ITimelineBinder defaultItemTimelineBinder;

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
    private void ApplyQteResult(QTEHitInfo hitInfo)
    {
        Debug.Log("QTE Result: " + hitInfo.Result);

        if (currentActData is ItemActData itemActData)
        {
            ApplyItemQteResult(itemActData, hitInfo);
            return;
        }

        var skillActData = currentActData as SkillActData;

        var hitContext = new HitContext
        {
            Attacker = currentActData.CastPlayerCharacter.GetCharacterStatus(),
            Target = currentActData.TargetPlayerCharacter.GetCharacterStatus(),
            Result = hitInfo.Result,
            HpDamageCoefficient = hitInfo.HpDamageCoefficient,
            StaminaDamageCoefficient = hitInfo.StaminaDamageCoefficient,
            Multipliers = hitInfo.Multipliers,
            HitEffects = skillActData?.UseSkill.hitEffects ?? System.Array.Empty<IHitEffect>()
        };

        DamageResult damage = DamageCalculation.Calculate(hitContext);

        DamageTextSpawner.Instance.SpawnDamageText(
            currentActData.TargetPlayerCharacter.transform.position,
            damage.HpDamage,
            hitInfo.Result);

        CharacterStatusCalculator.Instance.SkillHit(
            currentActData.TargetPlayerCharacter.GetCharacterStatus(),
            damage.HpDamage,
            damage.StaminaDamage);
    }

    // 아이템은 데미지 파이프라인 대신 QTE 판정 등급(Perfect/Good/Hit) 배율을 아이템 효과 크기에 그대로 곱해 적용한다.
    // 메인 타겟뿐 아니라 다중 타겟(AdditionalTargets)에도 같은 배율로 전부 적용한다.
    private void ApplyItemQteResult(ItemActData itemActData, QTEHitInfo hitInfo)
    {
        float multiplier = DamageCalculation.GetQteMultiplier(hitInfo.Result, hitInfo.Multipliers);
        CharacterStatus caster = itemActData.CastPlayerCharacter.GetCharacterStatus();

        ApplyItemToTarget(itemActData.UseItem, caster, itemActData.TargetPlayerCharacter, multiplier);

        if (itemActData.AdditionalTargets != null)
        {
            foreach (CharacterHandler additionalTarget in itemActData.AdditionalTargets)
            {
                if (additionalTarget == null) continue;

                // 메인 타겟은 StartCompeteCycle에서 이미 생존 여부를 걸러주지만, 추가 타겟은 그 체크를 안 받으므로
                // 여기서 직접 확인한다 — 안 그러면 죽은 캐릭터가 회복돼 currentHp>0인데 currentState는 Dead로 남는
                // 모순 상태가 생긴다(ApplyHpModify는 0을 찍을 때만 Dead로 바꾸고, 반대로 풀어주진 않음).
                CharacterStatus additionalStatus = additionalTarget.GetCharacterStatus();
                if (additionalStatus.currentState == CharacterState.Dead) continue;

                ApplyItemToTarget(itemActData.UseItem, caster, additionalTarget, multiplier);
            }
        }

        Debug.Log($"[CompeteContestController] '{itemActData.UseItem.itemName}' 효과 적용 (QTE {hitInfo.Result}, 배율 {multiplier})");
    }

    // 아이템 효과를 대상 한 명에게 적용하고, 수치로 표시할 결과(회복/피해량)가 있으면 그 자리에 텍스트로 띄운다.
    // 이펙트(파티클/사운드) 자체는 이미 아이템 타임라인(QTE)이 재생하므로 여기선 텍스트만 담당한다.
    // 색은 QTE 판정이 아니라 부호로 정해진다(회복=초록/피해=빨강) — SpawnItemEffectText 참고.
    private void ApplyItemToTarget(Item item, CharacterStatus caster, CharacterHandler targetHandler, float multiplier)
    {
        List<int> numericResults = item.Use(caster, targetHandler.GetCharacterStatus(), multiplier);

        foreach (int amount in numericResults)
            DamageTextSpawner.Instance.SpawnItemEffectText(targetHandler.transform.position, amount);
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
                || actData.CastPlayerCharacter.GetCharacterStatus().currentState == CharacterState.Staggered)
            {
                ReleaseReservationIfItem(actData); // 실행되지 못하고 버려지는 행동이므로 예약해둔 아이템이 있다면 풀어준다
                continue;
            }

            // Stay는 카메라/행동 모두 스킵 — 턴 종료 후 스테미나 회복은 루프 외부에서 처리
            if (actData is StayActData) continue;

            // 도망은 판정이 이미 경합 페이즈 시작 시 끝났다(실패했으니 여기 도달함) — 아무 효과 없이 그냥 턴 소모
            if (actData is RunActData) continue;

            if (actData.TargetPlayerCharacter.GetCharacterStatus().currentState == CharacterState.Dead)
            {
                ReleaseReservationIfItem(actData);
                continue;
            }

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
                // 아이템은 이동 없이 그 자리에서 사용 — 스킬의 MP/스태미나 차감처럼 소모부터 처리하고 타임라인 재생
                if (itemActData.UseItem.category == ItemCategory.Consumable)
                    InventoryManager.Instance.ConfirmReservedUse(itemActData.UseItem, 1); // 선택 시점의 예약을 해제하며 실제 소모

                await PlayItemCompete(itemActData);
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
        float arrivalDistance = skillAct?.UseSkill?.skillStartDistance ?? 0f;

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
        
    // 아이템은 항상 QTE를 진행한다 — 전용 타임라인(item.itemTimelineAsset)이 있으면 그걸, 없으면 공용 타임라인을 사용
    private async Task PlayItemCompete(ItemActData actData)
    {
        Item item = actData.UseItem;
        Debug.Log("PlayItemCompete: " + item?.itemName);

        CharacterHandler caster = actData.CastPlayerCharacter;

        TimelineAsset timeline = item.itemTimelineAsset != null ? item.itemTimelineAsset : defaultItemTimelineAsset;
        ITimelineBinder binder = item.timelineBinder != null ? item.timelineBinder : defaultItemTimelineBinder;

        if (timeline == null)
        {
            Debug.LogWarning($"[CompeteContestController] '{item.itemName}' 전용 타임라인도, 공용 아이템 타임라인도 설정되어 있지 않습니다. 재생을 건너뜁니다.");
            return;
        }

        await caster.timelineDirector.PlayAsync(caster, timeline, binder, actData);
    }

    private Task Wait(float seconds)
    {
        return Task.Delay((int)(seconds * 1000));
    }

    private void ReleaseReservationIfItem(ActData actData)
    {
        if (actData is ItemActData itemActData)
            InventoryManager.Instance.ReleaseReservation(itemActData.UseItem, 1);
    }
}
}