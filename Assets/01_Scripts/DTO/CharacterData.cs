using UnityEngine;
using UnityEngine.Timeline;

namespace _01_Scripts.DTO
{
[CreateAssetMenu(menuName = "ProjectNeoulgyeol/Character Data", fileName = "New Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Info")]
    public string characterName;
    public Sprite portrait;

    [Header("Stats")]
    public int maxHp;
    public int maxMp;
    public int maxStamina;
    public int attack;
    public int defense;

    public int slotCount; // 스킬 슬롯 갯수

    public int characterSpeedLowLimit;
    public int characterSpeedHighLimit;

    [Tooltip("도망 방해 계수 — 적 캐릭터에서만 의미 있음. 클수록 아군이 이 캐릭터로부터 도망치기 어려워진다.")]
    public float escapeResistance = 1f;

    [Space(10)]

    [SerializeField] public AnimationClip idleAnimation;
    [SerializeField] public AnimationClip runAnimation;
    [SerializeField] public AnimationClip hitAnimation;
    [SerializeField] public AnimationClip staggeredAnimation;
    [SerializeField] public AnimationClip deadAnimation;

    [Tooltip("체력/스테미나 바가 캐릭터 기준 어디에 떠야 하는지 (스프라이트 크기에 맞춰 캐릭터별로 조정)")]
    public Vector3 statBarOffset = new Vector3(0, 1.5f, 0);

    // 피격이랑 회피랑 가드랑 흐트러짐 만들기

    [Header("Skills")]
    public CharacterSkill[] characterAttacks;
    public CharacterSkill[] characterSkills;

    [Header("Battle Inventory")]
    [Tooltip("캐릭터 전투 아이템 슬롯 수. 스킬 등으로 변동 가능")]
    [Min(1)] public int battleItemSlotCount = 15;

    [Header("전투 진입 연출")]
    [Tooltip("비어있으면 기존처럼 그냥 지정 위치에 즉시 배치(연출 없음).")]
    public TimelineAsset entryTimelineAsset;

    [Tooltip("entryTimelineAsset이 있을 때, 시작 지점을 최종 배치 위치 기준 오프셋으로 지정. " +
             "MotionClip의 로컬 좌표계와 동일한 규칙 — 타임라인의 MotionClip 경로가 (0,0)에서 -이 값으로 끝나야 정확히 제자리에 도착함.")]
    public Vector2 entrySpawnOffset;

    [Tooltip("2라운드부터 매 라운드 전환 시 재생. 비어있으면 아무 연출 없이 넘어감(연출 없어도 라운드 배너는 뜸).")]
    public TimelineAsset roundTransitionTimelineAsset;

    [Header("전투 종료 연출")]
    [Tooltip("BattleOutcome.Victory일 때 재생. 아군 승리 포즈용 — 이 outcome에서 적은 이미 전멸 상태라 적 쪽엔 의미 없음.")]
    public TimelineAsset victoryTimelineAsset;

    [Tooltip("BattleOutcome.Defeat일 때 재생. 적의 승리 조롱 등 — 이 outcome에서 아군은 이미 전멸 상태라 아군 쪽엔 의미 없음.")]
    public TimelineAsset defeatTimelineAsset;

    [Tooltip("BattleOutcome.Retreat일 때 재생. 양쪽 다 생존 상태라 아군/적군 모두 의미 있음(아군: 도주 동작, 적: 반응).")]
    public TimelineAsset retreatTimelineAsset;

    public int GetRandomSpeed()
    {
        return Random.Range(characterSpeedLowLimit, characterSpeedHighLimit + 1);
    }
}
}
