using _01_Scripts.DTO.Item;
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

    [Space(10)]

    [SerializeField] public AnimationClip idleAnimation;
    [SerializeField] public AnimationClip runAnimation;
    [SerializeField] public AnimationClip deadAnimation;

    // 피격이랑 회피랑 가드랑 흐트러짐 만들기

    [Header("Skills")]
    public CharacterSkill[] characterAttacks;
    public CharacterSkill[] characterSkills;

    [Header("Equipment")]
    // 전투 구현 단계에서 전리품 시스템과 함께 실제 장착 로직 추가 예정
    public EquipmentItem rightHand;
    public EquipmentItem leftHand;
    public EquipmentItem head;
    public EquipmentItem body;
    public EquipmentItem legs;
    public EquipmentItem accessory1;
    public EquipmentItem accessory2;

    [Header("Battle Inventory")]
    [Tooltip("캐릭터 전투 아이템 슬롯 수. 스킬 등으로 변동 가능")]
    [Min(1)] public int battleItemSlotCount = 15;

    public int GetRandomSpeed()
    {
        return Random.Range(characterSpeedLowLimit, characterSpeedHighLimit + 1);
    }
}
}
