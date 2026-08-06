using _01_Scripts.DTO;
using UnityEngine;
using UnityEngine.Timeline;

namespace _01_Scripts.DTO.Item
{
    // 장비/소모품/재료/재화/이벤트(중요) 아이템 5종. 필요하면 얼마든지 추가/삭제 가능.
    public enum ItemCategory
    {
        Equipment,
        Consumable,
        Material,
        Currency,
        EventItem,
    }

    [CreateAssetMenu(menuName = "ProjectNeoulgyeol/Item/Item", fileName = "New Item")]
    public class Item : ScriptableObject, ITargetSpec
    {
        [Tooltip("세이브 등에 쓰일 고유 문자열 ID. 예: consumable_hp_potion_small")]
        [SerializeField] private string itemId;
        public string ItemId => itemId;

        [SerializeField] public string itemName;
        [SerializeField][TextArea] public string itemDescription;
        [SerializeField] public Sprite icon;

        [SerializeField] public TargetScope targetScope = TargetScope.Any;
        [SerializeField] public TargetCount targetCount = TargetCount.Single;
        [Tooltip("targetCount가 Fixed/Random일 때만 사용하는 인원 수")]
        [SerializeField] public int targetCountValue = 1;

        TargetScope ITargetSpec.TargetScope => targetScope;
        TargetCount ITargetSpec.TargetCount => targetCount;
        int ITargetSpec.TargetCountValue => targetCountValue;

        [SerializeField] public ItemCategory category = ItemCategory.Consumable;

        [Tooltip("한 슬롯에 몇 개까지 겹칠 수 있는지. 0이면 겹치지 않는 것으로 취급하고 인벤토리에 수량을 표시하지 않는다 (장비/이벤트 아이템용).")]
        [SerializeField] public int maxStack = 999;

        [Tooltip("배열 순서(위→아래)대로 적용됩니다. 뒤 효과는 앞 효과가 반영된 스탯을 참조합니다(예: 최대체력 버프 다음에 '최대체력 비율 회복'을 넣으면 늘어난 최대체력 기준으로 회복량이 계산됨).")]
        [SerializeField] public ItemEffectBase[] effects;

        [Tooltip("이 아이템 전용 QTE 타임라인. 비어있으면 CompeteContestController의 공용(기본) 아이템 타임라인을 사용한다.")]
        [SerializeField] public TimelineAsset itemTimelineAsset;
        [Tooltip("itemTimelineAsset과 함께 쓰는 바인더. 비어있으면 공용 바인더를 사용한다.")]
        [SerializeField] public ITimelineBinder timelineBinder;

        public bool IsStackable => maxStack > 0;

        // qteMultiplier: QTE 판정 등급(Perfect/Good/Hit)에 따른 배율. 기본 1(배율 없음).
        public void Use(CharacterStatus caster, CharacterStatus target, float qteMultiplier = 1f)
        {
            if (effects == null) return;
            foreach (var effect in effects)
                if (effect != null)
                    effect.Apply(caster, target, qteMultiplier);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            // 자신(Self)은 항상 정확히 1명이라 모두/랜덤과 조합될 수 없음 — Self인 동안은 매 검증마다 Single로 되돌려서
            // "자신 선택 시 모두/랜덤 선택 불가"와 "모두/랜덤이면 자신 선택 불가"를 동시에 만족시킨다.
            if (targetScope == TargetScope.Self)
                targetCount = TargetCount.Single;

            if (string.IsNullOrEmpty(itemId))
                return;

            foreach (var guid in UnityEditor.AssetDatabase.FindAssets($"t:{nameof(Item)}"))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Item other = UnityEditor.AssetDatabase.LoadAssetAtPath<Item>(path);
                if (other != null && other != this && other.itemId == itemId)
                {
                    Debug.LogWarning($"[Item] '{itemId}' ID가 '{other.name}'과 중복됩니다 ({name}).", this);
                    break;
                }
            }
        }
#endif
    }
}
