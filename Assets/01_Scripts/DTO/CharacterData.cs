using UnityEngine;

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

    public int GetRandomSpeed()
    {
        return Random.Range(characterSpeedLowLimit, characterSpeedHighLimit + 1);
    }
}
}
