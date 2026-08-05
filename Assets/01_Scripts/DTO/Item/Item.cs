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

    // 장비/소모품/재료/재화/이벤트(중요) 아이템 5종. 필요하면 얼마든지 추가/삭제 가능.
    public enum ItemCategory
    {
        Equipment,
        Consumable,
        Material,
        Currency,
        EventItem,
    }

    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Item/Item", fileName = "New Item")]
    public class Item : ScriptableObject
    {
        [Tooltip("세이브 등에 쓰일 고유 문자열 ID. 예: consumable_hp_potion_small")]
        [SerializeField] private string itemId;
        public string ItemId => itemId;

        [SerializeField] public string itemName;
        [SerializeField][TextArea] public string itemDescription;
        [SerializeField] public Sprite icon;

        [SerializeField] public ItemTargetType targetType;
        [SerializeField] public ItemCategory category = ItemCategory.Consumable;

        [Tooltip("한 슬롯에 몇 개까지 겹칠 수 있는지. 0이면 겹치지 않는 것으로 취급하고 인벤토리에 수량을 표시하지 않는다 (장비/이벤트 아이템용).")]
        [SerializeField] public int maxStack = 999;

        [SerializeField] public ItemEffectBase[] effects;

        public bool IsStackable => maxStack > 0;

        public void Use(CharacterStatus target)
        {
            if (effects == null) return;
            foreach (var effect in effects)
                if (effect != null)
                    effect.Apply(target);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(itemId))
                return;

            foreach (var guid in UnityEditor.AssetDatabase.FindAssets($"t:{nameof(Item)}"))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Item other = UnityEditor.AssetDatabase.LoadAssetAtPath<Item>(path);
                if (other != null && other != this && other.itemId == itemId)
                {
                    Debug.LogWarning($"[Item] '{itemId}' ID가 '{other.name}'과 중복됩니다 ({name}).", this);
                    break;
                }
            }
        }
#endif
    }
}
