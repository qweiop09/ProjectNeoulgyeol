using _01_Scripts.DTO;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    public static class BossTriggerEvaluator
    {
        // 조건을 전부 만족하는(그리고 oneShot이면 아직 발동 안 한) 첫 엔트리를 찾는다. 순서 = 우선순위.
        // 주의: 여기선 oneShot 발동 기록을 남기지 않는다 — "조건이 맞는 트리거가 있는지 확인"과 "실제로 그 트리거를
        // 이번 슬롯의 행동으로 확정"은 다른 시점일 수 있어서(예: 흐트러진 상태 게이트 체크), 실제로 그 결정을 쓰기로
        // 확정한 호출부가 MarkFired를 명시적으로 불러야 한다 — 안 그러면 실행되지도 못한 트리거의 1회성이 낭비될 수 있음.
        public static bool TryEvaluate(EnemyDecisionContext ctx, out BossTriggerEntry entry)
        {
            EnemyAIProfile profile = ctx.Profile;
            if (profile == null || profile.bossTriggers == null)
            {
                entry = null;
                return false;
            }

            var casterStatus = ctx.Caster.GetCharacterStatus();

            foreach (BossTriggerEntry candidate in profile.bossTriggers)
            {
                if (candidate == null || candidate.forcedSkill == null) continue;
                if (candidate.oneShot && casterStatus.FiredOneShotBossTriggers.Contains(candidate)) continue;

                bool allMet = true;
                foreach (BossTriggerConditionBase condition in candidate.conditions)
                {
                    if (condition == null || !condition.IsMet(ctx))
                    {
                        allMet = false;
                        break;
                    }
                }

                if (!allMet) continue;

                entry = candidate;
                return true;
            }

            entry = null;
            return false;
        }

        // 실제로 이 트리거의 결정을 이번 슬롯 행동으로 확정했을 때 호출 — oneShot 발동 기록을 여기서 남긴다.
        public static void MarkFired(CharacterStatus casterStatus, BossTriggerEntry entry)
        {
            if (entry.oneShot && !casterStatus.FiredOneShotBossTriggers.Contains(entry))
                casterStatus.FiredOneShotBossTriggers.Add(entry);
        }
    }
}
