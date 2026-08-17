using System;
using System.Collections.Generic;
using System.Linq;
using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
public class EnemyActionController : MonoBehaviour
{
    [Tooltip("EnemyData.aiProfile이 비어있을 때(테스트 배틀 등 SourceEnemyData가 없는 경우 포함) 대신 쓰는 기본 프로필")]
    [SerializeField] private EnemyAIProfile defaultProfile;

    // SetTargeting 호출마다 1씩 증가 — OpenPhaseController가 라운드당 정확히 1번만 이 메서드를 부르므로
    // 이게 곧 현재 라운드 번호(1부터 시작) 역할을 한다. 보스 트리거의 TurnCountAtLeast/Equals 조건에 씀.
    private int _currentRound;

    public CharacterHandler[] SetTargeting(CharacterHandler[] casters, CharacterHandler[] targets)
    {
        _currentRound++;

        CharacterHandler[] allBattleCharacters = casters.Concat(targets).ToArray();

        // 이번 라운드(=이 SetTargeting 호출 전체) 동안 각 플레이어 캐릭터가 몇 번이나 타겟됐는지 — 집중공격 방지용
        Dictionary<CharacterHandler, int> targetHitCounts = new Dictionary<CharacterHandler, int>();

        for (int i = 0; i < casters.Length; i++)
        {
            CharacterHandler caster = casters[i];
            CharacterStatus casterStatus = caster.GetCharacterStatus();

            if (casterStatus.currentState == CharacterState.Dead)
                continue;

            EnemyAIProfile profile = ResolveProfile(caster);
            if (profile == null)
            {
                Debug.LogWarning($"[EnemyActionController] {caster.name}에 연결된 EnemyAIProfile이 없음(EnemyData.aiProfile / defaultProfile 둘 다 비어있음) — 이번 라운드 행동 스킵");
                continue;
            }

            int slotCount = casterStatus.CharacterData.slotCount;
            int remainingStamina = casterStatus.currentStamina;
            BossTriggerEntry pendingForcedTrigger = null;

            if (casterStatus.currentState == CharacterState.Staggered)
            {
                EnemyDecisionContext gateCtx = new EnemyDecisionContext
                {
                    Caster = caster,
                    AllBattleCharacters = allBattleCharacters,
                    CurrentRound = _currentRound,
                    Profile = profile
                };

                bool triggered = BossTriggerEvaluator.TryEvaluate(gateCtx, out BossTriggerEntry gateTrigger);
                if (!triggered || !gateTrigger.bypassExistingStagger)
                    continue; // 일반 캐릭터와 동일 — 이번 라운드는 완전히 스킵(다음 라운드에 조건 재검사)

                // 보스 트리거가 명시적으로 흐트러짐 무시를 허용함 — 강제로 즉시 회복시키고 이어서 진행
                pendingForcedTrigger = gateTrigger;
                CharacterStatusCalculator.Instance.ApplyStaminaModify(casterStatus, casterStatus.GetMaxStamina() / 2);
                casterStatus.SetCurrentState(CharacterState.Normal);
                remainingStamina = casterStatus.currentStamina;
            }

            for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
            {
                EnemyDecisionContext ctx = new EnemyDecisionContext
                {
                    Caster = caster,
                    AllBattleCharacters = allBattleCharacters,
                    CurrentRound = _currentRound,
                    Profile = profile,
                    TargetHitCounts = targetHitCounts
                };

                BossTriggerEntry firedTrigger;
                bool bossTriggerFired;

                if (pendingForcedTrigger != null)
                {
                    // 위 흐트러짐 게이트에서 이미 찾아둔 트리거를 그대로 이번 슬롯(슬롯 0)에 씀 — 다시 평가하면
                    // oneShot 발동 기록이 아직 안 남은 상태라 같은 트리거를 또 찾아버리는 게 아니라, 애초에
                    // 여기서 재평가 자체를 안 해서 이중 소모/재탐색 문제가 생기지 않는다.
                    firedTrigger = pendingForcedTrigger;
                    bossTriggerFired = true;
                    pendingForcedTrigger = null;
                }
                else
                {
                    bossTriggerFired = BossTriggerEvaluator.TryEvaluate(ctx, out firedTrigger);
                }

                EnemyDecisionCandidate chosen;

                if (bossTriggerFired)
                {
                    chosen = ResolveBossForcedCandidate(ctx, firedTrigger, remainingStamina);
                    BossTriggerEvaluator.MarkFired(casterStatus, firedTrigger);
                }
                else
                {
                    BuildCandidates(ctx, remainingStamina);
                    chosen = DecideFromRulesAndScoring(ctx);
                }

                ActData actData = BuildActData(ctx, chosen, slotIndex, targetHitCounts, allBattleCharacters);
                caster.TargetingData[slotIndex] = actData;

                if (!chosen.IsStay)
                {
                    remainingStamina -= chosen.Skill.skillStaminaCost;
                    continue;
                }

                if (profile.stayMode != StayMode.TurnForfeit)
                    continue; // PerSlot — 다음 슬롯에서 처음부터 다시 판단

                // 남은 슬롯 처리 — TargetingData는 배틀당 1회만 할당되고 라운드마다 리셋되지 않으므로,
                // 여기서 손대지 않고 그냥 break하면 이전 라운드에 이 슬롯에 있던 낡은 행동(화살표/실행 모두)이
                // 그대로 남아 이번 라운드에도 잘못 쓰일 수 있다 — 반드시 null이나 새 Stay로 명시적으로 덮어써야 함.
                for (int fillSlot = slotIndex + 1; fillSlot < slotCount; fillSlot++)
                    caster.TargetingData[fillSlot] = profile.stackStayRecoveryOnForfeit
                        ? new StayActData(caster, fillSlot)
                        : null;

                break; // TurnForfeit — 이 캐스터는 여기서 턴 종료
            }
        }

        return casters;
    }

