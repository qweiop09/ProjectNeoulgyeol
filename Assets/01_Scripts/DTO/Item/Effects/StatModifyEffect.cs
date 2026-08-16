using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles;
using UnityEngine;

namespace _01_Scripts.DTO.Item.Effects
{
    public enum InstantStatType { Hp, Mp, Stamina }

    // 즉시 효과 — Hp/Mp/Stamina 중 하나를 그 자리에서 바로 증감시킨다 (회복/감소 공용).
    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Item/Effect/Stat Modify", fileName = "New Stat Modify Effect")]
    public class StatModifyEffect : ItemEffectBase
    {
        [Tooltip("어떤 수치를 바꿀지")]
        public InstantStatType statType;

        [Tooltip("양수 = 회복, 음수 = 감소")]
        public EffectValue amount;

        public override int? Apply(CharacterStatus caster, CharacterStatus target, float qteMultiplier)
        {
            int value = Mathf.RoundToInt(amount.Resolve(caster, target) * qteMultiplier);

            // 피해(음수)일 때만 DamageCalculation이 "결론적인" 값을 내도록 거친다(흐트러짐 배율 등) — 회복은 대상 아님.
            // 반환값을 그대로 반환해야 아이템 효과 텍스트도 실제 적용치와 일치한다.
            switch (statType)
            {
                case InstantStatType.Hp:
                    if (value < 0) value = -DamageCalculation.ResolveFinalDamage(-value, target);
                    CharacterStatusCalculator.Instance.ApplyHpModify(target, value);
                    break;
                case InstantStatType.Mp:
                    CharacterStatusCalculator.Instance.ApplyMpModify(target, value);
                    break;
                case InstantStatType.Stamina:
                    if (value < 0) value = -DamageCalculation.ResolveFinalDamage(-value, target);
                    CharacterStatusCalculator.Instance.ApplyStaminaModify(target, value);
                    break;
            }

            return value;
        }
    }
}
