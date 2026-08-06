using System.Collections.Generic;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles
{
// 캐릭터 몸에 직접 붙어서, 버프가 유지되는 동안 오라 파티클을 돌리는 월드 오브젝트.
// CharacterBuffIconRowUI(체력바 캔버스 하위, 독/언독됨)와 달리 항상 캐릭터 트랜스폼을 따라가야 하므로 별도 컴포넌트로 둔다.
public class CharacterBuffAuraController : MonoBehaviour
{
    [SerializeField] private CharacterHandler characterHandler;

    private readonly Dictionary<BuffEffectBase, ParticleSystem> activeAuras = new();

    private void Awake()
    {
        if (characterHandler == null)
            characterHandler = GetComponentInParent<CharacterHandler>();
    }

    private void OnEnable()
    {
        CharacterStatusCalculator.Instance.onBuffApplied += HandleBuffApplied;
        CharacterStatusCalculator.Instance.onBuffExpired += HandleBuffExpired;
        CharacterStatusCalculator.Instance.isCharacterDead += HandleCharacterDead;
    }

    private void OnDisable()
    {
        if (CharacterStatusCalculator.Instance == null) return; // 씬 종료 중 순서 이슈 방지
        CharacterStatusCalculator.Instance.onBuffApplied -= HandleBuffApplied;
        CharacterStatusCalculator.Instance.onBuffExpired -= HandleBuffExpired;
        CharacterStatusCalculator.Instance.isCharacterDead -= HandleCharacterDead;
    }

    private void HandleBuffApplied(CharacterStatus status, ActiveBuff buff, bool isNew)
    {
        // 갱신(재부여)은 무시 — 이미 돌고 있는 오라를 그대로 유지한다
        if (!isNew || characterHandler == null || status != characterHandler.GetCharacterStatus()) return;
        if (buff.Source.auraParticlePrefab == null) return;
        if (activeAuras.ContainsKey(buff.Source)) return;

        ParticleSystem aura = Instantiate(buff.Source.auraParticlePrefab, transform);
        aura.transform.localPosition = Vector3.zero;

        if (characterHandler.bodyRenderer != null)
        {
            var shape = aura.shape;
            shape.spriteRenderer = characterHandler.bodyRenderer;
        }

        aura.Play();
        activeAuras[buff.Source] = aura;
    }

    private void HandleBuffExpired(CharacterStatus status, ActiveBuff buff)
    {
        if (characterHandler == null || status != characterHandler.GetCharacterStatus()) return;
        if (!activeAuras.TryGetValue(buff.Source, out ParticleSystem aura)) return;

        activeAuras.Remove(buff.Source);
        StopAndCleanup(aura);
    }

    // 사망 시 버프 데이터 자체는 그대로 두되(전투 종료까지 유지되는 기존 동작), 오라만 전부 꺼준다
    private void HandleCharacterDead(CharacterStatus status)
    {
        if (characterHandler == null || status != characterHandler.GetCharacterStatus()) return;
        if (activeAuras.Count == 0) return;

        foreach (ParticleSystem aura in activeAuras.Values)
            StopAndCleanup(aura);

        activeAuras.Clear();
    }

    // 새 파티클 생성만 멈추고, 이미 나온 파티클은 자연스럽게 사라진 뒤 정리한다
    private static void StopAndCleanup(ParticleSystem aura)
    {
        if (aura == null) return;

        aura.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(aura.gameObject, ParticleLifetimeUtility.GetTotalLifetime(aura));
    }
}
}
