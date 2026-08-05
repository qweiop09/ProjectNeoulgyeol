using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.DTO.Item
{
    // 지속 효과(버프/디버프)의 공통 뼈대. 등록/갱신 로직은 여기서 통일하고,
    // 실제 크기 계산만 하위 클래스(ResolveAmount)에 맡긴다 — 나중에 도트뎀지 같은
    // 다른 지속효과가 추가돼도 이 등록/갱신/이름·아이콘 처리는 그대로 재사용된다.
    public abstract class BuffEffectBase : ItemEffectBase
    {
        [Header("표시 정보")]
        public string buffName;
        public Sprite icon;

        [Tooltip("지속 라운드 수. 부여된 라운드부터 카운트하며, 그 라운드 종료 시 1 감소한다.")]
        public int durationRounds = 1;

        protected abstract int ResolveAmount(CharacterStatus caster, CharacterStatus target);

        public override void Apply(CharacterStatus caster, CharacterStatus target)
        {
            int resolved = ResolveAmount(caster, target);
            ActiveBuff existing = target.activeBuffs.Find(b => b.Source == this);

            if (existing != null)
            {
                // 같은 소스(같은 스킬/아이템)면 새로 쌓지 않고 갱신
                existing.ResolvedAmount = resolved;
                existing.RemainingRounds = durationRounds;
                Debug.Log($"[BuffEffectBase] '{buffName}' 갱신 (값 {resolved}, {durationRounds}라운드)");
            }
            else
            {
                target.activeBuffs.Add(new ActiveBuff
                {
                    Source = this,
                    ResolvedAmount = resolved,
                    RemainingRounds = durationRounds
                });
                Debug.Log($"[BuffEffectBase] '{buffName}' 신규 부여 (값 {resolved}, {durationRounds}라운드)");
            }
        }
    }
}
