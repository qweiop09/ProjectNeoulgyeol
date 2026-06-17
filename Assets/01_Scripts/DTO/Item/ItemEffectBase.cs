using _01_Scripts.Runtime.Battles;
using UnityEngine;

namespace _01_Scripts.DTO.Item
{
/// <summary>
/// 아이템 효과의 기반 클래스.
/// 새로운 효과를 추가하려면 이 클래스를 상속하고 Apply를 구현하세요.
/// </summary>
public abstract class ItemEffectBase : ScriptableObject
{
    /// <summary>아이템 사용 시 효과를 적용합니다.</summary>
    public abstract void Apply(CharacterHandler target);
}
}
