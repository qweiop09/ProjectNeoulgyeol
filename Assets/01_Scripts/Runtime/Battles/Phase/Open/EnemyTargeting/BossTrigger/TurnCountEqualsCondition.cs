using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Boss Trigger Conditions/Turn Count Equals")]
    public class TurnCountEqualsCondition : BossTriggerConditionBase
    {
        public int turnCount = 1;
        public override bool IsMet(EnemyDecisionContext ctx) => ctx.CurrentRound == turnCount;
    }
}
