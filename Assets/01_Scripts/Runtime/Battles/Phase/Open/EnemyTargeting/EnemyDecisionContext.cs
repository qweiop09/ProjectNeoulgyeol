using System.Collections.Generic;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    // 룰/스코어러에 전달되는 판단 컨텍스트 — 캐스터가 한 슬롯을 결정할 때마다 새로 만들어진다.
    public class EnemyDecisionContext
    {
        public CharacterHandler Caster;
        public CharacterHandler[] AllBattleCharacters;
        public int CurrentRound;
        public EnemyAIProfile Profile;

        // 안전(자기 흐트러짐 위험 없음) / 위험 후보 — 버프 재적용 필터까지 통과한 상태
        public List<EnemyDecisionCandidate> SafeCandidates = new();
        public List<EnemyDecisionCandidate> RiskyCandidates = new();

        // 이번 SetTargeting 호출(=이번 라운드) 동안 각 플레이어 캐릭터가 몇 번이나 메인/추가 타겟으로 선택됐는지 —
        // 집중공격 방지용 로컬 누적. TargetingData 배열은 배틀당 1회만 할당돼서 재사용되므로 스캔하면 안 되고
        // 반드시 이렇게 라운드마다(=SetTargeting 호출마다) 새로 만든 값을 써야 한다.
        public Dictionary<CharacterHandler, int> TargetHitCounts = new();

        public int GetHitCount(CharacterHandler target) =>
            TargetHitCounts.TryGetValue(target, out int count) ? count : 0;

        public void AddHitCount(CharacterHandler target, int amount = 1)
        {
            TargetHitCounts[target] = GetHitCount(target) + amount;
        }
    }
}
