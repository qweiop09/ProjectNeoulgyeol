using System.Collections.Generic;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using _01_Scripts.Runtime.Battles;
using UnityEngine;

// 캐릭터 한 명의 활성 버프 아이콘을 관리한다. CharacterStatBarUI의 자식으로 붙어서
// 체력/스태미나 바와 같은 트랜스폼 계층을 공유하므로(월드 추적, 독/언독) 별도 위치 갱신이 필요 없다.
public class CharacterBuffIconRowUI : MonoBehaviour
{
    [SerializeField] private CharacterBuffIconUI iconPrefab;

    [Tooltip("버프 전용 컨테이너. 디버프와 섞이지 않고 별도 줄에 쌓인다. " +
             "GridLayoutGroup + 아래쪽 피벗(하단 앵커) 조합으로 개수가 늘어날수록 위로 자라도록 배치할 것.")]
    [SerializeField] private Transform buffIconParent;

    [Tooltip("디버프 전용 컨테이너. buffIconParent와 같은 방식으로 별도 줄에 위로 쌓인다.")]
    [SerializeField] private Transform debuffIconParent;

    [SerializeField] private ParticleSystem defaultExpiredParticlePrefab; // 버프 쪽에 expiredParticlePrefab이 없을 때 폴백
    [SerializeField] private AudioClip defaultExpiredSoundClip;           // 버프 쪽에 expiredSoundClip이 없을 때 폴백

    private CharacterHandler characterHandler;
    private CharacterStatus target;
    private readonly Dictionary<BuffEffectBase, CharacterBuffIconUI> icons = new();

    public void Initialize(CharacterHandler handler)
    {
        characterHandler = handler;
        target = handler.GetCharacterStatus();

        // 재초기화 등으로 이미 걸려있는 버프가 있으면 아이콘을 바로 맞춰준다
        foreach (ActiveBuff buff in target.activeBuffs)
            SpawnIcon(buff);
    }

    private void OnEnable()
    {
        CharacterStatusCalculator.Instance.onBuffApplied += HandleBuffApplied;
        CharacterStatusCalculator.Instance.onBuffExpired += HandleBuffExpired;
    }

    private void OnDisable()
    {
        if (CharacterStatusCalculator.Instance == null) return; // 씬 종료 중 순서 이슈 방지
        CharacterStatusCalculator.Instance.onBuffApplied -= HandleBuffApplied;
        CharacterStatusCalculator.Instance.onBuffExpired -= HandleBuffExpired;
    }

    private void HandleBuffApplied(CharacterStatus status, ActiveBuff buff, bool isNew)
    {
        if (target == null || status != target) return;

        if (isNew)
        {
            SpawnIcon(buff);
            return;
        }

        if (icons.TryGetValue(buff.Source, out CharacterBuffIconUI icon))
        {
            icon.SetRemainingRounds(buff.RemainingRounds);
            icon.PlayRefreshPulse();
        }
    }

    private void HandleBuffExpired(CharacterStatus status, ActiveBuff buff)
    {
        if (target == null || status != target) return;
        if (!icons.TryGetValue(buff.Source, out CharacterBuffIconUI icon)) return;

        icons.Remove(buff.Source);

        ParticleSystem particlePrefab = buff.Source.expiredParticlePrefab != null
            ? buff.Source.expiredParticlePrefab
            : defaultExpiredParticlePrefab;
        ParticleLifetimeUtility.SpawnAndAutoDestroy(particlePrefab, icon.transform.position, Quaternion.identity);

        AudioClip soundClip = buff.Source.expiredSoundClip != null
            ? buff.Source.expiredSoundClip
            : defaultExpiredSoundClip;
        PlaySound(soundClip);

        icon.PlayExpireAndDestroy();
    }

    // 시각/청각 둘 다 선택 사항 — 클립이 없으면(버프에도 기본값에도) 조용히 아무것도 안 낸다
    private void PlaySound(AudioClip clip)
    {
        if (clip == null || characterHandler == null || characterHandler.sfxSource == null) return;
        characterHandler.sfxSource.PlayOneShot(clip);
    }

    private void SpawnIcon(ActiveBuff buff)
    {
        Transform parent = buff.Source.kind == BuffKind.Debuff ? debuffIconParent : buffIconParent;
        if (iconPrefab == null || parent == null) return;

        CharacterBuffIconUI icon = Instantiate(iconPrefab, parent);
        icon.Initialize(buff.Source.icon, buff.RemainingRounds);
        icons[buff.Source] = icon;
    }
}
