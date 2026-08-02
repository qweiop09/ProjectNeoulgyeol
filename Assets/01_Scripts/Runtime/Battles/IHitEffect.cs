namespace _01_Scripts.Runtime.Battles
{
public interface IHitEffect
{
    // 방어력 계산 전 훅 (관통, 방어 무시 등)
    float ModifyDefense(HitContext ctx, float currentDefense) => currentDefense;

    // 데미지 계산 후 훅 (흡혈, 상태이상 부여, 버프 생성 등)
    void OnHitResolved(HitContext ctx, DamageResult result) { }
}
}
