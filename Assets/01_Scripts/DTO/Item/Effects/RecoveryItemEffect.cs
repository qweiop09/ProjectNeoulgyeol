using _01_Scripts.Runtime.Battles;
using UnityEngine;

namespace _01_Scripts.DTO.Item.Effects
{
[CreateAssetMenu(menuName = "ProjectNeoulgyeol/Item/Effect/Recovery", fileName = "New Recovery Effect")]
public class RecoveryItemEffect : ItemEffectBase
{
    [Tooltip("양수 = 회복, 음수 = 감소")]
    [SerializeField] private int hp;
    [SerializeField] private int mp;
    [SerializeField] private int stamina;

    public override void Apply(CharacterHandler target)
    {
        if (hp != 0)      CharacterStatusCalculator.Instance.ApplyHpModify(target, hp);
        if (mp != 0)      CharacterStatusCalculator.Instance.ApplyMpModify(target, mp);
        if (stamina != 0) CharacterStatusCalculator.Instance.ApplyStaminaModify(target, stamina);
    }
}
}
