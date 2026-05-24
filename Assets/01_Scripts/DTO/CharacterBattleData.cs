using UnityEngine;

namespace _01_Scripts.DTO
{
public class CharacterBattleData
{
    // TODO: 캐릭터 스테이터스 및 정보
    
    public CharacterData CharacterData;
    
    private int _currentHp;
    private int _currentMp;
    
    public int CurrentSpeed;

    public int PlacementOrder;
    
    // 캐릭터가 보유한 스킬들의 정보
    // private CharacterSkill[] characterSkills;
    
    // 사용하는 행동의 정보 ( 사용 스킬, 대상 등 )
    // 본 배열의 인덱스는 슬롯의 순서와 같음
    public ActData[] TargetingData;
    
    public Transform CharacterTransform;
    

    public CharacterBattleData(CharacterData characterData)
    {
        CharacterData = characterData;
        
        _currentHp = CharacterData.maxHp;
        _currentMp = CharacterData.maxMp;
    }
    
    public void SetRandomSpeed()
    {
        CurrentSpeed = CharacterData.GetRandomSpeed();
    }
    
    public void DebugPrintStatusData()
    {
        Debug.Log(
            "체력 : " + CharacterData.maxHp + "\n" +
            "마나 : " + CharacterData.maxMp + "\n" +
            "공격 : " + CharacterData.attack + "\n" +
            "방어 : " + CharacterData.defense + "\n" +
            "현재 속도 : " + CurrentSpeed );
    }
    

}
}
