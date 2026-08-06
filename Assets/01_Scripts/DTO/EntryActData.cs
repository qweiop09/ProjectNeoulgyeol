using _01_Scripts.Runtime.Battles;

/// <summary>
/// 전투 진입 연출(등장) 행동 데이터. 자기 자신을 대상으로 한 번만 재생되며,
/// OpenPhaseController가 캐릭터별 entryTimelineAsset을 재생할 때 사용한다.
/// </summary>
public class EntryActData : ActData
{
    public EntryActData() { }

    public EntryActData(CharacterHandler caster, int useSlot)
    {
        CastPlayerCharacter   = caster;
        UseSlot               = useSlot;
        TargetPlayerCharacter = caster; // 자기 자신이 대상
        TargetSlot            = 0;
    }
}
