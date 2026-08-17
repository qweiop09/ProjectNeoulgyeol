using System;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    public enum StayMode { TurnForfeit, PerSlot }

    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Enemy AI Profile", fileName = "New Enemy AI Profile")]
    public class EnemyAIProfile : ScriptableObject
    {
        [Tooltip("순서 = 우선순위. 위에서부터 검사해서 처음 매치되는 룰이 즉시 확정된다.")]
        public EnemyDecisionRuleBase[] rules = Array.Empty<EnemyDecisionRuleBase>();

        [Header("폴백 스코어링 (룰이 아무것도 안 걸렸을 때)")]
        public float damageWeight = 1f;
        public float lowHpWeight = 1f;
        [Tooltip("Attack 역할 스킬에만 적용 — 이미 이번 라운드에 다른 적이 노린 대상일수록 점수를 나눠서 집중공격을 완화한다(소프트 페널티)")]
        public float focusFirePenaltyPerHit = 0.5f;
        [Tooltip("0이면 비활성. 0보다 크면 같은 대상이 이번 라운드에 이 횟수만큼 이미 타겟됐을 때 그 대상을 후보에서 아예 제외한다(하드 캡)")]
        public int maxHitsPerTargetPerRound = 0;
        [Tooltip("Stay 후보의 점수 — 다른 (스킬,타겟) 후보들과 동일한 스코어 경쟁에 참여한다. PerSlot 모드에서 이 값을 높이면 Stay를 더 자주/일찍 선택하게 된다.")]
        public float stayWeight = 0.1f;

        [Header("Stay 동작 방식")]
        public StayMode stayMode = StayMode.PerSlot;
        [Tooltip("stayMode == TurnForfeit일 때만 의미 있음 — true면 포기되는 남은 슬롯도 각각 Stay로 채워 스태미나 회복이 슬롯 수만큼 누적된다. false면 남은 슬롯은 그냥 비워둬서(no-op) 첫 Stay 1회만 회복한다.")]
        public bool stackStayRecoveryOnForfeit = false;

        [Header("보스 전용 특수행동 (일반 적은 비워둠)")]
        [Tooltip("순서 = 우선순위, 일반 룰 스택보다 먼저 전부 검사된다. 엔트리 내부 조건은 AND, 엔트리끼리는 OR.")]
        public BossTriggerEntry[] bossTriggers = Array.Empty<BossTriggerEntry>();
    }
}
