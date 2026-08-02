using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.DTO.Item
{
    public enum ItemTargetType
    {
        Self,
        Ally,
        Enemy,
        AllAllies,
        AllEnemies,
    }

    public enum ItemCategory
    {
        Consumable,
        DropItem,
    }

    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Item/Item", fileName = "New Item")]
    public class Item : ScriptableObject
    {
        [SerializeField] public string itemName;
        [SerializeField][TextArea] public string itemDescription;
        [SerializeField] public Sprite icon;

        [SerializeField] public ItemTargetType targetType;
        [SerializeField] public ItemCategory category = ItemCategory.Consumable;

        [Tooltip("사용 후 인벤토리에서 제거할지 여부")]
        [SerializeField] public bool isConsumable = true;

        [SerializeField] public ItemEffectBase[] effects;

        public void Use(CharacterStatus target)
        {
            if (effects == null) return;
            foreach (var effect in effects)
                if (effect != null)
                    effect.Apply(target);
        }
    }
}
