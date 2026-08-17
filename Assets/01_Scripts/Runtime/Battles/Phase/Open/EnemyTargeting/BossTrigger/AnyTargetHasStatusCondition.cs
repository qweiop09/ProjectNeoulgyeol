using System.Collections.Generic;
using System.Linq;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Boss Trigger Conditions/Any Target Has Status")]
    public class AnyTargetHasStatusCondition : BossTriggerConditionBase
    {
        [Tooltip("이 버프/디버프를 검사한다")]
        public BuffEffectBase status;
        [Tooltip("어느 진영에서 찾을지 — 보통 Enemy(=플레이어 파티) 기본값")]
        public TargetScope scope = TargetScope.Enemy;

        public override bool IsMet(EnemyDecisionContext ctx)
        {
            if (status == null) return false;

            IEnumerable<CharacterHandler> candidates = TargetResolution.GetCandidates(scope, ctx.Caster, ctx.AllBattleCharacters);
            return candidates.Any(c => c.GetCharacterStatus().activeBuffs.Any(b => b.Source == status));
        }
    }
}
