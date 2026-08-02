using _01_Scripts.DTO;

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

// 순수 계산부: 부작용 없이 데미지 값만 산출한다.
public static class DamageCalculation
{
    public static DamageResult Calculate(HitContext ctx)
    {
        float defense = ApplyDefense(ctx, ctx.Target.GetDefense()); // 현재 결과 미사용 (아래 참고)
        float qteMultiplier = GetQteMultiplier(ctx.Result, ctx.Multipliers);

        int hpDamage = (int)(ctx.Attacker.GetAttack() * qteMultiplier * ctx.HpDamageCoefficient);
        int staminaDamage = (int)(ctx.Attacker.GetAttack() * qteMultiplier * ctx.StaminaDamageCoefficient);

        var result = new DamageResult { HpDamage = hpDamage, StaminaDamage = staminaDamage };

        if (ctx.HitEffects != null)
            foreach (var effect in ctx.HitEffects)
                effect.OnHitResolved(ctx, result);

        return result;
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
    private static float GetQteMultiplier(QTEResult result, QteMultiplierSet multipliers)
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
