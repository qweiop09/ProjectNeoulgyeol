using _01_Scripts.DTO;
using _01_Scripts.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Runtime.Battles
{
public class CharacterHandler : MonoBehaviour
{
    [SerializeField] private PlayableDirector director; // 내 캐릭터의 디렉터
    
    [SerializeField] private TimelineAsset moveToTargetTimelineAsset; // 재생할 타임라인 에셋
    
    [SerializeField] private MoveToTargetTimelineDirector moveToTargetTimelineManager;
    // [SerializeField] private /* 합 진행 타임라인 */
    
    // 애니메니터 (대기모션 등 타임라인 재생이 필요없는 애니메이션을 관리)
    
    [Space(10)]
    [Header("internal fields")]
    
    [SerializeField] private CharacterBattleData characterBattleData;

    public void SetCharacterBattleData(CharacterBattleData _characterBattleData)
    {
        characterBattleData = _characterBattleData;
    }
    public CharacterBattleData GetCharacterBattleData() => characterBattleData;

    public void PlayFight()
    {
        
    }

    public void PlayMoveToTarget(Transform targetTransform)
    {
        if (moveToTargetTimelineManager == null)
        {
            moveToTargetTimelineManager = GetComponent<MoveToTargetTimelineDirector>();
        }

        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
        }

        moveToTargetTimelineManager.PlayMoveToTargetClip(
            director,
            moveToTargetTimelineAsset,
            transform,
            targetTransform);
    }
    
    
}
}
