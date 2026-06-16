using System;
using _01_Scripts.DTO;
using _01_Scripts.Interfacese;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles
{
public class CharacterStatusCalculator : Singleton<CharacterStatusCalculator>
{

    public Action<CharacterHandler> isCharacterDead;
    public Action<CharacterHandler> isCharacterStagger;
    
    
    public void UseSkill(CharacterHandler caster, CharacterSkill skill)
    {
        ApplyMpModify(caster, -skill.skillMpCost);
        ApplyStaminaModify(caster, -skill.skillStaminaCost);
    }

    public void SkillHit(CharacterHandler hitTarget, int hpDecisivePower, int staminaDecisivePower)
    {
        ApplyHpModify(hitTarget, -hpDecisivePower);
        ApplyStaminaModify(hitTarget, -staminaDecisivePower);
    }

    // HP 변화
    public void ApplyHpModify(CharacterHandler target, int amount)
    {
        target.GetCharacterBattleData().currentHp += amount;
        target.GetCharacterBattleData().currentHp 
            = Mathf.Clamp(target.GetCharacterBattleData().currentHp,
                0, target.GetCharacterBattleData().CharacterData.maxHp);
        
        if(IsDead(target))
            isCharacterDead?.Invoke(target);

        Debug.Log(target.name + " 회복: " + amount + " / 현재 HP: " + target.GetCharacterBattleData().currentHp);
    }

    // MP 변화
    public void ApplyMpModify(CharacterHandler target, int amount)
    {
        target.GetCharacterBattleData().currentMp += amount;
        target.GetCharacterBattleData().currentMp 
            = Mathf.Clamp(target.GetCharacterBattleData().currentMp,
                0, target.GetCharacterBattleData().CharacterData.maxMp);
            
        Debug.Log(target.name + " MP 회복: " + amount + " / 현재 MP: " + target.GetCharacterBattleData().currentMp);
    }

    // 스테미나 변화
    public void ApplyStaminaModify(CharacterHandler target, int amount)
    {

        target.GetCharacterBattleData().currentStamina += amount;
        target.GetCharacterBattleData().currentStamina 
            = Mathf.Clamp(target.GetCharacterBattleData().currentStamina,
                0, target.GetCharacterBattleData().CharacterData.maxStamina);

        if(IsStaggered(target))
            isCharacterStagger?.Invoke(target);
        
        Debug.Log(target.name + " 스테미나 회복: " + amount + " / 현재 스테미나: " + target.GetCharacterBattleData().currentStamina);
    }

    // 사망 여부 체크
    private bool IsDead(CharacterHandler target)
    {
        return target.GetCharacterBattleData().currentHp == 0;
    }
    
    private bool IsStaggered(CharacterHandler target)
    {
        return target.GetCharacterBattleData().currentStamina == 0;
    }
}
}