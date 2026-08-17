using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    // 보스 트리거 조건 하나의 공통 뼈대. ScriptableObject 에셋으로 만들어서 여러 트리거/보스가 공유할 수 있다.
    public abstract class BossTriggerConditionBase : ScriptableObject
    {
        public abstract bool IsMet(EnemyDecisionContext ctx);
    }
}
