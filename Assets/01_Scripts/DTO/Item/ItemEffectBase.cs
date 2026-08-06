using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.DTO.Item
{
    public abstract class ItemEffectBase : ScriptableObject
    {
        // qteMultiplier: QTE 판정 등급(Perfect/Good/Hit)에 따른 배율 — 효과 크기에 곱해서 반영한다.
        // 반환값: 대상 위에 수치 텍스트로 띄울 값(회복/피해량)이 있으면 그 값, 없으면(버프 등 이미 자체 연출이 있는 효과) null.
        public abstract int? Apply(CharacterStatus caster, CharacterStatus target, float qteMultiplier);
    }
}
