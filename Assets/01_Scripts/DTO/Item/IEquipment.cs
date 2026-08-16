using _01_Scripts.Runtime.Battles;

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

        // 이 장비를 낀 캐릭터가 피격당했을 때 반응(반격, 버프 부여 등) — HitResolver.ApplyHit이 호출. 기본은 아무 것도 안 함.
        void OnHitReceived(HitContext ctx, DamageResult damage) { }
    }
}
