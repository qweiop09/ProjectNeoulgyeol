using _01_Scripts.Runtime.Battles;
using _01_Scripts.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.DTO
{
[CreateAssetMenu(menuName = "ProjectNeoulgyeol/SkillData", fileName = "New Skill Data")]
public class CharacterSkill : ScriptableObject, ITargetSpec
{
    // 뭔가 나중에 쓸 것 같음
    // [SerializeField] private int skillStartPriority; // 스킬의 행동 우선순위 (낮을수록 먼저 행동)

    [SerializeField] public string skillName; // 스킬 이름

    [SerializeField] public int skillMpCost; // 스킬 사용에 필요한 마나 비용
    [SerializeField] public int skillStaminaCost; // 스킬 사용에 필요한 스태미나 비용

    [SerializeField] public int skillStartDistance; // 스킬의 행동 시작 거리 타입 (근거리, 원거리 등)

    [SerializeField] public TargetScope targetScope = TargetScope.Enemy;
    [SerializeField] public TargetCount targetCount = TargetCount.Single;
    [Tooltip("targetCount가 Fixed/Random일 때만 사용하는 인원 수")]
    [SerializeField] public int targetCountValue = 1;

    TargetScope ITargetSpec.TargetScope => targetScope;
    TargetCount ITargetSpec.TargetCount => targetCount;
    int ITargetSpec.TargetCountValue => targetCountValue;

    [SerializeField] public ITimelineBinder timelineBinder; // 스킬이 실행될 때 타임라인과 데이터를 바인딩하는 인터페이스
    [SerializeField] public TimelineAsset skillTimelineAsset; // 스킬이 실행될 때 재생되는 타임라인 에셋

    // 방어관통/디버프/흡혈/버프 등 추가 타격 효과 (구현체는 추후)
    public IHitEffect[] hitEffects = System.Array.Empty<IHitEffect>();

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        // 자신(Self)은 항상 정확히 1명이라 모두/랜덤과 조합될 수 없음 — Item.cs와 동일한 규칙.
        if (targetScope == TargetScope.Self)
            targetCount = TargetCount.Single;
    }
#endif
}
}
