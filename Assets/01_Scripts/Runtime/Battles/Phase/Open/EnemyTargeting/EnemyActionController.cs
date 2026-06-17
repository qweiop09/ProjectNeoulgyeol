using System.Collections.Generic;
using System.Linq;
using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
public class EnemyActionController : MonoBehaviour
{
    public CharacterHandler[] SetTargeting(CharacterHandler[] casters, CharacterHandler[] targets)
    {
        for (int i = 0; i < casters.Length; i++)
        {
            CharacterHandler caster = casters[i];
            
            if(caster.GetCharacterBattleData().currentState == CharacterState.Dead
               || caster.GetCharacterBattleData().currentState == CharacterState.Staggered)
                continue; // 행동 불가 상태면 스킵
            
            int remainingStamina = caster.GetCharacterBattleData().currentStamina; // 참조만, 차감 안 함
            CharacterSkill[] availableAttacks = caster.characterBattleData.CharacterData.characterAttacks;

            for (int slotIndex = 0; slotIndex < caster.characterBattleData.CharacterData.slotCount; slotIndex++)
            {
                CharacterSkill selectedSkill = SelectSkillByStamina(availableAttacks, remainingStamina);
                if (selectedSkill == null) break;

                CharacterHandler[] aliveTargets = targets
                    .Where(t => t.GetCharacterBattleData().currentState != CharacterState.Dead)
                    .ToArray();
                if (aliveTargets.Length == 0) break;

                CharacterHandler selectedTarget = aliveTargets[Random.Range(0, aliveTargets.Length)];

                caster.GetCharacterBattleData().TargetingData[slotIndex] = new SkillActData(
                    caster,
                    slotIndex,
                    selectedSkill,
                    selectedTarget,
                    0
                );

                // 실제 차감 아님 — 슬롯별 스킬 선택 시 누적 비용 계산용
                remainingStamina -= selectedSkill.skillStaminaCost;
            }
        }

        return casters;
    }

    private CharacterSkill SelectSkillByStamina(CharacterSkill[] skills, int remainingStamina)
    {
        List<CharacterSkill> affordableSkills = new List<CharacterSkill>();

        foreach (CharacterSkill skill in skills)
        {
            if (skill.skillStaminaCost <= remainingStamina)
                affordableSkills.Add(skill);
        }

        if (affordableSkills.Count == 0) return null;

        return affordableSkills[Random.Range(0, affordableSkills.Count)];
    }
}
}