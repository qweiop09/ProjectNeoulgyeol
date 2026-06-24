using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.DTO.Item
{
    public abstract class ItemEffectBase : ScriptableObject
    {
        public abstract void Apply(CharacterBattleData target);
    }
}