    private EnemyAIProfile ResolveProfile(CharacterHandler caster)
    {
        EnemyAIProfile profile = caster.GetCharacterStatus().SourceEnemyData != null
            ? caster.GetCharacterStatus().SourceEnemyData.aiProfile
            : null;

        return profile != null ? profile : defaultProfile;
    }

    // 이번 슬롯에서 캐스터가 쓸 수 있는 모든 (스킬,타겟) 후보 + Stay 후보를 ctx에 채운다(안전/위험 파티션, 버프 재적용 필터 포함).
    private void BuildCandidates(EnemyDecisionContext ctx, int remainingStamina)
    {
        ctx.SafeCandidates.Clear();
        ctx.RiskyCandidates.Clear();

        CharacterHandler caster = ctx.Caster;
        CharacterData data = caster.GetCharacterStatus().CharacterData;

        IEnumerable<CharacterSkill> affordableSkills = (data.characterAttacks ?? Array.Empty<CharacterSkill>())
            .Concat(data.characterSkills ?? Array.Empty<CharacterSkill>())
            .Where(s => s != null && s.skillStaminaCost <= remainingStamina);

        foreach (CharacterSkill skill in affordableSkills)
        {
            IEnumerable<CharacterHandler> scopeCandidates =
                TargetResolution.GetCandidates(skill.targetScope, caster, ctx.AllBattleCharacters);

            foreach (CharacterHandler target in scopeCandidates)
            {
                if (IsRedundantBuff(skill, target)) continue;

                bool risky = DamagePrediction.WouldTriggerSelfStagger(caster, remainingStamina, skill);
                EnemyDecisionCandidate candidate = new EnemyDecisionCandidate(skill, target);
                (risky ? ctx.RiskyCandidates : ctx.SafeCandidates).Add(candidate);
            }
        }

        ctx.SafeCandidates.Add(EnemyDecisionCandidate.Stay());
    }

