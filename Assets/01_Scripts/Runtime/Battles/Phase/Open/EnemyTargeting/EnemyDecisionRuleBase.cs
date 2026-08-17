using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    // 킬확정/흐트러짐노림수/힐임계치 등 조건부 오버라이드 룰의 공통 뼈대.
    // ScriptableObject 에셋으로 만들어서 여러 적이 같은 룰 인스턴스를 공유할 수 있고,
    // EnemyAIProfile.rules 배열에서 디자이너가 순서를 드래그로 조정할 수 있게 한다.
    public abstract class EnemyDecisionRuleBase : ScriptableObject
    {
        [Tooltip("true면 이 룰은 '위험'(자기 흐트러짐을 감수하는) 후보까지 검사한다. false면 안전한 후보에서만 찾는다.")]
        public bool allowRiskySelf;

        // 이 룰의 조건을 만족하는 후보가 있으면 decision을 채우고 true를 반환한다 — 그 즉시 확정, 뒤 룰은 검사 안 함.
        public abstract bool TryDecide(EnemyDecisionContext ctx, out EnemyDecisionCandidate decision);
    }
}
