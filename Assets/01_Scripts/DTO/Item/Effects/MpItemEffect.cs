using _01_Scripts.Runtime.Battles;
using UnityEngine;

namespace _01_Scripts.DTO.Item.Effects
{
[CreateAssetMenu(menuName = "ProjectNeoulgyeol/Item/Effect/MP", fileName = "New MP Effect")]
public class MpItemEffect : ItemEffectBase
{
    [Tooltip("양수 = 회복, 음수 = 감소")]
    [SerializeField] private int amount;

    public override void Apply(CharacterHandler target)
    {
        CharacterStatusCalculator.Instance.ApplyMpModify(target, amount);
    }
}
}