    private static bool IsRedundantBuff(CharacterSkill skill, CharacterHandler target)
    {
        if (skill.appliedBuffSource == null) return false;
        var existing = target.GetCharacterStatus().activeBuffs.Find(b => b.Source == skill.appliedBuffSource);
        return existing != null && existing.RemainingRounds > 0;
    }

    private EnemyDecisionCandidate DecideFromRulesAndScoring(EnemyDecisionContext ctx)
    {
        if (ctx.Profile.rules != null)
        {
            foreach (EnemyDecisionRuleBase rule in ctx.Profile.rules)
            {
                if (rule == null) continue;
                if (rule.TryDecide(ctx, out EnemyDecisionCandidate ruleDecision))
                    return ruleDecision;
            }
        }

        EnemyDecisionCandidate? scored = EnemyUtilityScorer.Choose(ctx);
        return scored ?? EnemyDecisionCandidate.Stay(); // 안전망 — 후보가 정말 하나도 없을 때만 해당(Stay는 항상 안전후보라 이론상 발생 안 함)
    }

    // 보스 트리거로 강제된 스킬의 타겟만 고른다(스킬 자체는 이미 확정됨). 위험/버프 필터를 우회하지 않는 트리거는
    // 조건을 못 만족하면 Stay로 대체된다(트리거 자체는 이미 이번 슬롯에서 소모된 것으로 취급 — MarkFired는 호출부가 별도로 부름).
    private EnemyDecisionCandidate ResolveBossForcedCandidate(EnemyDecisionContext ctx, BossTriggerEntry trigger, int remainingStamina)
    {
        CharacterSkill skill = trigger.forcedSkill;
        CharacterHandler caster = ctx.Caster;

        if (skill.skillStaminaCost > remainingStamina)
            return EnemyDecisionCandidate.Stay();

        if (!trigger.bypassSelfRiskFilter && DamagePrediction.WouldTriggerSelfStagger(caster, remainingStamina, skill))
            return EnemyDecisionCandidate.Stay();

        IEnumerable<CharacterHandler> candidates =
            TargetResolution.GetCandidates(skill.targetScope, caster, ctx.AllBattleCharacters);

        if (!trigger.bypassSelfRiskFilter)
            candidates = candidates.Where(t => !IsRedundantBuff(skill, t));

        CharacterHandler bestTarget = null;
        float bestScore = float.NegativeInfinity;

        foreach (CharacterHandler target in candidates)
        {
            float score = EnemyUtilityScorer.ScoreTargetFactor(ctx.Profile, caster, target, skill);
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = target;
            }
        }

        return bestTarget != null ? new EnemyDecisionCandidate(skill, bestTarget) : EnemyDecisionCandidate.Stay();
    }

    private ActData BuildActData(EnemyDecisionContext ctx, EnemyDecisionCandidate chosen, int slotIndex,
        Dictionary<CharacterHandler, int> targetHitCounts, CharacterHandler[] allBattleCharacters)
    {
        CharacterHandler caster = ctx.Caster;

        if (chosen.IsStay)
            return new StayActData(caster, slotIndex);

        IEnumerable<CharacterHandler> scopeCandidates =
            TargetResolution.GetCandidates(chosen.Skill.targetScope, caster, allBattleCharacters);

        CharacterHandler[] additionalTargets = TargetResolution.ResolveAdditionalTargets(
            chosen.MainTarget, chosen.Skill, scopeCandidates,
            c => targetHitCounts.TryGetValue(c, out int n) && n > 0);

        SkillActData skillActData = new SkillActData(caster, slotIndex, chosen.Skill, chosen.MainTarget, 0)
        {
            AdditionalTargets = additionalTargets
        };

        IncrementHitCount(targetHitCounts, chosen.MainTarget);
        foreach (CharacterHandler extra in additionalTargets)
            IncrementHitCount(targetHitCounts, extra);

        return skillActData;
    }

    private static void IncrementHitCount(Dictionary<CharacterHandler, int> counts, CharacterHandler target)
    {
        counts[target] = (counts.TryGetValue(target, out int n) ? n : 0) + 1;
    }
}
}
