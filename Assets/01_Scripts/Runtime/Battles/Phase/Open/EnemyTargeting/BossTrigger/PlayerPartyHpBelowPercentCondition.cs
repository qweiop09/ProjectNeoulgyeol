using System.Linq;
using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Boss Trigger Conditions/Player Party HP Below Percent")]
    public class PlayerPartyHpBelowPercentCondition : BossTriggerConditionBase
    {
        [Range(0f, 1f)] public float thresholdPercent = 0.5f;
        [Tooltip("true면 파티 평균 HP%로 판단, false면 파티 전체 HP 합 / 전체 최대HP 합으로 판단")]
        public bool useAverage = true;

        public override bool IsMet(EnemyDecisionContext ctx)
        {
            CharacterHandler[] playerParty = ctx.AllBattleCharacters
                .Where(c => c != null && c.characterType != ctx.Caster.characterType
                            && c.GetCharacterStatus().currentState != CharacterState.Dead)
                .ToArray();

            if (playerParty.Length == 0) return false;

            if (useAverage)
            {
                float sum = 0f;
                foreach (CharacterHandler c in playerParty)
                {
                    var s = c.GetCharacterStatus();
                    sum += s.currentHp / (float)Mathf.Max(1, s.GetMaxHp());
                }
                return (sum / playerParty.Length) < thresholdPercent;
            }
            else
            {
                int currentSum = 0, maxSum = 0;
                foreach (CharacterHandler c in playerParty)
                {
                    var s = c.GetCharacterStatus();
                    currentSum += s.currentHp;
                    maxSum += s.GetMaxHp();
                }
                return maxSum > 0 && (currentSum / (float)maxSum) < thresholdPercent;
            }
        }
    }
}
