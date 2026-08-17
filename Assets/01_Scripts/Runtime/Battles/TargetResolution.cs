using System;
using System.Collections.Generic;
using System.Linq;
using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles
{
// TargetScope/TargetCount 해석 로직 — 플레이어 타겟팅(ActionSelectionPhaseManager)과 적 AI가 공유한다.
// isTargeted 판정은 호출부에서 주입: 플레이어 경로는 allBattleCharacters 배열 스캔(그 라운드 전체 확정된 타겟팅),
// 적 AI 경로는 그 라운드 안에서 로컬로 누적한 카운트를 써야 한다(TargetingData가 배틀당 1회 할당이라
// 배열을 스캔하면 아직 이번 라운드에 결정 안 된 다른 캐릭터의 지난 라운드 데이터를 잘못 읽을 수 있음).
public static class TargetResolution
{
    // scope에 맞는 타겟 후보(생존자만) 조회. Ally scope엔 caster 자신도 포함된다.
    // Self scope는 항상 caster 자신 1명만 — 플레이어 경로(ActionSelectionPhaseManager)는 지금까지 Self를
    // 이 메서드 호출 전에 따로 처리해서 여기로 넘어온 적이 없었지만, 적 AI는 스코프 구분 없이 모든 스킬을
    // 똑같이 이 메서드로 후보를 구하므로 Self를 여기서도 반드시 정확히 처리해야 한다.
    public static IEnumerable<CharacterHandler> GetCandidates(
        TargetScope scope, CharacterHandler caster, IEnumerable<CharacterHandler> allBattleCharacters)
    {
        if (scope == TargetScope.Self)
            return new[] { caster };

        if (allBattleCharacters == null) return Enumerable.Empty<CharacterHandler>();

        IEnumerable<CharacterHandler> alive = allBattleCharacters
            .Where(c => c != null && c.GetCharacterStatus().currentState != CharacterState.Dead);

        return scope switch
        {
            TargetScope.Ally  => alive.Where(c => c.characterType == caster.characterType),
            TargetScope.Enemy => alive.Where(c => c.characterType != caster.characterType),
            _                 => alive // Any
        };
    }

    // 메인 타겟 제외 후보를 빈슬롯 > 진영(메인과 동일) > 속도(느린 순)로 정렬해 totalCount-1명 선택
    public static CharacterHandler[] SelectPriorityTargets(
        CharacterHandler mainTarget, IEnumerable<CharacterHandler> candidates, int totalCount,
        Func<CharacterHandler, bool> isTargeted)
    {
        return candidates
            .Where(c => c != mainTarget)
            .OrderByDescending(c => !isTargeted(c))
            .ThenByDescending(c => c.characterType == mainTarget.characterType)
            .ThenBy(c => c.CurrentSpeed)
            .Take(Mathf.Max(0, totalCount - 1))
            .ToArray();
    }

    // 메인 타겟 제외 후보에서 비복원 랜덤으로 totalCount-1명 선택
    public static CharacterHandler[] SelectRandomTargets(
        CharacterHandler mainTarget, IEnumerable<CharacterHandler> candidates, int totalCount)
    {
        List<CharacterHandler> pool = candidates.Where(c => c != mainTarget).ToList();
        List<CharacterHandler> picked = new List<CharacterHandler>();
        int need = Mathf.Max(0, totalCount - 1);

        for (int i = 0; i < need && pool.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            picked.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return picked.ToArray();
    }

    // targetSpec.TargetCount에 맞춰 mainTarget 외 추가 타겟을 채운다 (Single이면 빈 배열)
    public static CharacterHandler[] ResolveAdditionalTargets(
        CharacterHandler mainTarget, ITargetSpec targetSpec, IEnumerable<CharacterHandler> candidates,
        Func<CharacterHandler, bool> isTargeted)
    {
        if (targetSpec == null) return Array.Empty<CharacterHandler>();

        return targetSpec.TargetCount switch
        {
            TargetCount.Fixed  => SelectPriorityTargets(mainTarget, candidates, targetSpec.TargetCountValue, isTargeted),
            TargetCount.All    => candidates.Where(c => c != mainTarget).ToArray(),
            TargetCount.Random => SelectRandomTargets(mainTarget, candidates, targetSpec.TargetCountValue),
            _                  => Array.Empty<CharacterHandler>() // Single
        };
    }
}
}
