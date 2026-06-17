using _01_Scripts.DTO.Item;
using _01_Scripts.Runtime.Battles;

/// <summary>아이템을 사용하는 행동 데이터.</summary>
public class ItemActData : ActData
{
    public Item UseItem;

    public ItemActData() { }

    public ItemActData(CharacterHandler castPlayerCharacter, int useSlot, Item useItem,
        CharacterHandler targetPlayerCharacter, int targetSlot)
    {
        CastPlayerCharacter   = castPlayerCharacter;
        UseSlot               = useSlot;
        UseItem               = useItem;
        TargetPlayerCharacter = targetPlayerCharacter;
        TargetSlot            = targetSlot;
    }
}
