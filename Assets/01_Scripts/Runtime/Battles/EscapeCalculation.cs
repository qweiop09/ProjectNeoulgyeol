using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles
{
// 순수 계산부 — StatusCalculation/DamageCalculation과 같은 자리, 부작용 없이 확률만 산출한다.
public static class EscapeCalculation
{
    // allySpeedSum: 도망을 선택한 아군들의 속도 합
    // enemies: 살아있는 적 각각의 (속도, 그 적의 escapeResistance)
    // battleEscapeDifficulty: 전투 전체에 걸리는 배율
    // minChance/maxChance: 최종 확률을 이 범위로 clamp (하한 없으면 영원히 도망 불가능해질 수 있고, 상한 없으면 100% 확정 탈출이 나올 수 있음)
    public static float CalculateChance(
        float allySpeedSum,
        IEnumerable<(float speed, float resistance)> enemies,
        float battleEscapeDifficulty,
        float minChance,
        float maxChance)
    {
        float denominator = battleEscapeDifficulty * enemies.Sum(e => e.speed * e.resistance);
        if (denominator <= 0f) return maxChance; // 적이 없거나 저항 총합이 0이면 사실상 확정 성공

        float raw = allySpeedSum / denominator;
        return Mathf.Clamp(raw, minChance, maxChance);
    }
}
}
