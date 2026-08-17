using System.Linq;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Enemy AI Rules/Kill Confirm")]
    public class KillConfirmRule : EnemyDecisionRuleBase
    {
        public override bool TryDecide(EnemyDecisionContext ctx, out EnemyDecisionCandidate decision)
        {
            var pool = allowRiskySelf ? ctx.SafeCandidates.Concat(ctx.RiskyCandidates) : ctx.SafeCandidates;

            EnemyDecisionCandidate? best = null;
            int bestOverkill = int.MaxValue;

            foreach (EnemyDecisionCandidate candidate in pool)
            {
                if (candidate.IsStay) continue;
                if (!DamagePrediction.WouldKill(ctx.Caster, candidate.MainTarget, candidate.Skill)) continue;

                int delta = DamagePrediction.PredictHpDelta(ctx.Caster, candidate.MainTarget, candidate.Skill);
                int overkill = (-delta) - candidate.MainTarget.GetCharacterStatus().currentHp;
                if (overkill < bestOverkill)
                {
                    bestOverkill = overkill;
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
