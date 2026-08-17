using System;
using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting
{
    [Serializable]
    public class BossTriggerEntry
    {
        [Tooltip("전부 AND — 여기 있는 조건이 모두 참이어야 이 엔트리가 발동한다. OR이 필요하면 엔트리를 여러 개로 나눠서 표현한다.")]
        public BossTriggerConditionBase[] conditions = Array.Empty<BossTriggerConditionBase>();

        public CharacterSkill forcedSkill;

        [Tooltip("true면 전투 중 딱 한 번만 발동(발동 기록은 CharacterStatus에 배틀 인스턴스별로 저장되므로 SO를 공유해도 배틀 간 상태가 안 새어나간다). false면 조건이 참일 때마다 매번 발동 가능.")]
        public bool oneShot;

        [Tooltip("true면 이 강제 스킬은 '자기 흐트러짐 위험 회피'/'버프 재적용 방지' 필터를 건너뛰고 무조건 실행된다. false면 일반 룰처럼 그 필터들을 거친다.")]
        public bool bypassSelfRiskFilter;

        [Tooltip("true면 캐스터가 이미 흐트러진(행동불가) 상태여도 강제로 즉시 회복시키고 이 트리거를 발동한다. false면 일반 캐릭터처럼 그 라운드는 스킵하고(트리거 미발동) 다음 라운드에 조건을 다시 검사한다.")]
        public bool bypassExistingStagger;
    }
}
