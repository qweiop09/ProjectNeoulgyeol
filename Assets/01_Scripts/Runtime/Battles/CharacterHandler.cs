using _01_Scripts.DTO;
using _01_Scripts.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace _01_Scripts.Runtime.Battles
{
public class CharacterHandler : MonoBehaviour
{
    [SerializeField] public TimelineDirector timelineDirector; 
    [SerializeField] public PlayableDirector director; 
    
    // 애니메니터 (대기모션 등 타임라인 재생이 필요없는 애니메이션을 관리)
    
    [Space(10)]
    [Header("internal fields")]
    
    [SerializeField] public CharacterBattleData characterBattleData;

    public void SetCharacterBattleData(CharacterBattleData _characterBattleData)
    {
        characterBattleData = _characterBattleData;
    }
    public CharacterBattleData GetCharacterBattleData() => characterBattleData;
    
}
}
