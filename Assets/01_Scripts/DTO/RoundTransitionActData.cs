using _01_Scripts.Runtime.Battles;

// 라운드 전환(2라운드부터) 연출 행동 데이터. 자기 자신을 대상으로 한 번만 재생되며,
// OpenPhaseController가 캐릭터별 roundTransitionTimelineAsset을 재생할 때 사용한다.
public class RoundTransitionActData : ActData
{
    public RoundTransitionActData() { }

    public RoundTransitionActData(CharacterHandler caster, int useSlot)
    {
        CastPlayerCharacter   = caster;
        UseSlot               = useSlot;
        TargetPlayerCharacter = caster; // 자기 자신이 대상
        TargetSlot            = 0;
    }
}
