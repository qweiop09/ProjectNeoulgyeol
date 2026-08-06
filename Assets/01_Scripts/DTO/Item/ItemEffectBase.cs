using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.DTO.Item
{
    public abstract class ItemEffectBase : ScriptableObject
    {
        // qteMultiplier: QTE 판정 등급(Perfect/Good/Hit)에 따른 배율 — 효과 크기에 곱해서 반영한다.
        public abstract void Apply(CharacterStatus caster, CharacterStatus target, float qteMultiplier);
    }
}
