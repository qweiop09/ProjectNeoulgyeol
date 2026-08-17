using System.Linq;
using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Enemy AI Rules/Heal Threshold")]
    public class HealThresholdRule : EnemyDecisionRuleBase
    {
        public override bool TryDecide(EnemyDecisionContext ctx, out EnemyDecisionCandidate decision)
        {
            var pool = allowRiskySelf ? ctx.SafeCandidates.Concat(ctx.RiskyCandidates) : ctx.SafeCandidates;

            EnemyDecisionCandidate? best = null;
            float bestHpPercent = float.MaxValue;

            foreach (EnemyDecisionCandidate candidate in pool)
            {
                if (candidate.IsStay) continue;
                if (candidate.Skill.skillRole != SkillRole.Heal) continue;

                CharacterStatus targetStatus = candidate.MainTarget.GetCharacterStatus();
                float hpPercent = targetStatus.currentHp / (float)Mathf.Max(1, targetStatus.GetMaxHp());
                if (hpPercent >= candidate.Skill.healThresholdPercent) continue;

                if (hpPercent < bestHpPercent)
                {
                    bestHpPercent = hpPercent;
                    best = candidate;
                }
            }

            if (best.HasValue)
            {
                decision = best.Value;
                return true;
            }

            decision = default;
            return false;
        }
    }
}
