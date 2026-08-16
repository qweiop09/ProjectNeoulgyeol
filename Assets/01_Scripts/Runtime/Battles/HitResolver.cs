namespace _01_Scripts.Runtime.Battles
{
// 피해가 실제로 대상에게 적용되는 "그 순간"을 한 곳에서 처리한다.
// 공격자 쪽 효과(스킬의 IHitEffect)와 피격자 쪽 반응(장비의 IEquipment.OnHitReceived)을
// 여기서 순서대로 같이 처리해서, 히트 하나를 놓고 로직이 여러 파일에 흩어지지 않게 한다.
public static class HitResolver
{
    // finalDamage는 호출부가 DamageCalculation.ResolveFinalDamage로 이미 "결론적인" 값까지 낸 뒤 넘겨야 한다 —
    // 여기선 그 값을 실제로 스탯에 반영하고, 공격자/피격자 훅을 그 값 기준으로 돌리기만 한다.
    public static void ApplyHit(HitContext ctx, DamageResult finalDamage)
    {
        CharacterStatusCalculator.Instance.ApplyHpModify(ctx.Target, -finalDamage.HpDamage);
        if (finalDamage.StaminaDamage != 0)
            CharacterStatusCalculator.Instance.ApplyStaminaModify(ctx.Target, -finalDamage.StaminaDamage);

        if (ctx.HitEffects != null)
            foreach (IHitEffect effect in ctx.HitEffects)
                effect.OnHitResolved(ctx, finalDamage);

        foreach (var equipment in ctx.Target.GetAllEquipment())
            equipment.OnHitReceived(ctx, finalDamage);
    }
}
}
