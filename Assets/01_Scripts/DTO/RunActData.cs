using _01_Scripts.Runtime.Battles;

/// <summary>
/// 도망 시도 행동 데이터. 실행 자체는 아무것도 안 하고(Stay와 달리 스태미나 회복도 없음),
/// 도망 성공 여부는 경합 페이즈 시작 시 CompetePhaseController가 그룹 단위로 한 번에 판정한다.
/// </summary>
public class RunActData : ActData
{
    public RunActData() { }

    public RunActData(CharacterHandler caster, int useSlot)
    {
        CastPlayerCharacter   = caster;
        UseSlot               = useSlot;
        TargetPlayerCharacter = caster; // 자기 자신이 대상
        TargetSlot            = 0;
    }
}
