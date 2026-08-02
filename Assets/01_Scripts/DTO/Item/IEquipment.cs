namespace _01_Scripts.DTO.Item
{
    public interface IEquipment
    {
        EquipmentSlotType SlotType { get; }
        int AttackBonus { get; }
        int DefenseBonus { get; }
        int MaxHpBonus { get; }
        int MaxMpBonus { get; }
        int MaxStaminaBonus { get; }
    }
}
