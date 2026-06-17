using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles;

/// <summary>스킬/공격을 사용하는 행동 데이터.</summary>
public class SkillActData : ActData
{
    public CharacterSkill UseSkill;

    public SkillActData() { }

    public SkillActData(CharacterHandler castPlayerCharacter, int useSlot, CharacterSkill useSkill,
        CharacterHandler targetPlayerCharacter, int targetSlot)
    {
        CastPlayerCharacter = castPlayerCharacter;
        UseSlot             = useSlot;
        UseSkill            = useSkill;
        TargetPlayerCharacter = targetPlayerCharacter;
        TargetSlot          = targetSlot;
    }
}
