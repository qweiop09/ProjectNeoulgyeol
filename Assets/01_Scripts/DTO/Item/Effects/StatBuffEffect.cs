using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.DTO.Item.Effects
{
    public enum BuffStatType { Attack, Defense, MaxHp, MaxMp, MaxStamina }

    // 지속 효과 — 공격력/방어력/최대치를 N라운드 동안 증감시킨다 (버프/디버프 공용, amount 부호로 구분).
    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Item/Effect/Stat Buff", fileName = "New Stat Buff Effect")]
    public class StatBuffEffect : BuffEffectBase
    {
        [Header("효과")]
        public BuffStatType statType;
        public EffectValue amount;

        [Tooltip("statType이 MaxHp/MaxMp/MaxStamina일 때만 의미 있음 — 켜면 최대치가 늘어난 만큼 현재치도 비율을 유지하며 같이 늘어난다. 끄면 현재치는 그대로 둔다.")]
        public bool scaleCurrentWithIncrease;

        protected override int ResolveAmount(CharacterStatus caster, CharacterStatus target) => amount.Resolve(caster, target);

        public override void Apply(CharacterStatus caster, CharacterStatus target, float qteMultiplier)
        {
            int previousMax = GetRelevantMax(target);

            base.Apply(caster, target, qteMultiplier); // 등록/갱신

            if (previousMax >= 0)
                ApplyMaxChange(target, previousMax);
        }

        // Attack/Defense는 "현재치" 개념이 없어서 -1(해당 없음)
        private int GetRelevantMax(CharacterStatus target)
        {
            return statType switch
            {
                BuffStatType.MaxHp => target.GetMaxHp(),
                BuffStatType.MaxMp => target.GetMaxMp(),
                BuffStatType.MaxStamina => target.GetMaxStamina(),
                _ => -1
            };
        }

        private void ApplyMaxChange(CharacterStatus target, int previousMax)
        {
            switch (statType)
            {
                case BuffStatType.MaxHp:
                    target.OnMaxHpChanged(previousMax, scaleCurrentWithIncrease);
                    break;
                case BuffStatType.MaxMp:
                    target.OnMaxMpChanged(previousMax, scaleCurrentWithIncrease);
                    break;
                case BuffStatType.MaxStamina:
                    target.OnMaxStaminaChanged(previousMax, scaleCurrentWithIncrease);
                    break;
            }
        }
    }
}
