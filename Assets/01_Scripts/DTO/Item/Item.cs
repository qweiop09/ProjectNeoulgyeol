using _01_Scripts.Runtime.Battles;
using UnityEngine;

namespace _01_Scripts.DTO.Item
{
public enum ItemTargetType
{
    Self,       // 자신에게만
    Ally,       // 아군 한 명
    Enemy,      // 적 한 명
    AllAllies,  // 아군 전체
    AllEnemies, // 적 전체
}

[CreateAssetMenu(menuName = "ProjectNeoulgyeol/Item/Item", fileName = "New Item")]
public class Item : ScriptableObject
{
    [SerializeField] public string itemName;
    [SerializeField][TextArea] public string itemDescription;

    [SerializeField] public ItemTargetType targetType;

    [Tooltip("사용 후 인벤토리에서 제거할지 여부")]
    [SerializeField] public bool isConsumable = true;

    [SerializeField] public ItemEffectBase[] effects;

    /// <summary>아이템의 모든 효과를 대상에게 적용합니다.</summary>
    public void Use(CharacterHandler target)
    {
        if (effects == null) return;
        foreach (ItemEffectBase effect in effects)
        {
            if (effect != null)
                effect.Apply(target);
        }
    }
}
}
