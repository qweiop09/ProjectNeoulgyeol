using System;
using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.DTO.Item
{
    public enum ValueMode
    {
        Flat,       // 고정 수치 그대로 사용
        Reference,  // 캐스터 또는 대상의 특정 스탯 대비 비율로 계산
    }

    public enum ReferenceOwner
    {
        Caster,
        Target,
    }

    public enum ReferenceStat
    {
        CurrentHp,
        MaxHp,
        CurrentMp,
        MaxMp,
        CurrentStamina,
        MaxStamina,
        Attack,
        Defense,
    }

    // 효과 하나의 크기를 "고정값" 또는 "누군가의 스탯 대비 비율"로 표현한다.
    // 예: Flat 30 = 그냥 30 / Reference(Target, MaxHp, 0.5) = 대상 최대체력의 50%
    [Serializable]
    public struct EffectValue
    {
        public ValueMode mode;

        [Tooltip("Flat일 때 사용되는 고정 수치")]
        public int flatAmount;

        [Tooltip("Reference일 때: 누구의 스탯을 기준으로 할지")]
        public ReferenceOwner referenceOwner;
        [Tooltip("Reference일 때: 어떤 스탯을 기준으로 할지")]
        public ReferenceStat referenceStat;
        [Tooltip("Reference일 때: 기준 스탯 대비 비율 (0.5 = 50%)")]
        public float referenceRatio;

        public int Resolve(CharacterStatus caster, CharacterStatus target)
        {
            if (mode == ValueMode.Flat)
                return flatAmount;

            CharacterStatus source = referenceOwner == ReferenceOwner.Caster ? caster : target;
            if (source == null) return 0;

            return Mathf.RoundToInt(GetStat(source, referenceStat) * referenceRatio);
        }

        private static int GetStat(CharacterStatus status, ReferenceStat stat)
        {
            return stat switch
            {
                ReferenceStat.CurrentHp      => status.currentHp,
                ReferenceStat.MaxHp          => status.GetMaxHp(),
                ReferenceStat.CurrentMp      => status.currentMp,
                ReferenceStat.MaxMp          => status.GetMaxMp(),
                ReferenceStat.CurrentStamina => status.currentStamina,
                ReferenceStat.MaxStamina     => status.GetMaxStamina(),
                ReferenceStat.Attack         => status.GetAttack(),
                ReferenceStat.Defense        => status.GetDefense(),
                _ => 0
            };
        }
    }
}
