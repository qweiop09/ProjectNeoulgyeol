using System;
using _01_Scripts.Runtime.Battles;

/// <summary>
/// 전투에서 한 캐릭터가 한 슬롯에 수행할 행동의 공통 정보.
/// 행동 종류에 따라 SkillActData, ItemActData 등 서브클래스를 사용하세요.
/// </summary>
public abstract class ActData
{
    public CharacterHandler CastPlayerCharacter;
    public int UseSlot;

    public CharacterHandler TargetPlayerCharacter;
    public int TargetSlot;

    // 메인 타겟(TargetPlayerCharacter) 외에 같은 행동이 추가로 적용하는 대상들 (다중 타겟용, 기본은 없음)
    public CharacterHandler[] AdditionalTargets = Array.Empty<CharacterHandler>();
}
