using System;
using _01_Scripts.DTO;
using _01_Scripts.Interfacese;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles
{
// 순수 계산부: 부작용 없이 값만 산출한다. MonoBehaviour에 의존하지 않으므로
// 월드/전투 어느 쪽에서도 동일하게 재사용할 수 있다.
public static class StatusCalculation
{
    public struct Result
    {
        public int NewValue;
        public bool HitFloor; // 0으로 클램프됨
        public bool HitCap;   // 최대치로 클램프됨
    }

    public static Result Modify(int current, int amount, int max)
    {
        int newValue = Mathf.Clamp(current + amount, 0, max);
        return new Result
        {
            NewValue = newValue,
            HitFloor = newValue == 0,
            HitCap = newValue == max
        };
    }
}

// 적용부: 계산 결과를 CharacterStatus에 반영하고 Dead/Stagger 이벤트를 발행한다.
// Dead/Stagger 구독을 MonoBehaviour 이벤트로 제공하기 위해 Singleton으로 유지하지만,
// 대상 타입이 CharacterHandler가 아니라 CharacterStatus이므로 월드 씬에서도 호출 가능하다.
public class CharacterStatusCalculator : Singleton<CharacterStatusCalculator>
{

    public Action<CharacterStatus> isCharacterDead;
    public Action<CharacterStatus> isCharacterStagger;


    public void UseSkill(CharacterStatus caster, CharacterSkill skill)
    {
        ApplyMpModify(caster, -skill.skillMpCost);
        ApplyStaminaModify(caster, -skill.skillStaminaCost);
    }

    public void SkillHit(CharacterStatus hitTarget, int hpDecisivePower, int staminaDecisivePower)
    {
        ApplyHpModify(hitTarget, -hpDecisivePower);
        ApplyStaminaModify(hitTarget, -staminaDecisivePower);
    }

    // HP 변화
    public void ApplyHpModify(CharacterStatus target, int amount)
    {
        var result = StatusCalculation.Modify(target.currentHp, amount, target.GetMaxHp());
        target.currentHp = result.NewValue;

        if (result.HitFloor)
        {
            target.SetCurrentState(CharacterState.Dead);
            isCharacterDead?.Invoke(target);
        }

        Debug.Log(target.CharacterData.characterName + " 회복: " + amount + " / 현재 HP: " + target.currentHp);
    }

    // MP 변화
    public void ApplyMpModify(CharacterStatus target, int amount)
    {
        var result = StatusCalculation.Modify(target.currentMp, amount, target.GetMaxMp());
        target.currentMp = result.NewValue;

        Debug.Log(target.CharacterData.characterName + " MP 회복: " + amount + " / 현재 MP: " + target.currentMp);
    }

    // 스테미나 변화
    public void ApplyStaminaModify(CharacterStatus target, int amount)
    {
        var result = StatusCalculation.Modify(target.currentStamina, amount, target.GetMaxStamina());
        target.currentStamina = result.NewValue;

        if (result.HitFloor)
        {
            target.SetCurrentState(CharacterState.Staggered);
            isCharacterStagger?.Invoke(target);
        }

        Debug.Log(target.CharacterData.characterName + " 스테미나 회복: " + amount + " / 현재 스테미나: " + target.currentStamina);
    }
}
}