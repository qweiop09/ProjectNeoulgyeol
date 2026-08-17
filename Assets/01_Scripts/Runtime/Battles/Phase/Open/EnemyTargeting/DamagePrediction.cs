using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    // DamageCalculation을 부작용 없이 호출해서 AI가 실행 전에 결과를 미리 계산하게 해주는 얇은 래퍼.
    // 실제 QTE 판정 대신 QTEResult.Hit(1.0배)을 가정하고, CharacterSkill의 AI 예측용 계수를 사용한다.
    // 실전 계산(DamageCalculation.Calculate/ResolveFinalDamage)을 그대로 재사용하므로 예측값과 실제
    // 적용값이 구조적으로 어긋나지 않는다(흐트러짐 배율도 ResolveFinalDamage가 자동 적용).
    public static class DamagePrediction
    {
        // 부호 있는 HP 변화량 예측 (음수 = 피해, 양수 = 회복)
        public static int PredictHpDelta(CharacterHandler caster, CharacterHandler target, CharacterSkill skill)
        {
            HitContext ctx = new HitContext
            {
                Attacker = caster.GetCharacterStatus(),
                Target = target.GetCharacterStatus(),
                Result = QTEResult.Hit,
                HpDamageCoefficient = skill.predictedHpDamageCoefficient,
                StaminaDamageCoefficient = skill.predictedStaminaDamageCoefficient,
                Multipliers = new QteMultiplierSet(1f, 1f, 1f),
                HitEffects = System.Array.Empty<IHitEffect>()
            };

            DamageResult raw = DamageCalculation.Calculate(ctx);
            DamageResult final = DamageCalculation.ResolveFinalDamage(raw, target.GetCharacterStatus());
            return -final.HpDamage; // HpDamage는 "깎일 양" 기준 — 실제 델타는 부호 반전(음수 계수면 회복이라 양수가 됨)
        }

        public static bool WouldKill(CharacterHandler caster, CharacterHandler target, CharacterSkill skill)
        {
            int delta = PredictHpDelta(caster, target, skill);
            return delta < 0 && -delta >= target.GetCharacterStatus().currentHp;
        }

        // 이번 슬롯에서 이 스킬을 쓰면 캐스터 자신이 흐트러짐에 들어가는가 —
        // CharacterStatusCalculator.ApplyStaminaModify의 진입 조건을 그대로 재현한 read-only 예측(상태를 바꾸지 않음).
        public static bool WouldTriggerSelfStagger(CharacterHandler caster, int remainingStaminaThisTurn, CharacterSkill skill)
        {
            CharacterStatus status = caster.GetCharacterStatus();
            if (status.currentState != CharacterState.Normal) return false; // Normal이 아니면 애초에 재진입 대상이 아님

            int newStamina = Mathf.Clamp(remainingStaminaThisTurn - skill.skillStaminaCost, 0, status.GetMaxStamina());
            return newStamina == 0;
        }
    }
}
