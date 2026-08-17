using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Boss Trigger Conditions/Self HP Below Percent")]
    public class SelfHpBelowPercentCondition : BossTriggerConditionBase
    {
        [Range(0f, 1f)] public float thresholdPercent = 0.5f;

        public override bool IsMet(EnemyDecisionContext ctx)
        {
            var status = ctx.Caster.GetCharacterStatus();
            float hpPercent = status.currentHp / (float)Mathf.Max(1, status.GetMaxHp());
            return hpPercent < thresholdPercent;
        }
    }
}
