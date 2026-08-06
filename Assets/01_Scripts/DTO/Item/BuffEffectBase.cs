using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles;
using UnityEngine;

namespace _01_Scripts.DTO.Item
{
    // 지속 효과(버프/디버프)의 공통 뼈대. 등록/갱신 로직은 CharacterStatusCalculator.ApplyBuff로 통일하고,
    // 실제 크기 계산만 하위 클래스(ResolveAmount)에 맡긴다 — 나중에 도트뎀지 같은
    // 다른 지속효과가 추가돼도 이 등록/갱신/이름·아이콘 처리는 그대로 재사용된다.
    // 버프인지 디버프인지 — 수치 부호로 추론하지 않고 명시적으로 표기한다.
    // (나중에 기절/침묵처럼 수치가 없는 상태이상이 추가돼도 그대로 쓸 수 있도록)
    public enum BuffKind { Buff, Debuff }

    public abstract class BuffEffectBase : ItemEffectBase
    {
        [Header("표시 정보")]
        public string buffName;
        public Sprite icon;
        public BuffKind kind = BuffKind.Buff;

        [Tooltip("지속 라운드 수. 부여된 라운드부터 카운트하며, 그 라운드 종료 시 1 감소한다.")]
        public int durationRounds = 1;

        [Tooltip("버프가 만료될 때 재생할 파티클. 비워두면 버프 아이콘 UI의 기본 만료 파티클을 사용한다.")]
        public ParticleSystem expiredParticlePrefab;

        [Tooltip("버프가 만료될 때 재생할 사운드. 비워두면 버프 아이콘 UI의 기본 만료 사운드를 사용하고, 그것도 없으면 아무 소리도 안 난다.")]
        public AudioClip expiredSoundClip;

        [Tooltip("버프가 유지되는 동안 캐릭터 몸에 계속 붙어있는 오라 파티클(Loop). 비워두면 오라 없음. " +
                 "Shape 모듈을 Sprite/SpriteRenderer로 설정해두면 런타임에 그 캐릭터의 실루엣을 따라간다.")]
        public ParticleSystem auraParticlePrefab;

        protected abstract int ResolveAmount(CharacterStatus caster, CharacterStatus target);

        public override void Apply(CharacterStatus caster, CharacterStatus target, float qteMultiplier)
        {
            int resolved = Mathf.RoundToInt(ResolveAmount(caster, target) * qteMultiplier);
            CharacterStatusCalculator.Instance.ApplyBuff(target, this, resolved, durationRounds);
        }
    }
}
