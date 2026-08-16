using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles
{
public struct HitContext
{
    public CharacterStatus Attacker;
    public CharacterStatus Target;
    public QTEResult Result;
    public float HpDamageCoefficient;
    public float StaminaDamageCoefficient;
    public QteMultiplierSet Multipliers;
    public IHitEffect[] HitEffects; // 없으면 System.Array.Empty<IHitEffect>()
}

public struct DamageResult
{
    public int HpDamage;
    public int StaminaDamage;
}

// 순수 계산부: 부작용 없이 데미지 값만 산출한다. 히트가 실제로 적용될 때 벌어지는 일
// (공격자 IHitEffect.OnHitResolved, 피격자 IEquipment.OnHitReceived 등)은 HitResolver가 담당한다.
public static class DamageCalculation
{
    public static DamageResult Calculate(HitContext ctx)
    {
        float defense = ApplyDefense(ctx, ctx.Target.GetDefense()); // 현재 결과 미사용 (아래 참고)
        float qteMultiplier = GetQteMultiplier(ctx.Result, ctx.Multipliers);

        int hpDamage = (int)(ctx.Attacker.GetAttack() * qteMultiplier * ctx.HpDamageCoefficient);
        int staminaDamage = (int)(ctx.Attacker.GetAttack() * qteMultiplier * ctx.StaminaDamageCoefficient);

        return new DamageResult { HpDamage = hpDamage, StaminaDamage = staminaDamage };
    }

    // 원본 피해량(양수)에 "최종 반영 직전" 배율(흐트러짐 등, 앞으로 크리티컬/속성 저항 등이 추가될 자리)을 전부
    // 적용해서 결론적인 피해량을 낸다 — 스킬/아이템 소스와 무관하게 공용으로 쓰인다. 이 반환값이 곧 실제로
    // 적용될 값이므로, 데미지 텍스트 등 표시에도 이 값을 그대로 써야 한다.
    public static int ResolveFinalDamage(int rawDamage, CharacterStatus target)
    {
        if (target.currentState == CharacterState.Staggered)
            rawDamage = Mathf.RoundToInt(rawDamage * CharacterStatusCalculator.Instance.StaggeredDamageMultiplier);

        return rawDamage;
    }

    // HP+스태미나를 한 번에 처리하는 오버로드 — HitResolver처럼 DamageResult를 통째로 다루는 곳에서 사용.
    public static DamageResult ResolveFinalDamage(DamageResult rawDamage, CharacterStatus target)
    {
        return new DamageResult
        {
            HpDamage = ResolveFinalDamage(rawDamage.HpDamage, target),
            StaminaDamage = ResolveFinalDamage(rawDamage.StaminaDamage, target)
        };
    }

    // 방어력 반영 지점 — 실제 공식 미정, 지금은 항등(no-op)
    private static float ApplyDefense(HitContext ctx, float currentDefense)
    {
        float defense = currentDefense;
        if (ctx.HitEffects != null)
            foreach (var effect in ctx.HitEffects)
                defense = effect.ModifyDefense(ctx, defense);
        return defense; // TODO: 데미지 계산에 반영 (공식 정해지면)
    }

    // 캐스터가 아군/적군인지와 무관하게 QteClip이 들고 있는 배율을 그대로 적용한다.
    // 아군용/적군용 스킬을 애초에 따로 만들 것이므로, 코드에서 캐스터별로 배율을 바꿀 필요가 없다.
    // public: 아이템 효과 크기 계산(ApplyQteResult의 아이템 분기)에서도 동일하게 재사용된다.
    public static float GetQteMultiplier(QTEResult result, QteMultiplierSet multipliers)
    {
        return result switch
        {
            QTEResult.Perfect => multipliers.perfect,
            QTEResult.Good    => multipliers.good,
            QTEResult.Hit     => multipliers.hit,
            _                 => 1.0f
        };
    }
}
}
