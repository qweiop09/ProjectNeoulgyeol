using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles;
using UnityEngine;

public class ActData
{
    public CharacterHandler CastPlayerCharacter;
    
    public int UseSlot;
    
    // 상대 배열의 index
    public CharacterHandler TargetPlayerCharacter;
    
    // slot은 림버스 슬롯같은거
    public int TargetSlot;

    public CharacterSkill UseSkill;

    public ActData(CharacterHandler castPlayerCharacter, int useSlot, CharacterSkill useSkill
        , CharacterHandler targetPlayerCharacter, int targetSlot)
    {
        CastPlayerCharacter = castPlayerCharacter;
        UseSlot = useSlot;
        UseSkill = useSkill;
        TargetPlayerCharacter = targetPlayerCharacter;
        TargetSlot = targetSlot;
    }

    public ActData() { }
}