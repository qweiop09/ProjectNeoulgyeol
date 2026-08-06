using _01_Scripts.Runtime.Battles;

// 전투 종료(승리/패배/도주) 연출 행동 데이터. 자기 자신을 대상으로 한 번만 재생되며,
// BattleManager가 캐릭터별 victory/defeat/retreatTimelineAsset을 재생할 때 사용한다.
public class BattleEndActData : ActData
{
    public BattleEndActData() { }

    public BattleEndActData(CharacterHandler caster, int useSlot)
    {
        CastPlayerCharacter   = caster;
        UseSlot               = useSlot;
        TargetPlayerCharacter = caster; // 자기 자신이 대상
        TargetSlot            = 0;
    }
}
