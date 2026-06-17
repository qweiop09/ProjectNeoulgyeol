using _01_Scripts.Runtime.Battles;

/// <summary>
/// 한 턴 대기(Stay) 행동 데이터.
/// 아무 행동도 하지 않고 턴이 끝난 후 스테미나를 회복합니다.
/// </summary>
public class StayActData : ActData
{
    public StayActData() { }

    public StayActData(CharacterHandler caster, int useSlot)
    {
        CastPlayerCharacter   = caster;
        UseSlot               = useSlot;
        TargetPlayerCharacter = caster; // 자기 자신이 대상
        TargetSlot            = 0;
    }
}
