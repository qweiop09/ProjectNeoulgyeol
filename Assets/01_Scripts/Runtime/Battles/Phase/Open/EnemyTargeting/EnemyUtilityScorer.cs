using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    // 오버라이드 룰이 아무것도 안 걸렸을 때, 안전 후보(Stay 포함) 중에서 최댓값을 고르는 폴백 스코어러.
    public static class EnemyUtilityScorer
    {
        public static EnemyDecisionCandidate? Choose(EnemyDecisionContext ctx)
        {
            EnemyDecisionCandidate? best = null;
            float bestScore = float.NegativeInfinity;

            foreach (EnemyDecisionCandidate candidate in ctx.SafeCandidates)
            {
                float score = Score(ctx, candidate);
                if (float.IsNaN(score)) continue; // 하드캡 등으로 제외된 후보

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private static float Score(EnemyDecisionContext ctx, EnemyDecisionCandidate candidate)
        {
            EnemyAIProfile profile = ctx.Profile;

            if (candidate.IsStay)
                return profile.stayWeight;

            CharacterSkill skill = candidate.Skill;
            CharacterHandler target = candidate.MainTarget;
            CharacterStatus targetStatus = target.GetCharacterStatus();
            bool isAttack = skill.skillRole == SkillRole.Attack;
            int hitCount = isAttack ? ctx.GetHitCount(target) : 0;

            if (isAttack && profile.maxHitsPerTargetPerRound > 0 && hitCount >= profile.maxHitsPerTargetPerRound)
                return float.NaN; // 하드캡 초과 — 후보 자체 제외

            float targetFactor = ScoreTargetFactor(profile, ctx.Caster, target, skill);
            float score = skill.aiWeight * targetFactor;

            if (isAttack)
                score /= (1f + profile.focusFirePenaltyPerHit * hitCount);

            return score;
        }

        // damageWeight/lowHpWeight만 반영한 순수 "이 타겟이 얼마나 매력적인가" 팩터 — aiWeight/집중공격 페널티는 뺀 값.
        // 보스 강제 스킬처럼 스킬이 이미 정해져 있고 타겟만 고르면 되는 경우에도 재사용한다.
        public static float ScoreTargetFactor(EnemyAIProfile profile, CharacterHandler caster, CharacterHandler target, CharacterSkill skill)
        {
            CharacterStatus targetStatus = target.GetCharacterStatus();
            int predictedDelta = DamagePrediction.PredictHpDelta(caster, target, skill);
            float normalizedDamage = Mathf.Clamp01(Mathf.Abs(predictedDelta) / (float)Mathf.Max(1, targetStatus.currentHp));
            float hpPercent = targetStatus.currentHp / (float)Mathf.Max(1, targetStatus.GetMaxHp());

            return profile.damageWeight * normalizedDamage + profile.lowHpWeight * (1f - hpPercent);
        }
    }
}
