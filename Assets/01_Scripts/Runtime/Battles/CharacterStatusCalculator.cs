using System;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using _01_Scripts.Interfacese;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles
{
// 순수 계산부: 부작용 없이 값만 산출한다. MonoBehaviour에 의존하지 않으므로
// 월드/전투 어느 쪽에서도 동일하게 재사용할 수 있다.
public static class StatusCalculation
{
    public struct Result
    {
        public int NewValue;
        public bool HitFloor; // 0으로 클램프됨
        public bool HitCap;   // 최대치로 클램프됨
    }

    public static Result Modify(int current, int amount, int max)
    {
        int newValue = Mathf.Clamp(current + amount, 0, max);
        return new Result
        {
            NewValue = newValue,
            HitFloor = newValue == 0,
            HitCap = newValue == max
        };
    }
}

// 적용부: 계산 결과를 CharacterStatus에 반영하고 Dead/Stagger 이벤트를 발행한다.
// Dead/Stagger 구독을 MonoBehaviour 이벤트로 제공하기 위해 Singleton으로 유지하지만,
// 대상 타입이 CharacterHandler가 아니라 CharacterStatus이므로 월드 씬에서도 호출 가능하다.
public class CharacterStatusCalculator : Singleton<CharacterStatusCalculator>
{
    [Tooltip("흐트러짐 상태인 캐릭터가 받는 모든 피해(HP/스태미나)에 마지막으로 곱해지는 배율 — 실제 계산은 DamageCalculation.ResolveFinalDamage가 담당, 여기선 인스펙터 튜닝용 값만 들고 있음")]
    [SerializeField] private float staggeredDamageMultiplier = 1.5f;
    public float StaggeredDamageMultiplier => staggeredDamageMultiplier;

    public Action<CharacterStatus> isCharacterDead;
    public Action<CharacterStatus> isCharacterStagger;
    public Action<CharacterStatus> onStatusChanged; // hp/mp/stamina 중 하나라도 바뀔 때마다 발행 (체력바 등 UI 갱신용)
    public Action<CharacterStatus, ActiveBuff, bool> onBuffApplied; // bool: 신규 부여(true) / 갱신(false) — 버프 아이콘 UI 갱신용
    public Action<CharacterStatus, ActiveBuff> onBuffExpired;       // 버프 아이콘 UI 제거용
    public Action<CharacterStatus, ActiveBuff> onBuffTicked;        // 라운드 종료로 남은 라운드만 줄어들 때(아직 안 만료) — 아이콘 숫자만 갱신용


    public void UseSkill(CharacterStatus caster, CharacterSkill skill)
    {
        ApplyMpModify(caster, -skill.skillMpCost);
        ApplyStaminaModify(caster, -skill.skillStaminaCost);
    }

    // HP 변화 — 순수하게 스탯 반영 + 이벤트만 담당한다. 흐트러짐 배율 등 "얼마나 깎일지"는 이 메서드에
    // 도달하기 전에 DamageCalculation.ResolveFinalDamage가 이미 끝내둔 값이어야 한다.
    public void ApplyHpModify(CharacterStatus target, int amount)
    {
        var result = StatusCalculation.Modify(target.currentHp, amount, target.GetMaxHp());
        target.currentHp = result.NewValue;

        if (result.HitFloor)
        {
            target.SetCurrentState(CharacterState.Dead);
            isCharacterDead?.Invoke(target);
        }

        onStatusChanged?.Invoke(target);

        Debug.Log(target.CharacterData.characterName + " 회복: " + amount + " / 현재 HP: " + target.currentHp);
    }

    // MP 변화
    public void ApplyMpModify(CharacterStatus target, int amount)
    {
        var result = StatusCalculation.Modify(target.currentMp, amount, target.GetMaxMp());
        target.currentMp = result.NewValue;

        onStatusChanged?.Invoke(target);

        Debug.Log(target.CharacterData.characterName + " MP 회복: " + amount + " / 현재 MP: " + target.currentMp);
    }

    // 스테미나 변화 — ApplyHpModify와 마찬가지로 순수 반영만 담당(배율은 호출 전에 이미 반영된 값이어야 함).
    public void ApplyStaminaModify(CharacterStatus target, int amount)
    {
        var result = StatusCalculation.Modify(target.currentStamina, amount, target.GetMaxStamina());
        target.currentStamina = result.NewValue;

        // Normal 상태에서만 흐트러짐 진입 — 이미 흐트러진 상태에서 또 맞아도 재발동(회복 타이밍 리셋) 안 되게,
        // 사망 상태에서도 안 걸리게(한 히트에 HP/스테미나가 동시에 0이 되는 경우 대비) 막는다.
        if (result.HitFloor && target.currentState == CharacterState.Normal)
        {
            target.SetCurrentState(CharacterState.Staggered);
            target.StaggerJustStarted = true; // 흐트러진 이번 라운드는 회복 카운트에서 제외(ClosePhaseController가 참고)
            isCharacterStagger?.Invoke(target);
        }

        onStatusChanged?.Invoke(target);

        Debug.Log(target.CharacterData.characterName + " 스테미나 회복: " + amount + " / 현재 스테미나: " + target.currentStamina);
    }

    // 버프 부여 — 같은 Source가 이미 걸려있으면 갱신(재적용), 없으면 새로 추가한다.
    public ActiveBuff ApplyBuff(CharacterStatus target, BuffEffectBase source, int resolvedAmount, int durationRounds)
    {
        ActiveBuff existing = target.activeBuffs.Find(b => b.Source == source);
        bool isNew = existing == null;

        ActiveBuff buff;
        if (existing != null)
        {
            existing.ResolvedAmount = resolvedAmount;
            existing.RemainingRounds = durationRounds;
            buff = existing;
            Debug.Log($"[CharacterStatusCalculator] '{source.buffName}' 갱신 (값 {resolvedAmount}, {durationRounds}라운드)");
        }
        else
        {
            buff = new ActiveBuff { Source = source, ResolvedAmount = resolvedAmount, RemainingRounds = durationRounds };
            target.activeBuffs.Add(buff);
            Debug.Log($"[CharacterStatusCalculator] '{source.buffName}' 신규 부여 (값 {resolvedAmount}, {durationRounds}라운드)");
        }

        onBuffApplied?.Invoke(target, buff, isNew);
        return buff;
    }

    // 버프 만료/해제
    public void ExpireBuff(CharacterStatus target, ActiveBuff buff)
    {
        target.activeBuffs.Remove(buff);
        onBuffExpired?.Invoke(target, buff);
    }
}
}