using UnityEngine;

namespace _01_Scripts.DTO
{
[CreateAssetMenu(menuName = "Game/Character Data", fileName = "New Character Data")]
public class CharacterStatus : ScriptableObject
{
    // TODO: 캐릭터 스테이터스 및 정보
    public int maxHp;
    public int maxMp;
    public int attack;
    public int defense;

    public int slotCount; // 스킬 슬롯 갯수
    
    public int characterSpeedLowLimit;
    public int characterSpeedHighLimit;
    
    
    // 캐릭터가 보유한 스킬들의 정보
    private CharacterSkill[] characterSkills;
    
    public int GetRandomSpeed()
    {
        return Random.Range(characterSpeedLowLimit, characterSpeedHighLimit + 1);
    }
    
    // 장비한 장비들의 정보
    
    
}
}
