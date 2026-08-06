using System.Linq;
using _01_Scripts.Runtime.Battles;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline.Binder
{
// SkillTimelineBinder의 애니메이션/QTE 바인딩을 그대로 재사용하고, 아이템 전용으로
// "손에 든 아이템" 슬롯의 스프라이트 교체 + ItemHoldTrack(Activation Track) 바인딩만 추가한다.
[CreateAssetMenu(menuName = "ProjectNeoulgyeol/Binder/Item", fileName = "ItemTimelineBinder")]
public class ItemTimelineBinder : SkillTimelineBinder
{
    [Header("아이템 손 슬롯 Track 이름")]
    [SerializeField] private string itemHoldTrackName = "ItemHoldTrack";

    public override void Bind(PlayableDirector director, ActData data)
    {
        base.Bind(director, data); // 애니메이션/QTE 트랙은 기존 그대로

        if (data is not ItemActData itemData) return;

        CharacterHandler caster = itemData.CastPlayerCharacter;
        if (caster == null || itemData.UseItem == null)
        {
            Debug.LogError("ItemTimelineBinder: 바인딩에 필요한 값이 비어있습니다.");
            return;
        }

        if (caster.heldItemRenderer == null)
        {
            Debug.LogWarning("ItemTimelineBinder: heldItemRenderer가 캐릭터에 없습니다.");
            return;
        }

        caster.heldItemRenderer.sprite = itemData.UseItem.icon;

        var timeline = director.playableAsset as TimelineAsset;
        TrackAsset holdTrack = timeline?.GetOutputTracks().FirstOrDefault(t => t.name == itemHoldTrackName);

        if (holdTrack != null)
        {
            director.SetGenericBinding(holdTrack, caster.heldItemRenderer.gameObject);
            Debug.Log("ItemTimelineBinder: 손 슬롯 트랙 바인딩 완료");
        }
        else
        {
            Debug.LogWarning($"ItemTimelineBinder: '{itemHoldTrackName}' 트랙을 찾을 수 없습니다.");
        }
    }
}
}
