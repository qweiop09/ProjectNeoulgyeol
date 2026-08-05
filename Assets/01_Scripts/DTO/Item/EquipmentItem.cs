using UnityEngine;

namespace _01_Scripts.DTO.Item
{
    public enum EquipmentSlotType
    {
        RightHand,   // 무기 또는 방패
        LeftHand,    // 무기 또는 방패
        Head,
        Body,
        Legs,
        Accessory
    }

    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Item/Equipment Item", fileName = "New Equipment Item")]
    public class EquipmentItem : Item, IEquipment
    {
        public EquipmentSlotType slotType;

        // 전투 스탯 보정값 (CharacterStatusCalculator에서 장착 시 적용)
        public int attackBonus;
        public int defenseBonus;
        public int maxHpBonus;
        public int maxMpBonus;
        public int maxStaminaBonus;

        EquipmentSlotType IEquipment.SlotType => slotType;
        int IEquipment.AttackBonus => attackBonus;
        int IEquipment.DefenseBonus => defenseBonus;
        int IEquipment.MaxHpBonus => maxHpBonus;
        int IEquipment.MaxMpBonus => maxMpBonus;
        int IEquipment.MaxStaminaBonus => maxStaminaBonus;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            category = ItemCategory.Equipment;
            maxStack = 0; // 장비는 겹치지 않음

            base.OnValidate(); // ID 중복 검사 등 공통 검증 유지
        }
#endif
    }
}
