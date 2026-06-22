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
    public class EquipmentItem : ScriptableObject
    {
        public string itemName;
        [TextArea] public string itemDescription;
        public Sprite icon;
        public EquipmentSlotType slotType;

        // 전투 스탯 보정값 (CharacterStatusCalculator에서 장착 시 적용)
        public int attackBonus;
        public int defenseBonus;
        public int maxHpBonus;
        public int maxMpBonus;
        public int maxStaminaBonus;
    }
}
