namespace _01_Scripts.DTO
{
public class CompeteData
{
    // 한 합에대한 정보, 누가 누구를 공격하는지, 어떤 스킬을 사용하는지, 공격 대상이 누구인지 등
    
    
    public CharacterBattleData mainTargetPositionIndex; // 공격 대상의 위치 인덱스
    public CharacterBattleData[] additionalTargetPositionIndex; // 추가 공격 대상의 위치 인덱스 (예: 광역 공격)
    
    public CharacterSkill useSkill; // 사용 스킬 정보
    public CharacterSkill mainReactionSkill; // 반응 스킬 정보 (예: 반격, 회피 등)
    public CharacterSkill[] additionalReactionSkill; // 추가 반응 스킬 정보 (예: 광역 공격에 대한 반응 등)
    
}
}
