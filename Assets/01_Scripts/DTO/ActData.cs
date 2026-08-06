using System;
using _01_Scripts.DTO;
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

    // 이 행동의 타겟 지정 정보(Item/CharacterSkill 공용) — ActionSelectionPhaseManager/AttackArrowController가
    // 구체 타입을 몰라도 이걸로 타겟팅을 처리할 수 있도록 한다.
    public ITargetSpec GetTargetSpec() => this switch
    {
        ItemActData i => i.UseItem,
        SkillActData s => s.UseSkill,
        _ => null,
    };
}
